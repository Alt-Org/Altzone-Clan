using System;
using UnityEngine;

namespace Ball.Scripts
{
    [Serializable]
    internal class BallSettings
    {
        [Header("Time Scale")] public float _timeScale;

        [Header("Ball Movement")] public Vector2 _initialVelocity;
        public float _minVelocity;

        [Header("Layers")] public LayerMask _bounceMask;
    }
    public class BallNoPhysics : MonoBehaviour
    {
        [SerializeField] private BallSettings _settings;

        private Rigidbody2D _rigidbody;

        private int _bounceMaskValue;

        private void Awake()
        {
            _bounceMaskValue = _settings._bounceMask.value;
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _rigidbody.velocity = _settings._initialVelocity;
            if (_settings._timeScale > 0 && Math.Abs(_settings._timeScale - 1f) > Mathf.Epsilon)
            {
                Time.timeScale = _settings._timeScale;
                Debug.Log($"SET Time.timeScale {Time.timeScale:F3}");
            }
        }

        private int _trailCountDown;
        private float _trailRendererTime;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"ignore {other.name} layer {other.gameObject.layer}");
                return;
            }
            Debug.Log($"{name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
            var colliderMask = 1 << layer;
            if (_bounceMaskValue == (_bounceMaskValue | colliderMask))
            {
                Bounce(other);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"ignore {other.name} layer {other.gameObject.layer}");
                return;
            }
            Debug.Log($"{name} -- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"ignore {other.name} layer {other.gameObject.layer}");
                return;
            }
            Debug.Log($"{name} -> {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void Bounce(Collider2D other)
        {
            // Do the bounce
            BounceAndReflect(other);
        }

        private void BounceAndReflect(Collider2D other)
        {
            var currentVelocity = _rigidbody.velocity;
            var position = _rigidbody.position;
            var closestPoint = other.ClosestPoint(position);
            var direction = closestPoint - position;
            Reflect(currentVelocity, direction.normalized);
            Debug.Log(
                $"bounce {other.name} @ {position} closest {closestPoint} dir {currentVelocity} <- {direction} frame {Time.frameCount}");
        }

        private void Reflect(Vector2 currentVelocity, Vector2 collisionNormal)
        {
            var speed = currentVelocity.magnitude;
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            _rigidbody.velocity = direction * Mathf.Max(speed, _settings._minVelocity);
        }
    }
}
