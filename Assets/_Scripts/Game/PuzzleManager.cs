using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("설정")]
    public int gridSize = 3;
    public GameObject tilePrefab;
    public Transform boardParent;

    [Header("게임 데이터")]
    private Tile[,] tiles;
    private Vector2Int emptyPos;

    void Start()
    {
        InitBoard();
    }

    void InitBoard()
    {
        tiles = new Tile[gridSize, gridSize];
        int number = 1;

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // 마지막 칸은 빈칸!
                if (number == gridSize * gridSize)
                {
                    emptyPos = new Vector2Int(x, y);
                    continue;
                }

                GameObject tileObj = Instantiate(tilePrefab, boardParent);
                Tile tile = tileObj.GetComponent<Tile>();

                tile.manager = this;
                tile.SetNumber(number);
                tile.SetPosition(x, y);

                tiles[x, y] = tile;

                number++;
            }
        }
    }

    public void TryMoveTile(int x, int y)
    {
        // 인접한 경우만 이동 가능
        if (IsAdjacent(x, y, emptyPos.x, emptyPos.y))
        {
            Tile tile = tiles[x, y];

            // 빈칸으로 자리 바꾸기
            tiles[emptyPos.x, emptyPos.y] = tile;
            tiles[x, y] = null;

            tile.SetPosition(emptyPos.x, emptyPos.y);

            // 새 빈 칸 위치 갱신
            emptyPos = new Vector2Int(x, y);
        }
    }

    bool IsAdjacent(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) == 1;
    }
}