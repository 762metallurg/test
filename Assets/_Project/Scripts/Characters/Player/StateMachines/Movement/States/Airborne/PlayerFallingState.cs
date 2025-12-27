using UnityEngine;

namespace MovementSystem
{
    public class PlayerFallingState : PlayerAirborneState
    {
        private Vector3 _playerPositionOnEnter;

        public PlayerFallingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            StartAnimation(StateMachine.Player.AnimationData.FallParameterHash);

            StateMachine.ReusableData.MovementSpeedModifier = 0f;

            _playerPositionOnEnter = StateMachine.Player.transform.position;

            ResetVerticalVelocity();
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(StateMachine.Player.AnimationData.FallParameterHash);
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            LimitVerticalVelocity();
        }

        private void LimitVerticalVelocity()
        {
            Vector3 playerVerticalVelocity = GetPlayerVerticalVelocity();

            if (playerVerticalVelocity.y >= -AirborneData.FallData.FallSpeedLimit)
            {
                return;
            }

            Vector3 limitedVelocityForce = new Vector3(0f, -AirborneData.FallData.FallSpeedLimit - playerVerticalVelocity.y, 0f);

            StateMachine.Player.Rigidbody.AddForce(limitedVelocityForce, ForceMode.VelocityChange);
        }

        protected override void ResetSprintState()
        {
        }

        protected override void OnContactWithGround(Collider collider)
        {
            float fallDistance = _playerPositionOnEnter.y - StateMachine.Player.transform.position.y;

            if (fallDistance < AirborneData.FallData.MinimumDistanceToBeConsideredHardFall)
            {
                StateMachine.ChangeState(StateMachine.LightLandingState);

                return;
            }

            if (StateMachine.ReusableData.ShouldWalk && !StateMachine.ReusableData.ShouldSprint || StateMachine.ReusableData.MovementInput == Vector2.zero)
            {
                StateMachine.ChangeState(StateMachine.HardLandingState);

                return;
            }

            StateMachine.ChangeState(StateMachine.RollingState);

        }
    }
}