using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Weapon;

public class WeaponController : MonoBehaviour
{
    [Serializable]
    private struct AttackPoint
    {
        public float radius;
        public Transform attackRoot;
    }

    [SerializeField]
    private WeaponType weaponType;
    [SerializeField]
    private int damage;
    [SerializeField]
    private AttackPoint[] attackPoints;

    private GameObject owner;
    private Vector3[] prevPosition;
    private RaycastHit[] raycastHitBuffers;
    private List<GameObject> damagedObjectsList;
    private Damageable.DamageMessage damageMessage;

    private bool inAttacking;

    const int bufferSize = 32;

    public GameObject Owner
    {
        get { return owner; }
        set { Owner = value; }
    }

    public bool InAttacking
    {
        get { return inAttacking; }
    }

    private void Awake()
    {
        var gameObject = transform.parent.gameObject;
        while (gameObject.transform.tag != "Player" && gameObject.transform.tag != "Enemy")
        {
            gameObject = gameObject.transform.parent.gameObject;

            if (gameObject.transform == null)
            {
                throw new UnityException("The Weapone does not have owner");
            }
        }

        if (gameObject != null && gameObject.GetComponent<UnitController>() != null)
        {
            owner = gameObject;
        }

        prevPosition = new Vector3[attackPoints.Length];
        raycastHitBuffers = new RaycastHit[bufferSize];
        damagedObjectsList = new List<GameObject>();

        damageMessage = new Damageable.DamageMessage();
        damageMessage.damageAmount = damage;
        damageMessage.damager = this.gameObject;
    }

    public void beginAttack()
    {
        inAttacking = true;

        for (int idx = 0; idx < attackPoints.Length; idx++)
        {
            prevPosition[idx] = attackPoints[idx].attackRoot.position;
        }
    }

    public void endAttack()
    {
        inAttacking = false;
        damagedObjectsList.Clear();
    }

    void FixedUpdate()
    {
        if (inAttacking)
        {
            inAttack();
        }
    }

    private void inAttack()
    {
        for (int idx = 0; idx < attackPoints.Length; idx++)
        {
            Vector3 curPositon = attackPoints[idx].attackRoot.position;
            Vector3 attackVector = curPositon - prevPosition[idx];

            Ray r = new Ray(curPositon, attackVector.normalized);
            int contacts = Physics.SphereCastNonAlloc
                (r, attackPoints[idx].radius, raycastHitBuffers, attackVector.magnitude, ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < contacts; i++)
            {
                Collider collider = raycastHitBuffers[i].collider;
                checkDamage(collider, attackPoints[idx]);
            }

            prevPosition[idx] = curPositon;
        }
    }

    private bool checkDamage(Collider collider, AttackPoint attackPoint)
    {
        var damageableScript = collider.GetComponent<Damageable>();
        var gameObject = collider.gameObject;

        if (damageableScript != null && collider.gameObject != owner && !damagedObjectsList.Contains(gameObject))
        {
            damageMessage.damageSource = owner.transform.position;

            damagedObjectsList.Add(gameObject);
            damageableScript.applyDamage(damageMessage);
        }

        return true;
    }
}
