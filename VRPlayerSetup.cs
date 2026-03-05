using UnityEngine;
using Photon.Pun;

public class VRPlayerSetup : MonoBehaviourPunCallbacks
{
    [Header("XR Settings")]
    public GameObject xrOriginPrefab;

    void Start()
    {
        if (UnityEngine.XR.XRSettings.enabled)
        {
            Debug.Log("Обнаружен VR режим");
            
            if (photonView.IsMine)
            {
                SetupVR();
            }
        }
    }

    void SetupVR()
    { 
        Debug.Log("Локальный игрок настроен для VR");
    }
}