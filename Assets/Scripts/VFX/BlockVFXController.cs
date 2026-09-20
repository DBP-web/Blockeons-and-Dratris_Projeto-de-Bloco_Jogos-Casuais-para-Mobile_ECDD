using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using BlockeonsDratris.Blocks;

namespace BlockeonsDratris.Board
{
    /// <summary>
    /// Controlador centralizado de VFX (DOTween) para os blocos do tabuleiro.
    /// Não gerencia bloqueio de input: apenas dispara tweens e avisa via
    /// eventos/callbacks quando termina, para o BoardManager decidir
    /// quando liberar isProcessing. Áudio permanece 100% no BattleAudioController.
    /// </summary>
    public class BlockVFXController : MonoBehaviour
    {
        [Header("Seleção (Swipe) - Pulso de Escala")]
        [SerializeField] private float selectPulseScale = 1.15f;
        [SerializeField] private float selectPulseDuration = 0.35f;

        [Header("Swap")]
        [SerializeField] private float swapDuration = 0.2f;
        [SerializeField] private Ease swapEase = Ease.OutQuad;

        [Header("Queda com Quique (SetupBoard / Refill)")]
        [SerializeField] private float fallDuration = 0.28f;
        [SerializeField] private Ease fallEase = Ease.OutBounce;
        [SerializeField] private float fallSquashAmount = 0.85f;
        [SerializeField] private float fallSquashDuration = 0.08f;

        [Header("Sequência de Match")]
        [SerializeField] private float matchGrowScale = 1.25f;
        [SerializeField] private float matchGrowDuration = 0.12f;
        [SerializeField] private float matchShakeStrength = 0.08f;
        [SerializeField] private float matchShakeDuration = 0.15f;
        [SerializeField] private int matchShakeVibrato = 12;
        [SerializeField] private float matchShrinkDuration = 0.15f;

        [Header("Explosão (Flash + Partícula)")]
        [SerializeField] private float flashDuration = 0.12f;
        [SerializeField] private Color offensiveFlashColor = Color.red;
        [SerializeField] private Color supportFlashColor = Color.green;
        [SerializeField] private Color shieldFlashColor = Color.cyan;
        [SerializeField] private ParticleSystem explosionParticlePrefab;

        // Pulsos de seleção ativos, indexados por Transform (para poder parar por bloco)
        private readonly Dictionary<Transform, Tween> _activeSelectPulses = new Dictionary<Transform, Tween>();

        // Eventos para o BoardManager saber quando cada fase de animação terminou.
        // O BoardManager decide o que fazer com isProcessing a partir deles.
        public event Action OnFallAnimationComplete;
        public event Action OnMatchAnimationComplete;

        // Eventos "de instante", úteis para o BattleAudioController tocar SFX
        // no momento exato do impacto/explosão, sem o VFX precisar conhecer AudioClips.
        public event Action OnFallImpact;
        public event Action OnMatchStart;
        public event Action OnExplosionTriggered;


        // ---------------------------------------------------------------
        // SWAP - Move dois blocos trocando de posição (usado no BoardManager)
        // ---------------------------------------------------------------
        void Awake()
        {
            DOTween.SetTweensCapacity(500, 100); // ajuste conforme necessário
        }

        public void AnimateSwap(Transform block, Vector3 targetPos, Action onComplete)
        {
            if (block == null)
            {
                onComplete?.Invoke();
                return;
            }

            block.DOMove(targetPos, swapDuration)
                 .SetEase(swapEase)
                 .OnComplete(() => onComplete?.Invoke());
        }

        public void KillAllTweensFor(BlockBase block)
        {
            if (block == null) return;

            Transform t = block.transform;
            if (t != null) t.DOKill();

            if (block.spriteRenderer != null) block.spriteRenderer.DOKill();

            // Garante que não fique um pulso de seleção "preso" referenciando este bloco
            if (_activeSelectPulses.TryGetValue(t, out Tween pulse))
            {
                pulse.Kill();
                _activeSelectPulses.Remove(t);
            }
        }

        private void OnDestroy()
        {
            foreach (var kvp in _activeSelectPulses)
                kvp.Value?.Kill();

            _activeSelectPulses.Clear();
        }

        // ---------------------------------------------------------------
        // 1) SELEÇÃO (SWIPE) - Pulso de escala em Yoyo/Loop infinito
        // ---------------------------------------------------------------

        public void StartSelectPulse(Transform block)
        {
            if (block == null) return;

            StopSelectPulse(block);

            Vector3 baseScale = block.localScale;

            Tween pulse = block
                .DOScale(baseScale * selectPulseScale, selectPulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            _activeSelectPulses[block] = pulse;
        }

        public void StopSelectPulse(Transform block)
        {
            if (block == null) return;

            if (_activeSelectPulses.TryGetValue(block, out Tween pulse))
            {
                pulse.Kill();
                _activeSelectPulses.Remove(block);
            }

            block.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutQuad);
        }

        // ---------------------------------------------------------------
        // 2) QUEDA COM QUIQUE (SetupBoard e Refill pós-match)
        // ---------------------------------------------------------------

