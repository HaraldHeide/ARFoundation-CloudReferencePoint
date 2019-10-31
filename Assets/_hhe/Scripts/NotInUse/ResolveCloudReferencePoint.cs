using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;
using Photon.Pun;

public class ResolveCloudReferencePoint : MonoBehaviourPun
{
    public GameObject HostedPointPrefab;
    public GameObject ResolvedPointPrefab;
    public ARReferencePointManager ReferencePointManager;
    public ARRaycastManager RaycastManager;
    public TMP_InputField InputField;
    public TMP_Text OutputText;

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
        InputField.onEndEdit.AddListener(OnInputEndEdit);
    }

    void Update()
    {
        if (m_AppMode == AppMode.TouchToHostCloudReferencePoint)
        {
            OutputText.text = m_AppMode.ToString();

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
                        OutputText.text = "Create Failed!";
                        return;
                    }

                    // Wait for the reference point to be ready.
                    m_AppMode = AppMode.WaitingForHostedReferencePoint;
                }
            }
        }
        else if (m_AppMode == AppMode.WaitingForHostedReferencePoint)
        {
            OutputText.text = m_AppMode.ToString();


            CloudReferenceState cloudReferenceState =
                m_CloudReferencePoint.cloudReferenceState;
            OutputText.text += " - " + cloudReferenceState.ToString();

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
            OutputText.text = m_CloudReferenceId;

            if (Input.touchCount >= 1
                && Input.GetTouch(0).phase == TouchPhase.Began
                && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                m_CloudReferencePoint = ReferencePointManager.ResolveCloudReferenceId(m_CloudReferenceId);
                if (m_CloudReferencePoint == null)
                {
                    OutputText.text = "Resolve Failed!";
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
            OutputText.text = m_AppMode.ToString();

            CloudReferenceState cloudReferenceState = m_CloudReferencePoint.cloudReferenceState;
            OutputText.text += " - " + cloudReferenceState.ToString();

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
            OutputText.text = "Resolve Failed!";
            m_AppMode = AppMode.TouchToHostCloudReferencePoint;
            return;
        }

        // Wait for the reference point to be ready.
        m_AppMode = AppMode.WaitingForResolvedReferencePoint;
    }
    #endregion Private Methods
}
