using System.Collections.Generic;
using UnityEngine;
using BlockeonsDratris.Board;

namespace BlockeonsDratris.Combat
{
    public class ChainTracker : MonoBehaviour
    {
        public int CurrentChainIndex { get; private set; }
        public float TotalDamageThisTurn { get; private set; }
        public float TotalHealThisTurn { get; private set; }

        public System.Action<int, float> OnChainStepResolved; // chainIndex, damageThisStep
        public System.Action OnTurnResolved; // fim de todo o chain (board estabilizou)

        public void ResetTurn()
        {
            CurrentChainIndex = 0;
            TotalDamageThisTurn = 0f;
            TotalHealThisTurn = 0f;
        }

        public void RegisterChainStep(int chainIndex, float damage, float heal)
        {
            CurrentChainIndex = chainIndex;
            TotalDamageThisTurn += damage;
            TotalHealThisTurn += heal;

            OnChainStepResolved?.Invoke(chainIndex, damage);
        }

        public void FinishTurn()
        {
            OnTurnResolved?.Invoke();
        }
    }
}