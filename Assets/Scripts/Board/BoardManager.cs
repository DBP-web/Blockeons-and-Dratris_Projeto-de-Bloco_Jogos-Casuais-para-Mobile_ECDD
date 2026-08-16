using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockeonsDratris.Blocks;

namespace BlockeonsDratris.Board
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Configuração do Grid")]
        public int columns = 5;
        public int rows = 6;
        public float cellSize = 1f;
        public Vector2 boardOrigin = Vector2.zero;

        [Header("Referências")]
        public BlockSpawner spawner;

        [Header("Eventos")]
        public System.Action<List<MatchGroup>, int> OnChainStep; // matches, chainIndex (1-based)
        public System.Action OnBoardStable;

        private BlockBase[,] grid;
        private MatchFinder matchFinder;
        private bool isProcessing = false;

        private BlockBase selectedBlock = null;
        private Vector2 touchStartPos;
        private const float SwipeThreshold = 0.3f;

        void Awake()
        {
            grid = new BlockBase[columns, rows];
            matchFinder = new MatchFinder(columns, rows);
        }

        public void SetupBoard()
        {
            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    SpawnBlockAt(c, r);
                }
            }
        }

        public void ClearBoard()
        {
            // Interrompe qualquer swap/chain em andamento (importante no retry,
            // pois a corrotina antiga pode ainda estar rodando e usando o grid velho)
            StopAllCoroutines();
            isProcessing = false;
            selectedBlock = null;

            if (grid != null)
            {
                for (int c = 0; c < columns; c++)
                {
                    for (int r = 0; r < rows; r++)
                    {
                        if (grid[c, r] != null)
                        {
                            Destroy(grid[c, r].gameObject);
                            grid[c, r] = null;
                        }
                    }
                }
            }

            // Segurança extra: qualquer bloco que ainda seja filho deste transform
            // mas não esteja mais referenciado no grid (ex: preso numa animação
            // de swap/queda no momento do retry) também é destruído.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            grid = new BlockBase[columns, rows];
        }

        private void SpawnBlockAt(int c, int r)
        {
            GameObject obj = spawner.SpawnRandomBlock(transform);
            BlockBase block = obj.GetComponent<BlockBase>();
            block.Setup(c, r);
            block.SetWorldPosition(GridToWorld(c, r));
            grid[c, r] = block;
        }

        private Vector3 GridToWorld(int c, int r)
        {
            return new Vector3(boardOrigin.x + c * cellSize, boardOrigin.y + r * cellSize, 0f);
        }

        void Update()
        {
            if (isProcessing) return;

            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    selectedBlock = GetBlockUnderScreenPoint(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended && selectedBlock != null)
                {
                    Vector2 delta = touch.position - touchStartPos;
                    TrySwipe(delta);
                    selectedBlock = null;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                selectedBlock = GetBlockUnderScreenPoint(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && selectedBlock != null)
            {
                Vector2 delta = (Vector2)Input.mousePosition - touchStartPos;
                TrySwipe(delta);
                selectedBlock = null;
            }
        }

        private BlockBase GetBlockUnderScreenPoint(Vector2 screenPos)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null)
            {
                return hit.collider.GetComponent<BlockBase>();
            }
            return null;
        }

        private void TrySwipe(Vector2 delta)
        {
            if (delta.magnitude < SwipeThreshold) return;
            if (selectedBlock == null) return;

            int dx = 0, dy = 0;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                dx = delta.x > 0 ? 1 : -1;
            }
            else
            {
                dy = delta.y > 0 ? 1 : -1;
            }

            int targetC = selectedBlock.column + dx;
            int targetR = selectedBlock.row + dy;

            if (targetC < 0 || targetC >= columns || targetR < 0 || targetR >= rows) return;

            BlockBase targetBlock = grid[targetC, targetR];
            if (targetBlock == null) return;

            StartCoroutine(SwapAndResolve(selectedBlock, targetBlock));
        }

        private IEnumerator SwapAndResolve(BlockBase a, BlockBase b)
        {
            isProcessing = true;

            SwapBlocksInGrid(a, b);
            yield return AnimateSwap(a, b);

            var matches = matchFinder.FindAllMatches(grid);

            if (matches.Count == 0)
            {
                // Desfaz a jogada, pois não gerou nenhum match
                SwapBlocksInGrid(a, b);
                yield return AnimateSwap(a, b);
                isProcessing = false;
                yield break;
            }

            yield return ResolveChainLoop(matches);

            isProcessing = false;
            OnBoardStable?.Invoke();
        }

        private void SwapBlocksInGrid(BlockBase a, BlockBase b)
        {
            int ac = a.column, ar = a.row;
            int bc = b.column, br = b.row;

            grid[ac, ar] = b;
            grid[bc, br] = a;

            a.SetGridPosition(bc, br);
            b.SetGridPosition(ac, ar);
        }

        private IEnumerator AnimateSwap(BlockBase a, BlockBase b)
        {
            Vector3 posA = GridToWorld(a.column, a.row);
            Vector3 posB = GridToWorld(b.column, b.row);

            float duration = 0.15f;
            float elapsed = 0f;

            Vector3 startA = a.transform.position;
            Vector3 startB = b.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                a.transform.position = Vector3.Lerp(startA, posA, t);
                b.transform.position = Vector3.Lerp(startB, posB, t);
                yield return null;
            }

            a.transform.position = posA;
            b.transform.position = posB;
        }

        private IEnumerator ResolveChainLoop(List<MatchGroup> firstMatches)
        {
            var currentMatches = firstMatches;
            int chainIndex = 1;

            while (currentMatches != null && currentMatches.Count > 0)
            {
                OnChainStep?.Invoke(currentMatches, chainIndex);

                yield return RemoveMatchedBlocks(currentMatches);
                yield return ApplyGravityAndRefill();

                currentMatches = matchFinder.FindAllMatches(grid);
                chainIndex++;
            }
        }

        private IEnumerator RemoveMatchedBlocks(List<MatchGroup> matches)
        {
            foreach (var group in matches)
            {
                foreach (var block in group.blocks)
                {
                    if (block == null) continue;
                    grid[block.column, block.row] = null;
                    Destroy(block.gameObject);
                }
            }
            yield return null;
        }

        private IEnumerator ApplyGravityAndRefill()
        {
            for (int c = 0; c < columns; c++)
            {
                int writeRow = 0;

                for (int r = 0; r < rows; r++)
                {
                    if (grid[c, r] != null)
                    {
                        if (writeRow != r)
                        {
                            BlockBase block = grid[c, r];
                            grid[c, writeRow] = block;
                            grid[c, r] = null;
                            block.SetGridPosition(c, writeRow);
                        }
                        writeRow++;
                    }
                }

                for (int r = writeRow; r < rows; r++)
                {
                    SpawnBlockAt(c, r);
                }
            }

            yield return AnimateFallToGridPositions();
        }

        private IEnumerator AnimateFallToGridPositions()
        {
            float duration = 0.2f;
            float elapsed = 0f;

            var startPositions = new Dictionary<BlockBase, Vector3>();
            var targetPositions = new Dictionary<BlockBase, Vector3>();

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    BlockBase block = grid[c, r];
                    if (block == null) continue;

                    Vector3 target = GridToWorld(c, r);
                    if (block.transform.position != target)
                    {
                        startPositions[block] = block.transform.position;
                        targetPositions[block] = target;
                    }
                }
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                foreach (var kvp in targetPositions)
                {
                    BlockBase block = kvp.Key;
                    if (block == null) continue;
                    block.transform.position = Vector3.Lerp(startPositions[block], kvp.Value, t);
                }

                yield return null;
            }

            foreach (var kvp in targetPositions)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.transform.position = kvp.Value;
                }
            }
        }
    }
}