using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    [Header("퍼즐 설정")]
    public Texture2D puzzleTexture;
    public int width = 3;
    public int height = 3;
    public float spacing = 0.1f;
    public GameObject tilePrefab;

    [Header("게임 상태")]
    public bool isComplete = false;

    private Tile[,] tiles;
    private Vector2 emptyPos;

    void Start()
    {
        GeneratePuzzle();
        StartCoroutine(ShufflePuzzle());
    }
    

    void GeneratePuzzle()
    {
        tiles = new Tile[width, height];
        float tileSize = 1f / Mathf.Max(width, height);
        Vector3 startPos = new Vector3(-(width - 1) / 2f, 0, -(height - 1) / 2f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 && y == 0)
                {
                    emptyPos = new Vector2(x, y);
                    continue;
                }

                GameObject obj = Instantiate(tilePrefab, transform);
                Tile tile = obj.GetComponent<Tile>();

                Vector3 pos = startPos + new Vector3(x * (1 + spacing), 0, y * (1 + spacing));
                obj.transform.localPosition = pos;

                tile.Init(this, x, y, puzzleTexture, width, height);

                tiles[x, y] = tile;
            }
        }
    }

    IEnumerator ShufflePuzzle()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 30; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            TryMove(x, y);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void TryMove(int x, int y)
    {
        if (Mathf.Abs(x - emptyPos.x) + Mathf.Abs(y - emptyPos.y) != 1) return;

        Tile tile = tiles[x, y];
        if (tile == null) return;

        tiles[(int)emptyPos.x, (int)emptyPos.y] = tile;
        tiles[x, y] = null;

        tile.MoveTo((int)emptyPos.x, (int)emptyPos.y);

        emptyPos = new Vector2(x, y);

        CheckComplete();
    }

    void CheckComplete()
    {
        foreach (Tile tile in tiles)
        {
            if (tile != null && !tile.IsCorrect())
            {
                isComplete = false;
                return;
            }
        }

        isComplete = true;
        Debug.Log("퍼즐 완료!! 🎉");
    }
}
