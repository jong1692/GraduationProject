using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ObjectTypes;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager instance;

    public static ObjectManager Instance
    {
        get { return instance; }
    }

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

    [Serializable]
    private struct ObjectPool
    {
        public string name;
        public ObjectType objectType;

        public GameObject obj;
        public Transform parent;
        public GameObject[] objectPool;

        public int objectPoolSize;
    };

    [SerializeField]
    private ObjectPool[] objectPools;
  

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

        initParticlePools();
        initObjectPools();
    }

    private void initParticlePools()
    {
        for (int poolIdx = 0; poolIdx < particlePools.Length; poolIdx++)
        {
            int size = particlePools[poolIdx].particlePoolSize;
            particlePools[poolIdx].particlePool = new ParticleSystem[size];

            for (int idx = 0; idx < particlePools[poolIdx].particlePoolSize; idx++)
            {
                GameObject obj = Instantiate(particlePools[poolIdx].particle, particlePools[poolIdx].parent);

                ParticleSystem particle = obj.GetComponentInChildren<ParticleSystem>();
                particle.Stop();

                obj.SetActive(false);

                particlePools[poolIdx].particlePool[idx] = particle;
            }
        }
    }

    private void initObjectPools()
    {
        for (int poolIdx = 0; poolIdx < objectPools.Length; poolIdx++)
        {
            int size = objectPools[poolIdx].objectPoolSize;
            objectPools[poolIdx].objectPool = new GameObject[size];

            for (int idx = 0; idx < objectPools[poolIdx].objectPoolSize; idx++)
            {
                GameObject obj = Instantiate(objectPools[poolIdx].obj, objectPools[poolIdx].parent);
                obj.SetActive(false);

                objectPools[poolIdx].objectPool[idx] = obj;
            }
        }
    }

    public ParticleSystem getParticle(ParticleType particleType)
    {
        for (int poolIdx = 0; poolIdx < particlePools.Length; poolIdx++)
        {
            if (particlePools[poolIdx].particleType == particleType)
            {
                for (int idx = 0; idx < particlePools[poolIdx].particlePoolSize; idx++)
                {
                    if (!particlePools[poolIdx].particlePool[idx].isPlaying)
                    {
                        return particlePools[poolIdx].particlePool[idx];
                    }
                }

                ParticlePool particlePool = particlePools[poolIdx];
                ParticleSystem particle = Instantiate(particlePool.particle, particlePool.parent).GetComponent<ParticleSystem>();
                particle.Stop();

                Destroy(particle.gameObject, 10.0f);

                return particle;
            }
        }

        return null;
    }

    public GameObject getObject(ObjectType objectType)
    {
        for (int poolIdx = 0; poolIdx < objectPools.Length; poolIdx++)
        {
            if (objectPools[poolIdx].objectType == objectType)
            {
                for (int idx = 0; idx < objectPools[poolIdx].objectPoolSize; idx++)
                {
                    if (!objectPools[poolIdx].objectPool[idx].activeInHierarchy)
                        return objectPools[poolIdx].objectPool[idx];
                }

                ObjectPool objectPool = objectPools[poolIdx];
                GameObject obj = Instantiate(objectPool.obj, objectPool.parent);
                obj.SetActive(false);

                Destroy(obj.gameObject, 10.0f);

                return obj;
            }
        }

        return null;
    }
}
