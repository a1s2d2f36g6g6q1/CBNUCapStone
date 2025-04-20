using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public TimerManager timerManager;
    public TMP_Text clickToStartText;
    public FlashEffect flashEffect;

    public GameObject emptyTilePrefab; // EmptyTile 프리팹을 위한 변수

    public Texture2D puzzleImage;

    public GameObject tilePrefab;
    public int width = 3;
    public int height = 3;
    public float spacing = 0.1f;
    public float fadeDuration = 0.4f;
    private Vector2Int emptyPos;
    private GameObject emptyTileInstance; // EmptyTile 인스턴스
    private bool isShuffling = true;
    private Material[] puzzleMaterials;

    private Tile[,] tiles;
    private bool tilesRevealed;
    private bool waitingForReveal;

    private bool puzzleCleared = false;

    
    private void Start()
    {
        if (GameData.difficulty < 2 || GameData.difficulty > 5) GameData.difficulty = 3; // 기본값

        width = GameData.difficulty;
        height = GameData.difficulty;

        clickToStartText.gameObject.SetActive(false);
        GeneratePuzzle();
        CacheMaterials();
        StartCoroutine(FadeInTiles());
        StartCoroutine(ShufflePuzzle());
    }

    private void Update()
    {
        if (waitingForReveal && !tilesRevealed)
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                foreach (var tile in tiles)
                    if (tile != null)
                        tile.Restore();

                tilesRevealed = true;
                waitingForReveal = false;
                timerManager.StartTimer(); // 여기서 타이머 시작
                clickToStartText.gameObject.SetActive(false);
            }
    }


    // 퍼즐 페이드 관련
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

            // 🔧 EmptyTile도 같이 페이드 처리
            if (emptyTileInstance != null)
            {
                var empty = emptyTileInstance.GetComponent<EmptyTile>();
                empty.SetFadeOut(t / duration);
            }

            yield return null;
        }

        // 기존 타일 비활성화
        foreach (var tile in GetComponentsInChildren<Tile>())
            tile.gameObject.SetActive(false);

        // 🔧 EmptyTile 비활성화도 포함
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

                // 초기에는 보이지 않게 생성 (알파 0)
                emptyTileInstance = Instantiate(emptyTilePrefab, transform);
                emptyTileInstance.transform.localPosition = GetTilePosition(x, y);

                var emptyTile = emptyTileInstance.GetComponent<EmptyTile>();
                emptyTile.Init(puzzleImage, width, height, x, y);
                emptyTile.SetAlpha(0f); // 초기에 보이지 않도록 설정
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
            (centerOffset.y - y) * (1 + spacing), // 위에서 아래로 Y축 정렬
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
            // 유효 방향 계산
            var validMoves = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                var next = emptyPos + dir;
                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height) continue;
                if (dir == -previousMove) continue;
                if (tiles[next.x, next.y] != null)
                    validMoves.Add(dir);
            }

            // 이동
            if (validMoves.Count > 0)
            {
                var move = validMoves[Random.Range(0, validMoves.Count)];
                var targetPos = emptyPos + move;
                TryMove(targetPos.x, targetPos.y);
                previousMove = move;
            }

            // 페이드 처리
            if (i >= 60)
            {
                var fadeT = Mathf.InverseLerp(60, 80, i); // 0~1 범위
                foreach (var tile in tiles)
                    if (tile != null)
                        tile.SetFadeGray(fadeT);
            }

            yield return new WaitForSeconds(0.02f);
        }

        isShuffling = false;
        waitingForReveal = true; // 입력 대기 상태 진입
        tilesRevealed = false; // 아직 복원되지 않음

        CheckComplete();
        clickToStartText.gameObject.SetActive(true);


    }


    public void TryMove(Tile tile)
    {
        TryMove(tile.gridPosition.x, tile.gridPosition.y);
    }

    private bool CheckCompleteStatus()
    {
        var isComplete = true;

        foreach (var tile in tiles)
            if (tile != null && !tile.IsCorrect())
            {
                isComplete = false;
                break;
            }

        return isComplete;
    }

    public void TryMove(int x, int y)
    {
        // ✅ 게임이 이미 끝났다면 이동 불가
        if (puzzleCleared) return;

        if (Mathf.Abs(x - emptyPos.x) + Mathf.Abs(y - emptyPos.y) != 1) return;

        var tile = tiles[x, y];
        if (tile == null) return;

        // 퍼즐이 완성된 상태에서 다른 타일을 이동시키면 EmptyTile 제거
        if (emptyTileInstance != null && CheckCompleteStatus())
            RemoveEmptyTile();

        tiles[emptyPos.x, emptyPos.y] = tile;
        tiles[x, y] = null;

        var oldEmpty = emptyPos;
        emptyPos = new Vector2Int(x, y);

        tile.MoveTo(oldEmpty);

        if (!isShuffling)
            CheckComplete();
    }


    private void ShowEmptyTile()
    {
        if (emptyTileInstance == null) return;

        emptyTileInstance.SetActive(true); // 객체 활성화 (사실 알파 0이라 안 보임)
        emptyTileInstance.GetComponent<Collider>().enabled = true;
    }


    private void RemoveEmptyTile()
    {
        if (emptyTileInstance != null)
        {
            emptyTileInstance.GetComponent<EmptyTile>().SetAlpha(0f); // 다시 감춤
            emptyTileInstance.GetComponent<Collider>().enabled = false; // 클릭도 비활성화
        }
    }

    public void CollapseTiles()
    {
        foreach (var tile in tiles)
            if (tile != null)
            {
                var target = GetCollapsedTilePosition(tile.gridPosition.x, tile.gridPosition.y);
                tile.MoveToFinal(target);
            }

        // 🔧 EmptyTile 이동 추가
        if (emptyTileInstance != null)
        {
            Vector3 target = GetCollapsedTilePosition(0, 0); // emptyPos도 가능
            StartCoroutine(MoveEmptyTile(target));
        }
    }

    private IEnumerator MoveEmptyTile(Vector3 target)
    {
        Transform t = emptyTileInstance.transform;

        while (Vector3.Distance(t.localPosition, target) > 0.01f)
        {
            t.localPosition = Vector3.Lerp(t.localPosition, target, Time.deltaTime * 8f);
            yield return null;
        }

        t.localPosition = target;
    }


    public Vector3 GetCollapsedTilePosition(int x, int y)
    {
        var centerOffset = new Vector3((width - 1) / 2f, (height - 1) / 2f, 0);
        return new Vector3(
            x - centerOffset.x, // spacing 없이
            centerOffset.y - y,
            0f
        );
    }


    private void CheckComplete()
    {
        var isComplete = true;

        foreach (var tile in tiles)
            if (tile != null && !tile.IsCorrect())
            {
                isComplete = false;
                break;
            }

        if (isComplete)
        {
            Debug.Log("퍼즐 완료: 빈 타일 배치 대기중");

            // 퍼즐이 맞았을 때만 EmptyTile 표시
            ShowEmptyTile();
        }
    }

    private void HandleEmptyTileClick()
    {
        if (emptyTileInstance != null)
        {
            emptyTileInstance.GetComponent<EmptyTile>().SetAlpha(1f); // ✅ 명시적으로 추가
        }
        
        // ✅ 섬광 효과
        flashEffect.PlayFlash();

        // ✅ 타일 수렴 애니메이션
        CollapseTiles();

        // ✅ 타이머 종료
        timerManager.StopTimer();

        puzzleCleared = true;

        
        // ✅ 모든 타일 입력 차단
        foreach (var tile in tiles)
            if (tile != null)
                tile.enabled = false;

        Debug.Log("게임 완료!! 🎉");
    }


    public void FadeAndBack(FadeController fadeController)
    {
        StartCoroutine(FadeAndLoad(fadeController));
    }

    private IEnumerator FadeAndLoad(FadeController fadeController)
    {
        yield return StartCoroutine(FadeOutTiles(puzzleMaterials, fadeDuration));
        fadeController.GoBack();
    }
}