using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockeonsDratris.Core
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Cena de destino ao iniciar o jogo")]
        public string battleSceneName = "Battle_Scene";

        public void OnStartGameClicked()
        {
            SceneManager.LoadScene(battleSceneName);
        }
    }
}