using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class CustomARRaycasting : MonoBehaviour
{
    [SerializeField] private GameObject shadowPlaneGO;

    [SerializeField] private Camera mainCamera;

    [SerializeField] private CustomTouchInput customTouchInput;
    [SerializeField] private CustomPlacementInteractor customPlacementInteractor;

    [SerializeField] private ARRaycastManager m_RaycastManager;
    private List<ARRaycastHit> trackable_Hits = new List<ARRaycastHit>();


    int objectsLayerMask;

    //[SerializeField] private Text debugSelectedGOText;
    private bool isFirstDrag;
    private bool isDragingGO;

    private bool isGroundLevelSet;

    void OnEnable()
    {
        objectsLayerMask = LayerMask.GetMask("Objects");
        EventBus.Instance.OnTap += CustomTouchInput_OnTap;
        EventBus.Instance.OnRotateGesture += CustomTouchInput_OnRotateGesture;
        EventBus.Instance.OnDragStart += CustomTouchInput_OnDragStart;
        EventBus.Instance.OnDragEnd += CustomTouchInput_OnDragEnd;
        EventBus.Instance.OnDrag += CustomTouchInput_OnDrag;
    }

    private void OnDisable()
    {
        EventBus.Instance.OnTap -= CustomTouchInput_OnTap;
        EventBus.Instance.OnRotateGesture -= CustomTouchInput_OnRotateGesture;
        EventBus.Instance.OnDragStart -= CustomTouchInput_OnDragStart;
        EventBus.Instance.OnDragEnd -= CustomTouchInput_OnDragEnd;
        EventBus.Instance.OnDrag -= CustomTouchInput_OnDrag;
    }

    private void CustomTouchInput_OnDragEnd(Vector2 touchPos)
    {
        isDragingGO = false;
        isFirstDrag = false;
        CustomTouchInput_OnDrag(touchPos);
    }

    private void CustomTouchInput_OnDragStart(Vector2 touchPos)
    {
        //Debug.Log("Drag began");
        //Debug.Log("StaticGround.GroundPlane: " + StaticGround.GroundPlane);
        isFirstDrag = true;
        isDragingGO = false;
        CustomTouchInput_OnDrag(touchPos);
    }

    private void CustomTouchInput_OnDrag(Vector2 touchPos)
    {
        if (SelectionHandler.SelectedGO == null)
            return;

        if (isFirstDrag)
        {
            // cast against UI layer
            bool isOverUI = touchPos.IsPointerOverUI();
            if (isOverUI)
                return;
        }

        Ray ray = mainCamera.ScreenPointToRay(touchPos);
        if (isFirstDrag)
        {
            isFirstDrag = false;
            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                if (hit.collider.gameObject.layer == 10 && hit.collider.gameObject == SelectionHandler.SelectedGO)
                {
                    isDragingGO = true;
                }
                else
                {
                    isDragingGO = false;
                }
            }
        }

        if (isDragingGO)
        {
            float distance; // the distance from the ray origin to the ray intersection of the plane
            if (StaticGround.GroundPlane.Raycast(ray, out distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                EventBus.Instance.GameObjectDrag(worldPos);
                EventBus.Instance.GameObjectDragInputCallbackGo(SelectionHandler.SelectedGO);
                SelectionHandler.SelectedGO.transform.position = worldPos; // distance along the ray
            }
        }
        //from before when drag was used for rotating
        //selectedGO.transform.Rotate(Vector3.up, 1);
    }


    private void CustomTouchInput_OnRotateGesture(bool isRight, float speed)
    {
        if (SelectionHandler.SelectedGO == null)
            return;

        if (isRight)
            SelectionHandler.SelectedGO.transform.Rotate(Vector3.up, 1f * speed);
        else
        {
            SelectionHandler.SelectedGO.transform.Rotate(Vector3.up, -1f * speed);
        }
    }

    private void CustomTouchInput_OnTap(Vector2 touchPos)
    {
        CastARRaycastOnTap(touchPos);
    }

    private void CastARRaycastOnTap(Vector2 touchPos)
    {
        // cast against UI layer
        bool isOverUI = touchPos.IsPointerOverUI();
        if (isOverUI)
            return;

        // cast against 3d-objects     
        Ray ray = mainCamera.ScreenPointToRay(touchPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, objectsLayerMask))
        {
            //Debug.Log("SelectionHandler.SelectedGO: " + SelectionHandler.SelectedGO);
            //Debug.Log("SelectionHandler.latestSelectedGO: " + SelectionHandler.latestSelectedGO);

            //only temporary to avoid snap trigger colliders!
            if (!hit.collider.gameObject.GetComponent<ProductID>())
                return;

            if (SelectionHandler.SelectedGO == hit.collider.gameObject)
            {
                //Debug.Log("SelectionHandler: deselected: " + SelectionHandler.SelectedGO);
                SelectionHandler.latestSelectedGO = SelectionHandler.SelectedGO;
                SelectionHandler.SelectedGO = null;
                //Debug.Log("After deselect : SelectionHandler.SelectedGO: " + SelectionHandler.SelectedGO);
                EventBus.Instance.DeselectGO(SelectionHandler.latestSelectedGO);
            }
            else
            {
                SelectionHandler.SelectedGO = hit.collider.gameObject;
                if (SelectionHandler.latestSelectedGO)
                {
                    EventBus.Instance.DeselectGO(SelectionHandler.latestSelectedGO);
                    //Debug.Log("SelectionHandler: deselected (latestSelectedGO): " + SelectionHandler.latestSelectedGO);
                }

                SelectionHandler.latestSelectedGO = SelectionHandler.SelectedGO;
                //debugSelectedGOText.text = selectedGO.name;
                //Debug.Log("SelectionHandler: selected: " + SelectionHandler.SelectedGO);

                EventBus.Instance.SelectGO(SelectionHandler.SelectedGO);
            }
            return;
        }

        //non initialized Plane struct normal is Vector3.zero
        if (StaticGround.GroundPlane.normal != Vector3.zero)
        {
            StaticGround.YPosition = shadowPlaneGO.transform.position.y;
            StaticGround.GroundPlane = new Plane(shadowPlaneGO.transform.up, shadowPlaneGO.transform.position);
            float distance; // the distance from the ray origin to the ray intersection of the plane
            if (StaticGround.GroundPlane.Raycast(ray, out distance))
            {
                //read what object to place from placement script
                //SelectionHandler.SelectedGO.transform.position = ray.GetPoint(distance); // distance along the ray
                Debug.Log("run customPlacementInteractor.PlacePrefab after plane raycast");
                customPlacementInteractor.PlacePrefab(ray.GetPoint(distance), Quaternion.identity);
            }
        }
        // used for finding initial ground level in AR
        else
        {
            List<ARRaycastHit> t_Hits = new List<ARRaycastHit>();
            if (GestureTransformationUtility.Raycast(touchPos, t_Hits, TrackableType.PlaneWithinPolygon))
            {
                var trackableHit = t_Hits[0];

                //StaticGround.YPosition = trackableHit.pose.position.y;

                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if (Vector3.Dot(mainCamera.transform.position - trackableHit.pose.position,
                                trackableHit.pose.rotation * Vector3.up) < 0)
                    return;

                customPlacementInteractor.PlacePrefab(trackableHit.pose.position, trackableHit.pose.rotation);
            }
        }

        

        //    // special ar raycast (hits non physics AR TRACKABLES)
        //    // Only returns true if there is at least one hit        
        //    if (m_RaycastManager.Raycast(touchPos, trackable_Hits))
        //{
        //    // cast against plane?
        //    if (trackable_Hits[0].hitType == UnityEngine.XR.ARSubsystems.TrackableType.Planes)
        //    {
        //        // plane was hit
        //        StaticGround.YPosition = trackable_Hits[0].position.y;
        //        // this is handled by CustomPlacementInteractor
        //        // TODO raise event with hit.pose here instead
        //        return;
        //    }
        //}
    }
}