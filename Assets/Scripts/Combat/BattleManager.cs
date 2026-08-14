using System.Collections.Generic;
using UnityEngine;
using BlockeonsDratris.Data;
using BlockeonsDratris.Board;
using BlockeonsDratris.Blocks;

namespace BlockeonsDratris.Combat
{
    [RequireComponent(typeof(DamageCalculator))]
    public class BattleManager : MonoBehaviour
    {
        [Header("Dados da Batalha")]
        public SOHeroData heroData;
        public SOEnemyData enemyData;

        [Header("Referências")]
        public BoardManager boardManager;
        public DamageCalculator damageCalculator;

        [Header("Estado Runtime (somente leitura)")]
        public int currentHeroHP;
        public int currentEnemyHP;
        public int currentEnergy;
        public int movesSinceLastCounterAttack = 0;

        private bool battleEnded = false;

        // Eventos para a UI
        public System.Action OnBattleStarted;
        public System.Action<int, int> OnHeroHPChanged;      // currentHP, maxHP
        public System.Action<int, int> OnEnemyHPChanged;     // currentHP, maxHP
        public System.Action<int, int> OnEnergyChanged;      // currentEnergy, maxEnergy
        public System.Action<CombatResult, int> OnStepResolved; // resultado do passo, chainIndex
        public System.Action<int> OnEnemyCounterAttack;      // dano recebido
        public System.Action OnVictory;
        public System.Action OnDefeat;
        public System.Action OnSpecialAbilityUsed; // uso da habilidade especial

        void Awake()
        {
            if (damageCalculator == null)
                damageCalculator = GetComponent<DamageCalculator>();
        }

        void OnEnable()
        {
            if (boardManager != null)
            {
                boardManager.OnChainStep += HandleChainStep;
                boardManager.OnBoardStable += HandleBoardStable;
            }
        }

        void OnDisable()
        {
            if (boardManager != null)
            {
                boardManager.OnChainStep -= HandleChainStep;
                boardManager.OnBoardStable -= HandleBoardStable;
            }
        }

        public void StartBattle(SOHeroData hero, SOEnemyData enemy)
        {
            heroData = hero;
            enemyData = enemy;
            damageCalculator.heroData = hero;

            battleEnded = false;
            currentHeroHP = heroData.maxHP;
            currentEnemyHP = enemyData.maxHP;
            currentEnergy = 0;
            movesSinceLastCounterAttack = 0;

            boardManager.enabled = true;
            boardManager.SetupBoard();

            OnBattleStarted?.Invoke();
            OnHeroHPChanged?.Invoke(currentHeroHP, heroData.maxHP);
            OnEnemyHPChanged?.Invoke(currentEnemyHP, enemyData.maxHP);
            OnEnergyChanged?.Invoke(currentEnergy, heroData.maxEnergy);
        }

        private void HandleChainStep(List<MatchGroup> matches, int chainIndex)
        {
            if (battleEnded) return;

            CombatResult result = damageCalculator.CalculateStep(matches, chainIndex);

            currentEnemyHP = Mathf.Max(0, currentEnemyHP - Mathf.RoundToInt(result.totalDamageToEnemy));
            currentHeroHP = Mathf.Min(heroData.maxHP, currentHeroHP + Mathf.RoundToInt(result.totalHealToHero));
            currentEnergy = Mathf.Min(heroData.maxEnergy, currentEnergy + result.energyGained);

            OnStepResolved?.Invoke(result, chainIndex);
            OnEnemyHPChanged?.Invoke(currentEnemyHP, enemyData.maxHP);
            OnHeroHPChanged?.Invoke(currentHeroHP, heroData.maxHP);
            OnEnergyChanged?.Invoke(currentEnergy, heroData.maxEnergy);

            CheckBattleEnd();
        }

        private void HandleBoardStable()
        {
            if (battleEnded) return;

            movesSinceLastCounterAttack++;

            if (movesSinceLastCounterAttack >= enemyData.movesPerCounterAttack)
            {
                movesSinceLastCounterAttack = 0;
                ApplyEnemyCounterAttack();
            }
        }

        private void ApplyEnemyCounterAttack()
        {
            if (battleEnded) return;

            currentHeroHP = Mathf.Max(0, currentHeroHP - enemyData.counterAttackDamage);
            OnEnemyCounterAttack?.Invoke(enemyData.counterAttackDamage);
            OnHeroHPChanged?.Invoke(currentHeroHP, heroData.maxHP);

            CheckBattleEnd();
        }

        public bool TryUseSpecialAbility()
        {
            if (battleEnded) return false;
            if (currentEnergy < heroData.specialAbilityCost) return false;

            currentEnergy -= heroData.specialAbilityCost;
            OnEnergyChanged?.Invoke(currentEnergy, heroData.maxEnergy);
            OnSpecialAbilityUsed?.Invoke();

            // Efeito da habilidade especial será resolvido em módulo futuro
            // (ex.: SpecialAbilityResolver), acionado a partir daqui.

            return true;
        }

        private void CheckBattleEnd()
        {
            if (currentEnemyHP <= 0 && !battleEnded)
            {
                battleEnded = true;
                boardManager.enabled = false;
                OnVictory?.Invoke();
            }
            else if (currentHeroHP <= 0 && !battleEnded)
            {
                battleEnded = true;
                boardManager.enabled = false;
                OnDefeat?.Invoke();
            }
        }
    }
}