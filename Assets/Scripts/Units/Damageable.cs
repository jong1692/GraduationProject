using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Message;

public class Damageable : MonoBehaviour
{
    [Serializable]
    public struct DamageMessage
    {
        public int damageAmount;
        public GameObject damager;
        public Vector3 damageSource;
    }

    [SerializeField]
    private int maxHitPoint = 100;
    private int curHitPoint;

    public int CurHitPoint
    {
        get { return curHitPoint; }
    }

    void Awake()
    {
        curHitPoint = maxHitPoint;
    }

    void Update()
    {

    }

    public void applyDamage(DamageMessage msg)
    {
        if (curHitPoint <= 0)
        {
            return;
        }

        curHitPoint -= msg.damageAmount;

        MessageType messageType = curHitPoint <= 0 ? MessageType.DEAD : MessageType.DAMAGED;
        GetComponent<UnitController>().receiveMessage(messageType, this, msg);
    }
}
