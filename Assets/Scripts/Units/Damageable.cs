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

    [SerializeField]
    private float minStartTime = 4.0f;
    [SerializeField]
    private float maxStartTime = 6.0f;
    [SerializeField]
    private float delay = 3.0f;
    [SerializeField]
    private Material diffuseMaterial;
    [SerializeField]
    private UnityEvent OnDeath;

    private float timer;
    private float destroyStartTime;

    private ParticleSystem particle;
    private Renderer modelRenderer;
    private Action action;

    const string dissolveParticlesStr = "DissolveParticles";

    public int CurHitPoint
    {
        get { return curHitPoint; }
    }

    void Awake()
    {
        initialize();
    }

    private void initialize()
    {
        curHitPoint = maxHitPoint;

        destroyStartTime = UnityEngine.Random.Range(minStartTime, maxStartTime);
        timer = 0;

        particle = transform.Find(dissolveParticlesStr).GetComponent<ParticleSystem>();
        modelRenderer = GetComponentInChildren<Renderer>();
    }

    public IEnumerator destroyUnit()
    {
        if (!particle.isPlaying)
        {
            particle.Play();
        }

        while (timer < destroyStartTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0;
        modelRenderer.material = diffuseMaterial;

        while (timer < delay)
        {
            timer += Time.deltaTime;

            Color color = modelRenderer.material.color;
            color.a = Mathf.Lerp(color.a, 0, timer / delay);

            modelRenderer.material.color = color;

            yield return null;
        }

        action += OnDeath.Invoke;
        action();

        Destroy(gameObject);
    }

    public void applyDamage(DamageMessage msg)
    {
        curHitPoint -= msg.damageAmount;

        MessageType messageType = curHitPoint <= 0 ? MessageType.DEAD : MessageType.DAMAGED;
        GetComponent<UnitController>().receiveMessage(messageType, this, msg);

        if (messageType == MessageType.DEAD)
        {
            StartCoroutine(destroyUnit());
        }
    }
}
