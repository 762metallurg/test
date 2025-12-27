using Unity.Cinemachine;
using System;
using UnityEngine;
using System.Reflection;

namespace MovementSystem
{
    [Serializable]
    public class PlayerCameraRecenteringUtility
    {
        [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
        [field: SerializeField] public float DefaultHorizontalWaitTime { get; private set; } = 0f;
        [field: SerializeField] public float DefaultHorizontalRecenteringTime { get; private set; } = 4f;

        private object _aimComponent;

        public void Initialize()
        {
            if (VirtualCamera == null)
                return;

            var component = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            _aimComponent = component;
        }

        public void EnableRecentering(float waitTime = -1f, float recenteringTime = -1f, float baseMovementSpeed = 1f, float movementSpeed = 1f)
        {
            if (_aimComponent == null)
                return;

            if (waitTime == -1f)
            {
                waitTime = DefaultHorizontalWaitTime;
            }

            if (recenteringTime == -1f)
            {
                recenteringTime = DefaultHorizontalRecenteringTime;
            }

            recenteringTime = recenteringTime * baseMovementSpeed / movementSpeed;

            var type = _aimComponent.GetType();
            var horizField = type.GetField("m_HorizontalRecentering", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (horizField == null)
                return;

            var horiz = horizField.GetValue(_aimComponent);
            if (horiz == null)
                return;

            var enabledField = horiz.GetType().GetField("m_enabled", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            enabledField?.SetValue(horiz, true);

            var cancelMethod = horiz.GetType().GetMethod("CancelRecentering", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            cancelMethod?.Invoke(horiz, null);

            var waitField = horiz.GetType().GetField("m_WaitTime", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            waitField?.SetValue(horiz, waitTime);

            var timeField = horiz.GetType().GetField("m_RecenteringTime", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            timeField?.SetValue(horiz, recenteringTime);
        }

        public void DisableRecentering()
        {
            if (_aimComponent == null)
                return;

            var type = _aimComponent.GetType();
            var horizField = type.GetField("m_HorizontalRecentering", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (horizField == null)
                return;

            var horiz = horizField.GetValue(_aimComponent);
            if (horiz == null)
                return;

            var enabledField = horiz.GetType().GetField("m_enabled", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            enabledField?.SetValue(horiz, false);
        }
    }
}