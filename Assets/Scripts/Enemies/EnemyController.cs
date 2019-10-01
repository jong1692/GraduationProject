using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : UnitController
{
    private NavMeshAgent navMeshAgent;
    private GameObject target;

    private bool canFollowTarget;

    const string  twoHandSwordIdleAnimationStr = "2Hand-Sword-Idle";

    void Awake()
    {
        initialize();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updatePosition = false;

        target = GameObject.Find("Player");

        canFollowTarget = false;
        canFollowTarget = true;
    }

    void FixedUpdate()
    {
        navMeshAgent.SetDestination(target.transform.position);

        calculateHorizontalMovement();
        calculateVerticalMovement();
    }

    protected override void calculateHorizontalMovement()
    {
    
    }

    protected override void calculateVerticalMovement()
    {
        if (isGrounded)
        {
            verticalMovementSpeed = -gravity * fixingGravityProportion;
        }
        else
        {
            if (Mathf.Approximately(verticalMovementSpeed, 0f))
            {
                verticalMovementSpeed = 0f;
            }

            verticalMovementSpeed -= gravity * Time.deltaTime;
        }
    }

    protected override void updateOrientation()
    {
     
    }

    protected override void setTargetRotation()
    {
 
    }

    protected override void checkGrounded()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * groundedRayDistance * 0.5f, -Vector3.up);

        isGrounded = Physics.Raycast(ray, out hit, groundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
    }


    protected override void moveUnit()
    {
        checkGrounded();

        if (isGrounded && canFollowTarget)
        {
            navMeshAgent.speed = maxMovementSpeed;
            animator.SetFloat(movementSpeedStr, maxMovementSpeed);

            gameObject.transform.position = navMeshAgent.nextPosition;

            if ((transform.position - target.transform.position).magnitude < 1.8f)
            {
                canFollowTarget = false;
                animator.SetFloat(movementSpeedStr, 0);

                navMeshAgent.enabled = false;
            }
        }
        else if (!isGrounded)
        {
            Vector3 movementVec = verticalMovementSpeed * transform.up * Time.deltaTime;
            animator.SetFloat(verticalMovementSpeedStr, verticalMovementSpeed);

            characterController.Move(movementVec);

            navMeshAgent.Warp(transform.position);
        }
        else
        {
            navMeshAgent.Warp(transform.position);
        }

        animator.SetBool(groundedStr, isGrounded);
    }
}
