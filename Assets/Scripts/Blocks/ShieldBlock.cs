using BlockeonsDratris.Data;
using UnityEngine;

namespace BlockeonsDratris.Blocks
{
    public class ShieldBlock : BlockBase
    {
        [Header("Tipo Defensivo")]
        public ShieldBlockType shieldBlocktype;

        [Header("Valor de Escudo")]
        [Tooltip("Quanto de dano esse bloco absorve/reduz quando quebrado, se aplicável no futuro")]
        public int shieldValue = 5;

        public override void Setup(int col, int r)
        {
            base.Setup(col, r);
            category = BlockCategory.Shield;
        }
    }
}