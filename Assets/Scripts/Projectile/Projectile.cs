using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Particle;

public class Projectile : MonoBehaviour
{
    [Serializable]
    private struct AttackPoint
    {
        public float radius;
        public Transform attackRoot;
    }

    [SerializeField]
    private float movementSpeed = 10.0f;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private ParticleSystem burstParticle;
    [SerializeField]
    private AttackPoint[] attackPoints;
    [SerializeField]
    private LayerMask targetLayerMask;

    private int damage;

    private GameObject owner;
    private Vector3[] prevPosition;
    private Vector3 targetPostion;

    private RaycastHit[] raycastHitBuffers;
    private List<GameObject> damagedObjectsList;
    private ParticleSystem particle;


    private Damageable.DamageMessage damageMessage;

    const int hitBufferSize = 32;

    public GameObject Owner
    {
        get { return owner; }
    }

    void Awake()
    {
        prevPosition = new Vector3[attackPoints.Length];
        raycastHitBuffers = new RaycastHit[hitBufferSize];
        damagedObjectsList = new List<GameObject>();

        particle = GetComponent<ParticleSystem>();

        damageMessage = new Damageable.DamageMessage();
        damageMessage.damager = this.gameObject;
    }

    public void shootProjectile(Vector3 target, GameObject owner, int damage)
    {
        targetPostion = target;

        this.owner = owner;

        damageMessage.damageAmount = damage;

        StartCoroutine(moveProjectile());
    }

    private IEnumerator moveProjectile()
    {
        burstParticle.gameObject.SetActive(false);

        float distance;
        do
        {
            distance = (transform.position - (targetPostion + offset)).magnitude;
            
            transform.position =
              Vector3.MoveTowards(transform.position, targetPostion + offset, movementSpeed * Time.deltaTime);

            inAttacking();

            yield return null;
        } while (distance > 0.1f && damagedObjectsList.Count == 0);

        burstProjectile();
    }

    private void burnTarget()
    {
        Transform target = owner.GetComponent<UnitController>().Target.transform;
        SkinnedMeshRenderer skinnedMeshRenderer = target.GetComponentInChildren<SkinnedMeshRenderer>();

        var burnParticle = ParticleManager.Instance.getParticle(ParticleType.SHAMAN_BURN);

        var shape = burnParticle.shape;
        shape.skinnedMeshRenderer = skinnedMeshRenderer;

        burnParticle.Play();
    }

    private void burstProjectile()
    {
        particle.Stop();
   
        burstParticle.gameObject.SetActive(true);

        damagedObjectsList.Clear();
    }

    private void inAttacking()
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

            prevPosition[idx] = curPositon;
        }
    }

    private void checkDamage(Collider collider, AttackPoint attackPoint)
    {
        Damageable damageableScript = collider.GetComponent<Damageable>();
        GameObject gameObject = collider.gameObject;

        if (damageableScript != null && !damagedObjectsList.Contains(gameObject))
        {
            if (collider.gameObject != owner && collider.gameObject.tag != owner.tag)
            {
                damageMessage.damageSource = attackPoint.attackRoot.transform.position;

                damagedObjectsList.Add(gameObject);

                damageableScript.applyDamage(damageMessage);

                burnTarget();
            }
        }
    }
}
