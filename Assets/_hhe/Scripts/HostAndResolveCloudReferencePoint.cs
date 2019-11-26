using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class HostAndResolveCloudReferencePoint : MonoBehaviourPunCallbacks
{
    //public GameObject HostedPointPrefab;  //Blue sphere
    public GameObject ResolvedPointPrefab;  //Yellow Box

    private ARReferencePointManager ReferencePointManager; //Script that uses Red Cylinder
    private ARRaycastManager RaycastManager;

    private ARPlaneManager planeManager;
    private ARPointCloudManager pointCloudManager;

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
    private AppMode m_AppMode = AppMode.Finished;
    private ARCloudReferencePoint m_CloudReferencePoint;
    private string m_CloudReferenceId;

    void Start()
    {
        Message = GameObject.Find("Message").GetComponent<TMP_Text>();
        Message.text = "Start";

        planeManager = GameObject.Find("AR Session Origin").GetComponent<ARPlaneManager>();
        pointCloudManager = GameObject.Find("AR Session Origin").GetComponent<ARPointCloudManager>();
        ReferencePointManager = GameObject.Find("AR Session Origin").GetComponent<ARReferencePointManager>();

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            if (!photonView.IsMine)
            {
                Message.text = "Start IsMasterClient but not PhotonVIewIsMine";
                return;
            }

            Message.text = "MasterClient - please place common reference point by tapping a plane...  ";
            RaycastManager = GameObject.Find("AR Session Origin").GetComponent<ARRaycastManager>();

            m_AppMode = AppMode.TouchToHostCloudReferencePoint;
        }
        else
        {
            //Only MasterClient needs to see the environment interpretation (Points Planes)
            //Only Masterclient sets Cloud Reference Point
            VisualizePlanes(false);
            VisualizePoints(false);
            Message.text = "Not MasterClient";
            m_AppMode = AppMode.ResolveCloudReferencePoint;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        #region AppMode.TouchToHostCloudReferencePoint
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

                    //Ref point created we can now turn off environment visualization
                    StartCoroutine(VisualizePlanes(false));
                    StartCoroutine(VisualizePoints(false));
                    // Wait for the reference point to be ready.
                    m_AppMode = AppMode.WaitingForHostedReferencePoint;
                }
            }
        }
        #endregion AppMode.TouchToHostCloudReferencePoint

        #region AppMode.WaitingForHostedReferencePoint
        else if (m_AppMode == AppMode.WaitingForHostedReferencePoint)
        {
            CloudReferenceState cloudReferenceState = m_CloudReferencePoint.cloudReferenceState;

            if (cloudReferenceState == CloudReferenceState.Success)
            {
                //GameObject cloudAnchor = Instantiate(HostedPointPrefab, Vector3.zero, Quaternion.identity);
                //cloudAnchor.transform.SetParent(m_CloudReferencePoint.transform, false);

                m_CloudReferenceId = m_CloudReferencePoint.cloudReferenceId;  // Getting cloud id to share with Others

                PhotonPlayersSingleton.Instance.CloudReferencePointId = m_CloudReferenceId;

                //m_CloudReferencePoint = null;

                //m_AppMode = AppMode.TouchToResolveCloudReferencePoint;

                this.photonView.RPC("Set_CloudReferenceId", RpcTarget.OthersBuffered, m_CloudReferenceId);
                m_AppMode = AppMode.WaitingForResolvedReferencePoint;
            }
        }
        #endregion AppMode.WaitingForHostedReferencePoint

        #region AppMode.ResolveCloudReferencePoint
        //All other than MasterClient
        else if (m_AppMode == AppMode.ResolveCloudReferencePoint && PhotonPlayersSingleton.Instance.CloudReferencePointId != "" && PhotonPlayersSingleton.Instance.CloudReferencePointId != null)
        {
            m_CloudReferencePoint = null;
            m_CloudReferencePoint = ReferencePointManager.ResolveCloudReferenceId(PhotonPlayersSingleton.Instance.CloudReferencePointId);

            if (m_CloudReferencePoint == null)
            {
                Message.text = "Resolve Failed!";
                return;
            }
            m_AppMode = AppMode.WaitingForResolvedReferencePoint;
            Message.text = "Mode: AppMode.WaitingForResolvedReferencePoint";

        }
        #endregion AppMode.ResolveCloudReferencePoint

        #region AppMode.WaitingForResolvedReferencePoint
        else if (m_AppMode == AppMode.WaitingForResolvedReferencePoint)
        {
            CloudReferenceState cloudReferenceState = m_CloudReferencePoint.cloudReferenceState;
            if (cloudReferenceState == CloudReferenceState.Success)
            {
                Message.text = "XCloudReferenceId: " + PhotonPlayersSingleton.Instance.CloudReferencePointId +
                    "\nCloudReferencePoint position: " + m_CloudReferencePoint.transform.position.ToString();

                GameObject cloudAnchor = Instantiate(ResolvedPointPrefab, Vector3.zero, Quaternion.identity);

                //if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    cloudAnchor.transform.SetParent(m_CloudReferencePoint.transform, false);
                    StartCoroutine(VisualizePlanes(false));
                    StartCoroutine(VisualizePoints(false));
                }

                PhotonPlayersSingleton.Instance.LocalPlayerCloudReferencePose = m_CloudReferencePoint.pose;
                m_AppMode = AppMode.Finished;
                Message.text = "Mode: AppMode.Finished";
                //this.enabled = false;  //
            }
        }
        #endregion AppMode.WaitingForResolvedReferencePoint
    }

    #region Photon Callback
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            this.photonView.RPC("Set_CloudReferenceId", RpcTarget.OthersBuffered, PhotonPlayersSingleton.Instance.CloudReferencePointId);
        }
    }
    #endregion Photon Callback

    #region Photon RPC
    [PunRPC]
    void Set_CloudReferenceId(string id)
    {
        if (PhotonPlayersSingleton.Instance.CloudReferencePointId == "" || PhotonPlayersSingleton.Instance.CloudReferencePointId == null)
        {
            PhotonPlayersSingleton.Instance.CloudReferencePointId = id;
            m_AppMode = AppMode.ResolveCloudReferencePoint;
            Message.text = "PunRPC Mode: AppMode.ResolveCloudReferencePoint";
        }
    }
    #endregion Photon RPC

    #region Turn on Off Planes and cloudpoints
    private IEnumerator VisualizePlanes(bool active)
    {
        yield return new WaitForSeconds(2);
        if (planeManager == null)
        {
            yield return null;
        }
        planeManager.enabled = active;

        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(active);
        }
    }

    private IEnumerator VisualizePoints(bool active)
    {
        yield return new WaitForSeconds(2);
        if (pointCloudManager == null)
        {
            yield return null;
        }
        pointCloudManager.enabled = active;
        foreach (ARPointCloud point in pointCloudManager.trackables)
        {
            point.gameObject.SetActive(active);
        }
    }
    #endregion Turn on Off Planes and cloudpoints
}
