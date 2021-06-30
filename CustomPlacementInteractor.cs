using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// <see cref="UnityEvent"/> that responds to changes of hover and selection by this interactor.
    /// </summary>
    [Serializable] public class ARObjectPlacedEvent : UnityEvent<CustomPlacementInteractor, GameObject> { }

    /// <summary>
    /// Controls the placement of Andy objects via a tap gesture.
    /// </summary>
    public class CustomPlacementInteractor : ARBaseGestureInteractable
    {
        [SerializeField] private SnapSwipeMenu snapSwipeMenu;
        [SerializeField] private GameObject selectedGO;
        [SerializeField] private GameObject shadowPlaneGO;
        public GameObject SelectedGO
        {
            get => selectedGO; 
            set
            {
                selectedGO = value;
                OnSelection?.Invoke(selectedGO);
            }
        }
        private List<GameObject> pooledGOs = new List<GameObject>();
        [SerializeField] private PlacedPrefabSelector prefabSelector;
        public delegate void TouchHandler();
        public event TouchHandler OnScreenTouched;

        [SerializeField] private List<GameObject> spawnedGOs = new List<GameObject>();
        private Vector3 initialYPos;

        public delegate void SelectCallback(GameObject selectGO);
        public event SelectCallback OnSelection;

        [SerializeField]
        [Tooltip("A GameObject to place when a raycast from a user touch hits a plane.")]
        private GameObject m_PlacementPrefab;
        private bool shadowPrefabAdded;
        /// <summary>
        /// A <see cref="GameObject"/> to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject PlacementPrefab
        {
            get => m_PlacementPrefab;
            set => m_PlacementPrefab = value;
        }
        //Ray GenerateMouseRay()
        //{
        //    Vector3 mousePosFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);
        //    Vector3 mousePosNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        //    Vector3 mousePosF = Camera.main.ScreenToWorldPoint(mousePosFar); Vector3 mousePosN = Camera.main.ScreenToWorldPoint(mousePosNear);
        //    Ray ar = new Ray(mousePosN, mousePosF - mousePosN);
        //    return ar;
        //}
        
        [SerializeField, Tooltip("Called when the this interactable places a new GameObject in the world.")]
        ARObjectPlacedEvent m_OnObjectPlaced = new ARObjectPlacedEvent();

        /// <summary>
        /// The event that is called when the this interactable places a new <see cref="GameObject"/> in the world.
        /// </summary>
        public ARObjectPlacedEvent OnObjectPlaced
        {
            get => m_OnObjectPlaced;
            set => m_OnObjectPlaced = value;
        }
        public bool OnSelectOnce { get; private set; }
        public bool GestureSelected { get; private set; }

        static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        static GameObject s_TrackablesObject;
        private GameObject m_InfinitePlane;
        private Plane objPlane;

        private void OnEnable()
        {
            prefabSelector.OnSwitchPrefab += SwitchSelectedGO;
        }

        private void OnDisable()
        {
            prefabSelector.OnSwitchPrefab -= SwitchSelectedGO;
        }

        //public void HideAllModels()
        //{
        //    foreach (var go in spawnedGOs)
        //    {
        //        DestroyImmediate(go);
        //        //go.SetActive(false);
        //    }

        //    spawnedGOs = new List<GameObject>();
        //}
                

        public void PlacePrefab(Vector3 pos, Quaternion rot)
        {
            PlacementPrefab = snapSwipeMenu.CurrentSnapPrefab;

            if (!PlacementPrefab)
                return;            

            if (StaticGround.GroundPlane.normal == Vector3.zero)
            {
                //shadowPrefabAdded = true;
                shadowPlaneGO.transform.position = pos;
                StaticGround.YPosition = pos.y;
                StaticGround.GroundPlane = new Plane(shadowPlaneGO.transform.up, shadowPlaneGO.transform.position);
                shadowPlaneGO.SetActive(true);
                shadowPlaneGO.AddComponent<ARAnchor>();
            }

            //create anchor
            var anchorObject = new GameObject("PlacementAnchor");
            anchorObject.transform.position = new Vector3(pos.x, StaticGround.YPosition, pos.z);
            anchorObject.transform.rotation = rot;

            // Find trackables object in scene and use that as parent
            if (s_TrackablesObject == null)
                s_TrackablesObject = GameObject.Find("Trackables");
            if (s_TrackablesObject != null)
                anchorObject.transform.parent = s_TrackablesObject.transform;

            anchorObject.AddComponent<ARAnchor>();

            GameObject placementObject = Instantiate(PlacementPrefab, pos, rot, anchorObject.transform);

            //spawnedGOs.Add(placementObject);
            SelectionHandler.SelectedGO = placementObject;
            EventBus.Instance.SelectGO(SelectionHandler.SelectedGO);

            if (SelectionHandler.latestSelectedGO)
                EventBus.Instance.DeselectGO(SelectionHandler.latestSelectedGO);

            //parent the placement object to "PlacementAnchor"
            //placementObject.transform.parent = anchorObject.transform;

            m_OnObjectPlaced?.Invoke(this, placementObject);
        }
        
        //private void PlaceGroundIndicator()
        //{

        //}


        private void SwitchSelectedGO(GameObject placementGO)
        {
            m_PlacementPrefab = placementGO;

            if (SelectionHandler.SelectedGO == null)
            {
                //spawnedObject = Instantiate(m_PlacedPrefab, Vector3.zero, Quaternion.identity);
                //spawnedObject.SetActive(false);
                return;
            }
            GameObject parentAnchorGO = SelectionHandler.SelectedGO.transform.parent.gameObject;
            Vector3 currentPos = SelectionHandler.SelectedGO.transform.position;
            Quaternion currentRot = SelectionHandler.SelectedGO.transform.rotation;
            //GameObject foundGO = pooledGOs.Find(x => x.name == m_PlacementPrefab.name + "(Clone)");
            //SelectionHandler.SelectedGO.SetActive(false);
            foreach (Transform child in parentAnchorGO.transform)
            {
                Destroy(child.gameObject);
            }
               // Destroy(SelectionHandler.SelectedGO);
            //if (foundGO && !foundGO.activeInHierarchy)
            //{
            //    foundGO.transform.position = currentPos;
            //    foundGO.transform.rotation = currentRot;
            //    foundGO.SetActive(true);
            //    SelectionHandler.SelectedGO = foundGO;

            //    EventBus.Instance.SelectGO(SelectionHandler.SelectedGO);
            //    if (SelectionHandler.latestSelectedGO)
            //        EventBus.Instance.DeselectGO(SelectionHandler.latestSelectedGO);
            //}
            //else
            //{
                SelectionHandler.SelectedGO = Instantiate(m_PlacementPrefab, currentPos, currentRot);
                SelectionHandler.SelectedGO.transform.parent = parentAnchorGO.transform;
                SelectionHandler.SelectedGO.SetActive(true);
                //pooledGOs.Add(SelectionHandler.SelectedGO);

                EventBus.Instance.SelectGO(SelectionHandler.SelectedGO);
                if (SelectionHandler.latestSelectedGO)
                    EventBus.Instance.DeselectGO(SelectionHandler.latestSelectedGO);
            //}
        }

    }
}