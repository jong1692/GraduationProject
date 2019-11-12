using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private static TimeManager instance;

    public static TimeManager Instance
    {
        get { return instance; }
    }

    public double DeltaTime
    {
        get { return deltaTime; }
    }

    private double startTime;
    private double deltaTime;

    void Awake()
    {
        if (instance == null)
            instance = this;

        startTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        deltaTime = Time.realtimeSinceStartup - startTime;
        startTime = Time.realtimeSinceStartup;
    }
}
