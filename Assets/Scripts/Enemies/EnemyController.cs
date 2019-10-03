using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : UnitController
{
    private NavMeshAgent navMeshAgent;
    private GameObject target;
    private float attackDelay;

    void Awake()
    {
        initialize();
    }

    void FixedUpdate()
    {
        navMeshAgent.SetDestination(target.transform.position);

        calculateHorizontalMovement();
        calculateVerticalMovement();

        if (canAttack)
        {
            StopCoroutine(beginAttackMotion());
            StartCoroutine(beginAttackMotion());
        }

        Func();
    }

    private void Func()
    {
        float distance = (transform.position - target.transform.position).magnitude;
        if (distance < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.isStopped = true;

            animator.SetFloat(movementSpeedStr, 0);
        }
        else
        {
            navMeshAgent.isStopped = false;
        }
    }

    protected override void initialize()
    {
        base.initialize();

        navMeshAgent = GetComponent<NavMeshAgent>();

        target = GameObject.Find("Player");
    }

    protected override void calculateHorizontalMovement()
    {
        targetMovementSpeed = maxMovementSpeed;

        if (!navMeshAgent.isStopped)
        {
            acceleration = groundAcceleration;
            navMeshAgent.acceleration = groundAcceleration;
        }
        else
        {
            acceleration = groundDeceleration;
            navMeshAgent.acceleration = groundDeceleration;
        }

        base.calculateHorizontalMovement();

        navMeshAgent.speed = movementSpeed;
    }

    protected override void moveUnit()
    {
        if (isGrounded)
        {
            gameObject.transform.position = navMeshAgent.nextPosition;
        }
        else
        {
            Vector3 movementVec = verticalMovementSpeed * transform.up * Time.deltaTime;
            animator.SetFloat(verticalMovementSpeedStr, verticalMovementSpeed);

            characterController.Move(movementVec);

            navMeshAgent.Warp(transform.position);
        }

        checkGrounded();
        animator.SetBool(groundedStr, isGrounded);
    }
}
