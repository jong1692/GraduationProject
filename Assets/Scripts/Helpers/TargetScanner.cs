using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScanner : MonoBehaviour
{
    [SerializeField]
    private float detectionDistance = 10f;

    private bool isDetect;
    private GameObject target;

    public bool IsDetect
    {
        get { return isDetect; }
        set { isDetect = value; }
    }

    void Start()
    {
        initialize();
    }

    void Update()
    {
        if (isDetect)
        {
            detectTarget();
        }
    }

    private void initialize()
    {
        isDetect = true;

        target = PlayerController.Instance.gameObject;
    }

    private void detectTarget()
    {
        Vector3 playerPos = PlayerController.Instance.transform.position + Vector3.up * 1.0f;

        RaycastHit raycastHit;
        Physics.Raycast(transform.position, playerPos - transform.position, out raycastHit, detectionDistance);

        if (raycastHit.collider != null)
        {
            if (raycastHit.collider.gameObject == target)
            {
                GetComponent<EnemyController>().IsTargetInRange = true;
                isDetect = false;
            }
        }
    }

    public bool checkTargetInRange(GameObject target, float distance)
    {
        RaycastHit raycastHit;
        Physics.Raycast(transform.position, target.transform.position + Vector3.up * 1.0f - transform.position, out raycastHit, distance);

        if (raycastHit.collider != null)
        {
            if (raycastHit.collider.gameObject == target)
            {
                return true;
            }
        }

        return false;
    }
}