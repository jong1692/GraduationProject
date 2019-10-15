using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShamanController : EnemyController
{
    [SerializeField]
    private float runAwayStartDistance = 3.0f;

    [SerializeField]
    private float runAwayDistance = 6.0f;


    private bool isRunAway;

    private const string runAwayStr = "RunAway";

    void Start()
    {
        initialize();
    }

    protected override void initialize()
    {
        base.initialize();

        isRunAway = false;
    }

    void FixedUpdate()
    {
        checkRunAway();

        calculateHorizontalMovement();
        calculateVerticalMovement();

        if (isTargetInRange)
        {
            setTargetRotation();
            updateOrientation();
        }

        if (checkCanAttack())
        {
            StopCoroutine(beginAttackMotion());
            StartCoroutine(beginAttackMotion());
        }
    }

    private void checkRunAway()
    {
        if (IsTargetInRange)
        {
            if (targetScanner.checkTargetInRange(target, runAwayStartDistance))
            {
                isRunAway = true;

                Vector3 movePoint = transform.position + (transform.position - target.transform.position).normalized * runAwayDistance;

                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(movePoint);

                
            }
            else if ((transform.position - target.transform.position).magnitude >= runAwayDistance - navMeshAgent.stoppingDistance)
            {
                isRunAway = false;

                navMeshAgent.isStopped = true;

                animator.SetFloat(movementSpeedStr, 0);
            }
        }

        animator.SetBool(runAwayStr, isRunAway);
    }

    protected override void calculateHorizontalMovement()
    {
        if (isRunAway)
        {
            targetMovementSpeed = maxMovementSpeed;
            acceleration = groundAcceleration;
            navMeshAgent.acceleration = groundAcceleration;

            animator.SetFloat(movementSpeedStr, -movementSpeed);
        }
        else
        {
            targetMovementSpeed = 0;
            acceleration = groundDeceleration;
            navMeshAgent.acceleration = groundDeceleration;

            animator.SetFloat(movementSpeedStr, movementSpeed);
        }

        movementSpeed = Mathf.MoveTowards(movementSpeed, targetMovementSpeed, acceleration * Time.deltaTime);

       

        navMeshAgent.speed = movementSpeed;
    }
}
