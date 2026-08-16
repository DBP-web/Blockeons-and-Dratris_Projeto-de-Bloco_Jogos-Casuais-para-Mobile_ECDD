using UnityEngine;
using BlockeonsDratris.Data;

namespace BlockeonsDratris.Blocks
{
    public enum BlockCategory
    {
        Offensive,
        Support,
        Shield
    }

    public enum SupportBlockType
    {
        Cura,
        Ouro,
        Cristal
    }
    public enum ShieldBlockType
    {
        Shield
    }

    public abstract class BlockBase : MonoBehaviour
    {
        [Header("Identidade do Bloco")]
        public BlockCategory category;

        [Header("Visual")]
        public SpriteRenderer spriteRenderer;

        [Header("Posição no Grid")]
        public int column;
        public int row;

        public virtual void Setup(int col, int r)
        {
            column = col;
            row = r;
        }

        public void SetGridPosition(int col, int r)
        {
            column = col;
            row = r;
        }

        public void SetWorldPosition(Vector3 worldPos)
        {
            transform.position = worldPos;
        }
    }
}