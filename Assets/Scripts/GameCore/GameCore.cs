using UnityEngine;
using BlockeonsDratris.Data;
using BlockeonsDratris.Combat;

namespace BlockeonsDratris.Core
{
    public class GameCore : MonoBehaviour
    {
        [Header("Referências de Cena")]
        public BattleManager battleManager;

        [Header("Seleção Atual (definida pelo menu/seleção de herói)")]
        public SOHeroData selectedHero;
        public SOEnemyData selectedEnemy;

        void Start()
        {
            // Ponto único de entrada da batalha.
            // Futuramente, selectedHero/selectedEnemy podem vir de uma
            // tela de seleção anterior (ex.: via PlayerPrefs, singleton de
            // sessão, ou parâmetros de cena), em vez de fixos no Inspector.
            if (selectedHero != null && selectedEnemy != null)
            {
                battleManager.StartBattle(selectedHero, selectedEnemy);
            }
            else
            {
                Debug.LogError("GameCore: selectedHero ou selectedEnemy não foram definidos. " +
                                "A batalha não pode ser iniciada sem esses dados.");
            }
        }

        // Chamado por telas de seleção de herói/inimigo antes de carregar a cena de batalha,
        // ou por um sistema de progressão (mapa de fases, etc.).
        public void SetSelection(SOHeroData hero, SOEnemyData enemy)
        {
            selectedHero = hero;
            selectedEnemy = enemy;
        }

        // Útil para reiniciar a batalha (ex.: botão "Tentar novamente" após derrota)
        // sem precisar recarregar a cena inteira.
        public void RestartBattle()
        {
            if (selectedHero != null && selectedEnemy != null)
            {
                battleManager.StartBattle(selectedHero, selectedEnemy);
            }
        }
    }
}