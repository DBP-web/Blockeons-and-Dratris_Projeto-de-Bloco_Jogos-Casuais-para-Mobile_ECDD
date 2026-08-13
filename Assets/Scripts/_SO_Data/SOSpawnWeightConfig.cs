using UnityEngine;

namespace BlockeonsDratris.Data
{
    [CreateAssetMenu(fileName = "SOSpawnWeightConfig", menuName = "BlockeonsDratris/Spawn Weight Config")]
    public class SOSpawnWeightConfig : ScriptableObject
    {
        [Header("Sorteio 1: Raro vs Comum")]
        [Range(0f, 100f)] public float rareChance = 15f;

        [Header("Sorteio 2: Qual Raro (só é usado se Sorteio 1 cair em raro)")]
        [Range(0f, 100f)] public float healChance = 50f;
        [Range(0f, 100f)] public float goldChance = 33f;
        [Range(0f, 100f)] public float crystalChance = 17f;

        [Header("Sorteio 3: Ofensivo vs Escudo (dentro de comum)")]
        [Range(0f, 100f)] public float offensiveChance = 75f;
        [Range(0f, 100f)] public float shieldChance = 25f;

        [Header("Modos de dificuldade disponíveis")]
        public SOGameModeConfig[] gameModes;

        public SOGameModeConfig GetConfigForMode(GameModeType mode)
        {
            foreach (var config in gameModes)
            {
                if (config.modeType == mode) return config;
            }
            Debug.LogWarning($"GameModeConfig não encontrado para o modo {mode}. Usando o primeiro disponível.");
            return gameModes.Length > 0 ? gameModes[0] : null;
        }

        void OnValidate()
        {
            float rareGroupTotal = healChance + goldChance + crystalChance;
            if (Mathf.Abs(rareGroupTotal - 100f) > 0.1f)
                Debug.LogWarning($"[{name}] healChance + goldChance + crystalChance não soma 100 (atual: {rareGroupTotal}).");

            float commonGroupTotal = offensiveChance + shieldChance;
            if (Mathf.Abs(commonGroupTotal - 100f) > 0.1f)
                Debug.LogWarning($"[{name}] offensiveChance + shieldChance não soma 100 (atual: {commonGroupTotal}).");
        }
    }
}
