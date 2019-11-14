using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;

public class CameraSetting : MonoBehaviour
{
    private static CameraSetting instance;

    public static CameraSetting Instance
    {
        get { return instance; }
    }

    private Transform followTransform;
    private Transform lookAtTransform;

    private CinemachineFreeLook freeLookCamera;

    private float maxSpeed_X;
    private float maxSpeed_Y;

    private bool isFixed = false;

    public CinemachineFreeLook FreeLookCamera
    {
        get { return freeLookCamera; }
    }

    void Awake()
    {
        if (instance == null)
            instance = this;

        Transform freeLookCameraTransform = transform.Find("Player_FreeLookCamera");
        if (freeLookCameraTransform != null)
        {
            freeLookCamera = freeLookCameraTransform.GetComponent<CinemachineFreeLook>();
        }

        PlayerController playerController = FindObjectOfType<PlayerController>();

        if (playerController != null && playerController.name == "Player")
        {
            followTransform = playerController.transform;
            lookAtTransform = followTransform.Find("HeadTarget");

            if (playerController.CameraSetting == null)
                playerController.CameraSetting = this;
        }
    }

    public void changeFiexedCamera(Transform target)
    {
        lookAtTransform = target.Find("HeadTarget");

        maxSpeed_X = freeLookCamera.m_XAxis.m_MaxSpeed;
        freeLookCamera.m_XAxis.m_MaxSpeed = 0;

        maxSpeed_Y = freeLookCamera.m_YAxis.m_MaxSpeed;
        freeLookCamera.m_YAxis.m_MaxSpeed = 0;
        freeLookCamera.m_YAxis.Value = 0.3f;

        isFixed = true;

        updateCameraSetting();
    }

    public void changeFreeCamera()
    {
        lookAtTransform = followTransform.Find("HeadTarget");

        freeLookCamera.m_XAxis.m_MaxSpeed = maxSpeed_X;
        freeLookCamera.m_YAxis.m_MaxSpeed = maxSpeed_Y;

        isFixed = false;

        updateCameraSetting();
    }

    private void updateCameraSetting()
    {
        freeLookCamera.Follow = followTransform;
        freeLookCamera.LookAt = lookAtTransform;
    }

    private void Update()
    {
        if (isFixed)
        {
            freeLookCamera.m_XAxis.Value = 
                Mathf.LerpAngle(freeLookCamera.m_XAxis.Value, 
                followTransform.rotation.eulerAngles.y, 1.0f * Time.deltaTime);
        }
    }
}
