using UnityEngine;
using System;

public class TimerUI : MonoBehaviour
{
    public float minSwipeSpeed = 40.0f;

    public event EventHandler<SwipeArgs> SwipeEvent;
    
    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
            }
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 swipeDelta = touch.deltaPosition;

                if (swipeDelta.magnitude > minSwipeSpeed)
                {
                    SwipDirection dir;

                    if (Math.Abs(swipeDelta.x) > Math.Abs(swipeDelta.y))
                    {
                        if (Math.Sign(swipeDelta.x) > 0)
                        {
                            dir = SwipDirection.Right;
                        }
                        else
                        {
                            dir = SwipDirection.Left;
                        }
                    }
                    else
                    {
                        if (Math.Sign(swipeDelta.y) > 0)
                        {
                            dir = SwipDirection.Up;
                        }
                        else
                        {
                            dir = SwipDirection.Down;
                        }
                    }

                    OnSwipeEvent(new SwipeArgs(dir));
                }
            }
        }
    }
    
    protected virtual void OnSwipeEvent(SwipeArgs e)
    {
        EventHandler<SwipeArgs> handler = SwipeEvent;
        
        if (handler != null)
        {
            handler(this, e);
        }
    }
}

public class SwipeArgs : EventArgs
{
    private SwipDirection m_swipDirection;
    public SwipDirection SwipDirection
    {
        get { return m_swipDirection; }
    }

    public SwipeArgs(SwipDirection swipDirection)
    {
        m_swipDirection = swipDirection;
    }
}

public enum SwipDirection
{
    Up,
    Down,
    Left,
    Right,
}
