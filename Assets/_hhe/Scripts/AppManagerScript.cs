﻿using UnityEngine;
using Photon.Pun;
using TMPro;
public class AppManagerScript : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Message;

    [SerializeField]
    private GameObject photonPlayerPrefab;

    void Start()
    {
        //Message.text = "AppManagerScript: Start";

        if (PhotonNetwork.IsConnectedAndReady)
        {
            //Objects to be network instantiated must 
            //be in a Resources folder and contain a PhotonView component
            if (photonPlayerPrefab != null)
            {
                Vector3 _position = Camera.main.transform.position;
                Quaternion _rotation = Camera.main.transform.rotation;
                PhotonNetwork.Instantiate(photonPlayerPrefab.name, _position, _rotation);
            }
        }
    }
}
