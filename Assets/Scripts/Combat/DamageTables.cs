using System.Collections.Generic;
using BlockeonsDratris.Data;

namespace BlockeonsDratris.Combat
{
    public static class DamageTables
    {
        // Multiplicadores de afinidade: quanto o bloco X é forte/fraco/neutro
        // em relação ao HeroData.strongBlock / neutralBlock / weakBlock do herói ativo.
        public const float StrongMultiplier = 1.5f;
        public const float NeutralMultiplier = 1.0f;
        public const float WeakMultiplier = 0.5f;
        public const float FixedHealPerBlock = 8f;
        public const float ShieldPerBlock = 1f;

        public static float GetAffinityMultiplier(OffensiveBlockType blockType, SOHeroData hero)
        {
            if (blockType == hero.strongBlock)
                return StrongMultiplier;

            if (blockType == hero.weakBlock)
                return WeakMultiplier;

            return NeutralMultiplier;
        }

        // Multiplicador de chain (elo 1, elo 2, elo 3...). Cresce de forma controlada.
        private static readonly Dictionary<int, float> ChainMultipliers = new Dictionary<int, float>
        {
            { 1, 1.0f },
            { 2, 1.3f },
            { 3, 1.6f },
            { 4, 2.0f },
            { 5, 2.5f },
        };

        public static float GetChainMultiplier(int chainIndex)
        {
            if (ChainMultipliers.TryGetValue(chainIndex, out float value))
                return value;

            // Acima do índice 5, cresce +0.5 por elo extra (sem limite, mas com custo de balance a testar)
            return ChainMultipliers[5] + (chainIndex - 5) * 0.5f;
        }

        // Bônus por tamanho do grupo (match de 3 = base, 4 = +25%, 5+ = +50%)
        public static float GetGroupSizeMultiplier(int groupSize)
        {
            if (groupSize <= 3) return 1.0f;
            if (groupSize == 4) return 1.25f;
            return 1.5f;
        }
    }
}