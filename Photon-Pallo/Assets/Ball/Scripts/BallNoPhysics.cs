using System;
using UnityEngine;

namespace Ball.Scripts
{
    [Serializable]
    internal class BallSettings
    {
        [Header("Ball movement")] public Vector2 _initialVelocity;
        public float _minVelocity;

        [Header("Layers")] public LayerMask _bounceMask;
        public LayerMask _teamAreaMask;
        public LayerMask _headMask;
        public LayerMask _brickMask;
        public LayerMask _wallMask;
    }
    public class BallNoPhysics : MonoBehaviour
    {
        [SerializeField] private BallSettings _settings;

        [Header("Trail Testing")] public bool _isTeleportOnBrickCollision;
        public int _trailSkipFrames = 15;
        public TrailRenderer _trailRenderer;

        [Header("Live Data"), SerializeField] private bool _isRedTeamActive;
        [SerializeField] private bool _isBlueTeamActive;

        private Rigidbody2D _rigidbody;

        private int _bounceMaskValue;
        private int _teamAreaMaskValue;
        private int _headMaskValue;
        private int _brickMaskValue;
        private int _wallMaskValue;

        [Header("Time.timeScale")] public float _timeScale;

        [Header("Collider Debug")] public int _ignoredCount;
        public Collider2D[] _ignoredColliders = new Collider2D[4];
        public ContactFilter2D _contactFilter;
        private int _overlappingCount;
        private readonly Collider2D[] _overlappingColliders = new Collider2D[4];
        private readonly float[] _overlappingDistance = new float[4];

        private void Awake()
        {
            _bounceMaskValue = _settings._bounceMask.value;
            _teamAreaMaskValue = _settings._teamAreaMask.value;
            _headMaskValue = _settings._headMask.value;
            _brickMaskValue = _settings._brickMask.value;
            _wallMaskValue = _settings._wallMask.value;
            _rigidbody = GetComponent<Rigidbody2D>();
            // We need to track these colliders while ball bounces
            _contactFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = _settings._wallMask.value + _settings._brickMask.value // Implicitly converts an integer to a LayerMask
            };
        }

        private void OnEnable()
        {
            _rigidbody.velocity = _settings._initialVelocity;
            if (_timeScale > 1f)
            {
                Time.timeScale = _timeScale;
                Debug.Log($"SET Time.timeScale {Time.timeScale:F3}");
            }
        }

        private int _trailCountDown;
        private float _trailRendererTime;

