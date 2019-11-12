using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originPos;
    private CinemachineFreeLook freeLookCamera;

    void Awake()
    {
        originPos = transform.localPosition;

        freeLookCamera = transform.GetComponent<CinemachineFreeLook>();
    }

    public IEnumerator cameraShake(float amount, float duration)
    {
        originPos = freeLookCamera.LookAt.localPosition;

        float timer = 0;
        while (timer <= duration)
        {
            freeLookCamera.LookAt.localPosition = (Vector3)UnityEngine.Random.insideUnitCircle * amount + originPos;

            timer += Time.deltaTime;
            yield return null;
        }

        freeLookCamera.LookAt.localPosition = originPos;
    }
}
