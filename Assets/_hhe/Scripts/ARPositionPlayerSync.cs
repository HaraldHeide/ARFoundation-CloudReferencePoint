using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class ARPositionPlayerSync : MonoBehaviour, IPunObservable
{

    private PhotonView photonView;
    private Transform myCamera;
    private Transform otherCamera;

    TextMeshPro textMesh;

    string myName = "";

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        textMesh = GetComponentInChildren<TextMeshPro>();
    }


    void Start()
    {
        if(photonView.IsMine)
        {
            myCamera = Camera.main.transform;
            myName = PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            otherCamera = Camera.main.transform;
        }
        textMesh.text = myName;
    }

    void Update()
    {
        transform.position = Camera.main.transform.position;
        transform.LookAt(otherCamera);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if(stream.IsReading)
       {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
