using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFollowTarget : MonoBehaviour
{
    [SerializeField]
    private Transform targetHand_L;
    [SerializeField]
    private Transform targetHand_R;

    void FixedUpdate()
    {
        transform.rotation = targetHand_L.rotation;
        transform.position = targetHand_L.position;
    }
}
