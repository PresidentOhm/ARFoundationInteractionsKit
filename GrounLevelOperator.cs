using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class GrounLevelOperator : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [SerializeField] private CustomARRaycasting customARRaycasting;

    [SerializeField] private GameObject groundIndicator;

    [SerializeField] private GameObject shadowPlane;

    private (bool hasReading, float yLevel) currentGroundLevel;
    public (bool hasReading, float yLevel) CurrentGroundLevel { get => currentGroundLevel; set => currentGroundLevel = value; }

    private (bool isSet, float yLevel) selectedGroundLevel;
    public (bool isSet, float yLevel) SelectedGroundLevel { get => selectedGroundLevel; set => selectedGroundLevel = value; }

    private void Awake()
    {
        customARRaycasting.enabled = false;
        groundIndicator.SetActive(false);
        shadowPlane.SetActive(false);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void InitializeFindGroundLevel()
    {
        StartCoroutine(FindGroundLevel());
    }

    public void SetGroundLevel()
    {
        if (currentGroundLevel.hasReading)
        {
            shadowPlane.transform.position = groundIndicator.transform.position;
            shadowPlane.transform.rotation = groundIndicator.transform.rotation;
            StaticGround.YPosition = groundIndicator.transform.position.y;
            StaticGround.GroundPlane = new Plane(shadowPlane.transform.up, shadowPlane.transform.position);
            groundIndicator.SetActive(false);
            shadowPlane.SetActive(true);
            SelectedGroundLevel = (true, currentGroundLevel.yLevel);
            customARRaycasting.enabled = true;
            EventBus.Instance.GroundLevelSet(SelectedGroundLevel.yLevel);
        }
    }

    public void ClearGroundLevel()
    {
        StopAllCoroutines();
        groundIndicator.SetActive(false);
        SelectedGroundLevel = (false, 0);
        CurrentGroundLevel = (false, 0);
        StaticGround.YPosition = 0;
        StaticGround.GroundPlane.normal = Vector3.zero;
        EventBus.Instance.GroundLevelReset();
    }

    private IEnumerator FindGroundLevel()
    {
        while (SelectedGroundLevel.isSet == false)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

            List<ARRaycastHit> t_Hits = new List<ARRaycastHit>();
            if (GestureTransformationUtility.Raycast(screenCenter, t_Hits, TrackableType.PlaneWithinPolygon))
            {
                var trackableHit = t_Hits[0];

                //StaticGround.YPosition = trackableHit.pose.position.y;

                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if (Vector3.Dot(mainCamera.transform.position - trackableHit.pose.position,
                                trackableHit.pose.rotation * Vector3.up) < 0)
                    continue;

                if (!CurrentGroundLevel.hasReading)
                    EventBus.Instance.GroundLevelFound();

                CurrentGroundLevel = (true, trackableHit.pose.position.y);

                groundIndicator.transform.position = trackableHit.pose.position;
                groundIndicator.transform.rotation = trackableHit.pose.rotation;
                if (!groundIndicator.activeInHierarchy)
                {
                    groundIndicator.SetActive(true);
                }
            }
            yield return null;
        }
        yield return null;
    }
}