        private void Update()
        {
            if (--_trailCountDown == 0)
            {
                print($"{Time.frameCount} trailRenderer time {_trailRendererTime:0.00}");
                //_trailRenderer.time = _trailRendererTime;
                _trailRenderer.emitting = true;
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
            var colliderMask = 1 << layer;
            if (_bounceMaskValue == (_bounceMaskValue | colliderMask))
            {
                Bounce(other);
                return;
            }
            if (_teamAreaMaskValue == (_teamAreaMaskValue | colliderMask))
            {
                TeamEnter(otherGameObject);
                return;
            }
            if (_brickMaskValue == (_brickMaskValue | colliderMask))
            {
                Bounce(other);
                if (_isTeleportOnBrickCollision)
                {
                    _rigidbody.position = Vector2.zero;
                    _trailCountDown = _trailSkipFrames;
                    _trailRendererTime = _trailRenderer.time;
                    //_trailRenderer.time = 0;
                    _trailRenderer.emitting = false;
                    print($"{Time.frameCount} trailRenderer time {0f:0.00}");
                }
                Brick(otherGameObject);
                return;
            }
            Debug.Log($"UNHANDLED hit {other.name} layer {layer}");
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (_ignoredCount > 0)
            {
                for (var i = 0; i < _ignoredCount; ++i)
                    if (_ignoredColliders[i].Equals(other))
                    {
                        return;
                    }
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                return;
            }
            var colliderMask = 1 << layer;
            if (_teamAreaMaskValue == (_teamAreaMaskValue | colliderMask))
            {
                return;
            }

            Debug.Log($"STOP @ {_rigidbody.position} on STAY hit {other.name} frame {Time.frameCount}");
            _rigidbody.velocity = Vector2.zero;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (_ignoredCount > 0)
            {
                if (RemoveIgnoredCollider(other))
                {
                    return;
                }
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                return;
            }
            var colliderMask = 1 << layer;
            if (_teamAreaMaskValue == (_teamAreaMaskValue | colliderMask))
            {
                TeamExit(otherGameObject);
            }
        }

        private void AddIgnoredCollider(Collider2D other)
        {
            _ignoredColliders[_ignoredCount] = other;
            _ignoredCount += 1;
        }

        private bool RemoveIgnoredCollider(Collider2D other)
        {
            for (var i = 0; i < _ignoredCount; ++i)
                if (_ignoredColliders[i].Equals(other))
                {
                    Debug.Log($"REMOVE ignore {other.name} frame {Time.frameCount} ignored {_ignoredCount}");
                    if (_ignoredCount == 1)
                    {
                        _ignoredColliders[i] = null;
                        _ignoredCount = 0;
                        return true;
                    }
                    // Move items down by one
                    Array.Copy(_ignoredColliders, i + 1, _ignoredColliders, i, _ignoredColliders.Length - 2);
                    _ignoredColliders[_ignoredCount] = null;
                    _ignoredCount -= 1;
                    return true;
                }
            return false;
        }

        private void Bounce(Collider2D other)
        {
            if (_ignoredCount > 0)
            {
                for (var i = 0; i < _ignoredCount; ++i)
                    if (_ignoredColliders[i].Equals(other))
                    {
                        Debug.Log($"SKIP ignore {other.name} frame {Time.frameCount} ignored {_ignoredCount}");
                        return;
                    }
            }
            _overlappingCount = _rigidbody.OverlapCollider(_contactFilter, _overlappingColliders);
            if (_overlappingCount < 2)
            {
                BounceAndReflect(other);
                return;
            }
            // Count wall colliders and print print all colliders
            var wallColliderCount = 0;
            var position = _rigidbody.position;
            for (var i = 0; i < _overlappingColliders.Length; i++)
                if (i < _overlappingCount)
                {
                    var overlappingCollider = _overlappingColliders[i];
                    var closest = overlappingCollider.ClosestPoint(_rigidbody.position);
                    _overlappingDistance[i] = (closest - position).sqrMagnitude;
                    if (overlappingCollider.name.EndsWith("Wall"))
                    {
                        wallColliderCount += 1;
                    }
                    Debug.Log(
                        $"overlapping {other.name} {i}/{_overlappingCount} {overlappingCollider.name} pos {closest} dist {Mathf.Sqrt(_overlappingDistance[i]):F3}");
                }
                else
                {
                    _overlappingColliders[i] = null;
                }
            if (wallColliderCount == _overlappingCount)
            {
                // Let wall colliders run normally
                BounceAndReflect(other);
                return;
            }
            // Collide with nearest only
            var nearest = 0;
            for (var i = 1; i < _overlappingCount; i++)
                if (_overlappingDistance[i] < _overlappingDistance[nearest])
                {
                    nearest = i;
                }
            // Add everything to ignored colliders so that ball can move out while bouncing
            _ignoredCount = 0;
            for (var i = 0; i < _overlappingCount; i++) AddIgnoredCollider(_overlappingColliders[i]);
            // Do the bounce
            var nearestCollider = _overlappingColliders[nearest];
            BounceAndReflect(nearestCollider);
        }

        private void Brick(GameObject brick)
        {
        }

        private void TeamEnter(GameObject teamArea)
        {
        }

        private void TeamExit(GameObject teamArea)
        {
        }

        private void BounceAndReflect(Collider2D other)
        {
            var currentVelocity = _rigidbody.velocity;
            var position = _rigidbody.position;
            var closestPoint = other.ClosestPoint(position);
            var direction = closestPoint - position;
            Reflect(currentVelocity, direction.normalized);
            Debug.Log(
                $"bounce {other.name} @ {position} closest {closestPoint} dir {currentVelocity} <- {_rigidbody.velocity} frame {Time.frameCount} ol-count {_overlappingCount}");
        }

        private void Reflect(Vector2 currentVelocity, Vector2 collisionNormal)
        {
            var speed = currentVelocity.magnitude;
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            _rigidbody.velocity = direction * Mathf.Max(speed, _settings._minVelocity);
        }
    }
}
