using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class ARPositionPlayerSync : MonoBehaviour, IPunObservable
{

    private PhotonView photonView;

    private Transform localCamera;
    private Transform remoteCamera;

    TextMeshPro textMesh;

    string myName = "";

    Vector3 networkedLocalPos;
    Quaternion networkedLocalRot;

    Vector3 networkedRemotePos;
    Quaternion networkedRemoteRot;

    private Pose poseOrigo = new Pose(Vector3.zero,Quaternion.identity);
    private Pose poseLocalCloudRef;
    private Pose poseRemoteCloudRef;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        textMesh = GetComponentInChildren<TextMeshPro>();
    }


    void Start()
    {
        if(photonView.IsMine)
        {
            localCamera = Camera.main.transform;
            myName = PhotonNetwork.LocalPlayer.NickName;
            poseLocalCloudRef = PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose;
        }
        else  // Local View
        {
            remoteCamera = Camera.main.transform;
            poseRemoteCloudRef = PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose;
        }
        textMesh.text = myName;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            transform.position = localCamera.position;
            transform.rotation = localCamera.rotation;

            //Changing Coordinate reference from localOriogo (0, 0, 0)  to local CommonCloudPointReference.
            Pose poseOut = PhotonPlayersSingleton.Instance.GetNewPoseGameObject(
                           poseOrigo, 
                           poseLocalCloudRef,
                           new Pose(localCamera.position, localCamera.rotation));
            networkedLocalPos = poseOut.position;
            networkedLocalRot = poseOut.rotation;
        }
        else
        {
            //Changing Coordinate reference from remote CommonCloudPointReference to localOriogo (0, 0, 0).
            Pose poseOut = PhotonPlayersSingleton.Instance.GetNewPoseGameObject(
                           poseRemoteCloudRef,
                           poseOrigo,
                           new Pose(networkedRemotePos, networkedRemoteRot));

            transform.position = poseOut.position;
            //transform.rotation = poseOut.rotation;
            //transform.rotation = Quaternion.LookRotation(remoteCamera.position, Vector3.up);
            transform.LookAt(remoteCamera.position);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       if(stream.IsWriting)
        {
            stream.SendNext(networkedLocalPos);
            stream.SendNext(networkedLocalRot);
        }
        else // if(stream.IsReading)
       {
            networkedRemotePos = (Vector3)stream.ReceiveNext();
            networkedRemoteRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
