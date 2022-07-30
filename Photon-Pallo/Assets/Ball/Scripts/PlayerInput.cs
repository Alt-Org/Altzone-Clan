using UnityEngine;
using UnityEngine.InputSystem;

namespace Ball.Scripts
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Input Settings"), SerializeField] private InputActionReference _pointInputAction;
        [SerializeField] private InputActionReference _clickInputAction;

        [Header("Movement Settings"), SerializeField] private Camera _camera; 
        [SerializeField] private PlayerMovement _playerMovementReference;
        
        [Header("Live Data"), SerializeField] private Vector2 _pointerPosition;

        private IPlayerMovement _playerMovement;
        private double _lastButtonTime;

        private void Awake()
        {
            Debug.Log($"point {_pointInputAction.action}");
            _pointInputAction.action.performed += OnPointerMove;
            Debug.Log($"click {_clickInputAction.action}");
            _clickInputAction.action.performed += OnButtonClicked;
            _playerMovement = _playerMovementReference;
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
            Debug.Log($"{(isButtonDown ? "down" : "up")} {_pointerPosition} time {ctx.time - _lastButtonTime:0.00}");
            _lastButtonTime = ctx.time;
            if (!isButtonDown)
            {
                var worldPosition = _camera.ScreenToWorldPoint(_pointerPosition);
                _playerMovement.MoveTo(worldPosition);
            }
        }
    }
}