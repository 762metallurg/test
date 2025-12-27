using UnityEngine;
using UnityEngine.Events;

public class UnityEvents : MonoBehaviour
{
    public UnityEvent OnAwakeEvent;
    public UnityEvent OnStartEvent;
    public UnityEvent OnEnableEvent;
    public UnityEvent OnDisableEvent;
    public UnityEvent OnDestroyEvent;

    private void Awake()
    {
        OnAwakeEvent.Invoke();
    }

    private void Start()
    {
        OnStartEvent.Invoke();
    }

    private void OnEnable()
    {
        OnEnableEvent.Invoke();
    }

    private void OnDisable()
    {
        OnDisableEvent.Invoke();
    }

    private void OnDestroy()
    {
        OnDestroyEvent.Invoke();
    }
}
