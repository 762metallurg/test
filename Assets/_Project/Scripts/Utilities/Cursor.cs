using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private bool _setCursorVisibleOnStart;
    private void Start()
    {
        SetCursorState(_setCursorVisibleOnStart);
    }

    public void SetCursorState(bool isEnabled)
    {
        UnityEngine.Cursor.visible = isEnabled;
        if (isEnabled)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
    }

}
