using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Message;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public abstract class UnitController : MonoBehaviour, IMessageReceiver
{
    [SerializeField]
    protected float maxMovementSpeed = 5f;
    [SerializeField]
    protected float jumpPower = 20f;
    [SerializeField]
    protected float maxTurnSpeed = 1200f;
    [SerializeField]
    protected float minTurnSpeed = 400f;
    [SerializeField]
    protected float groundAcceleration = 20f;
    [SerializeField]
    protected float groundDeceleration = 30f;

    protected float targetMovementSpeed;
    protected float movementSpeed;
    protected float verticalMovementSpeed;

    protected Quaternion targetRotation;

    protected bool isGrounded;
    protected bool inAttacking;

    protected const float fixingGravityProportion = 0.3f;
    protected const float groundedRayDistance = 0.8f;
    protected const float gravity = 20f;
    protected const float airborneTurnSpeedProportion = 0.3f;

    protected CharacterController characterController;
    protected Animator animator;
    protected WeaponController weaponController;

    protected const string movementSpeedStr = "MovementSpeed";
    protected const string verticalMovementSpeedStr = "VerticalMovementSpeed";
    protected const string groundedStr = "Grounded";
    protected const string meleeAttackStr = "MeleeAttack";
    protected const string stateTimeStr = "StateTime";
    protected const string deathStr = "Death";
    protected const string hurtStr = "Hurt";
    protected const string hurtFromXStr = "HurtFromX";
    protected const string hurtFromYStr = "HurtFromY";

    protected void initialize()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        weaponController = GetComponentInChildren<WeaponController>();

        inAttacking = false;
    }

    protected IEnumerator beginAttackMotion()
    {
        animator.SetTrigger(meleeAttackStr);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f)
        {
            animator.SetFloat(stateTimeStr, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

            yield return null;
        }

        inAttacking = false;
    }

    public void beginAttack()
    {
        weaponController.beginAttack();
    }

    public void endAttack()
    {
        weaponController.endAttack();
    }

    public void resetAttackTrigger()
    {
        animator.ResetTrigger(meleeAttackStr);
    }

    public void receiveMessage(MessageType type, object sender, object msg)
    {
        switch (type)
        {
            case MessageType.DAMAGED:
                hurt((Damageable.DamageMessage)msg);
                break;

            case MessageType.DEAD:
                die((Damageable.DamageMessage)msg);
                break;
        }
    }

    protected void checkCanAttack()
    {

    }

    private void die(Damageable.DamageMessage msg)
    {
        animator.SetTrigger(deathStr);

        movementSpeed = 0;
        verticalMovementSpeed = 0;
    }

    private void hurt(Damageable.DamageMessage msg)
    {
        Vector3 direction = msg.damageSource - transform.position;
        direction.y = 0f;

        var localDirection = transform.InverseTransformDirection(direction);

        animator.SetTrigger(hurtStr);
        animator.SetFloat(hurtFromXStr, localDirection.x);
        animator.SetFloat(hurtFromYStr, localDirection.z);
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

        if (!isGrounded)
        {
            animator.SetFloat(verticalMovementSpeedStr, verticalMovementSpeed);
        }
        animator.SetBool(groundedStr, isGrounded);
    }

    protected virtual void checkGrounded()
    {
        isGrounded = characterController.isGrounded;
    }

    void OnAnimatorMove()
    {
        moveUnit();
    }

    protected abstract void calculateHorizontalMovement();

    protected abstract void calculateVerticalMovement();

    protected abstract void updateOrientation();

    protected abstract void setTargetRotation();

}
