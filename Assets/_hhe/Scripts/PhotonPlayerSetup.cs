using Photon.Pun;
using UnityEngine;
using TMPro;

public class PhotonPlayerSetup : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_Text PlayerName;

    public GameObject[] mePrefabs;
    public GameObject[] otherPrefabs;

    private Transform _Camera;
    private TMP_Text Message;

    private float CheckOthersPositionInterval = 01f;
    private float CheckOthersPositionTimer;

    void Start()
    {
        Message = GameObject.Find("Message").GetComponent<TMP_Text>();
        Message.text = "PhotonPlayerSetup: Start";

        _Camera = Camera.main.transform;

        if (photonView.IsMine)
        {
            Message.text = "PhotonView.IsMine";
            foreach (GameObject gameObject in mePrefabs)
            {
                gameObject.SetActive(true);
                Message.text = "gameObject.SetActive";

            }
            foreach (GameObject gameObject in otherPrefabs)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Message.text = "PhotonView.NotMine";

            foreach (GameObject gameObject in mePrefabs)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject gameObject in otherPrefabs)
            {
                gameObject.SetActive(true);
            }
        }
        if(PlayerName != null)
        {
            PlayerName.text = photonView.Owner.NickName;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (Vector3.Distance(_Camera.position, this.transform.position) > 0.5f)
            {
                Pose pose1 = new Pose(Vector3.zero, Quaternion.identity); //Local Origo
                Pose pose2 = PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose; // localCommonCloudReferencePose
                Pose pose3 = new Pose(_Camera.position, _Camera.rotation);

                Pose poseNew = PhotonPlayersSingleton.Instance.GetNewPoseGameObject(pose1, pose2, pose3);
                this.photonView.RPC("Send_My_Position", RpcTarget.AllBuffered, poseNew);
            }
            if(CheckOthersPositionTimer < CheckOthersPositionInterval)
            {
                CheckOthersPositionTimer += Time.deltaTime;
            }
            else
            {
                CheckOthersPositionTimer = 0.0f;

                //Find other players.
                for (int i = 0; i < PhotonPlayersSingleton.Instance.CloudReferencePoindId.Length - 1; i++)
                {
                    Vector3 position = PhotonPlayersSingleton.Instance.posePhotonPlayers[i].position;
                    position += PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose.position;
                    Quaternion rotation = PhotonPlayersSingleton.Instance.posePhotonPlayers[i].rotation;
                    rotation *= PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose.rotation;
                    string name = PhotonPlayersSingleton.Instance.namePhotonPlayers[i];
                    GameObject w1 = GameObject.Find(name);
                    if (w1 == null)
                    {
                        GameObject w = (GameObject)Instantiate(otherPrefabs[0], position, rotation);
                        w.name = name;
                    }
                    else
                    {
                        float step =  Time.deltaTime; // calculate distance to move
                        w1.transform.position = Vector3.MoveTowards(transform.position, position, step);
                        w1.transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step);

                    }
                }
            }
        }
    }

    [PunRPC]
    private void Send_My_Position(Pose pose)
    {
        PhotonPlayersSingleton.Instance.Update_Local_Player_Pose(photonView.Owner.NickName, pose);
    }
}
