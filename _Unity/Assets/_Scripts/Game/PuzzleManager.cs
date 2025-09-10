using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public TimerManager timerManager;
    public TMP_Text clickToStartText;
    public FlashEffect flashEffect;

    public GameObject emptyTilePrefab;
    public Texture2D puzzleImage;

    public GameObject tilePrefab;
    public int width = 3;
    public int height = 3;
    public float spacing = 0.1f;
    public float fadeDuration = 0.4f;

    private Vector2Int emptyPos;
    private GameObject emptyTileInstance;
    private bool isShuffling = true;
    private Material[] puzzleMaterials;
    private Tile[,] tiles;
    private bool tilesRevealed;
    private bool waitingForReveal;
    private bool puzzleCleared = false;

    // 퍼즐 초기화 전에는 아무 동작 안 함
    private void Start()
    {
        // 이제 퍼즐 생성 로직은 InitializePuzzle에서만 동작
    }

    private void Update()
    {
        if (waitingForReveal && !tilesRevealed)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                foreach (var tile in tiles)
                    if (tile != null)
                        tile.Restore();

                tilesRevealed = true;
                waitingForReveal = false;
                if (timerManager != null)
                    timerManager.StartTimer();
                if (clickToStartText != null)
                    clickToStartText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 외부에서 명시적으로 호출하는 퍼즐 초기화 함수
    /// </summary>
    public void InitializePuzzle(Texture2D texture, int width, int height)
    {
        this.puzzleImage = texture;
        this.width = width;
        this.height = height;
        this.puzzleCleared = false;
        this.tilesRevealed = false;
        this.waitingForReveal = false;

        if (clickToStartText != null)
            clickToStartText.gameObject.SetActive(false);

        GeneratePuzzle();
        CacheMaterials();
        StartCoroutine(FadeInTiles());
        StartCoroutine(ShufflePuzzle());
    }

    // 이하 기존 함수 동일
    private void CacheMaterials()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var mats = new List<Material>();
        foreach (var r in renderers)
            if (r.material != null)
                mats.Add(r.material);
        puzzleMaterials = mats.ToArray();
    }

    private IEnumerator FadeInTiles()
    {
        foreach (var mat in puzzleMaterials)
        {
            var c = mat.color;
            mat.color = new Color(c.r, c.g, c.b, 0f);
        }

        var t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            var a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            foreach (var mat in puzzleMaterials)
            {
                var c = mat.color;
                mat.color = new Color(c.r, c.g, c.b, a);
            }
            yield return null;
        }
    }

    public IEnumerator FadeOutTiles(Material[] materials, float duration)
    {
        var t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            var alpha = Mathf.Lerp(1f, 0f, t / duration);
            foreach (var mat in materials)
            {
                var c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
            if (emptyTileInstance != null)
            {
                var empty = emptyTileInstance.GetComponent<EmptyTile>();
                if (empty != null)
                    empty.SetFadeOut(t / duration);
            }
            yield return null;
        }
        foreach (var tile in GetComponentsInChildren<Tile>())
            tile.gameObject.SetActive(false);
        if (emptyTileInstance != null)
            emptyTileInstance.SetActive(false);
    }

    private void GeneratePuzzle()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        tiles = new Tile[width, height];

        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (x == 0 && y == 0)
                {
                    emptyPos = new Vector2Int(x, y);

                    emptyTileInstance = Instantiate(emptyTilePrefab, transform);
                    emptyTileInstance.transform.localPosition = GetTilePosition(x, y);

                    var emptyTile = emptyTileInstance.GetComponent<EmptyTile>();
                    emptyTile.Init(puzzleImage, width, height, x, y);
                    emptyTile.SetAlpha(0f);
                    emptyTile.OnClick += HandleEmptyTileClick;

                    continue;
                }

                var obj = Instantiate(tilePrefab, transform);
                obj.transform.localPosition = GetTilePosition(x, y);

                var tile = obj.GetComponent<Tile>();
                tile.Init(this, x, y, x, y, puzzleImage, width, height);
                tiles[x, y] = tile;
            }

        Debug.Log("퍼즐 생성!!");
    }

    public Vector3 GetTilePosition(int x, int y)
    {
        var centerOffset = new Vector3((width - 1) / 2f, (height - 1) / 2f, 0);
        return new Vector3(
            (x - centerOffset.x) * (1 + spacing),
            (centerOffset.y - y) * (1 + spacing),
            0f
        );
    }

    private IEnumerator ShufflePuzzle()
    {
        isShuffling = true;
        yield return new WaitForSeconds(0.5f);

        var previousMove = Vector2Int.zero;
        Vector2Int[] directions =
        {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0)
        };

        for (var i = 0; i < 100; i++)
        {
            var validMoves = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                var next = emptyPos + dir;
                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height) continue;
                if (dir == -previousMove) continue;
                if (tiles[next.x, next.y] != null)
                    validMoves.Add(dir);
            }

            if (validMoves.Count > 0)
            {
                var move = validMoves[Random.Range(0, validMoves.Count)];
                var targetPos = emptyPos + move;
                TryMove(targetPos.x, targetPos.y);
                previousMove = move;
            }

            if (i >= 60)
            {
                var fadeT = Mathf.InverseLerp(60, 80, i);
                foreach (var tile in tiles)
                    if (tile != null)
                        tile.SetFadeGray(fadeT);
            }

            yield return new WaitForSeconds(0.02f);
        }

        isShuffling = false;
        waitingForReveal = true;
        tilesRevealed = false;

        CheckComplete();
        if (clickToStartText != null)
            clickToStartText.gameObject.SetActive(true);
    }

    public void TryMove(Tile tile) => TryMove(tile.gridPosition.x, tile.gridPosition.y);

    private bool CheckCompleteStatus()
    {
        foreach (var tile in tiles)
            if (tile != null && !tile.IsCorrect())
                return false;
        return true;
    }

    public void TryMove(int x, int y)
    {
        if (puzzleCleared) return;
        if (isShuffling && tilesRevealed) return; // 게임 시작 후에만 셔플링 체크

        var distance = Mathf.Abs(emptyPos.x - x) + Mathf.Abs(emptyPos.y - y);
        if (distance != 1) return;

        var tile = tiles[x, y];
        if (tile == null) return;

        tiles[emptyPos.x, emptyPos.y] = tile;
        tiles[x, y] = null;

        tile.MoveTo(emptyPos);
        emptyPos = new Vector2Int(x, y);

        if (emptyTileInstance != null)
            emptyTileInstance.transform.localPosition = GetTilePosition(emptyPos.x, emptyPos.y);

        CheckComplete();
    }

    private void CheckComplete()
    {
        if (CheckCompleteStatus() && !isShuffling)
        {
            puzzleCleared = true;

            if (timerManager != null)
                timerManager.StopTimer();

            if (flashEffect != null)
                flashEffect.PlayFlash();

            Debug.Log("퍼즐 클리어!");
        }
    }

    private void HandleEmptyTileClick()
    {
        Debug.Log("빈 타일 클릭됨");
    }
}