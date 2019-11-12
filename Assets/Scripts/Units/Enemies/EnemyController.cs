﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetScanner))]
public class EnemyController : UnitController
{
    [SerializeField]
    protected float attackDelay = 2.0f;

    [SerializeField]
    protected float attackRange = 0.9f;

    [SerializeField]
    protected int numAttackMotion = 1;

    protected NavMeshAgent navMeshAgent;
    protected TargetScanner targetScanner;

    protected float attackTimer;

    protected bool isTargetInRange;

    protected const string randomAttackStr = "RandomAttack";

    public bool IsTargetInRange
    {
        get { return isTargetInRange; }
        set { isTargetInRange = value; }
    }

    void Start()
    {
        initialize();
    }

    void FixedUpdate()
    {
        calculateHorizontalMovement();
        calculateVerticalMovement();

        if (isTargetInRange)
        {
            setRotation();
            updateRotation();

            chaseTarget();
        }

        if (checkCanAttack())
        {
            int random = Random.Range(0, numAttackMotion);
            animator.SetInteger(randomAttackStr, random);

            StopCoroutine(beginAttackMotion());
            StartCoroutine(beginAttackMotion());
        }
    }

    protected override void moveUnit()
    {
        Vector3 movementVec = Vector3.zero;

        if (isGrounded)
        {
            if (checkCurAnimationWithTag(attackStr))
            {
                movementVec = animator.deltaPosition;
                characterController.Move(movementVec);
            }

            if (checkCurAnimationWithTag(locomotionStr))
                transform.position = navMeshAgent.nextPosition;
            else
                navMeshAgent.Warp(transform.position);
        }
        else
        {
            movementVec = verticalMovementSpeed * transform.up * Time.deltaTime;
            characterController.Move(movementVec);
        }

        characterController.transform.rotation *= animator.deltaRotation;

        checkGrounded();
        animator.SetBool(groundedStr, isGrounded);
    }

    public override void endAttack()
    {
        base.endAttack();

        resetAttackTrigger();
    }

    protected override void setRotation()
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        targetRotation = Quaternion.LookRotation(direction);
    }

    protected override void updateRotation()
    {
        targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxTurnSpeed * Time.deltaTime);

        transform.rotation = targetRotation;
    }

    protected void chaseTarget()
    {
        float distance = (transform.position - target.transform.position).magnitude;
        if (distance < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.isStopped = true;

            animator.SetFloat(movementSpeedStr, 0);
        }
        else
        {
            navMeshAgent.SetDestination(target.transform.position);

            navMeshAgent.isStopped = false;
        }
    }

    protected override void damaged(Damageable.DamageMessage msg)
    {
        base.damaged(msg);

        var cameraShakeScript = CameraSetting.Instance.FreeLookCamera.GetComponent<CameraShake>();
        StartCoroutine(cameraShakeScript.cameraShake(0.2f, 0.15f));
    }

    protected bool checkCanAttack()
    {
        if (attackTimer < attackDelay)
        {
            attackTimer += Time.deltaTime;
            return false;
        }

        float distance = (transform.position - target.transform.position).magnitude;
        if (attackRange > distance && isTargetInRange)
        {
            attackTimer = 0;
            return true;
        }

        return false;
    }

    protected override void initialize()
    {
        base.initialize();

        targetScanner = GetComponent<TargetScanner>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.isStopped = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updatePosition = false;

        target = PlayerController.Instance.gameObject.GetComponent<UnitController>();
        isTargetInRange = false;

        attackTimer = attackDelay;
    }

    protected override void calculateHorizontalMovement()
    {
        if (isTargetInRange)
        {
            targetMovementSpeed = maxMovementSpeed;
            acceleration = groundAcceleration;
            navMeshAgent.acceleration = groundAcceleration;
        }
        else
        {
            targetMovementSpeed = 0;
            acceleration = groundDeceleration;
            navMeshAgent.acceleration = groundDeceleration;
        }

        base.calculateHorizontalMovement();

        navMeshAgent.speed = movementSpeed;
    }

    protected override void die()
    {
        base.die();

        isTargetInRange = false;
    }
}
