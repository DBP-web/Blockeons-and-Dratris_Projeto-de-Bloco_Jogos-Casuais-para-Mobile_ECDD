using UnityEngine;

namespace BlockeonsDratris.Blocks
{
    public class SupportBlock : BlockBase
    {
        [Header("Tipo de Suporte")]
        public SupportBlockType supportType;

        [Header("Valor Base")]
        [Tooltip("Quantidade de cura, ouro ou cristal gerada por bloco quebrado")]
        public int baseValue = 5;

        public override void Setup(int col, int r)
        {
            base.Setup(col, r);
            category = BlockCategory.Support;
        }
    }
}