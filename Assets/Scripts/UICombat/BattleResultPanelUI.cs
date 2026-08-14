using UnityEngine;
using UnityEngine.UI;
using BlockeonsDratris.Core;

namespace BlockeonsDratris.UI
{
    public class BattleResultPanelUI : MonoBehaviour
    {
        [Header("Painéis")]
        public GameObject victoryPanel;
        public GameObject defeatPanel;

        [Header("Botões")]
        public Button retryButton;
        public Button continueButton;

        [Header("Dependência")]
        public GameCore gameCore;

        void Awake()
        {
            HideAll();

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);
        }

        public void ShowVictory()
        {
            HideAll();
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        public void ShowDefeat()
        {
            HideAll();
            if (defeatPanel != null) defeatPanel.SetActive(true);
        }

        private void HideAll()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        private void OnRetryClicked()
        {
            HideAll();
            if (gameCore != null) gameCore.RestartBattle();
        }
    }
}