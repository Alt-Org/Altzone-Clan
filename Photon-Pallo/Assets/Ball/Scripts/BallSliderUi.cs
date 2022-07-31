using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ball.Scripts
{
    [Serializable]
    internal class BallUi
    {
        [Header("Ball Movement")] public TMP_Text _ballVelocity;
        public TMP_Text _sliderVelocity;
        public Slider _ballSpeedSlider;
    }
    
    public class BallSliderUi : MonoBehaviour
    {
        [SerializeField] private BallUi _ui;

        private Rigidbody2D _rigidbody;
        private Action<float> _onVelocityChanged;
        
        private void Awake()
        {
            _ui._ballSpeedSlider.onValueChanged.AddListener(SetSliderVelocity);
        }

        private void SetSliderVelocity(float sliderValue)
        {
            _onVelocityChanged?.Invoke(sliderValue);
            _ui._sliderVelocity.text = $"Speed [{_ui._ballSpeedSlider.minValue:0}-{_ui._ballSpeedSlider.maxValue:0}] {sliderValue:0.0}";
        }
        
        private void Update()
        {
            _ui._ballVelocity.text = $"Ball speed min {_ui._ballSpeedSlider.minValue:0.0} cur {_rigidbody.velocity.magnitude:0.00}";
        }

        public void Connect(float velocity, float minVelocity, float maxVelocity, Rigidbody2D ballRigidbody, Action<float> onVelocityChanged)
        {
            _ui._ballSpeedSlider.minValue = minVelocity;
            _ui._ballSpeedSlider.maxValue = maxVelocity;
            _rigidbody = ballRigidbody;
            _onVelocityChanged = onVelocityChanged;
            _ui._ballSpeedSlider.value = velocity;
        }
    }
}