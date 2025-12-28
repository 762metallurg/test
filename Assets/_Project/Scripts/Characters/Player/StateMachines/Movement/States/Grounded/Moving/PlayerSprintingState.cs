using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementSystem
{
    public class PlayerSprintingState : PlayerMovingState
    {
        private float _startTime;

        private bool _keepSprinting;
        private bool _shouldResetSprintState;

        public PlayerSprintingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            StateMachine.ReusableData.MovementSpeedModifier = GroundedData.SprintData.SpeedModifier;

            base.Enter();

            StartAnimation(StateMachine.Player.AnimationData.SprintParameterHash);

            StateMachine.ReusableData.CurrentJumpForce = AirborneData.JumpData.StrongForce;

            _startTime = Time.time;

            _shouldResetSprintState = true;

            if (!StateMachine.ReusableData.ShouldSprint)
            {
                _keepSprinting = false;
            }
            Debug.Log("Entered Sprinting State");
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(StateMachine.Player.AnimationData.SprintParameterHash);

            if (_shouldResetSprintState)
            {
                _keepSprinting = false;

                StateMachine.ReusableData.ShouldSprint = false;
            }
            Debug.Log("Exited Sprinting State");
        }

        public override void Update()
        {
            base.Update();

            if (_keepSprinting)
            {
                return;
            }

            if (Time.time < _startTime + GroundedData.SprintData.SprintToRunTime)
            {
                return;
            }

            StopSprinting();
        }

        private void StopSprinting()
        {
            if (StateMachine.ReusableData.MovementInput == Vector2.zero)
            {
                StateMachine.ChangeState(StateMachine.IdlingState);

                return;
            }

            StateMachine.ChangeState(StateMachine.RunningState);
        }

        protected override void AddInputActionsCallbacks()
        {
            base.AddInputActionsCallbacks();
            Debug.Log("Adding Sprint Callback");
            StateMachine.Player.Input.PlayerActions.Sprint.performed += OnSprintPerformed;
        }

        protected override void RemoveInputActionsCallbacks()
        {
            base.RemoveInputActionsCallbacks();

            StateMachine.Player.Input.PlayerActions.Sprint.performed -= OnSprintPerformed;
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            _keepSprinting = true;
            Debug.Log("Sprinting Performed");
            StateMachine.ReusableData.ShouldSprint = true;
        }

        protected override void OnMovementCanceled(InputAction.CallbackContext context)
        {
            StateMachine.ChangeState(StateMachine.HardStoppingState);
            Debug.Log("Movement Canceled in Sprinting State");
            base.OnMovementCanceled(context);
        }

        protected override void OnJumpStarted(InputAction.CallbackContext context)
        {
            _shouldResetSprintState = false;

            base.OnJumpStarted(context);
        }

        protected override void OnFall()
        {
            _shouldResetSprintState = false;

            base.OnFall();
        }
    }
}