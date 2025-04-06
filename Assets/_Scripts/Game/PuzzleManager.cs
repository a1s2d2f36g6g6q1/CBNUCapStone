using System.Collections;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public Texture2D puzzleImage;
    public GameObject tilePrefab;
    public int width = 3;
    public int height = 3;
    public float spacing = 0.1f;

    private Tile[,] tiles;
    private Vector2Int emptyPos;
    private bool isShuffling = true;

    void Start()
    {
        GeneratePuzzle();
        StartCoroutine(ShufflePuzzle());
    }

    void GeneratePuzzle()
    {
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

        for (int i = 0; i < 80; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            TryMove(x, y);
            yield return new WaitForSeconds(0.03f);
        }

        // 이동이 끝났을 시간만큼 대기 후 검사
        yield return new WaitForSeconds(0.5f);
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
}
