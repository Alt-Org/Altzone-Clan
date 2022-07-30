using UnityEngine;
using UnityEngine.InputSystem;

namespace Ball.Scripts
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private InputActionReference _clickInputAction;
    }
}