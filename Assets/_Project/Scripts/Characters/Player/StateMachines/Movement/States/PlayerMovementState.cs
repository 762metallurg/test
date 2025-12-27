using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementSystem
{
    public class PlayerMovementState : IState
    {
        public bool IsWeaponDrawn = false;
        protected PlayerMovementStateMachine StateMachine;

        protected readonly PlayerGroundedData GroundedData;
        protected readonly PlayerAirborneData AirborneData;

        public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
        {
            StateMachine = playerMovementStateMachine;

            GroundedData = StateMachine.Player.Data.GroundedData;
            AirborneData = StateMachine.Player.Data.AirborneData;

            InitializeData();
        }

        public virtual void Enter()
        {
            AddInputActionsCallbacks();
        }

        public virtual void Exit()
        {
            RemoveInputActionsCallbacks();
        }

        public virtual void HandleInput()
        {
            ReadMovementInput();
        }

        public virtual void Update()
        {
        }

        public virtual void PhysicsUpdate()
        {
            Move();
        }

        public virtual void OnTriggerEnter(Collider collider)
        {
            if (StateMachine.Player.LayerData.IsGroundLayer(collider.gameObject.layer))
            {
                OnContactWithGround(collider);

                return;
            }
        }

        public virtual void OnTriggerExit(Collider collider)
        {
            if (StateMachine.Player.LayerData.IsGroundLayer(collider.gameObject.layer))
            {
                OnContactWithGroundExited(collider);

                return;
            }
        }

        public virtual void OnAnimationEnterEvent()
        {
        }

        public virtual void OnAnimationExitEvent()
        {
        }

        public virtual void OnAnimationTransitionEvent()
        {
        }

        private void InitializeData()
        {
            SetBaseCameraRecenteringData();

            SetBaseRotationData();
        }

        protected void SetBaseCameraRecenteringData()
        {
            StateMachine.ReusableData.SidewaysCameraRecenteringData = GroundedData.SidewaysCameraRecenteringData;
            StateMachine.ReusableData.BackwardsCameraRecenteringData = GroundedData.BackwardsCameraRecenteringData;
        }

        protected void SetBaseRotationData()
        {
            StateMachine.ReusableData.RotationData = GroundedData.BaseRotationData;

            StateMachine.ReusableData.TimeToReachTargetRotation = StateMachine.ReusableData.RotationData.TargetRotationReachTime;
        }

        protected void StartAnimation(int animationHash)
        {
            StateMachine.Player.Animator.SetBool(animationHash, true);
        }

        protected void StartAnimation(int hash, int value)
        {
            StateMachine.Player.Animator.SetInteger(hash, value);
        }

        protected void StopAnimation(int animationHash)
        {
            StateMachine.Player.Animator.SetBool(animationHash, false);
        }

        protected virtual void AddInputActionsCallbacks()
        {
            StateMachine.Player.Input.PlayerActions.WalkToggle.started += OnWalkToggleStarted;

            StateMachine.Player.Input.PlayerActions.Look.started += OnMouseMovementStarted;

            StateMachine.Player.Input.PlayerActions.Move.performed += OnMovementPerformed;
            StateMachine.Player.Input.PlayerActions.Move.canceled += OnMovementCanceled;
        }

        protected virtual void RemoveInputActionsCallbacks()
        {
            StateMachine.Player.Input.PlayerActions.WalkToggle.started -= OnWalkToggleStarted;

            StateMachine.Player.Input.PlayerActions.Look.started -= OnMouseMovementStarted;
            
            StateMachine.Player.Input.PlayerActions.Move.performed -= OnMovementPerformed;
            StateMachine.Player.Input.PlayerActions.Move.canceled -= OnMovementCanceled;
        }

        protected virtual void OnWalkToggleStarted(InputAction.CallbackContext context)
        {
            StateMachine.ReusableData.ShouldWalk = !StateMachine.ReusableData.ShouldWalk;
        }

        private void OnMouseMovementStarted(InputAction.CallbackContext context)
        {
            UpdateCameraRecenteringState(StateMachine.ReusableData.MovementInput);
        }

        protected virtual void OnMovementPerformed(InputAction.CallbackContext context)
        {
            UpdateCameraRecenteringState(context.ReadValue<Vector2>());
        }

        protected virtual void OnMovementCanceled(InputAction.CallbackContext context)
        {
            DisableCameraRecentering();
        }

        private void ReadMovementInput()
        {
            StateMachine.ReusableData.MovementInput = StateMachine.Player.Input.PlayerActions.Move.ReadValue<Vector2>();
        }

        private void Move()
        {
            if (StateMachine.ReusableData.MovementInput == Vector2.zero || StateMachine.ReusableData.MovementSpeedModifier == 0f)
            {
                return;
            }

            Vector3 movementDirection = GetMovementInputDirection();

            float targetRotationYAngle = Rotate(movementDirection);

            Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle);

            float movementSpeed = GetMovementSpeed();

            Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();

            StateMachine.Player.Rigidbody.AddForce(targetRotationDirection * movementSpeed - currentPlayerHorizontalVelocity, ForceMode.VelocityChange);
        }

        protected Vector3 GetMovementInputDirection()
        {
            return new Vector3(StateMachine.ReusableData.MovementInput.x, 0f, StateMachine.ReusableData.MovementInput.y);
        }

        private float Rotate(Vector3 direction)
        {
            float directionAngle = UpdateTargetRotation(direction);

            RotateTowardsTargetRotation();

            return directionAngle;
        }

        protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
        {
            float directionAngle = GetDirectionAngle(direction);

            if (shouldConsiderCameraRotation)
            {
                directionAngle = AddCameraRotationToAngle(directionAngle);
            }

            if (directionAngle != StateMachine.ReusableData.CurrentTargetRotation.y)
            {
                UpdateTargetRotationData(directionAngle);
            }

            return directionAngle;
        }

        private float GetDirectionAngle(Vector3 direction)
        {
            float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            if (directionAngle < 0f)
            {
                directionAngle += 360f;
            }

            return directionAngle;
        }

        private float AddCameraRotationToAngle(float angle)
        {
            angle += StateMachine.Player.MainCameraTransform.eulerAngles.y;

            if (angle > 360f)
            {
                angle -= 360f;
            }

            return angle;
        }

        private void UpdateTargetRotationData(float targetAngle)
        {
            StateMachine.ReusableData.CurrentTargetRotation.y = targetAngle;

            StateMachine.ReusableData.DampedTargetRotationPassedTime.y = 0f;
        }

        protected void RotateTowardsTargetRotation()
        {
            float currentYAngle = StateMachine.Player.Rigidbody.rotation.eulerAngles.y;

            if (currentYAngle == StateMachine.ReusableData.CurrentTargetRotation.y)
            {
                return;
            }

            float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, StateMachine.ReusableData.CurrentTargetRotation.y, ref StateMachine.ReusableData.DampedTargetRotationCurrentVelocity.y, StateMachine.ReusableData.TimeToReachTargetRotation.y - StateMachine.ReusableData.DampedTargetRotationPassedTime.y);

            StateMachine.ReusableData.DampedTargetRotationPassedTime.y += Time.deltaTime;

            Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

            StateMachine.Player.Rigidbody.MoveRotation(targetRotation);
        }

        protected Vector3 GetTargetRotationDirection(float targetRotationAngle)
        {
            return Quaternion.Euler(0f, targetRotationAngle, 0f) * Vector3.forward;
        }

        protected float GetMovementSpeed(bool shouldConsiderSlopes = true)
        {
            float movementSpeed = GroundedData.BaseSpeed * StateMachine.ReusableData.MovementSpeedModifier;

            if (shouldConsiderSlopes)
            {
                movementSpeed *= StateMachine.ReusableData.MovementOnSlopesSpeedModifier;
            }

            return movementSpeed;
        }

        protected Vector3 GetPlayerHorizontalVelocity()
        {
            Vector3 playerHorizontalVelocity = StateMachine.Player.Rigidbody.linearVelocity;

            playerHorizontalVelocity.y = 0f;

            return playerHorizontalVelocity;
        }

        protected Vector3 GetPlayerVerticalVelocity()
        {
            return new Vector3(0f, StateMachine.Player.Rigidbody.linearVelocity.y, 0f);
        }

        protected virtual void OnContactWithGround(Collider collider)
        {
        }

        protected virtual void OnContactWithGroundExited(Collider collider)
        {
        }

        protected void UpdateCameraRecenteringState(Vector2 movementInput)
        {
            if (movementInput == Vector2.zero)
            {
                return;
            }

            if (movementInput == Vector2.up)
            {
                DisableCameraRecentering();

                return;
            }

            float cameraVerticalAngle = StateMachine.Player.MainCameraTransform.eulerAngles.x;

            if (cameraVerticalAngle >= 270f)
            {
                cameraVerticalAngle -= 360f;
            }

            cameraVerticalAngle = Mathf.Abs(cameraVerticalAngle);

            if (movementInput == Vector2.down)
            {
                SetCameraRecenteringState(cameraVerticalAngle, StateMachine.ReusableData.BackwardsCameraRecenteringData);

                return;
            }

            SetCameraRecenteringState(cameraVerticalAngle, StateMachine.ReusableData.SidewaysCameraRecenteringData);
        }

        protected void SetCameraRecenteringState(float cameraVerticalAngle, List<PlayerCameraRecenteringData> cameraRecenteringData)
        {
            foreach (PlayerCameraRecenteringData recenteringData in cameraRecenteringData)
            {
                if (!recenteringData.IsWithinRange(cameraVerticalAngle))
                {
                    continue;
                }

                EnableCameraRecentering(recenteringData.WaitTime, recenteringData.RecenteringTime);

                return;
            }

            DisableCameraRecentering();
        }

        protected void EnableCameraRecentering(float waitTime = -1f, float recenteringTime = -1f)
        {
            float movementSpeed = GetMovementSpeed();

            if (movementSpeed == 0f)
            {
                movementSpeed = GroundedData.BaseSpeed;
            }

            StateMachine.Player.CameraRecenteringUtility.EnableRecentering(waitTime, recenteringTime, GroundedData.BaseSpeed, movementSpeed);
        }

        protected void DisableCameraRecentering()
        {
            StateMachine.Player.CameraRecenteringUtility.DisableRecentering();
        }

        protected void ResetVelocity()
        {
            StateMachine.Player.Rigidbody.linearVelocity = Vector3.zero;
        }

        protected void ResetVerticalVelocity()
        {
            Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

            StateMachine.Player.Rigidbody.linearVelocity = playerHorizontalVelocity;
        }

        protected void DecelerateHorizontally()
        {
            Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

            StateMachine.Player.Rigidbody.AddForce(-playerHorizontalVelocity * StateMachine.ReusableData.MovementDecelerationForce, ForceMode.Acceleration);
        }

        protected void DecelerateVertically()
        {
            Vector3 playerVerticalVelocity = GetPlayerVerticalVelocity();

            StateMachine.Player.Rigidbody.AddForce(-playerVerticalVelocity * StateMachine.ReusableData.MovementDecelerationForce, ForceMode.Acceleration);
        }

        protected bool IsMovingHorizontally(float minimumMagnitude = 0.1f)
        {
            Vector3 playerHorizontaVelocity = GetPlayerHorizontalVelocity();

            Vector2 playerHorizontalMovement = new Vector2(playerHorizontaVelocity.x, playerHorizontaVelocity.z);

            return playerHorizontalMovement.magnitude > minimumMagnitude;
        }

        protected bool IsMovingUp(float minimumVelocity = 0.1f)
        {
            return GetPlayerVerticalVelocity().y > minimumVelocity;
        }

        protected bool IsMovingDown(float minimumVelocity = 0.1f)
        {
            return GetPlayerVerticalVelocity().y < -minimumVelocity;
        }
    }
}