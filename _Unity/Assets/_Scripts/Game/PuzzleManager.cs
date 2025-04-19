using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public Texture2D puzzleImage;
    public GameObject tilePrefab;
    public int width = 3;
    public int height = 3;
    public float spacing = 0.1f;
    public float fadeDuration = 0.4f;
    private Material[] puzzleMaterials;

    private Tile[,] tiles;
    private Vector2Int emptyPos;
    private bool isShuffling = true;

    void Start()
    {
        if (GameData.difficulty < 2 || GameData.difficulty > 5)
        {
            GameData.difficulty = 3; // 기본값
        }

        width = GameData.difficulty;
        height = GameData.difficulty;

        GeneratePuzzle();
        CacheMaterials();
        StartCoroutine(FadeInTiles());
        StartCoroutine(ShufflePuzzle());
    }

    // 퍼즐 페이드 관련
    void CacheMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        List<Material> mats = new List<Material>();
        foreach (Renderer r in renderers)
        {
            if (r.material != null)
                mats.Add(r.material);
        }
        puzzleMaterials = mats.ToArray();
    }

    IEnumerator FadeInTiles()
    {
        foreach (Material mat in puzzleMaterials)
        {
            Color c = mat.color;
            mat.color = new Color(c.r, c.g, c.b, 0f);
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            foreach (Material mat in puzzleMaterials)
            {
                Color c = mat.color;
                mat.color = new Color(c.r, c.g, c.b, a);
            }
            yield return null;
        }
    }

    public IEnumerator FadeOutTiles(Material[] materials, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration);
            foreach (Material mat in materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
            yield return null;
        }

        // 투명도 0이 되면 타일 숨김
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            tile.gameObject.SetActive(false);
        }
    }


    void GeneratePuzzle()
    {
        // 기존 타일이 있으면 삭제
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        tiles = new Tile[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 && y == 0)
                {
                    emptyPos = new Vector2Int(x, y);
                    continue;
                }

                GameObject obj = Instantiate(tilePrefab, transform);
                obj.transform.localPosition = GetTilePosition(x, y);

                Tile tile = obj.GetComponent<Tile>();
                tile.Init(this, x, y, x, y, puzzleImage, width, height);
                tiles[x, y] = tile;
            }
        }

        Debug.Log("퍼즐 생성!!");
    }

    public Vector3 GetTilePosition(int x, int y)
    {
        Vector3 centerOffset = new Vector3((width - 1) / 2f, (height - 1) / 2f, 0);
        return new Vector3(
            (x - centerOffset.x) * (1 + spacing),
            (centerOffset.y - y) * (1 + spacing),  // 위에서 아래로 Y축 정렬
            0f
        );
    }

    IEnumerator ShufflePuzzle()
    {
        isShuffling = true;
        yield return new WaitForSeconds(0.5f);

        Vector2Int[] directions = {
            new Vector2Int(0, 1),   // 위
            new Vector2Int(1, 0),   // 오른쪽
            new Vector2Int(0, -1),  // 아래
            new Vector2Int(-1, 0)   // 왼쪽
        };

        Vector2Int previousMove = Vector2Int.zero;

        for (int i = 0; i < 100; i++)
        {
            List<Vector2Int> validMoves = new List<Vector2Int>();

            foreach (var dir in directions)
            {
                Vector2Int next = emptyPos + dir;

                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                    continue;

                if (dir == -previousMove)
                    continue; // 바로 이전 방향의 반대는 제외

                if (tiles[next.x, next.y] != null)
                    validMoves.Add(dir);
            }

            if (validMoves.Count > 0)
            {
                Vector2Int move = validMoves[Random.Range(0, validMoves.Count)];
                Vector2Int targetPos = emptyPos + move;
                TryMove(targetPos.x, targetPos.y);
                previousMove = move;
            }

            yield return new WaitForSeconds(0.02f);
        }

        yield return new WaitForSeconds(0.3f);
        isShuffling = false;

        CheckComplete();
    }


    public void TryMove(Tile tile)
    {
        TryMove(tile.gridPosition.x, tile.gridPosition.y);
    }

    public void TryMove(int x, int y)
    {
        if (Mathf.Abs(x - emptyPos.x) + Mathf.Abs(y - emptyPos.y) != 1) return;

        Tile tile = tiles[x, y];
        if (tile == null) return;

        tiles[emptyPos.x, emptyPos.y] = tile;
        tiles[x, y] = null;

        Vector2Int oldEmpty = emptyPos;
        emptyPos = new Vector2Int(x, y);

        tile.MoveTo(oldEmpty);

        if (!isShuffling)
        {
            CheckComplete();
        }
    }

    void CheckComplete()
    {
        foreach (Tile tile in tiles)
        {
            if (tile != null && !tile.IsCorrect())
                return;
        }

        Debug.Log("퍼즐 완료!! 🎉");
    }

    public void FadeAndBack(FadeController fadeController)
    {
        StartCoroutine(FadeAndLoad(fadeController));
    }

    IEnumerator FadeAndLoad(FadeController fadeController)
    {
        yield return StartCoroutine(FadeOutTiles(puzzleMaterials, fadeDuration));
        fadeController.GoBack();
    }
}