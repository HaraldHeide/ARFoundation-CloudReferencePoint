using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;
using Photon.Pun;
using System.Collections;
using System;
using Photon.Realtime;

public class HostAndResolveCloudReferencePoint : MonoBehaviourPunCallbacks
{
    public GameObject HostedPointPrefab;
    public GameObject ResolvedPointPrefab;
    private ARReferencePointManager ReferencePointManager;
    private ARRaycastManager RaycastManager;
    private TMP_Text Message;

    private enum AppMode
    {
        // Wait for user to tap screen to begin hosting a point.
        TouchToHostCloudReferencePoint,

        // Poll hosted point state until it is ready to use.
        WaitingForHostedReferencePoint,

        // Wait for user to tap screen to begin resolving the point.
        ResolveCloudReferencePoint,

        // Poll resolving point state until it is ready to use.
        WaitingForResolvedReferencePoint,

        // Poll resolving point state until it is ready to use.
        Finished,
    }

    //private AppMode m_AppMode = AppMode.TouchToHostCloudReferencePoint;
    private AppMode m_AppMode;
    private ARCloudReferencePoint m_CloudReferencePoint;
    private string m_CloudReferenceId;

    void Start()
    {
        Message = GameObject.Find("Message").GetComponent<TMP_Text>();

        ReferencePointManager = GameObject.Find("AR Session Origin").GetComponent<ARReferencePointManager>();

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Message.text = "IsMasterClient";
            RaycastManager = GameObject.Find("AR Session Origin").GetComponent<ARRaycastManager>();

            m_AppMode = AppMode.TouchToHostCloudReferencePoint;
        }
        else
        {
            Message.text = "Not IsMasterClient";
        }
    }

    void Update()
    {
        #region Hosting Cloud Reference Point
        if (m_AppMode == AppMode.TouchToHostCloudReferencePoint)
        {
            if (Input.touchCount >= 1
                && Input.GetTouch(0).phase == TouchPhase.Began
                && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
                RaycastManager.Raycast(Input.GetTouch(0).position, hitResults);
                if (hitResults.Count > 0)
                {
                    Pose pose = hitResults[0].pose;

                    // Create a reference point at the touch.
                    ARReferencePoint referencePoint = ReferencePointManager.AddReferencePoint(hitResults[0].pose);

                    // Create Cloud Reference Point.
                    m_CloudReferencePoint = ReferencePointManager.AddCloudReferencePoint(referencePoint);
                    if (m_CloudReferencePoint == null)
                    {
                        Message.text = "Create Failed!";
                        return;
                    }

                    // Wait for the reference point to be ready.
                    m_AppMode = AppMode.WaitingForHostedReferencePoint;
                }
            }
        }
        else if (m_AppMode == AppMode.WaitingForHostedReferencePoint)
        {
            CloudReferenceState cloudReferenceState = m_CloudReferencePoint.cloudReferenceState;

            if (cloudReferenceState == CloudReferenceState.Success)
            {
                GameObject cloudAnchor = Instantiate(HostedPointPrefab, Vector3.zero, Quaternion.identity);
                cloudAnchor.transform.SetParent(m_CloudReferencePoint.transform, false);

                m_CloudReferenceId = m_CloudReferencePoint.cloudReferenceId;  // Getting cloud id to share with Others

                PhotonPlayersSingleton.Instance.CloudReferencePoindId = m_CloudReferenceId;

                //m_CloudReferencePoint = null;

                //m_AppMode = AppMode.TouchToResolveCloudReferencePoint;

                this.photonView.RPC("Set_CloudReferenceId", RpcTarget.OthersBuffered, m_CloudReferenceId);
                m_AppMode = AppMode.WaitingForResolvedReferencePoint;
            }
        }
        #endregion

        #region Waiting for CloudReferencePointId
        else if (m_AppMode == AppMode.ResolveCloudReferencePoint && PhotonPlayersSingleton.Instance.CloudReferencePoindId != "" && PhotonPlayersSingleton.Instance.CloudReferencePoindId != null)
        {
            Message.text = "Waiting for cloudrefpoint: " + PhotonPlayersSingleton.Instance.CloudReferencePoindId;


            m_CloudReferenceId = string.Empty;

            m_CloudReferencePoint = ReferencePointManager.ResolveCloudReferenceId(PhotonPlayersSingleton.Instance.CloudReferencePoindId);
            if (m_CloudReferencePoint == null)
            {
                Message.text = "Resolve Failed!";
                return;
            }

            m_AppMode = AppMode.WaitingForResolvedReferencePoint;
        }
        #endregion Waiting for CloudReferencePoint

        #region Resolving cloudreference point
        else if (m_AppMode == AppMode.WaitingForResolvedReferencePoint)
        {
            CloudReferenceState cloudReferenceState = m_CloudReferencePoint.cloudReferenceState;
            if (cloudReferenceState == CloudReferenceState.Success)
            {
                //Message.text = "B CloudReferenceId: " + PhotonPlayersSingleton.Instance.CloudReferencePoindId +
                //    "\nCloudReferencePoint position: " + m_CloudReferencePoint.transform.position.ToString();

                GameObject cloudAnchor = Instantiate(ResolvedPointPrefab, Vector3.zero, Quaternion.identity);
                cloudAnchor.transform.SetParent(m_CloudReferencePoint.transform, false);

                PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose = m_CloudReferencePoint.pose;

                //m_CloudReferencePoint = null;

                m_AppMode = AppMode.Finished;
            }
        }
        #endregion
    }

    #region Photon RPC
    [PunRPC]
    void Set_CloudReferenceId(string id)
    {
        Message.text = "Refid: " + PhotonPlayersSingleton.Instance.CloudReferencePoindId;
        if (PhotonPlayersSingleton.Instance.CloudReferencePoindId == "" || PhotonPlayersSingleton.Instance.CloudReferencePoindId == null)
        {
            PhotonPlayersSingleton.Instance.CloudReferencePoindId = id;
            m_AppMode = AppMode.ResolveCloudReferencePoint;
        }
    }
    #endregion Photon RPC

    #region Photon Callback
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            this.photonView.RPC("Set_CloudReferenceId", RpcTarget.OthersBuffered, m_CloudReferenceId);
        }
    }
    #endregion
}
