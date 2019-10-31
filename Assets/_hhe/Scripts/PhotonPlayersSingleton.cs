/*
 * 
 * 
 * Struct
 * Players NickName
 * Pose - Related to CommonCloudReferencePoint
 * (Change coordinate system in Players own local app instance,
 * from ARFoundation as Origo to  using
 * CommonCloudReferencePoint as Origo and get a new Pose fro the player)
 * (Based on CommonCloudreferencepoint)
 * 
 * Then call Update_Local_Player_Pose with this new Pose)
 * 
 * 
 * Position must be added to localPlayers own CommonCloudReferencePoint position.
 * Rotation must be multiplied by localPlayers own CommonCloudReferencePoint rotation.
 * 
 * 
 */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

//Insert or update local player position (Pose based on commoncloudreferencepoint as origo)
public class PhotonPlayersSingleton : GenericSingletonClass<PhotonPlayersSingleton>
{
    public TMP_Text Message;

    public void Start()
    {
        Message = GameObject.Find("Message").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        Message.text = "PhotonPlayersSingleton: " + CloudReferencePoindId +
            "\nCloudReferencePose: " + LocalPlayerCloudReferencePose;
    }

    public string CloudReferencePoindId = "";
    public Pose LocalPlayerCloudReferencePose = new Pose();

    public List<string> namePhotonPlayers = new List<string>();
    public List<Pose> posePhotonPlayers = new List<Pose>();

    public void Update_Local_Player_Pose(string nickName, Pose pose)
    {
        if (namePhotonPlayers.Contains(nickName))
        {
            int i = namePhotonPlayers.IndexOf(nickName);
            posePhotonPlayers[i] = pose;
        }
        else
        {
            namePhotonPlayers.Add(nickName);
            posePhotonPlayers.Add(pose);
        }
    }


    //Change coordinate system for Object
    public Pose GetNewPoseGameObject(Pose OriginalOrigoPose, Pose NewOrigo, Pose OriginalGameObjectPose)
    {
        float x = OriginalGameObjectPose.position.x;
        float y = OriginalGameObjectPose.position.y;
        float z = OriginalGameObjectPose.position.z;

        float h = NewOrigo.position.x - OriginalOrigoPose.position.x;
        float k = NewOrigo.position.z - OriginalOrigoPose.position.z;

        float pDeg = Quaternion.Angle(NewOrigo.rotation, OriginalOrigoPose.rotation);
        float pRad = Mathf.Deg2Rad * pDeg;

        float xNew = (x - h) * Mathf.Cos(pRad) + (z - k) * Mathf.Sin(pRad);
        float zNew = (x - h) * -Mathf.Sin(pRad) + (z - k) * Mathf.Cos(pRad);

        Quaternion quat = OriginalGameObjectPose.rotation * NewOrigo.rotation;
        Vector3 pos = new Vector3(xNew, y, zNew);

        Pose newPose = new Pose(pos, quat);

        return newPose;
    }

}
