/*
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
 */
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Insert or update local player position (Pose based on commoncloudreferencepoint as origo)
public class PhotonPlayersSingleton : GenericSingletonClass<PhotonPlayersSingleton>
{
    public TMP_Text Message;

    public string CloudReferencePointId = "";
    public Pose LocalPlayerCloudReferencePose = new Pose();

    public List<string> namePhotonPlayers = new List<string>();
    public List<Pose> posePhotonPlayers = new List<Pose>();

    public void Start()
    {
        Message = GameObject.Find("Message").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        Message.text = "CloudReferencePointId: " + CloudReferencePointId;
        Message.text += "\nCloudReferencePosition: " + LocalPlayerCloudReferencePose.position;
        //Message.text += " CloudReferenceRotation: " + LocalPlayerCloudReferencePose.rotation.eulerAngles;
        //Message.text += " Count: " + namePhotonPlayers.Count;

        //for (int i = 0; i < namePhotonPlayers.Count; i++)
        //{
        //    Message.text += "\nName[" + i + "]: " + namePhotonPlayers[i];
        //    Message.text += " Pos: " + posePhotonPlayers[i].position;
        //    Message.text += " RotY: " + posePhotonPlayers[i].rotation.eulerAngles.y;
        //}
    }

    public void Update_Local_Player_Pose(string nickName, Vector3 _PosPlayer, Quaternion _RotPlayer)
    {
        Pose _PosePlayer = new Pose(_PosPlayer, _RotPlayer);
        if (namePhotonPlayers.Contains(nickName))
        {
            int i = namePhotonPlayers.IndexOf(nickName);
            posePhotonPlayers[i] = _PosePlayer;
        }
        else
        {
            namePhotonPlayers.Add(nickName);
            posePhotonPlayers.Add(_PosePlayer);
        }
    }

    //Change coordinate system for Object
    public Pose GetNewPoseGameObject(Pose OriginalOrigo, Pose NewOrigo, Pose OriginalGameObjectPose)
    {
        float x = OriginalGameObjectPose.position.x;
        float y = OriginalGameObjectPose.position.y;
        float z = OriginalGameObjectPose.position.z;

        float h = NewOrigo.position.x - OriginalOrigo.position.x;
        float k = NewOrigo.position.z - OriginalOrigo.position.z;

        float pDeg = Quaternion.Angle(NewOrigo.rotation, OriginalOrigo.rotation);  //Angle always Positive
        float pRad = Mathf.Deg2Rad * pDeg;
        // Since Quaternion.Angle always gives positive result We must check if angle is negative.
        if (NewOrigo.rotation.y > OriginalOrigo.rotation.y)
        {
            pRad *= -1;
        }

        float xNew = (x - h) * Mathf.Cos(pRad) + (z - k) * Mathf.Sin(pRad);
        float zNew = (x - h) * -Mathf.Sin(pRad) + (z - k) * Mathf.Cos(pRad);

        Quaternion quat = Quaternion.Inverse(OriginalGameObjectPose.rotation * NewOrigo.rotation);
        Vector3 pos = new Vector3(xNew, y, zNew);

        Pose newPose = new Pose(pos, quat);

        return newPose;
    }
}
