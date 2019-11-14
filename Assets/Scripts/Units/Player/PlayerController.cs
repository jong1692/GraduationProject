using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : UnitController
{
    private static PlayerController instance;

    [SerializeField]
    private AudioSource locomotionAudioSource;
    [SerializeField]
    private float lockOnRadius = 7.0f;
    [SerializeField]
    private LayerMask lockOnLayerMask;
    [SerializeField]
    private Transform lockOnImage;

    private RaycastHit[] raycastHitBuffers;

    private PlayerInput playerInput;
    private CameraSetting cameraSetting;

    const int hitBufferSize = 32;

    public CameraSetting CameraSetting
    {
        get { return cameraSetting; }
        set { cameraSetting = value; }
    }

    public static PlayerController Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        initialize();
    }

    void FixedUpdate()
    {
        calculateHorizontalMovement();
        calculateVerticalMovement();

        if ((playerInput.IsMoveInput && !isDied && !inAttacking))
        {
            setRotation();
            updateRotation();
        }

        if (playerInput.Attack)
        {
            StopCoroutine(beginAttackMotion());
            StartCoroutine(beginAttackMotion());
        }

        if (playerInput.Roll && !isDied)
        {
            StartCoroutine(beginRollMotion());
        }

        if (playerInput.Block)
        {
            beginBlock();
        }
        else if (!inLockOn)
        {
            endBlock();
        }

        if (playerInput.LockOn)
        {
            setTarget();
        }

        if (target != null && !target.IsDied)
        {
            locateTargetImage();
        }
        else if (target != null && target.IsDied)
        {
            resetTarget();
        }
    }

    private IEnumerator beginRollMotion()
    {
        animator.SetTrigger(rollStr);

        inRolling = true;
        animator.SetBool(inRollingStr, inRolling);

        damageable.Invincibility = true;

        while (!checkCurAnimationWithTag(rollStr))
        {
            yield return null;
        }

        while (checkCurAnimationWithTag(rollStr))
        {
            animator.SetFloat(stateTimeStr, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

            yield return null;
        }

        inRolling = false;
        animator.SetBool(inRollingStr, inRolling);

        damageable.Invincibility = false;
    }

    protected override void damaged(Damageable.DamageMessage msg)
    {
        base.damaged(msg);

        var cameraShakeScript = cameraSetting.FreeLookCamera.GetComponent<CameraShake>();
        StartCoroutine(cameraShakeScript.cameraShake(0.25f, 0.4f));
    }

    protected override void initialize()
    {
        if (instance == null) instance = this;

        base.initialize();

        playerInput = GetComponent<PlayerInput>();

        raycastHitBuffers = new RaycastHit[hitBufferSize];
    }

    protected override void calculateHorizontalMovement()
    {
        Vector3 movement = playerInput.Movement;
        movement.Normalize();

        targetMovementSpeed = movement.magnitude * maxMovementSpeed;

        if (playerInput.IsMoveInput)
        {
            acceleration = groundAcceleration;
        }
        else
        {
            acceleration = groundDeceleration;
        }

        base.calculateHorizontalMovement();
    }

    protected override void moveUnit()
    {
        base.moveUnit();

        if (checkCurAnimationWithTag(locomotionStr) && !locomotionAudioSource.isPlaying)
        {
            locomotionAudioSource.Play();
        }
        else if (!checkCurAnimationWithTag(locomotionStr))
        {
            locomotionAudioSource.Stop();
        }

        if (inLockOn)
        {
            setRotation();
            transform.rotation = targetRotation;
        }
    }

    protected override void calculateVerticalMovement()
    {
        base.calculateVerticalMovement();

        if (isGrounded && playerInput.Jump && !inAttacking)
        {
            verticalMovementSpeed = jumpPower;
            isGrounded = false;
        }
    }

    protected void resetTarget()
    {
        target = null;
        lockOnImage.gameObject.SetActive(false);

        inLockOn = false;
        animator.SetBool(inLockOnStr, inLockOn);

        cameraSetting.changeFreeCamera();
    }

    protected void beginBlock()
    {
        inBlocking = true;
        animator.SetBool(inBlockingStr, inBlocking);
    }

    protected void endBlock()
    {
        inBlocking = false;
        animator.SetBool(inBlockingStr, inBlocking);
    }

    protected void locateTargetImage()
    {
        Transform centerTarget = target.transform.Find("CenterTarget").gameObject.transform;
        Vector3 screen = Camera.main.WorldToScreenPoint(centerTarget.position);

        lockOnImage.transform.position = screen;
    }

    protected void setTarget()
    {
        if (target != null)
        {
            resetTarget();
            endBlock();

            return;
        }

        Ray ray = new Ray(transform.position, Vector3.up);
        int contacts = Physics.SphereCastNonAlloc
            (ray, lockOnRadius, raycastHitBuffers, 1, lockOnLayerMask);

        GameObject targetObj = null;
        float distance = Mathf.Infinity;
        for (int i = 0; i < contacts; i++)
        {
            Transform objTransform = raycastHitBuffers[i].transform;

            if (objTransform.GetComponent<UnitController>().IsDied == true) continue;

            if (distance > (transform.position - objTransform.position).sqrMagnitude)
            {
                distance = (transform.position - objTransform.position).sqrMagnitude;
                targetObj = objTransform.gameObject;
            }
        }

        if (targetObj == null) return;

        target = targetObj.GetComponent<UnitController>();
        lockOnImage.gameObject.SetActive(true);

        inLockOn = true;
        animator.SetBool(inLockOnStr, inLockOn);

        cameraSetting.changeFiexedCamera(target.transform);

        beginBlock();
    }

    protected override void updateRotation()
    {
        Vector3 localInput = new Vector3(playerInput.Movement.x, 0f, playerInput.Movement.z);

        float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, movementSpeed / targetMovementSpeed);

        float actualTurnSpeed;
        if (isGrounded)
        {
            actualTurnSpeed = groundedTurnSpeed;
        }
        else
        {
            actualTurnSpeed = Vector3.Angle(transform.forward, localInput) * airborneTurnSpeedProportion * groundedTurnSpeed;
        }

        targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.deltaTime);

        transform.rotation = targetRotation;
    }

    protected override void setRotation()
    {
        Vector3 movementDirection = playerInput.Movement;
        movementDirection.Normalize();

        if (target != null || inBlocking)
        {
            Vector3 direction;

            if (target != null) direction = target.transform.position - transform.position;
            else direction = transform.forward;

            direction.y = 0;
            direction.Normalize();

            targetRotation = Quaternion.LookRotation(direction);

            animator.SetFloat("MovementX", movementDirection.x);
            animator.SetFloat("MovementZ", movementDirection.z);

            return;
        }

        Vector3 forward = Quaternion.Euler(0f, cameraSetting.FreeLookCamera.m_XAxis.Value, 0f) * Vector3.forward;
        forward.y = 0f;
        forward.Normalize();

        Quaternion rotation;
        if (Mathf.Approximately(Vector3.Dot(movementDirection, Vector3.forward), -1.0f))
        {
            rotation = Quaternion.LookRotation(-forward);
        }
        else
        {
            Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, movementDirection);

            rotation = Quaternion.LookRotation(cameraToInputOffset * forward);
        }
        targetRotation = rotation;
    }
}
