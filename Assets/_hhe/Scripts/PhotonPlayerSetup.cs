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
        if (Vector3.Distance(_Camera.position, this.transform.position) > 0.5f)
        {
            this.transform.position = _Camera.position;
            this.transform.rotation = _Camera.rotation;
        }
    }
}
