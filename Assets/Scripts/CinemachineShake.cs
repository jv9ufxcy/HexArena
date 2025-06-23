using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake instance;
    private CinemachineVirtualCamera vCam;
    CinemachineBasicMultiChannelPerlin perlin;
    public CinemachineBrain cinemachineBrain;

    private void Awake()
    {
        instance = this;
    }
    void GetActiveCinemachineCamera()
    {
        vCam = CameraController.instance.GetCurrentVCAM();
        perlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }
    public void ShakeCamera(float intensity, float time)
    {
        GetActiveCinemachineCamera();
        perlin.m_AmplitudeGain = intensity;
        shakeTimer = time;
    }
    private float shakeTimer;
    private void Update()
    {
        if (shakeTimer>0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer<=0f)
            {
                perlin.m_AmplitudeGain = 0f;
            }
        }
    }
}
