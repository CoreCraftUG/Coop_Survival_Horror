using CoreCraft.Programming.HelpersAndTools;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft.Programming.GameSettings
{
    public class VideoGameSettings : MonoBehaviour
    {
        [Header("Scriptable Objects")]
        [SerializeField] private SO_Settings _soSettings;

        [Header("Sliders")]
        [SerializeField] private SliderSettings _mouseSensivitySlider;

        [Header("Toggles")]
        [SerializeField] private Toggle _invertMouseXToggle;
        [SerializeField] private Toggle _invertMouseYToggle;

        // Private Variables
        private const string _sliderMouseSensivity = "SL_MouseSensivity";

        public void ReceiveSliderValue(Slider slider, float value)
        {
            switch (slider.name)
            {
                case _sliderMouseSensivity:
                    _soSettings.SettingsData.MouseSensivity = _mouseSensivitySlider.ConvertVirtualToActualValue(
                        _mouseSensivitySlider.Slider.minValue, _mouseSensivitySlider.Slider.maxValue,
                        _mouseSensivitySlider.MinSliderValue, _mouseSensivitySlider.MaxSliderValue, value);
                    break;
            }
        }

        // Whenever the user changes the slider, it updates immediately
        private void SliderChanged(SliderSettings sliderSettings)
        {
            switch (sliderSettings.Slider.name)
            {
                case _sliderMouseSensivity:
                    sliderSettings.SliderVirtualToActualConversionF2(_mouseSensivitySlider,
                        sliderSettings.Slider.value);
                    break;
            }
        }

        public void MouseInvertX(bool boxChecked)
        {
            _invertMouseXToggle.SetIsOnWithoutNotify(boxChecked);
            _soSettings.SettingsData.MouseInvertX = boxChecked;
        }

        public void MouseInvertY(bool boxChecked)
        {
            _invertMouseYToggle.SetIsOnWithoutNotify(boxChecked);
            _soSettings.SettingsData.MouseInvertY = boxChecked;
        }

        private void Initialize()
        {
            //SetValues();

            _invertMouseXToggle.SetIsOnWithoutNotify(_soSettings.SettingsData.MouseInvertX);
            _invertMouseYToggle.SetIsOnWithoutNotify(_soSettings.SettingsData.MouseInvertY);
        }

        public void SetValues()
        {
            _mouseSensivitySlider.Slider.value =
                _mouseSensivitySlider.SliderActualToVirtualConversionF2(_mouseSensivitySlider,
                    _soSettings.SettingsData.MouseSensivity);
        }

        private void ButtonEvents()
        {
            _mouseSensivitySlider.Slider.onValueChanged.AddListener(delegate { SliderChanged(_mouseSensivitySlider); });
            _invertMouseXToggle.onValueChanged.AddListener(delegate { MouseInvertX(_invertMouseXToggle.isOn); });
            _invertMouseYToggle.onValueChanged.AddListener(delegate { MouseInvertY(_invertMouseYToggle.isOn); });
        }

        private void Start()
        {
            ButtonEvents();
            Initialize();
        }
    }
}