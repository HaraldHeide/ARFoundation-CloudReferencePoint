using Photon.Pun;
using UnityEngine;
using TMPro;

public class PhotonPlayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject photonPlayerNamePrefab;

    private Transform _Camera;
    private TMP_Text Message;

    private float CheckOthersPositionInterval = 1.0f;
    private float CheckOthersPositionTimer;

    void Start()
    {
        Message = GameObject.Find("Message").GetComponent<TMP_Text>();
        _Camera = Camera.main.transform;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            #region Sharing my own position
            // Changing localplayers pose coordinates from being based on ARFoundation Origo to being based on CloudReferencePoint.
            if (Vector3.Distance(_Camera.position, this.transform.position) > 0.03f)
            {
                Pose pose1 = new Pose(Vector3.zero, Quaternion.identity); //Local Origo
                Pose pose2 = PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose; // localCommonCloudReferencePose
                Pose pose3 = new Pose(_Camera.position, _Camera.rotation);
                Pose poseNew = PhotonPlayersSingleton.Instance.GetNewPoseGameObject(pose1, pose2, pose3);

                this.photonView.RPC("Send_My_Position", RpcTarget.AllBuffered, poseNew.position, poseNew.rotation);
            }
            #endregion

            #region Getting other Players position
            if (CheckOthersPositionTimer < CheckOthersPositionInterval)
            {
                CheckOthersPositionTimer += Time.deltaTime;
            }
            else
            {
                CheckOthersPositionTimer = 0.0f;

                //Find other players.
                for (int i = 0; i < PhotonPlayersSingleton.Instance.CloudReferencePointId.Length; i++)
                {
                    //Converting from using other Players (The one to be viewed) localCloudReferenhcePoint as Origo to start using
                    // this localplayers CloudReferencePoint as Origo for instantiated positioning/rotation. 
                    Pose pose1 = PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose;
                    Pose pose2 = PhotonPlayersSingleton.Instance.poseCloudReference[i];
                    Pose pose3 = PhotonPlayersSingleton.Instance.posePhotonPlayers[i];
                    Pose poseNew = PhotonPlayersSingleton.Instance.GetNewPoseGameObject(pose1, pose2, pose3);

                    Vector3 positionNew = poseNew.position;
                    Quaternion rotationNew = poseNew.rotation;
                    string name = PhotonPlayersSingleton.Instance.namePhotonPlayers[i];

                    GameObject w = GameObject.Find(name);
                    if (w == null)
                    {
                        w = Instantiate(photonPlayerNamePrefab, positionNew, rotationNew);
                        TextMeshPro textMesh = w.GetComponentInChildren<TextMeshPro>();
                        textMesh.text = name;
                        textMesh.name = name;
                        w.name = name;
                    }
                    else
                    {
                        //float step = Time.deltaTime; // calculate distance to move
                        //w.transform.position = Vector3.MoveTowards(transform.position, position, step);
                        //w.transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);
                        //w.transform.Translate(position + Vector3.left);
                        w.transform.position = positionNew;
                        w.transform.rotation =  rotationNew;
                        Message.text = "Position" + "[" + i + "]: " + positionNew;
                        Message.text += "\nw pos" + "[" + i + "]: " + w.transform.position;
                    }
                }
            }
            #endregion Getting other Players position
        }
    }

    [PunRPC]
    private void Send_My_Position(Vector3 pos, Quaternion rot)
    {
        PhotonPlayersSingleton.Instance.Update_Local_Player_Pose(photonView.Owner.NickName, pos, rot,
                                            PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose.position, 
                                            PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose.rotation);
    }
}