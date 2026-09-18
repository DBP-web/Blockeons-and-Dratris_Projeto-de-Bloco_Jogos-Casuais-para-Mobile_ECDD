using BlockeonsDratris.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BlockeonsDratris.UI
{
    public class BattleResultPanelUI : MonoBehaviour
    {
        [Header("Painéis")]
        public GameObject victoryPanel;
        public GameObject defeatPanel;

        [Header("Textos de Recompensa (Vitória)")]
        public TextMeshProUGUI goldGainedText;
        public TextMeshProUGUI crystalsGainedText;

        [Header("Botões")]
        public Button retryButton;
        public Button continueButton;

        [Header("Cena de destino do botão Continue")]
        public string mainMenuSceneName = "Tittle_Screen";

        [Header("Dependência")]
        public GameCore gameCore;

        void Awake()
        {
            HideAll();

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        // Nova versão: recebe os ganhos e exibe na UI
        public void ShowVictory(int goldGained, int crystalsGained)
        {
            HideAll();

            if (victoryPanel != null) victoryPanel.SetActive(true);

            if (goldGainedText != null)
                goldGainedText.text = $"+{goldGained}";

            if (crystalsGainedText != null)
                crystalsGainedText.text = $"+{crystalsGained}";
        }

        // Overload mantido para compatibilidade, caso algo ainda chame sem parâmetros
        public void ShowVictory()
        {
            ShowVictory(0, 0);
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

        private void OnContinueClicked()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}