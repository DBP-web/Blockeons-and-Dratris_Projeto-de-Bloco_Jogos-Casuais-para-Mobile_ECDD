using UnityEngine;

namespace BlockeonsDratris.Data
{
    [CreateAssetMenu(fileName = "SOHeroData", menuName = "BlockeonsDratris/Hero Data")]
    public class SOHeroData : ScriptableObject
    {
        [Header("Identidade")]
        public string heroName;
        public HeroClassType classType;
        public Sprite portrait;

        [Header("Status Base")]
        public int maxHP = 100;
        public int baseDamage = 10;
        public int strength;
        public int intelligence;
        public int dexterity;

        [Header("Energia (para habilidades especiais via Tap/Hold)")]
        public int maxEnergy = 100;
        [Tooltip("Custo de energia para ativar a habilidade especial do herói")]
        public int specialAbilityCost = 100;

        [Header("Afinidade Ofensiva (Forte/Neutro/Fraco)")]
        public OffensiveBlockType strongBlock;
        public OffensiveBlockType neutralBlock;
        public OffensiveBlockType weakBlock;

        public AffinityLevel GetAffinity(OffensiveBlockType blockType)
        {
            if (blockType == strongBlock) return AffinityLevel.Forte;
            if (blockType == neutralBlock) return AffinityLevel.Neutro;
            return AffinityLevel.Fraco;
        }
    }
}
