using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreCraft.Programming.HelpersAndTools
{
    public class CustomSlider : MonoBehaviour, IPointerUpHandler
    {
        [Header("Sliders")]
        public Slider Slider;
        public CustomSliderEvent SliderEvent;

        public void OnPointerUp(PointerEventData eventData)
        {
            SliderEvent.Invoke(Slider, Slider.value);
        }
    }

    [Serializable]
    public class CustomSliderEvent : UnityEvent<Slider, float> { }

    [Serializable]
    public class SliderSettings
    {
        public Slider Slider;
        public TextMeshProUGUI TextSlider;

        [Tooltip("The actual max value for the slider.")]
        public float MaxSliderValue;

        [Tooltip("The actual min value for the slider.")]
        public float MinSliderValue;

        // Convert virtual value to actual value
        public float ConvertVirtualToActualValue(float virtualMin, float virtualMax, float actualMin, float actualMax,
            float currentValue)
        {
            float ratio = (actualMax - actualMin) / (virtualMax - virtualMin);
            float returnValue = (currentValue * ratio) - (virtualMin * ratio) + actualMin;

            return returnValue;
        }

        // Convert from virtual value to actual and set the text appropriately
        public float SliderVirtualToActualConversionInt(SliderSettings settings, float currentValue)
        {
            float conversion = ConvertVirtualToActualValue(settings.Slider.minValue, settings.Slider.maxValue, settings.MinSliderValue,
                settings.MaxSliderValue, currentValue);
            settings.TextSlider.text = Mathf.RoundToInt(currentValue).ToString();

            return conversion;
        }

        // Convert from virtual value to actual and set the text appropriately
        public float SliderVirtualToActualConversionF2(SliderSettings settings, float currentValue)
        {
            float conversion = ConvertVirtualToActualValue(settings.Slider.minValue, settings.Slider.maxValue, settings.MinSliderValue,
                settings.MaxSliderValue, currentValue);
            settings.TextSlider.text = currentValue.ToString("F2");

            return conversion;
        }
        
        // Convert actual value to virtual value
        public float ConvertActualToVirtualValue(float virtualMin, float virtualMax, float actualMin, float actualMax,
            float currentValue)
        {
            float ratio = (actualMax - actualMin) / (virtualMax - virtualMin);
            float returnValue = ((currentValue - actualMin) + (virtualMin * ratio)) / ratio;

            return returnValue;
        }

        // Convert from actual value to virtual and set the text appropriately
        public float SliderActualToVirtualConversionInt(SliderSettings settings, float currentValue)
        {
            float conversion = ConvertActualToVirtualValue(settings.Slider.minValue, settings.Slider.maxValue, settings.MinSliderValue,
                settings.MaxSliderValue, currentValue);
            settings.TextSlider.text = Mathf.RoundToInt(conversion).ToString();

            return conversion;
        }

        // Convert from actual value to virtual and set the text appropriately
        public float SliderActualToVirtualConversionF2(SliderSettings settings, float currentValue)
        {
            float conversion = ConvertActualToVirtualValue(settings.Slider.minValue, settings.Slider.maxValue, settings.MinSliderValue,
                settings.MaxSliderValue, currentValue);
            settings.TextSlider.text = conversion.ToString("F2");

            return conversion;
        }
    }
}