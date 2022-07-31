using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Shared.Scripts
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Input Settings"), SerializeField] private InputActionReference _pointInputAction;
        [SerializeField] private InputActionReference _clickInputAction;
        [SerializeField] private float _limitWorldYTop;

        [Header("Movement Settings"), SerializeField] private Camera _camera;

        [Header("Live Data"), SerializeField] private Vector2 _pointerPosition;
        [SerializeField] private PlayerMovementBase _playerMovementBase;

        private IPlayerMovement _playerMovement;
        private double _lastButtonTime;

        private void Awake()
        {
            Debug.Log($"point {_pointInputAction.action}");
            _pointInputAction.action.performed += OnPointerMove;
            Debug.Log($"click {_clickInputAction.action}");
            _clickInputAction.action.performed += OnButtonClicked;
            // We have to grab PlayerMovementBase dynamically because actual implementation is in derived class
            // and unfortunately it can not be setup in the Editor in a reliable way :-(
            _playerMovementBase = GetComponent<PlayerMovementBase>();
            _playerMovement = _playerMovementBase;
            Assert.IsNotNull(_playerMovement, "_playerMovement != null");
        }

        private void OnEnable()
        {
            _clickInputAction.action.Enable();
            _clickInputAction.action.Enable();
        }

        private void OnDisable()
        {
            _clickInputAction.action.Disable();
            _clickInputAction.action.Disable();
        }

        private void OnPointerMove(InputAction.CallbackContext ctx)
        {
            _pointerPosition = ctx.ReadValue<Vector2>();
        }

        private void OnButtonClicked(InputAction.CallbackContext ctx)
        {
            var isButtonDown = ctx.ReadValue<float>() != 0;
            Debug.Log($"{(isButtonDown ? "down" : "up")} {_pointerPosition} time {ctx.time - _lastButtonTime:0.00} {ctx.control}");
            _lastButtonTime = ctx.time;
            if (isButtonDown)
            {
                return;
            }
            // Button is released.
            var worldPosition = _camera.ScreenToWorldPoint(_pointerPosition);
            if (worldPosition.y > _limitWorldYTop)
            {
                return;
            }
            _playerMovement.MoveTo(worldPosition);
        }
    }
}