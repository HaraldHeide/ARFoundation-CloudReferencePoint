using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;
using Photon.Pun;

public class HostCloudReferencePoint : MonoBehaviourPun
{
    public GameObject HostedPointPrefab;
    public GameObject ResolvedPointPrefab;
    private ARReferencePointManager ReferencePointManager;
    private ARRaycastManager RaycastManager;
    private TMP_Text Message;
    private TMP_InputField InputField;

    private enum AppMode
    {
        // Wait for user to tap screen to begin hosting a point.
        TouchToHostCloudReferencePoint,

        // Poll hosted point state until it is ready to use.
        WaitingForHostedReferencePoint,

        // Wait for user to tap screen to begin resolving the point.
        TouchToResolveCloudReferencePoint,

        // Poll resolving point state until it is ready to use.
        WaitingForResolvedReferencePoint,
    }

    private AppMode m_AppMode = AppMode.TouchToHostCloudReferencePoint;
    private ARCloudReferencePoint m_CloudReferencePoint;
    private string m_CloudReferenceId;

    void Start()
    {
        Message = GameObject.Find("Message").GetComponent<TMP_Text>();
        Message.text = "HostCloudReferencePoint: Start";

        ReferencePointManager = GameObject.Find("AR Session Origin").GetComponent<ARReferencePointManager>();
        RaycastManager = GameObject.Find("AR Session Origin").GetComponent<ARRaycastManager>();
        InputField = GameObject.Find("InputField (TMP)").GetComponent<TMP_InputField>();

        InputField.onEndEdit.AddListener(OnInputEndEdit);
    }

    void Update()
    {
        if (m_AppMode == AppMode.TouchToHostCloudReferencePoint)
        {
            Message.text = m_AppMode.ToString();

            if (Input.touchCount >= 1
                && Input.GetTouch(0).phase == TouchPhase.Began
                && !EventSystem.current.IsPointerOverGameObject(
                        Input.GetTouch(0).fingerId))
            {
                List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
                RaycastManager.Raycast(Input.GetTouch(0).position, hitResults);
                if (hitResults.Count > 0)
                {
                    Pose pose = hitResults[0].pose;

                    // Create a reference point at the touch.
                    ARReferencePoint referencePoint =
                        ReferencePointManager.AddReferencePoint(hitResults[0].pose);

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
            Message.text = m_AppMode.ToString();


            CloudReferenceState cloudReferenceState =
                m_CloudReferencePoint.cloudReferenceState;
            Message.text += " - " + cloudReferenceState.ToString();

            if (cloudReferenceState == CloudReferenceState.Success)
            {
                GameObject cloudAnchor = Instantiate(
                                             HostedPointPrefab,
                                             Vector3.zero,
                                             Quaternion.identity);
                cloudAnchor.transform.SetParent(
                    m_CloudReferencePoint.transform, false);

                m_CloudReferenceId = m_CloudReferencePoint.cloudReferenceId;
                m_CloudReferencePoint = null;

                m_AppMode = AppMode.TouchToResolveCloudReferencePoint;
            }
        }
        else if (m_AppMode == AppMode.TouchToResolveCloudReferencePoint)
        {
            Message.text = m_CloudReferenceId;

            if (Input.touchCount >= 1
                && Input.GetTouch(0).phase == TouchPhase.Began
                && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                m_CloudReferencePoint = ReferencePointManager.ResolveCloudReferenceId(m_CloudReferenceId);
                if (m_CloudReferencePoint == null)
                {
                    Message.text = "Resolve Failed!";
                    m_CloudReferenceId = string.Empty;
                    m_AppMode = AppMode.TouchToHostCloudReferencePoint;
                    return;
                }

                this.photonView.RPC("Set_CloudReferenceId", RpcTarget.OthersBuffered, m_CloudReferenceId);


                m_CloudReferenceId = string.Empty;

                // Wait for the reference point to be ready.
                m_AppMode = AppMode.WaitingForResolvedReferencePoint;
            }
        }
        else if (m_AppMode == AppMode.WaitingForResolvedReferencePoint)
        {
            Message.text = m_AppMode.ToString();

            CloudReferenceState cloudReferenceState = m_CloudReferencePoint.cloudReferenceState;
            Message.text += " - " + cloudReferenceState.ToString();

            if (cloudReferenceState == CloudReferenceState.Success)
            {
                GameObject cloudAnchor = Instantiate(ResolvedPointPrefab, Vector3.zero, Quaternion.identity);
                cloudAnchor.transform.SetParent(m_CloudReferencePoint.transform, false);

                m_CloudReferencePoint = null;

                m_AppMode = AppMode.TouchToHostCloudReferencePoint;
            }
        }
    }

    #region Photon RPC
    [PunRPC]
    void Set_CloudReferenceId(string id)
    {
        OnInputEndEdit(id);
    }
    #endregion Photon RPC

    #region Private Methods
    private void OnInputEndEdit(string text)
    {
        m_CloudReferenceId = string.Empty;

        m_CloudReferencePoint = ReferencePointManager.ResolveCloudReferenceId(text);
        if (m_CloudReferencePoint == null)
        {
            Message.text = "Resolve Failed!";
            m_AppMode = AppMode.TouchToHostCloudReferencePoint;
            return;
        }

        // Wait for the reference point to be ready.
        m_AppMode = AppMode.WaitingForResolvedReferencePoint;
    }
    #endregion Private Methods
}
