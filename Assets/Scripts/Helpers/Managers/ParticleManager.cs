using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Particle;

public class ParticleManager : MonoBehaviour
{
    private static ParticleManager instance;

    [Serializable]
    private struct ParticlePool
    {
        public string name;
        public ParticleType particleType;

        public GameObject particle;
        public Transform parent;
        public ParticleSystem[] particlePool;

        public int particlePoolSize;
    };

    [SerializeField]
    private ParticlePool[] particlePools;


    public static ParticleManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        initialize();
    }

    private void initialize()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            throw new UnityException("Cannot be more than one ParticleManager Script");
        }

        ParticleSystem particle;
        GameObject obj;

        for (int poolIdx = 0; poolIdx < particlePools.Length; poolIdx++)
        {
            int size = particlePools[poolIdx].particlePoolSize;
            particlePools[poolIdx].particlePool = new ParticleSystem[size];

            for (int idx = 0; idx < particlePools[poolIdx].particlePoolSize; idx++)
            {
                obj = Instantiate(particlePools[poolIdx].particle, particlePools[poolIdx].parent);

                particle = obj.GetComponentInChildren<ParticleSystem>();
                particle.Stop();

                particlePools[poolIdx].particlePool[idx] = particle;
            }
        }
    }

    public ParticleSystem getParticle(ParticleType particleType)
    {
        ParticleSystem particle = null;

        for (int poolIdx = 0; poolIdx < particlePools.Length; poolIdx++)
        {
            if (particlePools[poolIdx].particleType == particleType)
            {
                ParticlePool particlePool = particlePools[poolIdx];

                for (int idx = 0; idx < particlePools[poolIdx].particlePoolSize; idx++)
                {
                    if (!particlePools[poolIdx].particlePool[idx].isPlaying)
                    {
                        return particlePools[poolIdx].particlePool[idx];
                    }
                }

                particle = Instantiate(particlePool.particle, particlePool.parent).GetComponent<ParticleSystem>();
                particle.Stop();

                Destroy(particle.gameObject, 10.0f);

                return particle;
            }
        }

        throw new UnityException("There is not exist same particle.");
    }
}
