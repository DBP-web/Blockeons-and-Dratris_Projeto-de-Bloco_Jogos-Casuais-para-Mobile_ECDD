using UnityEngine;
using TMPro;

namespace BlockeonsDratris.UI
{
    public class ShieldBarUI : MonoBehaviour
    {
        [Header("Referências UI")]
        public TMP_Text shieldText;

        public void SetShield(int currentShield)
        {
            if (shieldText != null)
                shieldText.text = currentShield.ToString();
        }
    }
}