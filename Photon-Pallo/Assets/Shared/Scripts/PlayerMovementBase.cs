using UnityEngine;

namespace Shared.Scripts
{
    public interface IPlayerMovement
    {
        void MoveTo(Vector2 position);
    }

    public class PlayerMovementBase : MonoBehaviour, IPlayerMovement
    {
        [Header("Settings"), SerializeField] protected float _speed;

        [Header("Live Data"), SerializeField] protected Vector3 _targetPosition;

        public virtual void MoveTo(Vector2 position)
        {
            throw new System.NotImplementedException();
        }
    }
}
