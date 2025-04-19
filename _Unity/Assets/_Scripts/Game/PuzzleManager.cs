using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public Texture2D puzzleImage;
    public GameObject tilePrefab;
    public GameObject emptyTilePrefab;
    public GameObject blindTilePrefab;
    public int width = 3;
    public int height = 3;
    public float spacing = 0.1f;
    public float fadeDuration = 0.4f;
    public TMP_Text timerText;

    private Tile[,] tiles;
    private GameObject[,] blindTiles;
    private Vector2Int emptyPos;
    private bool isShuffling = true;
    private bool isGameStarted = false;
    private bool isGameCompleted = false;
    private bool isPendingClear = false;
    private float elapsedTime = 0f;

    void Start()
    {
        if (GameData.difficulty < 2 || GameData.difficulty > 5)
            GameData.difficulty = 3;

        width = GameData.difficulty;
        height = GameData.difficulty;

        GeneratePuzzle();
        StartCoroutine(ShufflePuzzle());
    }

    void Update()
    {
        if (isGameStarted && !isGameCompleted)
        {
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f);

            timerText.text = $"{minutes:00} : {seconds:00} : {milliseconds:000}";
        }

        if (isPendingClear && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            isGameCompleted = true;
            isGameStarted = false;
            isPendingClear = false;
            Debug.Log("퍼즐 최종 완료 처리됨");
        }
    }

    void GeneratePuzzle()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        tiles = new Tile[width, height];
        blindTiles = new GameObject[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 && y == 0)
                {
                    emptyPos = new Vector2Int(x, y);
                    GameObject empty = Instantiate(emptyTilePrefab, transform);
                    empty.transform.localPosition = GetTilePosition(x, y);
                    continue;
                }

                GameObject obj = Instantiate(tilePrefab, transform);
                obj.transform.localPosition = GetTilePosition(x, y);

                Tile tile = obj.GetComponent<Tile>();
                tile.Init(this, x, y, x, y, puzzleImage, width, height);
                tiles[x, y] = tile;

                GameObject blind = Instantiate(blindTilePrefab, transform);
                blind.transform.localPosition = GetTilePosition(x, y);
                SetAlpha(blind, 0f);
                blindTiles[x, y] = blind;
            }
        }
    }

    public Vector3 GetTilePosition(int x, int y)
    {
        Vector3 centerOffset = new Vector3((width - 1) / 2f, (height - 1) / 2f, 0);
        return new Vector3((x - centerOffset.x) * (1 + spacing), (centerOffset.y - y) * (1 + spacing), 0f);
    }

    IEnumerator ShufflePuzzle()
    {
        isShuffling = true;
        yield return new WaitForSeconds(0.3f);

        int shuffleCount = width * height * 5;
        Vector2Int lastPos = emptyPos;

        for (int i = 0; i < shuffleCount; i++)
        {
            List<Vector2Int> possibleMoves = new List<Vector2Int>();
            Vector2Int[] directions = {
                new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0)
            };

            foreach (var dir in directions)
            {
                Vector2Int target = emptyPos + dir;
                if (target.x >= 0 && target.x < width && target.y >= 0 && target.y < height && target != lastPos)
                    possibleMoves.Add(target);
            }

            if (possibleMoves.Count > 0)
            {
                Vector2Int chosen = possibleMoves[Random.Range(0, possibleMoves.Count)];
                TryMove(chosen.x, chosen.y);
                lastPos = emptyPos;
            }

            if (i == 80)
            {
                Debug.Log("블라인드 타일 등장");
                StartCoroutine(FadeInBlindTiles());
            }

            yield return new WaitForSeconds(0.015f);
        }

        yield return new WaitForSeconds(0.2f);
        isShuffling = false;
        isGameStarted = true;
        Debug.Log("셔플 완료, 게임 준비됨!");
    }

    IEnumerator FadeInBlindTiles()
    {
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / 0.5f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (blindTiles[x, y] != null)
                        SetAlpha(blindTiles[x, y], alpha);
                }
            }
            yield return null;
        }
    }

    void SetAlpha(GameObject obj, float alpha)
    {
        var mat = obj.GetComponent<Renderer>().material;
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
    }

    public void OnTileClicked(Tile tile)
    {
        if (!isGameStarted || isGameCompleted) return;

        if (tile.gridPosition == emptyPos)
        {
            CheckComplete();
            return;
        }

        TryMove(tile);
    }

    public void TryMove(Tile tile) => TryMove(tile.gridPosition.x, tile.gridPosition.y);

    public void TryMove(int x, int y)
    {
        if (!isGameStarted || isGameCompleted) return;
        if (Mathf.Abs(x - emptyPos.x) + Mathf.Abs(y - emptyPos.y) != 1) return;

        Tile tile = tiles[x, y];
        if (tile == null) return;

        tiles[emptyPos.x, emptyPos.y] = tile;
        tiles[x, y] = null;

        Vector2Int oldEmpty = emptyPos;
        emptyPos = new Vector2Int(x, y);
        tile.MoveTo(oldEmpty);

        if (!isShuffling)
            CheckComplete();
    }

    void CheckComplete()
    {
        if (emptyPos != new Vector2Int(0, 0)) return;

        foreach (Tile tile in tiles)
            if (tile != null && !tile.IsCorrect()) return;

        isPendingClear = true;
        Debug.Log("퍼즐 클리어 조건 충족. 확정 대기 중");
    }

    public void FadeAndBack(FadeController fadeController)
    {
        StartCoroutine(FadeAndLoad(fadeController));
    }

    IEnumerator FadeAndLoad(FadeController fadeController)
    {
        yield return new WaitForSeconds(0.2f);
        fadeController.GoBack();
    }
}