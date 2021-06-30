using UnityEngine;

public static class SelectionHandler
{
    public static GameObject SelectedGO;

    // when nothing is selected this is the previously SelectedGO
    // this is the same as SelectedGO when a product is selected
    public static GameObject latestSelectedGO;
}
