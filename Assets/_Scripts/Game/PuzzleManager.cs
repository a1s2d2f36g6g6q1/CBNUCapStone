using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    [Header("퍼즐 설정")]
    public Texture2D puzzleTexture;
    public int width = 3;
    public int height = 3;
    public float spacing = 5f;
    public GameObject tilePrefab;
    public Transform boardParent;

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

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == width - 1 && y == height - 1)
                {
                    emptyPos = new Vector2(x, y);
                    continue;
                }

                GameObject obj = Instantiate(tilePrefab, boardParent);
                Tile tile = obj.GetComponent<Tile>();

                tile.Init(this, x, y, puzzleTexture, width, height);
                tiles[x, y] = tile;
            }
        }

        GridLayoutGroup layout = boardParent.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = width;
        layout.spacing = new Vector2(spacing, spacing);
    }

    IEnumerator ShufflePuzzle()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 100; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            TryMove(x, y);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void TryMove(int x, int y)
    {
        if (Mathf.Abs(x - emptyPos.x) + Mathf.Abs(y - emptyPos.y) != 1)
            return;

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
        Debug.Log("🎉 퍼즐 완성!! 🎉");
    }
}
