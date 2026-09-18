using UnityEngine;

namespace BlockeonsDratris.Data
{
    [CreateAssetMenu(fileName = "SOPlayerWallet", menuName = "BlockeonsDratris/Player Wallet")]
    public class SOPlayerWallet : ScriptableObject
    {
        [Header("Saldo Atual (Runtime)")]
        [SerializeField] private int gold;
        [SerializeField] private int crystals;

        public int Gold => gold;
        public int Crystals => crystals;

        // Eventos para UI (HUD, loja, etc.)
        public event System.Action<int> OnGoldChanged;
        public event System.Action<int> OnCrystalsChanged;

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            gold += amount;
            OnGoldChanged?.Invoke(gold);
        }

        public void AddCrystals(int amount)
        {
            if (amount <= 0) return;
            crystals += amount;
            OnCrystalsChanged?.Invoke(crystals);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || gold < amount) return false;
            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            return true;
        }

        public bool TrySpendCrystals(int amount)
        {
            if (amount <= 0 || crystals < amount) return false;
            crystals -= amount;
            OnCrystalsChanged?.Invoke(crystals);
            return true;
        }

        // Útil para resetar em testes no Editor ou em um "New Game"
        public void ResetWallet(int startingGold = 0, int startingCrystals = 0)
        {
            gold = startingGold;
            crystals = startingCrystals;
            OnGoldChanged?.Invoke(gold);
            OnCrystalsChanged?.Invoke(crystals);
        }
    }
}