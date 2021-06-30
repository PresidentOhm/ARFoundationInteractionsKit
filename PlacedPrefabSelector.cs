using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;


// Builds buttons and updates shown buttons according to if prefab model tag = passed string tag parameter
public class PlacedPrefabSelector : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;

    [SerializeField] private List<ModelImageCombo> models = new List<ModelImageCombo>();
    [SerializeField] private List<GameObject> currentModelPrefabs = new List<GameObject>();
    public List<GameObject> CurrentModelPrefabs { get => currentModelPrefabs; set => currentModelPrefabs = value; }

    //[SerializeField] private PlaceOnPlane placeOnPlane;

    private List<GameObject> buttons = new List<GameObject>();

    public List<ModelImageCombo> Models { get => models; set => models = value; }

    public delegate void SwitchPrefabHandler(GameObject placementGO);
    public event SwitchPrefabHandler OnSwitchPrefab;

    [SerializeField] private GameObject inactiveButtonsParent;
    [SerializeField] private SnapSwipeMenu snapSwipeMenu;

    private ARSessionOrigin aRSessionOrigin;

    private void Awake()
    {
        for (int i = 0; i < Models.Count; i++)
        {
            GameObject newButtonGO = Instantiate(buttonPrefab, transform);
            newButtonGO.GetComponent<Button>().image.sprite = Models[i].modelThumbnail;
            buttons.Add(newButtonGO);
            GameObject currentModel = Models[i].modelPrefab;
            newButtonGO.GetComponent<Button>().onClick.AddListener(() => SwitchModel(currentModel));
            newButtonGO.SetActive(false);
            //newButtonGO.GetComponent<Button>().onClick.AddListener(delegate { SwitchModel(models[i].modelPrefab); });
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.SetParent(inactiveButtonsParent.transform, false);
        }
    }

    public void UpdateButtons(string tag)
    {
        // change color palette by family selection also
        snapSwipeMenu.StopSnapMenu();

        CurrentModelPrefabs.Clear();
        for (int i = 0; i < buttons.Count; i++)
        {
            if (Models[i].modelPrefab.tag.ToLower() == tag.ToLower())
            {
                CurrentModelPrefabs.Add(Models[i].modelPrefab);
                buttons[i].transform.SetParent(this.transform, false);
                buttons[i].SetActive(true);
            }
            else
            {
                if (CurrentModelPrefabs.Contains(Models[i].modelPrefab))
                    CurrentModelPrefabs.Remove(Models[i].modelPrefab);
                buttons[i].transform.SetParent(inactiveButtonsParent.transform, false);
                buttons[i].SetActive(false);
            }
        }

        //StartCoroutine(StartMenuDelay());
        snapSwipeMenu.StartSnapMenu();
    }

    private IEnumerator StartMenuDelay()
    {
        yield return new WaitForSeconds(1);
        snapSwipeMenu.StartSnapMenu();
    }

    private void SwitchModel(GameObject model)
    {               
        //placeOnPlane.placedPrefab = model;
        //placeOnPlane.spawnedObject = model;
        OnSwitchPrefab?.Invoke(model);
    }
}


[System.Serializable]
public struct ModelImageCombo
{
    public GameObject modelPrefab;
    public Sprite modelThumbnail; 
}

//public struct GOModelSpriteCombo
//{
//    public GameObject button;
//    public GameObject modelPrefab;
//    public Sprite sprite;
//}
