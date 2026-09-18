using UnityEngine;

namespace BlockeonsDratris.Data
{
    [CreateAssetMenu(fileName = "SOGameModeConfig", menuName = "BlockeonsDratris/Game Mode Config")]
    public class SOGameModeConfig : ScriptableObject
    {
        public GameModeType modeType;

        [Header("Pesos de afinidade ofensiva (%)")]
        [Tooltip("Soma deve ser 100")]
        public float strongWeight = 33.3f;
        public float neutralWeight = 33.3f;
        public float weakWeight = 33.3f;

        void OnValidate()
        {
            float total = strongWeight + neutralWeight + weakWeight;
            if (Mathf.Abs(total - 100f) > 0.1f)
            {
                Debug.LogWarning($"[{name}] Pesos de afinidade não somam 100 (atual: {total}). " +
                                  "Ajuste strongWeight/neutralWeight/weakWeight.");
            }
        }

    }
}