using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ball.Scripts
{
    [Serializable]
    internal class BallSettings
    {
        [Header("Ball Movement")] public Vector2 _initialDirection;
        public float _requestedVelocity;
        public float _minVelocity;
        public float _maxVelocity;
        public TMP_Text _ballVelocity;
        public TMP_Text _sliderVelocity;
        public Slider _ballSpeedSlider;

        [Header("Layers")] public LayerMask _bounceMask;

        [Header("Time Scale")] public float _timeScale;
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
            _settings._ballSpeedSlider.minValue = _settings._minVelocity;
            _settings._ballSpeedSlider.maxValue = _settings._maxVelocity;
            _settings._ballSpeedSlider.onValueChanged.AddListener(SetSliderVelocity);
        }

        private void SetSliderVelocity(float sliderValue)
        {
            _settings._requestedVelocity = sliderValue;
            _rigidbody.velocity = _rigidbody.velocity.normalized * _settings._requestedVelocity;
            _settings._sliderVelocity.text = $"Speed [{_settings._ballSpeedSlider.minValue:0}-{_settings._ballSpeedSlider.maxValue:0}] {_settings._requestedVelocity:0.0}";
        }
        
        private void OnEnable()
        {
            if (_settings._timeScale > 0 && Math.Abs(_settings._timeScale - 1f) > Mathf.Epsilon)
            {
                Time.timeScale = _settings._timeScale;
                Debug.Log($"SET Time.timeScale {Time.timeScale:F3}");
            }
            // Set ball moving
            _rigidbody.velocity = _settings._initialDirection;
            SetSliderVelocity(_settings._requestedVelocity);
        }

        private void Update()
        {
            _settings._ballVelocity.text = $"Ball speed min {_settings._minVelocity:0.0} cur {_rigidbody.velocity.magnitude:0.00}";
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