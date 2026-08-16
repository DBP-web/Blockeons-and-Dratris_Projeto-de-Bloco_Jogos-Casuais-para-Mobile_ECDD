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

        [Header("Persistência de Recompensas")]
        public SOPlayerWallet playerWallet;

        [Header("Estado Runtime (somente leitura)")]
        public int currentHeroHP;
        public int currentEnemyHP;
        public int currentEnergy;
        public int movesSinceLastCounterAttack = 0;
        public int totalGoldGained = 0;
        public int totalCrystalsGained = 0;

        [Header("Configuração de Spawn")]
        public SOSpawnWeightConfig spawnConfig;
        public GameModeType currentMode;

        private bool battleEnded = false;

        // Eventos para a UI
        public event System.Action OnBattleStarted;
        public event System.Action<int, int> OnHeroHPChanged;
        public event System.Action<int, int> OnEnemyHPChanged;
        public event System.Action<int, int> OnEnergyChanged;
        public event System.Action<CombatResult, int> OnStepResolved;
        public event System.Action<int> OnEnemyCounterAttack;
        public event System.Action<int, int> OnVictory; // (totalGold, totalCrystals)
        public event System.Action OnDefeat;
        public event System.Action OnSpecialAbilityUsed; // uso da habilidade especial

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
            if (boardManager != null)
                boardManager.enabled = true;

            currentHeroHP = heroData.maxHP;
            currentEnemyHP = enemyData.maxHP;
            currentEnergy = 0;
            movesSinceLastCounterAttack = 0;
            totalGoldGained = 0;
            totalCrystalsGained = 0;

            boardManager.spawner.Initialize(spawnConfig, hero, currentMode);
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

            totalGoldGained += result.goldGained;
            totalCrystalsGained += result.crystalsGained;

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
            // pois ainda nao sabemos se vamos colocar ataque especial ou nao. 
            // (ex.: SpecialAbilityResolver), acionado a partir daqui.

            return true;
        }

        private void CheckBattleEnd()
        {
            if (currentEnemyHP <= 0 && !battleEnded)
            {
                battleEnded = true;
                boardManager.enabled = false;

                if (playerWallet != null)
                {
                    playerWallet.AddGold(totalGoldGained);
                    playerWallet.AddCrystals(totalCrystalsGained);
                }

                OnVictory?.Invoke(totalGoldGained, totalCrystalsGained);
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