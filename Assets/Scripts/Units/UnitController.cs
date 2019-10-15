using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Message;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Damageable))]
public abstract class UnitController : MonoBehaviour, IMessageReceiver
{
    [SerializeField]
    protected float maxMovementSpeed = 5f;
    [SerializeField]
    protected float jumpPower = 10f;
    [SerializeField]
    [Range(0.0f, 3600.0f)]
    protected float maxTurnSpeed = 1200f;
    [SerializeField]
    [Range(0.0f, 3600.0f)]
    protected float minTurnSpeed = 600f;
    [SerializeField]
    protected float groundAcceleration = 15f;
    [SerializeField]
    protected float groundDeceleration = 30f;

    protected float targetMovementSpeed;
    protected float movementSpeed;
    protected float verticalMovementSpeed;
    protected float acceleration;

    protected bool isGrounded;
    protected bool isPrevGrounded;
    protected bool inAttacking;
    protected bool isDied;

    protected const float groundedRayDistance = 0.8f;
    protected const float gravity = 20f;
    protected const float airborneTurnSpeedProportion = 0.5f;
    protected const float fixingGravityProportion = 1.0f;

    protected Quaternion targetRotation;
    protected CharacterController characterController;
    protected Animator animator;
    protected WeaponController weaponController;
    protected GameObject target;

    protected const string movementSpeedStr = "MovementSpeed";
    protected const string verticalMovementSpeedStr = "VerticalMovementSpeed";
    protected const string groundedStr = "Grounded";
    protected const string attackStr = "Attack";
    protected const string inAttackingStr = "InAttacking";
    protected const string stateTimeStr = "StateTime";
    protected const string deathStr = "Death";
    protected const string hurtStr = "Hurt";
    protected const string hurtFromXStr = "HurtFromX";
    protected const string hurtFromZStr = "HurtFromZ";

    public bool IsDied
    {
        get { return isDied; }
    }

    public GameObject Target
    {
        get { return target; }
    }

    protected virtual void initialize()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        weaponController = GetComponentInChildren<WeaponController>();

        inAttacking = false;
        isDied = false;
    }

    protected IEnumerator beginAttackMotion()
    {
        inAttacking = true;

        animator.SetBool(inAttackingStr, inAttacking);
        animator.SetTrigger(attackStr);

        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag(attackStr))
        {
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).IsTag(attackStr))
        {
            animator.SetFloat(stateTimeStr, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

            yield return null;
        }

        inAttacking = false;

        animator.SetBool(inAttackingStr, inAttacking);
        animator.ResetTrigger(hurtStr);
    }

    public void beginAttack()
    {
        weaponController.beginAttack();
    }

    public virtual void endAttack()
    {
        weaponController.endAttack();
    }

    public void resetAttackTrigger()
    {
        animator.ResetTrigger(attackStr);
    }

    public void receiveMessage(MessageType type, object sender, object msg)
    {
        switch (type)
        {
            case MessageType.DAMAGED:
                hurt((Damageable.DamageMessage)msg);
                break;

            case MessageType.DEAD:
                die();
                break;
        }
    }

    protected virtual void die()
    {
        animator.SetTrigger(deathStr);

        movementSpeed = 0;
        verticalMovementSpeed = 0;

        isDied = true;

        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void hurt(Damageable.DamageMessage msg)
    {
        Vector3 direction = msg.damageSource - transform.position;
        direction.y = 0f;

        var localDirection = transform.InverseTransformDirection(direction);

        animator.SetTrigger(hurtStr);
        animator.SetFloat(hurtFromXStr, localDirection.x);
        animator.SetFloat(hurtFromZStr, localDirection.z);
    }

    protected virtual void moveUnit()
    {
        Vector3 movementVec;

        if (isGrounded)
        {
            movementVec = animator.deltaPosition;
        }
        else
        {
            movementVec = movementSpeed * transform.forward * Time.deltaTime;
        }
        movementVec += verticalMovementSpeed * transform.up * Time.deltaTime;

        characterController.transform.rotation *= animator.deltaRotation;
        characterController.Move(movementVec);

        checkGrounded();
        animator.SetBool(groundedStr, isGrounded);
    }

    protected virtual void checkGrounded()
    {
        isPrevGrounded = isGrounded;
        isGrounded = characterController.isGrounded;
    }

    void OnAnimatorMove()
    {
        moveUnit();
    }

    protected virtual void calculateHorizontalMovement()
    {
        movementSpeed = Mathf.MoveTowards(movementSpeed, targetMovementSpeed, acceleration * Time.deltaTime);

        animator.SetFloat(movementSpeedStr, movementSpeed);
    }

    protected virtual void calculateVerticalMovement()
    {
        if (isGrounded)
        {
            verticalMovementSpeed = -gravity * fixingGravityProportion;
        }
        else
        {
            if (isGrounded != isPrevGrounded)
            {
                verticalMovementSpeed = 0;
            }

            verticalMovementSpeed -= gravity * Time.deltaTime;
        }

        animator.SetFloat(verticalMovementSpeedStr, verticalMovementSpeed);
    }

    protected virtual void updateOrientation() { }

    protected virtual void setTargetRotation() { }

}
