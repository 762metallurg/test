using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MovementSystem
{
    [Serializable]
    public class PlayerAnimationData
    {
        [Header("State Group Parameter Names")]
        [SerializeField] private string _groundedParameterName = "IsGrounded";
        [SerializeField] private string _movingParameterName = "IsMoving";
        [SerializeField] private string _stoppingParameterName = "IsStopping";
        [SerializeField] private string _landingParameterName = "IsLanding";
        [SerializeField] private string _airborneParameterName = "IsAirborne";

        [Header("Grounded Parameter Names")]
        [SerializeField] private string _idleParameterName = "IsIdling";
        [SerializeField] private string _dashParameterName = "IsDashing";
        [SerializeField] private string _walkParameterName = "IsWalking";
        [SerializeField] private string _runParameterName = "IsRunning";
        [SerializeField] private string _sprintParameterName = "IsSprinting";
        [SerializeField] private string _mediumStopParameterName = "IsMediumStopping";
        [SerializeField] private string _hardStopParameterName = "IsHardStopping";
        [SerializeField] private string _rollParameterName = "IsRolling";
        [SerializeField] private string _hardLandParameterName = "IsHardLanding";
        
        [Header("Airborne Parameter Names")]
        [SerializeField] private string _fallParameterName = "IsFalling";

        public int GroundedParameterHash { get; private set; }
        public int MovingParameterHash { get; private set; }
        public int StoppingParameterHash { get; private set; }
        public int LandingParameterHash { get; private set; }
        public int AirborneParameterHash { get; private set; }
        public int AttackParameterHash { get; private set; }

        public int IdleParameterHash { get; private set; }
        public int DashParameterHash { get; private set; }
        public int WalkParameterHash { get; private set; }
        public int RunParameterHash { get; private set; }
        public int SprintParameterHash { get; private set; }
        public int MediumStopParameterHash { get; private set; }
        public int HardStopParameterHash { get; private set; }
        public int RollParameterHash { get; private set; }
        public int HardLandParameterHash { get; private set; }

        public int FallParameterHash { get; private set; }
        
        public int AttackingCounterParameterHash { get; private set; }

        public void Initialize()
        {
            GroundedParameterHash = Animator.StringToHash(_groundedParameterName);
            MovingParameterHash = Animator.StringToHash(_movingParameterName);
            StoppingParameterHash = Animator.StringToHash(_stoppingParameterName);
            LandingParameterHash = Animator.StringToHash(_landingParameterName);
            AirborneParameterHash = Animator.StringToHash(_airborneParameterName);

            IdleParameterHash = Animator.StringToHash(_idleParameterName);
            DashParameterHash = Animator.StringToHash(_dashParameterName);
            WalkParameterHash = Animator.StringToHash(_walkParameterName);
            RunParameterHash = Animator.StringToHash(_runParameterName);
            SprintParameterHash = Animator.StringToHash(_sprintParameterName);
            MediumStopParameterHash = Animator.StringToHash(_mediumStopParameterName);
            HardStopParameterHash = Animator.StringToHash(_hardStopParameterName);
            RollParameterHash = Animator.StringToHash(_rollParameterName);
            HardLandParameterHash = Animator.StringToHash(_hardLandParameterName);

            FallParameterHash = Animator.StringToHash(_fallParameterName);
        }
    }
}