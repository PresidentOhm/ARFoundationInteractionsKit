using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Vector2Extensions
{
    public static bool IsPointerOverUI(this Vector2 pos)
    {
        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    if (EventSystem.current.game)
        //    {

        //    }
        //    return false;
        //}

        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(pos.x, pos.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.layer == 5) //5 = UI layer
            {
                return true;
            }
        }
        
        return false;

        //return results.Count > 0;
    }
}
