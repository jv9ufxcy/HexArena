using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour {
    
    [SerializeField]
    Transform objectToFollow;

    CinemachineVirtualCamera currentVCAM;

    public static CameraController instance;

    // Use this for initialization
    void Start ()
    {
        instance = this;
        currentVCAM = vCams[0];
	}
    public CinemachineVirtualCamera GetCurrentVCAM()
    {
        return currentVCAM;
    }
    [SerializeField] private CinemachineVirtualCamera[] vCams;
    public void DefaultCam()
    {
        ChangeActiveCamera(0);
    }
    public void ChangeActiveCamera(int index)
    {
        for (int i = 0; i < vCams.Length; i++)
        {
            if (i == index)
            {
                vCams[i].Priority = 1;
                currentVCAM=vCams[i];
            }
            else
                vCams[i].Priority = 0;
        }
    }
}
