using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClearPopup : MonoBehaviour
{
    [Header("UI References")]
    public Button exitButton;
    public Image clearImage;
    public TMP_InputField titleInputField;
    public TMP_InputField descriptionInputField;
    public Button uploadButton;
    public GameObject uploadStatusDefault; // _ (기본 상태)
    public GameObject uploadStatusChecked; // Checked (성공 상태)
    public GameObject uploadStatusError; // Error (실패 상태)
    public FadeController fadeController;

    [Header("Upload Settings")]
    public float uploadSimulationTime = 2f;

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;

    private string currentClearTime;
    private bool isUploading = false;
    private Vector3 originalScale;

    private void Start()
    {
        // 패널 자체가 이 스크립트가 부착된 GameObject
        gameObject.SetActive(false);
        // 원본 스케일 저장
        originalScale = transform.localScale;

        // 업로드 상태 초기화 (모든 상태 숨기기)
        SetUploadStatus("_");
    }

    public void ShowClearPopup(Texture2D completedPuzzleImage, string clearTime)
    {
        currentClearTime = clearTime;

        // 패널을 먼저 활성화해야 코루틴이 실행 가능
        gameObject.SetActive(true);

        StartCoroutine(ShowPopupWithDelay(completedPuzzleImage, clearTime));
    }

    private IEnumerator ShowPopupWithDelay(Texture2D completedPuzzleImage, string clearTime)
    {
        // 패널을 일단 투명하게 만들기 (활성화되어 있지만 보이지 않게)
        var canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // 카메라 회전이 완료될 때까지 대기 (즉시 시작)
        yield return new WaitForSeconds(0.1f);

        // 팝업 준비
        PreparePopupContent(completedPuzzleImage, clearTime);

        // 애니메이션으로 등장 (반대편에서 나타나는 느낌)
        yield return StartCoroutine(ShowPopupFromBackAnimation());
    }

    private void PreparePopupContent(Texture2D completedPuzzleImage, string clearTime)
    {
        // 클리어 이미지 설정
        if (clearImage != null && completedPuzzleImage != null)
        {
            var sprite = Sprite.Create(completedPuzzleImage,
                new Rect(0, 0, completedPuzzleImage.width, completedPuzzleImage.height),
                new Vector2(0.5f, 0.5f));
            clearImage.sprite = sprite;
        }

        // 입력 필드 초기화
        if (titleInputField != null)
            titleInputField.text = "";

        if (descriptionInputField != null)
            descriptionInputField.text = "";

        // 업로드 상태 초기화
        SetUploadStatus("_");

        // 버튼 상태 초기화
        if (uploadButton != null)
        {
            uploadButton.interactable = true;
            var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "업로드";
        }

        // 버튼 이벤트 설정
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitClick);
        }

        if (uploadButton != null)
        {
            uploadButton.onClick.RemoveAllListeners();
            uploadButton.onClick.AddListener(OnUploadClick);
        }
    }

    private IEnumerator ShowPopupFromBackAnimation()
    {
        // 원본 스케일과 회전 그대로 유지
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;

        // 투명도도 바로 보이게
        var canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        // 위치만 조정: 아래(-500)에서 중앙(0)으로
        Vector3 originalPos = transform.localPosition;
        Vector3 startPos = new Vector3(originalPos.x, -500f, originalPos.z); // 아래쪽에서 시작
        Vector3 endPos = new Vector3(originalPos.x, 0f, originalPos.z); // 중앙으로

        transform.localPosition = startPos;

        // Y좌표 애니메이션 (아래에서 중앙으로 더 강한 이즈아웃)
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            // 더 강한 이즈아웃 커브 (빠르게 와서 엄청 천천히 느려지게)
            t = 1f - (1f - t) * (1f - t) * (1f - t) * (1f - t); // Ease Out (더 강한 감속)

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 최종 위치 확정
        transform.localPosition = endPos;
    }

    public void OnUploadClick()
    {
        if (isUploading) return;

        // 빈 필드 기본값 설정
        SetDefaultValuesIfEmpty();

        StartCoroutine(UploadCoroutine());
    }

    private void SetDefaultValuesIfEmpty()
    {
        // 제목이 비어있으면 현재 날짜/시간으로 설정
        if (titleInputField != null && string.IsNullOrEmpty(titleInputField.text.Trim()))
        {
            var now = System.DateTime.Now;
            titleInputField.text = $"{now.Year:D4}. {now.Month:D2}. {now.Day:D2}. {now.Hour:D2}:{now.Minute:D2}";
        }

        // 설명이 비어있으면 사용한 태그로 설정
        if (descriptionInputField != null && string.IsNullOrEmpty(descriptionInputField.text.Trim()))
        {
            var tags = GetUsedTags();
            if (tags.Count > 0)
            {
                var tagText = "";
                for (int i = 0; i < Mathf.Min(4, tags.Count); i++)
                {
                    tagText += $"[{tags[i]}]";
                    if (i < Mathf.Min(3, tags.Count - 1))
                        tagText += ", ";
                }
                descriptionInputField.text = tagText;
            }
        }
    }

    private List<string> GetUsedTags()
    {
        // UserSession에서 태그 가져오기
        if (UserSession.Instance != null && UserSession.Instance.Tags != null)
        {
            return UserSession.Instance.Tags;
        }

        // 기본 태그 반환
        return new List<string> { "puzzle", "game", "clear", "achievement" };
    }

    private IEnumerator UploadCoroutine()
    {
        isUploading = true;

        // 업로드 시작 - _ 상태 표시
        SetUploadStatus("_");

        // 버튼 비활성화
        if (uploadButton != null)
        {
            uploadButton.interactable = false;
            var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "Uploading...";
        }

        // 업로드 시뮬레이션 (실제로는 서버 통신)
        yield return new WaitForSeconds(uploadSimulationTime);

        // TODO: 실제 서버 업로드 로직 구현
        bool uploadSuccess = SimulateUpload();

        if (uploadSuccess)
        {
            // 업로드 성공
            SetUploadStatus("Checked");

            if (uploadButton != null)
            {
                var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = "Upload Complete";
                // 버튼은 비활성화 상태 유지
            }

            Debug.Log($"Upload completed - Title: {titleInputField.text}, Description: {descriptionInputField.text}");
        }
        else
        {
            // 업로드 실패
            SetUploadStatus("Error");

            if (uploadButton != null)
            {
                uploadButton.interactable = true;
                var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = "Retry Upload";
            }

            Debug.LogWarning("Upload failed");
        }

        isUploading = false;
    }

    private void SetUploadStatus(string status)
    {
        // 모든 상태 오브젝트 비활성화
        if (uploadStatusDefault != null)
            uploadStatusDefault.SetActive(false);
        if (uploadStatusChecked != null)
            uploadStatusChecked.SetActive(false);
        if (uploadStatusError != null)
            uploadStatusError.SetActive(false);

        // 해당 상태만 활성화
        switch (status)
        {
            case "_":
                if (uploadStatusDefault != null)
                    uploadStatusDefault.SetActive(true);
                break;
            case "Checked":
                if (uploadStatusChecked != null)
                    uploadStatusChecked.SetActive(true);
                break;
            case "Error":
                if (uploadStatusError != null)
                    uploadStatusError.SetActive(true);
                break;
        }
    }

    private bool SimulateUpload()
    {
        // TODO: 실제 서버 업로드 구현
        // 현재는 항상 성공으로 시뮬레이션 (실패 테스트를 위해 랜덤으로 할 수도 있음)

        var uploadData = new
        {
            title = titleInputField.text,
            description = descriptionInputField.text,
            clearTime = currentClearTime,
            tags = GetUsedTags(),
            difficulty = GameData.difficulty,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        Debug.Log($"Upload data: {JsonUtility.ToJson(uploadData)}");

        // 10% 확률로 실패 시뮬레이션 (테스트용)
        // return Random.Range(0f, 1f) > 0.1f;

        return true; // 성공 시뮬레이션
    }

    public void OnExitClick()
    {
        // 메인 메뉴로 돌아가기
        if (fadeController != null)
        {
            fadeController.FadeToScene("000_MainMenu");
        }
        else
        {
            Debug.LogWarning("FadeController is not assigned.");
            // fallback으로 직접 씬 로드
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 제거
        if (uploadButton != null)
            uploadButton.onClick.RemoveAllListeners();

        if (exitButton != null)
            exitButton.onClick.RemoveAllListeners();
    }
}