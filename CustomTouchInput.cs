using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomTouchInput : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private bool rotating = false;
    /// The squared rotation width determining an rotation
    public const float TOUCH_ROTATION_WIDTH = 1; // Always
    /// The threshold in angles which must be exceeded so a touch rotation is recognized as one
    public const float TOUCH_ROTATION_MINIMUM = 1;
    /// Start vector of the current rotation
    Vector2 startVector = Vector2.zero;

    private Vector2 touchBeganPos;
    private Vector2 touchCurrentPos;
    private Vector2 touchDrag1Pos;
    private Vector2 touchDrag2Pos;
    private float dragDistanceTreshold = 100f;
    private float tapCheckTimer;
    private bool isDragging;

    bool isOverUI;

    //[SerializeField] private Text debugRotateText;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            //is it one finger?
            if (Input.touchCount == 1)
            {
                Touch touchSingle = Input.GetTouch(0);
                // touch in pixel coordinates bottom-left (0, 0) top-right (Screen.width, Screen.height)
                touchCurrentPos = touchSingle.position;
                if (touchSingle.phase == TouchPhase.Canceled)
                    return;

                // drag?
                else if (touchSingle.phase == TouchPhase.Moved)
                {                    
                    if (!isDragging)
                    {
                        // has the drag distance passed a threshold?
                        // Debug.Log(("Drag distance sqr magnitude: " + (touchCurrentPos - touchBeganPos).sqrMagnitude));
                        if ((touchBeganPos - touchCurrentPos).sqrMagnitude > dragDistanceTreshold)
                        {
                            //Drag started event
                            if (!isOverUI)
                            {
                                EventBus.Instance.DragStart(touchCurrentPos);
                                isDragging = true;
                            }
                        }
                    }
                    else
                    {
                        // DRAG EVENT!
                        EventBus.Instance.Drag(touchCurrentPos);
                        //debugTouchText.text = "DRAGGED";
                        //Debug.Log("DRAGGED!!!!!!!!!!");
                    }                    
                }
                // tap?
                else if (touchSingle.phase == TouchPhase.Ended)
                {
                    // if timer was short => tap
                    if (tapCheckTimer < 0.2f)
                    {
                        // TAP EVENT!
                        
                        if (!isOverUI)
                        {
                            EventBus.Instance.Tap(touchCurrentPos);
                        }
                        //debugTouchText.text = "TAPPED";
                        //Debug.Log("TAPPED!!!!!!!!!!!!!!!!!");
                    }
                    else
                    {
                        // it was a drag => no touch event!
                        // DRAG ENDED EVENT!
                        EventBus.Instance.DragEnd(touchCurrentPos);
                    }

                    isDragging = false;
                }
                
                else if (touchSingle.phase == TouchPhase.Began)
                {
                    // cast against UI layer
                    //bool isOverUI = touchCurrentPos.IsPointerOverUI();
                    //  set touch timer to 0
                    isOverUI = touchCurrentPos.IsPointerOverUI();
                    tapCheckTimer = 0;
                    touchBeganPos = touchCurrentPos;
                    EventBus.Instance.TouchBegan(touchCurrentPos);
                }                

                tapCheckTimer += Time.deltaTime;
                
                //raycast at UI to see if UI is hit

                //raycast at objects (and plane?)
                //if object hit = select

                //raycast at plane
            }
            else if (Input.touchCount > 1)
            {
                //debugTouchText.text = "Touch count is 2";
                touchDrag1Pos = Input.touches[0].position;
                touchDrag2Pos = Input.touches[1].position;
                //avoid tap on twist release with one finger sending touch end
                tapCheckTimer = Mathf.Infinity;
                TwistGesture(touchDrag1Pos, touchDrag2Pos);
                return;
            }
        }

        rotating = false;
    }

    private void TwistGesture(Vector2 drag1Pos, Vector2 drag2Pos)
    {
        if (!rotating)
        {
            startVector = drag2Pos - drag1Pos;
            rotating = startVector.sqrMagnitude > TOUCH_ROTATION_WIDTH;
        }
        else
        {
            Vector2 currVector = drag2Pos - drag1Pos;
            float angleOffset = Vector2.Angle(startVector, currVector);

            if (angleOffset > TOUCH_ROTATION_MINIMUM)
            {
                Vector3 LR = Vector3.Cross(startVector, currVector); // z > 0 left rotation, z < 0 right rotation
                float clampedAngleOffset = Mathf.Clamp(angleOffset, 1f, 10f);
                //debugRotateText.text = "clampedAngleOffset: " + clampedAngleOffset;

                if (LR.z > 0)
                {
                    // rotate left (use some arbitrary tick value i guess?)
                    EventBus.Instance.RotateGesture(false, clampedAngleOffset);
                }    
                else if (LR.z < 0)
                {
                    EventBus.Instance.RotateGesture(true, clampedAngleOffset);
                    // rotate right (use some arbitrary tick value i guess?)
                }

                // might be wrong, makes no sense to keep resetting?
                startVector = currVector;
            }
        }
    }
}