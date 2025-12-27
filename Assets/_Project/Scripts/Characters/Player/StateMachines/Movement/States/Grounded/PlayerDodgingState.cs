using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementSystem
{
    public class PlayerDodgingState : PlayerGroundedState
    {
        private float _startTime;

        private int _consecutiveDashesUsed;

        private bool _shouldKeepRotating;

        public PlayerDodgingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            StateMachine.ReusableData.MovementSpeedModifier = GroundedData.DashData.SpeedModifier;

            base.Enter();

            StartAnimation(StateMachine.Player.AnimationData.DashParameterHash);

            StateMachine.ReusableData.CurrentJumpForce = AirborneData.JumpData.StrongForce;

            StateMachine.ReusableData.RotationData = GroundedData.DashData.RotationData;

            Dash();

            _shouldKeepRotating = StateMachine.ReusableData.MovementInput != Vector2.zero;

            UpdateConsecutiveDashes();

            _startTime = Time.time;
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(StateMachine.Player.AnimationData.DashParameterHash);

            SetBaseRotationData();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (!_shouldKeepRotating)
            {
                return;
            }

            RotateTowardsTargetRotation();
        }

        public override void OnAnimationTransitionEvent()
        {
            if (StateMachine.ReusableData.MovementInput == Vector2.zero)
            {
                StateMachine.ChangeState(StateMachine.HardStoppingState);

                return;
            }

            StateMachine.ChangeState(StateMachine.SprintingState);
        }

        protected override void AddInputActionsCallbacks()
        {
            base.AddInputActionsCallbacks();

            StateMachine.Player.Input.PlayerActions.Move.performed += OnMovementPerformed;

        }

        protected override void RemoveInputActionsCallbacks()
        {
            base.RemoveInputActionsCallbacks();

            StateMachine.Player.Input.PlayerActions.Move.performed -= OnMovementPerformed;
        }

        protected override void OnMovementPerformed(InputAction.CallbackContext context)
        {
            base.OnMovementPerformed(context);

            _shouldKeepRotating = true;
        }

        private void Dash()
        {
            Vector3 dashDirection = StateMachine.Player.transform.forward;

            dashDirection.y = 0f;

            UpdateTargetRotation(dashDirection, false);

            if (StateMachine.ReusableData.MovementInput != Vector2.zero)
            {
                UpdateTargetRotation(GetMovementInputDirection());

                dashDirection = GetTargetRotationDirection(StateMachine.ReusableData.CurrentTargetRotation.y);
            }

            StateMachine.Player.Rigidbody.linearVelocity = dashDirection * GetMovementSpeed(false);
        }

        private void UpdateConsecutiveDashes()
        {
            if (!IsConsecutive())
            {
                _consecutiveDashesUsed = 0;
            }

            ++_consecutiveDashesUsed;

            if (_consecutiveDashesUsed == GroundedData.DashData.ConsecutiveDashesLimitAmount)
            {
                _consecutiveDashesUsed = 0;

                StateMachine.Player.Input.DisableActionFor(StateMachine.Player.Input.PlayerActions.Dodge, GroundedData.DashData.DashLimitReachedCooldown);
            }
        }

        private bool IsConsecutive()
        {
            return Time.time < _startTime + GroundedData.DashData.TimeToBeConsideredConsecutive;
        }

        protected override void OnDashStarted(InputAction.CallbackContext context)
        {
        }
    }
}