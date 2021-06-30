using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductHighlighter : MonoBehaviour
{
    [SerializeField] private List<GameObject> highlighter = new List<GameObject>();


    private void OnEnable()
    {
        highlighter.ForEach(x => x.SetActive(false));
        EventBus.Instance.OnSelectGO += Instance_OnSelectGO;
        EventBus.Instance.OnDeselectGO += Instance_OnDeselectGO;
    }

    private void OnDisable()
    {
        EventBus.Instance.OnSelectGO -= Instance_OnSelectGO;
        EventBus.Instance.OnDeselectGO -= Instance_OnDeselectGO;
    }

    private void Instance_OnSelectGO(GameObject go)
    {
        if (go == gameObject)
            highlighter.ForEach(x => x.SetActive(true));
        else
            highlighter.ForEach(x => x.SetActive(false));
    }
    private void Instance_OnDeselectGO(GameObject go)
    {
        if (go == gameObject)
            highlighter.ForEach(x => x.SetActive(false));
    }

}
