using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Weapon;
using ObjectTypes;

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
    [SerializeField]
    private Transform crashAttackPointRoot;
    [SerializeField]
    private LayerMask targetLayerMask;
    [SerializeField]
    private LayerMask crashLayerMask;

    private GameObject owner;
    private Vector3[] prevPosition;
    private RaycastHit[] raycastHitBuffers;
    private List<GameObject> damagedObjectsList;
    private Damageable.DamageMessage damageMessage;
    private AudioSource audioSource;

    private bool inMeleeAttacking;
    private bool inRangeAttacking;

    const int hitBufferSize = 32;

    public GameObject Owner
    {
        get { return owner; }
        set { Owner = value; }
    }

    private void Awake()
    {
        initialize();
    }

    private void initialize()
    {
        GameObject gameObject = transform.parent.gameObject;
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

        audioSource = GetComponent<AudioSource>();

        prevPosition = new Vector3[attackPoints.Length];
        raycastHitBuffers = new RaycastHit[hitBufferSize];
        damagedObjectsList = new List<GameObject>();

        damageMessage = new Damageable.DamageMessage();
        damageMessage.damageAmount = damage;
        damageMessage.damager = owner;
    }

    public void beginAttack()
    {
        switch (weaponType)
        {
            case WeaponType.EMPTY_HAND:
                meleeAttack();

                break;

            case WeaponType.ONE_HAND_MELEE:
                meleeAttack();

                break;

            case WeaponType.TWO_HAND_MELEE:
                meleeAttack();

                break;

            case WeaponType.ONE_HAND_RANGE:
                rangeAttack();

                break;

            case WeaponType.TWO_HAND_RANGE:
                rangeAttack();

                break;

        }
    }

    private void meleeAttack()
    {
        inMeleeAttacking = true;

        for (int idx = 0; idx < attackPoints.Length; idx++)
        {
            prevPosition[idx] = attackPoints[idx].attackRoot.position;
        }
    }

    private void rangeAttack()
    {
        Transform target = owner.GetComponent<UnitController>().Target.transform.Find("CenterTarget");

        ParticleSystem projectile = ObjectManager.Instance.getParticle(ParticleType.SHAMAN_PROJECTILE);

        projectile.transform.position = transform.position;
        projectile.GetComponent<Projectile>().shootProjectile(target.position, owner, damage);
        projectile.Play();
    }

    public void endAttack()
    {
        inMeleeAttacking = false;
        inRangeAttacking = false;

        damagedObjectsList.Clear();
    }

    void FixedUpdate()
    {
        if (inMeleeAttacking)
        {
            inMeleeAttack();
        }
    }

    private IEnumerator locateHitParticle(Vector3 position, Vector3 lookAtPos)
    {
        ParticleSystem hitParticle = ObjectManager.Instance.getParticle(ParticleType.HIT);

        hitParticle.transform.position = position;
        hitParticle.transform.LookAt(lookAtPos);
        hitParticle.gameObject.SetActive(true);
        hitParticle.Play();

        while (hitParticle.isPlaying)
        {
            yield return null;
        }

        hitParticle.gameObject.SetActive(false);
    }

    private IEnumerator locateCrashParticle(Vector3 position, Vector3 lookAtPos)
    {
        ParticleSystem crashParticle = ObjectManager.Instance.getParticle(ParticleType.CRASH);

        crashParticle.transform.position = position;
        crashParticle.transform.LookAt(lookAtPos);
        crashParticle.gameObject.SetActive(true);
        crashParticle.Play();

        crashParticle.GetComponentInChildren<Light>().enabled = true;

        yield return new WaitForSeconds(0.05f);

        crashParticle.GetComponentInChildren<Light>().enabled = false;

        while (crashParticle.isPlaying)
        {
            yield return null;
        }

        crashParticle.gameObject.SetActive(false);
    }

    private void inMeleeAttack()
    {
        for (int idx = 0; idx < attackPoints.Length; idx++)
        {
            Vector3 curPositon = attackPoints[idx].attackRoot.position;
            Vector3 attackVector = curPositon - prevPosition[idx];

            Ray ray = new Ray(curPositon, attackVector.normalized);
            int contacts = Physics.SphereCastNonAlloc
                (ray, attackPoints[idx].radius, raycastHitBuffers, attackVector.magnitude, targetLayerMask);

            for (int i = 0; i < contacts; i++)
            {
                Collider collider = raycastHitBuffers[i].collider;
                checkDamage(collider, attackPoints[idx]);
            }

            contacts = Physics.SphereCastNonAlloc
               (ray, attackPoints[idx].radius, raycastHitBuffers, attackVector.magnitude, crashLayerMask);
            for (int i = 0; i < contacts; i++)
            {
                Collider collider = raycastHitBuffers[i].collider;
                crash(collider, attackPoints[idx]);

                if (attackPoints[idx].attackRoot == crashAttackPointRoot)
                {
                    owner.GetComponent<UnitController>().setCrashTrigger();
                    inMeleeAttacking = false;
                    return;
                }
            }


            prevPosition[idx] = curPositon;
        }
    }

    private void crash(Collider collider, AttackPoint attackPoint)
    {
        if (damagedObjectsList.Contains(collider.gameObject)) return;
        damagedObjectsList.Add(collider.gameObject);

        if (audioSource)
            audioSource.Play();

        StartCoroutine(locateCrashParticle(attackPoint.attackRoot.transform.position, owner.transform.position));
    }

    private void checkDamage(Collider collider, AttackPoint attackPoint)
    {
        Damageable damageableScript = collider.GetComponent<Damageable>();
        GameObject gameObject = collider.gameObject;

        if (damageableScript != null && !damagedObjectsList.Contains(gameObject))
        {
            if (damageableScript.Invincibility || collider.gameObject == owner ||
                collider.gameObject.tag == owner.tag) return;

            damageMessage.damageSource = attackPoint.attackRoot.position;

            damagedObjectsList.Add(gameObject);
            damageableScript.applyDamage(damageMessage);

            Vector3 hitPos = Vector3.Lerp(attackPoint.attackRoot.transform.position,
                gameObject.transform.Find("CenterTarget").transform.position, attackPoint.radius);

            StartCoroutine(locateHitParticle(hitPos, attackPoint.attackRoot.position));
        }
    }
}
