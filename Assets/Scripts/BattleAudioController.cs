using System.Collections.Generic;
using UnityEngine;
using BlockeonsDratris.Board;
using BlockeonsDratris.Blocks;
using BlockeonsDratris.Combat;
using BlockeonsDratris.Data;

namespace BlockeonsDratris.Audio
{
    public class BattleAudioController : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private BoardManager boardManager;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip sword;
        [SerializeField] private AudioClip bowShot;
        [SerializeField] private AudioClip cancel;
        [SerializeField] private AudioClip heroHurt;
        [SerializeField] private AudioClip enemyHurt;
        [SerializeField] private AudioClip heal;
        [SerializeField] private AudioClip vial;
        [SerializeField] private AudioClip shield;
        [SerializeField] private AudioClip special;
        [SerializeField] private AudioClip specialAttack;
        [SerializeField] private AudioClip victory;
        [SerializeField] private AudioClip defeat;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.OnHeroHPChanged += HandleHeroHPChanged;
                battleManager.OnEnemyHPChanged += HandleEnemyHPChanged;
                battleManager.OnEnergyChanged += HandleEnergyChanged;
                battleManager.OnShieldChanged += HandleShieldChanged;
                battleManager.OnStepResolved += HandleStepResolved;
                battleManager.OnEnemyCounterAttack += HandleEnemyCounterAttack;
                battleManager.OnVictory += HandleVictory;
                battleManager.OnDefeat += HandleDefeat;
                battleManager.OnSpecialAbilityUsed += HandleSpecialAbilityUsed;
            }

            if (boardManager != null)
            {
                boardManager.OnChainStep += HandleChainStep;
                boardManager.OnInvalidMove += HandleInvalidMove;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.OnHeroHPChanged -= HandleHeroHPChanged;
                battleManager.OnEnemyHPChanged -= HandleEnemyHPChanged;
                battleManager.OnEnergyChanged -= HandleEnergyChanged;
                battleManager.OnShieldChanged -= HandleShieldChanged;
                battleManager.OnStepResolved -= HandleStepResolved;
                battleManager.OnEnemyCounterAttack -= HandleEnemyCounterAttack;
                battleManager.OnVictory -= HandleVictory;
                battleManager.OnDefeat -= HandleDefeat;
                battleManager.OnSpecialAbilityUsed -= HandleSpecialAbilityUsed;
            }

            if (boardManager != null)
            {
                boardManager.OnChainStep -= HandleChainStep;
                boardManager.OnInvalidMove -= HandleInvalidMove;
            }
        }

        private void HandleHeroHPChanged(int current, int max)
        {
            // Evita tocar som no início da batalha.
            if (current < max)
                Play(heroHurt);
        }

        private void HandleEnemyHPChanged(int current, int max)
        {
            // Dano no inimigo será tratado pelo resultado do match.
        }

        private void HandleEnergyChanged(int current, int max)
        {
            if (current >= max)
                Play(special);
        }

        private void HandleShieldChanged(int currentShield)
        {
            // O escudo só deve tocar quando uma quantidade nova é recebida.
            if (currentShield > 0)
                Play(shield);
        }

        private void HandleStepResolved(CombatResult result, int chainIndex)
        {
            if (result.totalHealToHero > 0)
                Play(heal);

            if (result.shieldGained > 0)
                Play(shield);
        }

        private void HandleEnemyCounterAttack(int damage)
        {
            Play(heroHurt);
        }

        private void HandleVictory(int goldGained, int crystalsGained)
        {
            Play(victory);
        }

        private void HandleDefeat()
        {
            Play(defeat);
        }

        private void HandleSpecialAbilityUsed()
        {
            Play(specialAttack);
        }

        private void HandleChainStep(List<MatchGroup> matches, int chainIndex)
        {
            foreach (MatchGroup group in matches)
            {
                if (group == null || group.blocks == null || group.blocks.Count == 0)
                    continue;

                BlockBase sample = group.blocks[0];

                switch (sample)
                {
                    case OffensiveBlock offensive:

                        switch (offensive.offensiveType)
                        {
                            case OffensiveBlockType.Espada:
                                Play(sword);
                                break;

                            case OffensiveBlockType.Flecha:
                                Play(bowShot);
                                break;

                            case OffensiveBlockType.Pocao:
                                Play(vial);
                                break;
                        }

                        Play(enemyHurt);
                        break;

                    case SupportBlock support:

                        if (support.supportType == SupportBlockType.Cura)
                            Play(heal);

                        break;

                    case ShieldBlock:
                        Play(shield);
                        break;
                }
            }
        }

        private void HandleInvalidMove()
        {
            Play(cancel);
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || audioSource == null)
                return;

            audioSource.PlayOneShot(clip);
        }
    }
}