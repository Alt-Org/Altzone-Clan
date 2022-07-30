using System.Collections;
using UnityEngine;

namespace Ball.Scripts
{
    public interface IPlayerMovement
    {
        void MoveTo(Vector2 position);
    }

    public class PlayerMovement : MonoBehaviour, IPlayerMovement
    {
        [Header("Settings"), SerializeField] private float _speed;

        [Header("Live Data"), SerializeField] private Vector3 _targetPosition;

        private Transform _transform;
        private Coroutine _coroutine;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private IEnumerator MoveToPosition()
        {
            for (;;)
            {
                var maxDistanceDelta = _speed * Time.deltaTime;
                var tempPosition = Vector3.MoveTowards(_transform.position, _targetPosition, maxDistanceDelta);
                _transform.position = tempPosition;
                var hasReachedTarget = Mathf.Approximately(tempPosition.x, _targetPosition.x) &&
                                       Mathf.Approximately(tempPosition.y, _targetPosition.y);
                if (hasReachedTarget)
                {
                    break;
                }
                yield return null;
            }
            _coroutine = null;
        }

        void IPlayerMovement.MoveTo(Vector2 position)
        {
            Debug.Log($"move {position}");
            _targetPosition = position;
            _coroutine ??= StartCoroutine(MoveToPosition());
        }
    }
}