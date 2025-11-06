using UnityEngine;

/// <summary>
/// 퍼즐 게임 세션 정보 관리
/// 갤러리 이미지 재플레이 시 사용
/// </summary>
public class PuzzleSession : MonoBehaviour
{
    public static PuzzleSession Instance { get; private set; }

    // 재플레이 정보
    public string ReplayImageUrl { get; private set; }
    public string[] ReplayTags { get; private set; }
    public int GridSize { get; private set; } = 3; // 기본 3x3
    public bool CanUpload { get; private set; } = true; // 업로드 가능 여부

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 갤러리 이미지 재플레이 설정
    /// </summary>
    public void SetImageForReplay(string imageUrl, string[] tags, int gridSize = 3, bool canUpload = false)
    {
        ReplayImageUrl = imageUrl;
        ReplayTags = tags;
        GridSize = gridSize;
        CanUpload = canUpload;

        Debug.Log($"[PuzzleSession] 재플레이 설정 - URL: {imageUrl}, GridSize: {gridSize}, Upload: {canUpload}");
    }

    /// <summary>
    /// 재플레이 정보 초기화
    /// </summary>
    public void ClearReplayData()
    {
        ReplayImageUrl = null;
        ReplayTags = null;
        GridSize = 3;
        CanUpload = true;

        Debug.Log("[PuzzleSession] 재플레이 정보 초기화");
    }

    /// <summary>
    /// 재플레이 모드인지 확인
    /// </summary>
    public bool IsReplayMode()
    {
        return !string.IsNullOrEmpty(ReplayImageUrl);
    }
}