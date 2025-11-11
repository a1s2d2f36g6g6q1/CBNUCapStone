using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public TimerManager timerManager;
    public TMP_Text clickToStartText;
    public FlashEffect flashEffect;
    public ClearPopup clearPopup;
    public MultiplayRankPopup multiplayRankPopup;
    public Camera mainCamera;

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

    private bool puzzleCompleted = false;
    private bool gameFinished = false;
    private bool waitingForFinalClick = false;

    private void Start()
    {
        // Puzzle generation handled by InitializePuzzle()
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

                // Save start time
                GameData.startTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                if (clickToStartText != null)
                    clickToStartText.gameObject.SetActive(false);
            }
        }

        if (waitingForFinalClick && !gameFinished)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TriggerGameClear();
            }
        }
    }

    public void InitializePuzzle(Texture2D texture, int width, int height)
    {
        this.puzzleImage = texture;
        this.width = width;
        this.height = height;
        this.tilesRevealed = false;
        this.waitingForReveal = false;
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

        Debug.Log("Puzzle created!");
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
            puzzleCompleted = true;
            waitingForFinalClick = true;

            if (emptyTileInstance != null)
            {
                emptyTileInstance.SetActive(true);
                var emptyTile = emptyTileInstance.GetComponent<EmptyTile>();
                if (emptyTile != null)
                    emptyTile.SetAlpha(0f);
            }

            Debug.Log("Puzzle completed! Click empty tile or press space bar to finish.");
        }
        else if (!isComplete && puzzleCompleted)
        {
            puzzleCompleted = false;
            waitingForFinalClick = false;

            if (emptyTileInstance != null)
            {
                var emptyTile = emptyTileInstance.GetComponent<EmptyTile>();
                if (emptyTile != null)
                    emptyTile.SetAlpha(0f);
            }
        }
    }

    private void HandleEmptyTileClick()
    {
        if (waitingForFinalClick && puzzleCompleted)
        {
            TriggerGameClear();
        }
    }

    private void TriggerGameClear()
    {
        if (gameFinished) return;

        gameFinished = true;
        waitingForFinalClick = false;

        if (timerManager != null)
            timerManager.StopTimer();

        // Save end time
        GameData.endTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        if (flashEffect != null)
            flashEffect.PlayFlash();

        StartCoroutine(CompletePuzzleAnimation());
    }

    private IEnumerator CompletePuzzleAnimation()
    {
        ShowCompletedImage();
        yield return StartCoroutine(CameraRotationAnimation());

        if (GameData.isMultiplay)
        {
            yield return StartCoroutine(HandleMultiplayCompletion());
        }
        else
        {
            yield return StartCoroutine(HandleSingleplayCompletion());
        }

        Debug.Log("Game Clear!");
    }

    private IEnumerator HandleSingleplayCompletion()
    {
        // Submit to server
        yield return StartCoroutine(SubmitSingleplayCompletion());

        // Show clear popup
        if (clearPopup != null)
        {
            string clearTime = timerManager != null ? timerManager.GetFormattedTime() : "00:00:000";
            clearPopup.ShowClearPopup(puzzleImage, clearTime);
        }
        else
        {
            Debug.LogError("[PuzzleManager] ClearPopup not assigned!");
        }
    }

    private IEnumerator HandleMultiplayCompletion()
    {
        Debug.Log("[PuzzleManager] === MULTIPLAY COMPLETION START ===");
        Debug.Log($"[PuzzleManager] GameData.isMultiplay: {GameData.isMultiplay}");
        Debug.Log($"[PuzzleManager] MultiplayRankPopup assigned: {multiplayRankPopup != null}");

        // Send completion via WebSocket
        SubmitMultiplayCompletion();  // ← 이 부분이 문제!

        // Wait a bit for WebSocket to process
        yield return new WaitForSeconds(0.5f);

        // Get clear time
        string clearTime = timerManager != null ? timerManager.GetFormattedTime() : "00:00:000";
        Debug.Log($"[PuzzleManager] Clear time: {clearTime}");

        // Show rank popup
        if (multiplayRankPopup != null)
        {
            Debug.Log("[PuzzleManager] Showing MultiplayRankPopup");
            multiplayRankPopup.ShowRankPopup(puzzleImage, clearTime);
        }
        else
        {
            Debug.LogError("[PuzzleManager] !!! MultiplayRankPopup is NULL !!!");
            Debug.LogError("[PuzzleManager] Please assign MultiplayRankPopup in Inspector!");

            // Fallback: Show regular clear popup
            if (clearPopup != null)
            {
                Debug.LogWarning("[PuzzleManager] Using ClearPopup as fallback");
                clearPopup.ShowClearPopup(puzzleImage, clearTime);
            }
            else
            {
                Debug.LogError("[PuzzleManager] Both popups are NULL!");
            }
        }

        Debug.Log("[PuzzleManager] === MULTIPLAY COMPLETION END ===");
    }

    private IEnumerator SubmitSingleplayCompletion()
    {
        Debug.Log("[PuzzleManager] Submitting singleplay completion...");

        SingleGameCompleteRequest request = new SingleGameCompleteRequest
        {
            gameCode = GameData.gameCode,
            startTime = GameData.startTime,
            endTime = GameData.endTime
        };

        bool completed = false;

        yield return APIManager.Instance.Post(
            "/games/single/complete",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[PuzzleManager] Completion response: {response}");

                SingleGameCompleteResponse apiResponse = JsonUtility.FromJson<SingleGameCompleteResponse>(response);

                if (apiResponse != null && apiResponse.result != null)
                {
                    Debug.Log($"[PuzzleManager] Game completed! ClearTime: {apiResponse.result.clearTimeMs}ms");
                }

                completed = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[PuzzleManager] Failed to submit completion: {error}");
                completed = true; // Continue anyway
            }
        );

        // Wait for completion
        while (!completed)
        {
            yield return null;
        }
    }

    private void SubmitMultiplayCompletion()
    {
        Debug.Log("[PuzzleManager] Submitting multiplay completion via API...");

        if (MultiplaySession.Instance == null || MultiplaySession.Instance.CurrentRoom == null)
        {
            Debug.LogError("[PuzzleManager] MultiplaySession or CurrentRoom is null!");
            return;
        }

        StartCoroutine(SubmitMultiplayCompletionCoroutine());
    }

    private IEnumerator SubmitMultiplayCompletionCoroutine()
    {
        string gameCode = MultiplaySession.Instance.CurrentRoom.gameCode;
        float myTime = timerManager != null ? timerManager.GetElapsedTime() : 0f;

        Debug.Log($"[PuzzleManager] Submitting completion for gameCode: {gameCode}, Time: {myTime}s");

        // Create request with gameCode only
        var request = new { gameCode = gameCode };

        bool requestCompleted = false;

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/complete",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[PuzzleManager] Multiplay completion API response: {response}");

                try
                {
                    MultiplayCompleteResponse jsonResponse = JsonUtility.FromJson<MultiplayCompleteResponse>(response);

                    if (jsonResponse != null && jsonResponse.result != null)
                    {
                        Debug.Log($"[PuzzleManager] Completion success - IsWinner: {jsonResponse.result.isWinner}");

                        // Update my player's clear time immediately
                        string myUserId = UserSession.Instance.UserID;
                        MultiplaySession.Instance.UpdatePlayerClearTime(myUserId, myTime);

                        Debug.Log($"[PuzzleManager] My clear time set: {myTime}s");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PuzzleManager] Failed to parse completion response: {e.Message}");
                }

                requestCompleted = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[PuzzleManager] Failed to submit completion: {error}");

                // Even on error, update local time for display
                string myUserId = UserSession.Instance.UserID;
                MultiplaySession.Instance.UpdatePlayerClearTime(myUserId, myTime);

                requestCompleted = true;
            }
        );

        // Wait for API call to complete
        while (!requestCompleted)
        {
            yield return null;
        }
    }

    private IEnumerator CameraRotationAnimation()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is not assigned.");
            yield break;
        }

        Vector3 startRot = mainCamera.transform.eulerAngles;
        Vector3 endRot = new Vector3(startRot.x + 180f, startRot.y, startRot.z);

        float rotDuration = 1.0f;
        float elapsedTime = 0f;

        while (elapsedTime < rotDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotDuration;
            t = t * t * t * t;

            mainCamera.transform.eulerAngles = Vector3.Lerp(startRot, endRot, t);
            yield return null;
        }

        mainCamera.transform.eulerAngles = endRot;
    }

    private void ShowCompletedImage()
    {
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].gameObject.SetActive(false);
                }
            }

        if (emptyTileInstance != null)
        {
            emptyTileInstance.SetActive(false);
        }

        var completedImageObj = new GameObject("CompletedImage");
        completedImageObj.transform.SetParent(transform);
        completedImageObj.transform.localPosition = Vector3.zero;

        var meshRenderer = completedImageObj.AddComponent<MeshRenderer>();
        var meshFilter = completedImageObj.AddComponent<MeshFilter>();

        meshFilter.mesh = CreateQuadMesh();

        var material = new Material(Shader.Find("Unlit/Texture"));
        if (puzzleImage != null)
        {
            material.mainTexture = puzzleImage;
        }
        meshRenderer.material = material;

        float imageSize = Mathf.Max(width, height);
        completedImageObj.transform.localScale = new Vector3(imageSize, imageSize, 1f);
    }

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