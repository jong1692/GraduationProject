using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Message;
using UnityEngine.UI;
using ObjectTypes;

public class Damageable : MonoBehaviour
{
    [Serializable]
    public struct DamageMessage
    {
        public int damageAmount;
        public GameObject damager;
        public Vector3 damageSource;
        public float radius;
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
    [SerializeField]
    private Transform hpBar;
    [SerializeField]
    private AudioSource hitAudioSource;
    [SerializeField]
    private AudioSource blockAudioSource;
    
    private float destroyTimer;
    private float destroyStartTime;
    private bool invincibility = false;

    private ParticleSystem particle;
    private Renderer modelRenderer;
    private Action action;

    const string dissolveParticlesStr = "DissolveParticles";

    public int CurHitPoint
    {
        get { return curHitPoint; }
    }

    public bool Invincibility
    {
        get { return invincibility; }
        set { invincibility = value; }
    }

    void Awake()
    {
        initialize();
    }

    void Update()
    {
        if (hitAudioSource != null)
        {
            if (hitAudioSource.isPlaying)
                hitAudioSource.pitch = Time.timeScale;
        }

        if (hpBar == null || GetComponent<PlayerController>() != null) return;

        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.8f);

        hpBar.transform.position = screen;
    }

    private void initialize()
    {
        curHitPoint = maxHitPoint;

        if (hpBar != null)
            hpBar.GetChild(0).GetComponent<Image>().fillAmount = curHitPoint / (float)maxHitPoint;

        destroyStartTime = UnityEngine.Random.Range(minStartTime, maxStartTime);
        destroyTimer = 0;

        particle = transform.Find(dissolveParticlesStr).GetComponent<ParticleSystem>();
        modelRenderer = GetComponentInChildren<Renderer>();
    }

    public IEnumerator destroyUnit()
    {
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(false);
            hpBar = null;
        }

        particle.gameObject.SetActive(true);
        if (!particle.isPlaying)
            particle.Play();

        while (destroyTimer < destroyStartTime)
        {
            destroyTimer += Time.deltaTime;
            yield return null;
        }

        destroyTimer = 0;
        modelRenderer.material = diffuseMaterial;

        while (destroyTimer < delay)
        {
            destroyTimer += Time.deltaTime;

            Color color = modelRenderer.material.color;
            color.a = Mathf.Lerp(color.a, 0, destroyTimer / delay);

            modelRenderer.material.color = color;

            yield return null;
        }

        action += OnDeath.Invoke;
        action();

        Destroy(gameObject);
    }

    public IEnumerator locateCrashParticle(Vector3 position, Vector3 lookAtPos)
    {
        ParticleSystem crashParticle = ObjectManager.Instance.getParticle(ParticleType.CRASH);

        crashParticle.transform.position = position;
        crashParticle.transform.LookAt(lookAtPos);
        crashParticle.gameObject.SetActive(true);
        crashParticle.Play();

        crashParticle.GetComponentInChildren<Light>().enabled = true;

        yield return new WaitForSeconds(0.02f);

        crashParticle.GetComponentInChildren<Light>().enabled = false;

        while (crashParticle.isPlaying)
        {
            yield return null;
        }

        crashParticle.gameObject.SetActive(false);
    }

    public IEnumerator locateHitParticle(Vector3 position, Vector3 lookAtPos)
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

    public void playAudio(MessageType type)
    {
        if (type == MessageType.DAMAGED && hitAudioSource != null)
            hitAudioSource.Play();

        if (type == MessageType.BLOCKED && blockAudioSource != null)
            blockAudioSource.Play();
    }

    public void applyDamage(DamageMessage msg)
    {
        curHitPoint -= msg.damageAmount;

        if (hpBar == null)
        {
            hpBar = ObjectManager.Instance.getObject(ObjectType.HP_BAR).GetComponent<Image>().transform;
            hpBar.gameObject.SetActive(true);
        }

        StartCoroutine(slowMotion());

        hpBar.GetChild(0).GetComponent<Image>().fillAmount = curHitPoint / (float)maxHitPoint;

        MessageType messageType = curHitPoint <= 0 ? MessageType.DEAD : MessageType.DAMAGED;
        GetComponent<UnitController>().receiveMessage(messageType, this, msg);

        if (messageType == MessageType.DEAD)
            StartCoroutine(destroyUnit());
    }

    private IEnumerator slowMotion()
    {
        Time.timeScale = 0.001f;

        yield return new WaitForSeconds(Time.timeScale * 0.01f);

        Time.timeScale = 1f;
    }
}
