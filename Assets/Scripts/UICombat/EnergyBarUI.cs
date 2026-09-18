using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockeonsDratris.Combat;

namespace BlockeonsDratris.UI
{
    public class EnergyBarUI : MonoBehaviour
    {
        [Header("Referências UI")]
        public Slider slider;
        public TMP_Text energyText;
        public Button specialAbilityButton;

        [Header("Dependência")]
        public BattleManager battleManager;

        void Awake()
        {
            if (specialAbilityButton != null)
                specialAbilityButton.onClick.AddListener(OnSpecialAbilityClicked);
        }

        public void SetEnergy(int current, int max)
        {
            if (slider != null)
            {
                slider.maxValue = max;
                slider.value = current;
            }

            if (energyText != null)
            {
                energyText.text = $"{current} / {max}";
            }

            if (specialAbilityButton != null)
            {
                bool canUse = battleManager != null && battleManager.heroData != null
                              && current >= battleManager.heroData.specialAbilityCost;
                specialAbilityButton.interactable = canUse;
            }
        }

        private void OnSpecialAbilityClicked()
        {
            if (battleManager == null) return;
            battleManager.TryUseSpecialAbility();
        }
    }
}