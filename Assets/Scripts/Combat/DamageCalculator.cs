using System.Collections.Generic;
using UnityEngine;
using BlockeonsDratris.Data;
using BlockeonsDratris.Blocks;
using BlockeonsDratris.Board;

namespace BlockeonsDratris.Combat
{
    public struct CombatResult
    {
        public float totalDamageToEnemy;
        public float totalHealToHero;
        public int goldGained;
        public int crystalsGained;
        public int energyGained;
    }

    public class DamageCalculator : MonoBehaviour
    {
        public SOHeroData heroData;

        [Header("Configuração de Energia")]
        [Tooltip("Energia gerada por bloco em um grupo de match (qualquer tipo)")]
        public int energyPerBlock = 5;

        public CombatResult CalculateStep(List<MatchGroup> matches, int chainIndex)
        {
            CombatResult result = new CombatResult();

            float chainMult = DamageTables.GetChainMultiplier(chainIndex);

            foreach (var group in matches)
            {
                float sizeMult = DamageTables.GetGroupSizeMultiplier(group.Size);

                if (group.blocks.Count == 0) continue;

                BlockBase sample = group.blocks[0];

                // Energia é gerada por qualquer bloco combinado, independente do tipo
                result.energyGained += energyPerBlock * group.Size;

                switch (sample)
                {
                    case OffensiveBlock offensive:
                        {
                            float affinityMult = DamageTables.GetAffinityMultiplier(offensive.offensiveType, heroData);
                            float baseDamage = heroData.baseDamage * group.Size;
                            float finalDamage = baseDamage * affinityMult * sizeMult * chainMult;
                            result.totalDamageToEnemy += finalDamage;
                            break;
                        }

                    case SupportBlock support:
                        {
                            ApplySupportEffect(support, group.Size, sizeMult, chainMult, ref result);
                            break;
                        }

                    case ShieldBlock:
                        {
                            // Bloco de escudo não gera dano nem cura no protótipo atual.
                            // Reservado para futura mecânica de defesa/mitigação.
                            break;
                        }
                }
            }

            return result;
        }

        private void ApplySupportEffect(SupportBlock support, int groupSize, float sizeMult, float chainMult, ref CombatResult result)
        {
            switch (support.supportType)
            {
                case SupportBlockType.Cura:
                    float baseHeal = DamageTables.FixedHealPerBlock * groupSize;
                    result.totalHealToHero += baseHeal * sizeMult * chainMult;
                    break;

                case SupportBlockType.Ouro:
                    result.goldGained += Mathf.RoundToInt(groupSize * 10 * sizeMult);
                    break;

                case SupportBlockType.Cristal:
                    result.crystalsGained += Mathf.RoundToInt(groupSize * 1 * sizeMult);
                    break;
            }
        }
    }
}