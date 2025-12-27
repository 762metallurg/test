using UnityEngine;
using UnityEngine.Events;

public class AnimationEvent : MonoBehaviour
{
    public UnityEvent animationEvent;
    public UnityEvent animationEventEnd;
    public UnityEvent animationEventStart;

    public void OnAnimationEvent()
    {
        animationEvent.Invoke();
    }

    public void OnAnimationStartEvent()
    {
        animationEventStart.Invoke();
    }

    public void OnAnimationEndEvent()
    {
        animationEventEnd.Invoke();
    }
}
