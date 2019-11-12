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

    void Update()
    {
        updateCameraSetting();
    }

    private void updateCameraSetting()
    {
        freeLookCamera.Follow = followTransform;
        freeLookCamera.LookAt = lookAtTransform;
    }
}
