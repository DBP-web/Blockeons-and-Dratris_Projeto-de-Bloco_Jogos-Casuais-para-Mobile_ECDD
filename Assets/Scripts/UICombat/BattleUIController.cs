using UnityEngine;
using BlockeonsDratris.Combat;

namespace BlockeonsDratris.UI
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Dependência principal")]
        public BattleManager battleManager;

        [Header("Componentes de UI")]
        public HealthBarUI heroHealthBar;
        public HealthBarUI enemyHealthBar;
        public EnergyBarUI energyBar;
        public ChainIndicatorUI chainIndicator;
        public BattleResultPanelUI resultPanel;

        [Header("Feedback de Contra-ataque (opcional)")]
        public GameObject enemyAttackFeedbackPrefab;
        public Transform feedbackSpawnPoint;

        void OnEnable()
        {
            if (battleManager == null) return;

            battleManager.OnBattleStarted += HandleBattleStarted;
            battleManager.OnHeroHPChanged += HandleHeroHPChanged;
            battleManager.OnEnemyHPChanged += HandleEnemyHPChanged;
            battleManager.OnEnergyChanged += HandleEnergyChanged;
            battleManager.OnStepResolved += HandleStepResolved;
            battleManager.OnEnemyCounterAttack += HandleEnemyCounterAttack;
            battleManager.OnVictory += HandleVictory;
            battleManager.OnDefeat += HandleDefeat;
        }

        void OnDisable()
        {
            if (battleManager == null) return;

            battleManager.OnBattleStarted -= HandleBattleStarted;
            battleManager.OnHeroHPChanged -= HandleHeroHPChanged;
            battleManager.OnEnemyHPChanged -= HandleEnemyHPChanged;
            battleManager.OnEnergyChanged -= HandleEnergyChanged;
            battleManager.OnStepResolved -= HandleStepResolved;
            battleManager.OnEnemyCounterAttack -= HandleEnemyCounterAttack;
            battleManager.OnVictory -= HandleVictory;
            battleManager.OnDefeat -= HandleDefeat;
        }

        private void HandleBattleStarted()
        {
            if (chainIndicator != null)
                chainIndicator.ShowChainStep(default, -1); // limpa indicador inicial
        }

        private void HandleHeroHPChanged(int current, int max)
        {
            if (heroHealthBar != null)
                heroHealthBar.SetHP(current, max);
        }

        private void HandleEnemyHPChanged(int current, int max)
        {
            if (enemyHealthBar != null)
                enemyHealthBar.SetHP(current, max);
        }

        private void HandleEnergyChanged(int current, int max)
        {
            if (energyBar != null)
                energyBar.SetEnergy(current, max);
        }

        private void HandleStepResolved(CombatResult result, int chainIndex)
        {
            if (chainIndicator != null)
                chainIndicator.ShowChainStep(result, chainIndex);
        }

        private void HandleEnemyCounterAttack(int damage)
        {
            if (enemyAttackFeedbackPrefab != null && feedbackSpawnPoint != null)
            {
                Instantiate(enemyAttackFeedbackPrefab, feedbackSpawnPoint.position, Quaternion.identity);
            }
        }

        private void HandleVictory(int goldGained, int crystalsGained)
        {
            if (resultPanel != null)
                resultPanel.ShowVictory(goldGained, crystalsGained);
        }

        private void HandleDefeat()
        {
            if (resultPanel != null)
                resultPanel.ShowDefeat();
        }
    }
}