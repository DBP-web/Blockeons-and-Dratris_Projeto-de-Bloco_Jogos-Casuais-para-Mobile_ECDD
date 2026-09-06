using BlockeonsDratris.Blocks;
using UnityEngine;

namespace BlockeonsDratris.Data
{
    [System.Serializable]
    public class OffensiveBlockVisual
    {
        public OffensiveBlockType type;
        public Sprite sprite;
    }

    [System.Serializable]
    public class SupportBlockVisual
    {
        public SupportBlockType type;
        public Sprite sprite;
    }

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

        [Header("Sprites - Blocos Ofensivos (3 tipos)")]
        public OffensiveBlockVisual[] offensiveVisuals;

        [Header("Sprites - Blocos de Suporte (3 tipos)")]
        public SupportBlockVisual[] supportVisuals;

        [Header("Sprite - Bloco de Escudo")]
        public Sprite shieldSprite;

        public Sprite GetSpriteFor(OffensiveBlockType type) 
        {
            foreach (var visual in offensiveVisuals)
            {
                if (visual.type == type)
                    return visual.sprite;
            }
            Debug.Log($"[{name}] Sprite não encontrado para OffensiveBlockType {type}.");
            return null;
        }
        public Sprite GetSpriteFor(SupportBlockType type) 
        {
            foreach (var visual in supportVisuals)
            {
                if (visual.type == type)
                    return visual.sprite;
            }
            Debug.Log($"[{name}] Sprite não encontrado para SupportBlockType {type}.");
            return null;

        }

        public SOGameModeConfig GetConfigForMode(GameModeType mode)
        {
            foreach (var config in gameModes)
            {
                if (config.modeType == mode) return config;
            }
            Debug.Log($"GameModeConfig não encontrado para o modo {mode}. Usando o primeiro disponível.");
            return gameModes.Length > 0 ? gameModes[0] : null;
        }

        void OnValidate()
        {
            float rareGroupTotal = healChance + goldChance + crystalChance;
            if (Mathf.Abs(rareGroupTotal - 100f) > 0.1f)
                Debug.Log($"[{name}] healChance + goldChance + crystalChance não soma 100 (atual: {rareGroupTotal}).");

            float commonGroupTotal = offensiveChance + shieldChance;
            if (Mathf.Abs(commonGroupTotal - 100f) > 0.1f)
                Debug.Log($"[{name}] offensiveChance + shieldChance não soma 100 (atual: {commonGroupTotal}).");
        }
    }
}
