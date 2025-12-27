using UnityEngine;
using UnityEngine.Events;

public class DelayActivate : MonoBehaviour
{
    public UnityEvent Delay;

    public void Activate(float seconds)
    {
        Invoke(nameof(ActivateDelay), seconds);
    }

    private void ActivateDelay()
    {
        Delay.Invoke();
    }
}
