using UnityEngine;

namespace MovementSystem
{
    public class PlayerAnimationEventTrigger : MonoBehaviour
    {
        [SerializeField] private Player _player;

        public void TriggerOnMovementStateAnimationEnterEvent()
        {
            if (IsInAnimationTransition())
            {
                return;
            }

            _player.OnMovementStateAnimationEnterEvent();
        }

        public void TriggerOnMovementStateAnimationExitEvent()
        {
            if (IsInAnimationTransition())
            {
                return;
            }

            _player.OnMovementStateAnimationExitEvent();
        }

        public void TriggerOnMovementStateAnimationTransitionEvent()
        {
            if (IsInAnimationTransition())
            {
                return;
            }

            _player.OnMovementStateAnimationTransitionEvent();
        }

        private bool IsInAnimationTransition()
        {
            return _player.Animator.IsInTransition(0);
        }
    }
    
}