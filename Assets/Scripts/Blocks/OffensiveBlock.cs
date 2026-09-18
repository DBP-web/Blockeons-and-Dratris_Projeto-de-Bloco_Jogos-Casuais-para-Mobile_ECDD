using UnityEngine;
using BlockeonsDratris.Data;

namespace BlockeonsDratris.Blocks
{
    public class OffensiveBlock : BlockBase
    {
        [Header("Tipo Ofensivo")]
        public OffensiveBlockType offensiveType;

        [Header("Dano Base do Bloco")]
        public int baseValue = 10;

        public override void Setup(int col, int r)
        {
            base.Setup(col, r);
            category = BlockCategory.Offensive;
        }
    }
}