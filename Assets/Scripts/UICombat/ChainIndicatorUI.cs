using UnityEngine;
using TMPro;
using BlockeonsDratris.Combat;
using System.Collections;

namespace BlockeonsDratris.UI
{
    public class ChainIndicatorUI : MonoBehaviour
    {
        [Header("Referências UI")]
        public TMP_Text chainText;       // Ex.: "Chain x3"
        public TMP_Text stepResultText;  // Ex.: "-120 HP" ou "+40 Cura"
        public CanvasGroup stepResultGroup;

        [Header("Timing")]
        public float displayDuration = 1.2f;
        public float fadeDuration = 0.3f;

        private Coroutine fadeRoutine;

        public void ShowChainStep(CombatResult result, int chainIndex)
        {
            if (chainText != null)
            {
                chainText.gameObject.SetActive(chainIndex > 0);
                chainText.text = $"Chain x{chainIndex}";
            }

            if (stepResultText != null)
            {
                string msg = BuildResultMessage(result);
                stepResultText.text = msg;

                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(FadeInOut());
            }
        }

        private string BuildResultMessage(CombatResult result)
        {
            var parts = new System.Collections.Generic.List<string>();

            if (result.totalDamageToEnemy > 0)
                parts.Add($"-{Mathf.RoundToInt(result.totalDamageToEnemy)} HP");

            if (result.totalHealToHero > 0)
                parts.Add($"+{Mathf.RoundToInt(result.totalHealToHero)} Cura");

            if (result.goldGained > 0)
                parts.Add($"+{result.goldGained} Ouro");

            if (result.crystalsGained > 0)
                parts.Add($"+{result.crystalsGained} Cristais");

            if (result.energyGained > 0)
                parts.Add($"+{result.energyGained} Energia");

            if (result.shieldGained > 0)
                parts.Add($"+{result.shieldGained} Escudo");

            return parts.Count > 0 ? string.Join("  ", parts) : string.Empty;
        }

        public void ShowSpecialAbilityResult(int damage)
        {
            if (stepResultText != null)
            {
                stepResultText.text = $"-{damage} HP (Especial!)";

                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(FadeInOut());
            }
        }

        private IEnumerator FadeInOut()
        {
            if (stepResultGroup == null) yield break;

            stepResultGroup.alpha = 1f;
            yield return new WaitForSeconds(displayDuration);

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                stepResultGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                yield return null;
            }
            stepResultGroup.alpha = 0f;
        }
    }
}