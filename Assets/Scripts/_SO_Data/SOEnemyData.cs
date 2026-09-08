using UnityEngine;

namespace BlockeonsDratris.Data
{
    [CreateAssetMenu(fileName = "SOEnemyData", menuName = "BlockeonsDratris/Enemy Data")]
    public class SOEnemyData : ScriptableObject
    {
        [Header("Identidade")]
        public string enemyName;
        public Sprite portrait;

        [Header("Status")]
        public int maxHP = 500;
        public int counterAttackDamage = 10;

        [Header("Comportamento")]
        [Tooltip("A cada quantas jogadas do jogador o inimigo contra-ataca")]
        public int movesPerCounterAttack = 3;
    }
}