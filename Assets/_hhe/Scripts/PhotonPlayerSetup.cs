using Photon.Pun;
using UnityEngine;
using TMPro;

public class PhotonPlayerSetup : MonoBehaviourPunCallbacks
{
    //[SerializeField]
    //private TMP_Text PlayerName;

    public GameObject photonPlayerNamePrefab;

    private Transform _Camera;
    private TMP_Text Message;

    private float CheckOthersPositionInterval = 0.1f;
    private float CheckOthersPositionTimer;

    void Start()
    {
        Message = GameObject.Find("Message").GetComponent<TMP_Text>();
        //Message.text = "PhotonPlayerSetup: Start";

        _Camera = Camera.main.transform;

        //if (photonView.IsMine)
        //{
        //    //Message.text = "PhotonView.IsMine";
        //    foreach (GameObject gameObject in mePrefabs)
        //    {
        //        gameObject.SetActive(true);
        //    }
        //    foreach (GameObject gameObject in otherPrefabs)
        //    {
        //        gameObject.SetActive(false);
        //    }
        //}
        //else
        //{
        //    //Message.text = "PhotonView.NotMine";
        //    foreach (GameObject gameObject in mePrefabs)
        //    {
        //        gameObject.SetActive(false);
        //    }
        //    foreach (GameObject gameObject in otherPrefabs)
        //    {
        //        gameObject.SetActive(true);
        //    }
        //}
        //if(PlayerName != null)
        //{
        //    PlayerName.text = photonView.Owner.NickName;
        //}
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            #region Sharing my own position
            //if (Vector3.Distance(_Camera.position, this.transform.position) > 0.02f)
            {
                Pose pose1 = new Pose(Vector3.zero, Quaternion.identity); //Local Origo
                Pose pose2 = PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose; // localCommonCloudReferencePose
                Pose pose3 = new Pose(_Camera.position, _Camera.rotation);
                Pose poseNew = PhotonPlayersSingleton.Instance.GetNewPoseGameObject(pose1, pose2, pose3);

                this.photonView.RPC("Send_My_Position", RpcTarget.AllBuffered, poseNew.position, poseNew.rotation);
            }
            #endregion

            #region Getting other Players position
            //if (CheckOthersPositionTimer < CheckOthersPositionInterval)
            //{
            //    CheckOthersPositionTimer += Time.deltaTime;
            //}
            //else
            {
                CheckOthersPositionTimer = 0.0f;

                //Find other players.
                for (int i = 0; i < PhotonPlayersSingleton.Instance.CloudReferencePointId.Length; i++)
                {

                    Pose pose1 = PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose;
                    Pose pose2 = PhotonPlayersSingleton.Instance.poseCloudReference[i];
                    Pose pose3 = PhotonPlayersSingleton.Instance.posePhotonPlayers[i];
                    Pose poseNew = PhotonPlayersSingleton.Instance.GetNewPoseGameObject(pose1, pose2, pose3);


                    Vector3 position = poseNew.position;
                    Quaternion rotation = poseNew.rotation;
                    string name = PhotonPlayersSingleton.Instance.namePhotonPlayers[i];

                    position = position - (Vector3.up * 10);  //Kun test

                    GameObject w = GameObject.Find(name);
                    if (w == null)
                    {
                        //Message.text = "Kilroy 1";
                        GameObject testPrefab = Instantiate(photonPlayerNamePrefab, position, rotation);
                        TextMeshPro textMesh = testPrefab.GetComponent<TextMeshPro>();
                        textMesh.text = name;
                        testPrefab.name = name;
                    }
                    else
                    {
                        //float step = Time.deltaTime; // calculate distance to move
                        //w.transform.position = Vector3.MoveTowards(transform.position, position, step);
                        //w.transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);
                        w.transform.position = position;
                        w.transform.rotation =  rotation;
                    }
                }
            }
            #endregion
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