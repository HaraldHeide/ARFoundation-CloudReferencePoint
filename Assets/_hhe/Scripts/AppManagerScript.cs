using UnityEngine;
using Photon.Pun;
using TMPro;
public class AppManagerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject photonPlayerPrefab;

    [SerializeField]
    private TMP_Text Message;

    void Start()
    {
        Message.text = "AppManagerScript: Start";

        if (PhotonNetwork.IsConnectedAndReady)
        {
            Message.text = "PhotonNetwork IsConnectedAndReady";

            //Objects to be network instantiated must 
            //be in a Resources folder and contain a PhotonView component
            if (photonPlayerPrefab != null)
            {
                Vector3 _position = Camera.main.transform.position;
                Quaternion _rotation = Camera.main.transform.rotation;
                PhotonNetwork.Instantiate(photonPlayerPrefab.name, _position, _rotation);
                Message.text = "PhotonPlayerPrefab: " + photonPlayerPrefab.name;
                Message.text = "AppManagerScript: End OK";
            }
            else
            {
                Message.text = "Error: PlayerPrefab not instanciated!";
            }
        }
    }

    private void Update()
    {
        //Message.text = "Camera Position: " + Camera.main.transform.position;
    }
}
