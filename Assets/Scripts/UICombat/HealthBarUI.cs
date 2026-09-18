using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlockeonsDratris.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("Referências UI")]
        public Slider slider;
        public TMP_Text hpText;

        [Header("Animação")]
        public float lerpSpeed = 8f;

        private float targetValue;

        void Awake()
        {
            targetValue = slider != null ? slider.value : 1f;
        }

        void Update()
        {
            if (slider == null) return;
            if (Mathf.Abs(slider.value - targetValue) > 0.001f)
            {
                slider.value = Mathf.Lerp(slider.value, targetValue, Time.deltaTime * lerpSpeed);
            }
        }

        public void SetHP(int current, int max)
        {
            if (slider != null)
            {
                slider.maxValue = max;
                targetValue = current;
            }

            if (hpText != null)
            {
                hpText.text = $"{current} / {max}";
            }
        }
    }
}
