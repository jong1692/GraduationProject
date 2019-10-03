using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : UnitController
{
    private static PlayerController instance;

    private PlayerInput playerInput;
    private CameraSetting cameraSetting;

    public CameraSetting CameraSetting
    {
        get { return cameraSetting; }
        set { cameraSetting = value; }
    }

    void Awake()
    {
        initialize();
    }

    void FixedUpdate()
    {
        calculateHorizontalMovement();
        calculateVerticalMovement();

        if (playerInput.IsMoveInput)
        {
            setTargetRotation();
            updateOrientation();
        }

        if (playerInput.Attack)
        {
            StopCoroutine(beginAttackMotion());
            StartCoroutine(beginAttackMotion());
        }
    }

    protected override void initialize()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            throw new UnityException("Cannot be more than one PlayerController Script");
        }

        base.initialize();

        playerInput = GetComponent<PlayerInput>();
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

    protected override void calculateVerticalMovement()
    {
        base.calculateVerticalMovement();

        if (isGrounded && playerInput.Jump && !inAttacking)
        {
            verticalMovementSpeed = jumpPower;
            isGrounded = false;
        }
    }

    protected override void updateOrientation()
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

    protected override void setTargetRotation()
    {
        Vector3 movementDirection = playerInput.Movement;
        movementDirection.Normalize();

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
