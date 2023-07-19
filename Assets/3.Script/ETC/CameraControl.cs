using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    Cinemachine.CinemachineVirtualCamera vCamera;

    private void Awake()
    {
        TryGetComponent(out vCamera);
        StartCoroutine(Init_co());
    }

    private IEnumerator Init_co()
    {
        while (true)
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                vCamera.Follow = GameObject.FindGameObjectWithTag("Player").transform;
                vCamera.LookAt = GameObject.FindGameObjectWithTag("Player").transform;
                break;
            }
            yield return null;
        }
    }
}
