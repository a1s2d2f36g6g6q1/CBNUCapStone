using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public TimerManager timerManager;
    private bool waitingForReveal = false;
    private bool tilesRevealed = false;
    public TMP_Text clickToStartText;

    
    public GameObject emptyTilePrefab; // EmptyTile 프리팹을 위한 변수
    private GameObject emptyTileInstance; // EmptyTile 인스턴스

    public Texture2D puzzleImage;
    private bool waitingForClickToRestore = false;

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

        clickToStartText.gameObject.SetActive(false);
        GeneratePuzzle();
        CacheMaterials();
        StartCoroutine(FadeInTiles());
        StartCoroutine(ShufflePuzzle());
    }
    
    void Update()
    {
        if (waitingForReveal && !tilesRevealed)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                foreach (Tile tile in tiles)
                    if (tile != null)
                        tile.Restore();
    
                tilesRevealed = true;
                waitingForReveal = false;
                timerManager.StartTimer(); // 여기서 타이머 시작
                clickToStartText.gameObject.SetActive(false);

            }
        }
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
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    
        tiles = new Tile[width, height];
    
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 && y == 0)
                {
                    emptyPos = new Vector2Int(x, y);
    
                    // 초기에는 보이지 않게 생성 (알파 0)
                    emptyTileInstance = Instantiate(emptyTilePrefab, transform);
                    emptyTileInstance.transform.localPosition = GetTilePosition(x, y);
    
                    EmptyTile emptyTile = emptyTileInstance.GetComponent<EmptyTile>();
                    emptyTile.Init(puzzleImage, width, height, x, y);
                    emptyTile.SetAlpha(0f); // 초기에 보이지 않도록 설정
                    emptyTile.OnClick += HandleEmptyTileClick;
    
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

        Vector2Int previousMove = Vector2Int.zero;
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };

        for (int i = 0; i < 100; i++)
        {
            // 유효 방향 계산
            List<Vector2Int> validMoves = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                Vector2Int next = emptyPos + dir;
                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height) continue;
                if (dir == -previousMove) continue;
                if (tiles[next.x, next.y] != null)
                    validMoves.Add(dir);
            }

            // 이동
            if (validMoves.Count > 0)
            {
                Vector2Int move = validMoves[Random.Range(0, validMoves.Count)];
                Vector2Int targetPos = emptyPos + move;
                TryMove(targetPos.x, targetPos.y);
                previousMove = move;
            }

            // 페이드 처리
            if (i >= 60)
            {
                float fadeT = Mathf.InverseLerp(60, 80, i); // 0~1 범위
                foreach (Tile tile in tiles)
                    if (tile != null)
                        tile.SetFadeGray(fadeT);
            }

            yield return new WaitForSeconds(0.02f);
        }

        isShuffling = false;
        waitingForReveal = true; // 입력 대기 상태 진입
        tilesRevealed = false;   // 아직 복원되지 않음
        
        CheckComplete();
        clickToStartText.gameObject.SetActive(true);


        waitingForClickToRestore = true; // 클릭 감지 대기 시작
    }



    public void TryMove(Tile tile)
    {
        TryMove(tile.gridPosition.x, tile.gridPosition.y);
    }
    
    bool CheckCompleteStatus()
    {
        bool isComplete = true;
    
        foreach (Tile tile in tiles)
        {
            if (tile != null && !tile.IsCorrect())
            {
                isComplete = false;
                break;
            }
        }
    
        return isComplete;
    }

    public void TryMove(int x, int y)
    {
        if (Mathf.Abs(x - emptyPos.x) + Mathf.Abs(y - emptyPos.y) != 1) return;
    
        Tile tile = tiles[x, y];
        if (tile == null) return;
    
        // 퍼즐이 완성된 상태에서 다른 타일을 이동시키면 EmptyTile 제거
        if (emptyTileInstance != null && CheckCompleteStatus())
        {
            RemoveEmptyTile();
        }
    
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
    
void ShowEmptyTile()
{
    if (emptyTileInstance == null) return;

    emptyTileInstance.SetActive(true); // 객체 활성화 (사실 알파 0이라 안 보임)
    emptyTileInstance.GetComponent<Collider>().enabled = true;
}

    
void RemoveEmptyTile()
{
    if (emptyTileInstance != null)
    {
        emptyTileInstance.GetComponent<EmptyTile>().SetAlpha(0f); // 다시 감춤
        emptyTileInstance.GetComponent<Collider>().enabled = false; // 클릭도 비활성화
    }
}

    
    void CheckComplete()
    {
        bool isComplete = true;
    
        foreach (Tile tile in tiles)
        {
            if (tile != null && !tile.IsCorrect())
            {
                isComplete = false;
                break;
            }
        }
    
        if (isComplete)
        {
            Debug.Log("퍼즐 완료: 빈 타일 배치 대기중");
            
            // 퍼즐이 맞았을 때만 EmptyTile 표시
            ShowEmptyTile();
        }
    }
    
void HandleEmptyTileClick()
{
    if (emptyTileInstance != null)
    {
        EmptyTile tile = emptyTileInstance.GetComponent<EmptyTile>();
        tile.SetAlpha(1f); // 알파값 1 → 시각적으로 표시
    }

    timerManager.StopTimer();
    Debug.Log("게임 완료!! 🎉");

    // 모든 타일의 입력 막기
    foreach (Tile tile in tiles)
        if (tile != null)
            tile.enabled = false;
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