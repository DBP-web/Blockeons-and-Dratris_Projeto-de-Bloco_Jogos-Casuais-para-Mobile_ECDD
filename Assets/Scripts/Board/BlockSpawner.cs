using UnityEngine;
using BlockeonsDratris.Data;
using BlockeonsDratris.Blocks;

namespace BlockeonsDratris.Board
{
    public class BlockSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject offensiveBlockPrefab;
        public GameObject supportBlockPrefab;
        public GameObject shieldBlockPrefab;

        [Header("Configuração (lida uma vez no início da run)")]
        public SOSpawnWeightConfig spawnConfig;
        public SOHeroData heroData;
        public GameModeType currentGameMode;

        private SOGameModeConfig activeModeConfig;

        public void Initialize(SOSpawnWeightConfig config, SOHeroData hero, GameModeType mode)
        {
            spawnConfig = config;
            heroData = hero;
            currentGameMode = mode;
            activeModeConfig = spawnConfig.GetConfigForMode(mode);
        }

        public GameObject SpawnRandomBlock(Transform parent)
        {
            // Sorteio 1: Raro vs Comum
            float roll1 = Random.Range(0f, 100f);

            if (roll1 <= spawnConfig.rareChance)
            {
                return SpawnRareBlock(parent);
            }
            else
            {
                return SpawnCommonBlock(parent);
            }
        }

        private GameObject SpawnRareBlock(Transform parent)
        {
            float roll2 = Random.Range(0f, 100f);
            SupportBlockType chosenType;

            if (roll2 <= spawnConfig.healChance)
            {
                chosenType = SupportBlockType.Cura;
            }
            else if (roll2 <= spawnConfig.healChance + spawnConfig.goldChance)
            {
                chosenType = SupportBlockType.Ouro;
            }
            else
            {
                chosenType = SupportBlockType.Cristal;
            }

            GameObject obj = Instantiate(supportBlockPrefab, parent);
            SupportBlock support = obj.GetComponent<SupportBlock>();
            support.supportType = chosenType;
            return obj;
        }

        private GameObject SpawnCommonBlock(Transform parent)
        {
            float roll3 = Random.Range(0f, 100f);

            if (roll3 <= spawnConfig.offensiveChance)
            {
                return SpawnOffensiveBlock(parent);
            }
            else
            {
                return Instantiate(shieldBlockPrefab, parent);
            }
        }

        private GameObject SpawnOffensiveBlock(Transform parent)
        {
            float roll4 = Random.Range(0f, 100f);

            OffensiveBlockType chosenType;

            if (roll4 <= activeModeConfig.strongWeight)
            {
                chosenType = heroData.strongBlock;
            }
            else if (roll4 <= activeModeConfig.strongWeight + activeModeConfig.neutralWeight)
            {
                chosenType = heroData.neutralBlock;
            }
            else
            {
                chosenType = heroData.weakBlock;
            }

            GameObject obj = Instantiate(offensiveBlockPrefab, parent);
            OffensiveBlock offensive = obj.GetComponent<OffensiveBlock>();
            offensive.offensiveType = chosenType;
            return obj;
        }
    }
}