        /// <summary>
        /// Anima uma lista de blocos caindo simultaneamente até suas posições
        /// finais, com leve squash ao "tocar o chão" para dar sensação de
        /// solidez. Todos os blocos animam em paralelo.
        /// </summary>
        public void AnimateFall(List<(Transform block, Vector3 targetPos)> fallData, Action onComplete)
        {
            if (fallData == null || fallData.Count == 0)
            {
                onComplete?.Invoke();
                OnFallAnimationComplete?.Invoke();
                return;
            }

            int pending = fallData.Count;

            void CompleteOne()
            {
                pending--;
                if (pending <= 0)
                {
                    onComplete?.Invoke();
                    OnFallAnimationComplete?.Invoke();
                }
            }

            foreach (var entry in fallData)
            {
                Transform block = entry.block;
                Vector3 targetPos = entry.targetPos;

                if (block == null)
                {
                    CompleteOne();
                    continue;
                }

                Vector3 originalScale = block.localScale;

                Sequence seq = DOTween.Sequence();
                seq.Append(block.DOMove(targetPos, fallDuration).SetEase(fallEase));

                seq.Append(block.DOScale(new Vector3(
                        originalScale.x * (2f - fallSquashAmount),
                        originalScale.y * fallSquashAmount,
                        originalScale.z),
                    fallSquashDuration).SetEase(Ease.OutQuad));

                seq.Append(block.DOScale(originalScale, fallSquashDuration).SetEase(Ease.OutQuad));

                seq.OnComplete(() =>
                {
                    OnFallImpact?.Invoke();
                    CompleteOne();
                });
            }
        }

        // ---------------------------------------------------------------
        // 3) SEQUÊNCIA DE MATCH: cresce -> treme -> encolhe -> explode
        // ---------------------------------------------------------------

        /// <summary>
        /// Toca a sequência completa de match para um grupo de blocos
        /// (todos simultaneamente). Ao final de cada bloco, dispara a
        /// explosão (flash + partícula). O BoardManager deve aguardar
        /// onComplete antes de destruir os GameObjects / limpar o grid.
        /// </summary>
        public void PlayMatchSequence(List<BlockBase> blocks, Action onComplete)
        {
            if (blocks == null || blocks.Count == 0)
            {
                onComplete?.Invoke();
                OnMatchAnimationComplete?.Invoke();
                return;
            }

            OnMatchStart?.Invoke();

            int pending = blocks.Count;

            void CompleteOne()
            {
                pending--;
                if (pending <= 0)
                {
                    onComplete?.Invoke();
                    OnMatchAnimationComplete?.Invoke();
                }
            }

            foreach (var block in blocks)
            {
                if (block == null)
                {
                    CompleteOne();
                    continue;
                }

                Transform t = block.transform;
                Vector3 originalPos = t.position;
                Vector3 originalScale = t.localScale;

                Sequence seq = DOTween.Sequence();

                seq.Append(t.DOScale(originalScale * matchGrowScale, matchGrowDuration).SetEase(Ease.OutQuad));
                seq.Append(t.DOShakePosition(matchShakeDuration, matchShakeStrength, matchShakeVibrato, 90f, false, true));
                seq.Append(t.DOScale(Vector3.zero, matchShrinkDuration).SetEase(Ease.InQuad));

                seq.OnComplete(() =>
                {
                    if (block == null || t == null)
                    {
                        CompleteOne();
                        return;
                    }

                    t.position = originalPos; // corrige qualquer resíduo do shake

                    PlayExplosion(block, () =>
                    {
                        // Mata qualquer tween residual (Transform e SpriteRenderer) antes de liberar o bloco
                        KillAllTweensFor(block);
                        CompleteOne();
                    });
                });
            }
        }

        // ---------------------------------------------------------------
        // EXPLOSÃO: flash + partícula, variando cor por categoria do bloco
        // ---------------------------------------------------------------

        private void PlayExplosion(BlockBase block, Action onExplosionComplete)
        {
            if (block == null)
            {
                onExplosionComplete?.Invoke();
                return;
            }

            OnExplosionTriggered?.Invoke();

            SpriteRenderer sr = block.spriteRenderer;
            Vector3 explosionCenter = block.transform.position;
            Color flashColor = GetFlashColorFor(block.category);

            if (sr != null)
            {
                sr.DOColor(flashColor, flashDuration * 0.5f)
                  .SetEase(Ease.OutQuad)
                  .OnComplete(() => sr.DOFade(0f, flashDuration * 0.5f).SetEase(Ease.InQuad));
            }

            SpawnExplosionParticle(explosionCenter, block.category);

            DOVirtual.DelayedCall(flashDuration, () => onExplosionComplete?.Invoke());
        }

        private void SpawnExplosionParticle(Vector3 position, BlockCategory category)
        {
            if (explosionParticlePrefab == null) return;

            ParticleSystem instance = Instantiate(explosionParticlePrefab, position, Quaternion.identity);

            var main = instance.main;
            main.startColor = GetFlashColorFor(category);
            instance.Play();

            float lifetime = main.duration + main.startLifetime.constantMax;
            Destroy(instance.gameObject, lifetime);
        }

        private Color GetFlashColorFor(BlockCategory category)
        {
            switch (category)
            {
                case BlockCategory.Offensive: return offensiveFlashColor;
                case BlockCategory.Support: return supportFlashColor;
                case BlockCategory.Shield: return shieldFlashColor;
                default: return Color.white;
            }
        }
    }
}
