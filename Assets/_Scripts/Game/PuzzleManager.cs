using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public TimerManager timerManager;
    public TMP_Text clickToStartText;
    public FlashEffect flashEffect;
    public ClearPopup clearPopup; // 추가
    public Camera mainCamera; // 추가 - 메인 카메라 레퍼런스

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

    // 새로 추가되는 상태 변수들 (완료 처리용)
    private bool puzzleCompleted = false; // 퍼즐이 맞춰진 상태 (아직 클리어 아님)
    private bool gameFinished = false; // 게임이 완전히 끝난 상태
    private bool waitingForFinalClick = false; // 최종 클릭 대기 상태

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

        // 퍼즐 완료 후 최종 클릭 대기 (새로 추가)
        if (waitingForFinalClick && !gameFinished)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TriggerGameClear();
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
        this.tilesRevealed = false;
        this.waitingForReveal = false;
        // 새로 추가된 상태 변수들 초기화
        this.puzzleCompleted = false;
        this.gameFinished = false;
        this.waitingForFinalClick = false;

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

        Debug.Log("Puzzle created!!");
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

    public void TryMove(int x, int y)
    {
        // 게임이 끝났으면 타일 이동 불가 (추가)
        if (gameFinished) return;

        if (IsValidMove(x, y))
        {
            var tile = tiles[x, y];
            tiles[x, y] = null;
            tiles[emptyPos.x, emptyPos.y] = tile;

            tile.MoveTo(new Vector2Int(emptyPos.x, emptyPos.y));
            emptyPos = new Vector2Int(x, y);

            emptyTileInstance.transform.localPosition = GetTilePosition(emptyPos.x, emptyPos.y);

            CheckComplete();
        }
    }

    private bool IsValidMove(int x, int y)
    {
        return Mathf.Abs(x - emptyPos.x) + Mathf.Abs(y - emptyPos.y) == 1;
    }

    public void CheckComplete()
    {
        if (isShuffling || !tilesRevealed) return;

        var isComplete = true;
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (x == 0 && y == 0) continue;
                var tile = tiles[x, y];
                if (tile == null || !tile.IsCorrect())
                {
                    isComplete = false;
                    break;
                }
            }

        if (isComplete && !puzzleCompleted)
        {
            // 퍼즐이 완성되었지만 아직 게임 클리어는 아님
            puzzleCompleted = true;
            waitingForFinalClick = true;

            // 빈 타일을 클릭 가능하게 하되 완전히 투명하게
            if (emptyTileInstance != null)
            {
                emptyTileInstance.SetActive(true); // 활성화 확실히
                var emptyTile = emptyTileInstance.GetComponent<EmptyTile>();
                if (emptyTile != null)
                    emptyTile.SetAlpha(0f); // 완전히 투명 (클릭은 가능)
            }

            Debug.Log("Puzzle completed! Click empty tile or press space bar to finish.");
        }
        else if (!isComplete && puzzleCompleted)
        {
            // 퍼즐이 다시 흐트러짐
            puzzleCompleted = false;
            waitingForFinalClick = false;

            // 빈 타일 다시 완전히 투명하게
            if (emptyTileInstance != null)
            {
                var emptyTile = emptyTileInstance.GetComponent<EmptyTile>();
                if (emptyTile != null)
                    emptyTile.SetAlpha(0f);
            }
        }
    }

    // 빈 타일 클릭 처리 (새로 추가)
    private void HandleEmptyTileClick()
    {
        if (waitingForFinalClick && puzzleCompleted)
        {
            TriggerGameClear();
        }
    }

    // 게임 클리어 처리 (새로 추가)
    private void TriggerGameClear()
    {
        if (gameFinished) return;

        gameFinished = true;
        waitingForFinalClick = false;

        // 타이머 정지
        if (timerManager != null)
            timerManager.StopTimer();

        // 플래시 효과
        if (flashEffect != null)
            flashEffect.PlayFlash();

        // 빈 타일 채우기 및 클리어 연출
        StartCoroutine(SavePuzzleResult());
        StartCoroutine(CompletePuzzleAnimation());
    }
    private System.Collections.IEnumerator SavePuzzleResult()
    {
        if (puzzleImage == null)
        {
            Debug.LogWarning("저장할 이미지가 없습니다.");
            yield break;
        }

        // 이미지를 Base64로 인코딩
        byte[] imageBytes = puzzleImage.EncodeToPNG();
        string base64Image = System.Convert.ToBase64String(imageBytes);

        // 태그 정보 가져오기
        string[] tags = UserSession.Instance.Tags?.ToArray() ?? new string[] { "puzzle", "cleared" };

        // 클리어 시간 설명
        string clearTime = timerManager != null ? timerManager.GetFormattedTime() : "Unknown";
        string description = $"Puzzle cleared in {clearTime}";

        // 업로드 요청 데이터
        GalleryUploadRequest uploadData = new GalleryUploadRequest
        {
            imageBase64 = base64Image,
            description = description,
            tags = tags
        };

        Debug.Log("갤러리에 이미지 업로드 중...");

        yield return APIManager.Instance.Post(
            $"/planets/{UserSession.Instance.UserID}/gallery",  // 내 행성에 업로드
            uploadData,
            onSuccess: (response) =>
            {
                GalleryUploadResponse uploadResponse = JsonUtility.FromJson<GalleryUploadResponse>(response);
                Debug.Log($"갤러리 업로드 성공! ImageId: {uploadResponse.imageId}");
            },
            onError: (error) =>
            {
                Debug.LogError("갤러리 업로드 실패: " + error);
                // 업로드 실패해도 게임은 계속 진행
            }
        );
    }

    // 클리어 연출 코루틴 (새로 추가)
    private IEnumerator CompletePuzzleAnimation()
    {
        // 플래시 효과와 함께 바로 완성된 사진으로 덮어버리기
        ShowCompletedImage();

        // 카메라 180도 회전 연출
        yield return StartCoroutine(CameraRotationAnimation());

        // 클리어 팝업 표시 (반대편에서 등장하는 느낌)
        if (clearPopup != null)
        {
            string clearTime = timerManager != null ? timerManager.GetFormattedTime() : "00:00:000";
            clearPopup.ShowClearPopup(puzzleImage, clearTime);
        }

        Debug.Log("Game Clear!");
    }

    // 카메라 회전 애니메이션 (새로 추가)
    private IEnumerator CameraRotationAnimation()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is not assigned.");
            yield break;
        }

        Vector3 startRot = mainCamera.transform.eulerAngles;
        Vector3 endRot = new Vector3(startRot.x + 180f, startRot.y, startRot.z); // X축으로 180도 회전만

        // 카메라 회전 (점점 빠르게, 1초)
        float rotDuration = 1.0f;
        float elapsedTime = 0f;

        while (elapsedTime < rotDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotDuration;
            // 더 강한 가속 커브 (천천히 시작해서 확 빠르게)
            t = t * t * t * t; // Ease In (더 강한 가속)

            mainCamera.transform.eulerAngles = Vector3.Lerp(startRot, endRot, t);
            yield return null;
        }

        // 최종 회전 확정
        mainCamera.transform.eulerAngles = endRot;
    }

    // 완성된 사진으로 덮어버리기 (새로 추가)
    private void ShowCompletedImage()
    {
        // 기존 모든 타일들 비활성화
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].gameObject.SetActive(false);
                }
            }

        // 빈 타일도 비활성화
        if (emptyTileInstance != null)
        {
            emptyTileInstance.SetActive(false);
        }

        // 완성된 이미지를 표시할 새로운 오브젝트 생성
        var completedImageObj = new GameObject("CompletedImage");
        completedImageObj.transform.SetParent(transform);
        completedImageObj.transform.localPosition = Vector3.zero;

        // 쿼드 메시로 완성된 이미지 표시
        var meshRenderer = completedImageObj.AddComponent<MeshRenderer>();
        var meshFilter = completedImageObj.AddComponent<MeshFilter>();

        // 쿼드 메시 생성
        meshFilter.mesh = CreateQuadMesh();

        // 재질 생성하고 완성된 이미지 적용
        var material = new Material(Shader.Find("Unlit/Texture"));
        if (puzzleImage != null)
        {
            material.mainTexture = puzzleImage;
        }
        meshRenderer.material = material;

        // 크기 조정 (퍼즐 크기에 맞게)
        float imageSize = Mathf.Max(width, height);
        completedImageObj.transform.localScale = new Vector3(imageSize, imageSize, 1f);
    }

    // 쿼드 메시 생성 헬퍼 함수
    private Mesh CreateQuadMesh()
    {
        var mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };

        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}