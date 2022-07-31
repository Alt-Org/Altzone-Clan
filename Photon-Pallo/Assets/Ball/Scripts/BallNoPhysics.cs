using System;
using UnityEngine;

namespace Ball.Scripts
{
    [Serializable]
    public class BallSettings
    {
        [Header("Ball Movement")] public Vector2 _initialDirection;
        public float _requestedVelocity;
        public float _minVelocity;
        public float _maxVelocity;
    }

    public class BallNoPhysics : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private BallSettings _settings;
        [SerializeField] private BallSliderUi _ballSliderUi;

        [Header("Layers"), SerializeField] private LayerMask _bounceMask;

        private Rigidbody2D _rigidbody;

        private int _bounceMaskValue;

        private void Awake()
        {
            _bounceMaskValue = _bounceMask.value;
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void SetBallVelocity(float velocity)
        {
            _settings._requestedVelocity = velocity;
            _rigidbody.velocity = _rigidbody.velocity.normalized * _settings._requestedVelocity;
        }

        private void OnEnable()
        {
            // Set ball moving
            _rigidbody.velocity = _settings._initialDirection;
            if (_ballSliderUi != null)
            {
                _ballSliderUi.Connect(
                    _settings._requestedVelocity, _settings._minVelocity, _settings._maxVelocity, _rigidbody, SetBallVelocity);
            }
            else
            {
                SetBallVelocity(_settings._requestedVelocity);
            }
        }

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
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            _rigidbody.velocity = direction * Mathf.Max(_settings._requestedVelocity, _settings._minVelocity);
        }
    }
}