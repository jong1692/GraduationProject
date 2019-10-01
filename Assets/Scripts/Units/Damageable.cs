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
        public MonoBehaviour damager;
        public Vector3 damageSource;
    }

    [SerializeField]
    private int maxHitPoint = 100;
    private int curHitPoint;

    [SerializeField]
    private UnityEvent OnDeath, OnReceiveDamage;
    System.Action action;


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
        if (curHitPoint <= 0)
        {
            action += OnDeath.Invoke;
        }
        else
        {
            action += OnReceiveDamage.Invoke;
        }

        MessageType messageType = curHitPoint <= 0 ? MessageType.DEAD : MessageType.DAMAGED;
        GetComponent<UnitController>().receiveMessage(messageType, this, msg);
    }

    void FixedUpdate()
    {
        if (action != null)
        {
            action();
        }
    }
}
