using System.Collections;
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

    protected NavMeshAgent navMeshAgent;
    protected TargetScanner targetScanner;

    protected float attackTimer;

    protected bool isTargetInRange;

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
            setTargetRotation();
            updateOrientation();

            chaseTarget();
        }

        if (checkCanAttack())
        {
            StopCoroutine(beginAttackMotion());
            StartCoroutine(beginAttackMotion());
        }
    }

    public override void endAttack()
    {
        base.endAttack();

        resetAttackTrigger();
    }

    protected override void setTargetRotation()
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        targetRotation = Quaternion.LookRotation(direction);
    }

    protected override void updateOrientation()
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

    protected bool checkCanAttack()
    {
        if (attackTimer < attackDelay)
        {
            attackTimer += Time.deltaTime;
        }
        else
        {
            float distance = (transform.position - target.transform.position).magnitude;
            if (attackRange > distance && !inAttacking && isTargetInRange)
            {
                attackTimer = 0;
                return true;
            }
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

        target = PlayerController.Instance.gameObject;
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
