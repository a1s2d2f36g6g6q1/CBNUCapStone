using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TagInputManager : MonoBehaviour
{
    [Header("페이드 전환용")]
    public FadeController fadeController;

    [Header("태그 입력 필드")]
    public TMP_InputField[] inputFields; // 0: Style, 1: Subject, 2: Mood, 3: Background

    [Header("주사위 버튼들")]
    public Button[] diceButtons;

    [Header("난이도 버튼들")]
    public Button[] difficultyButtons;

    [Header("태그 데이터 옵션들")]
    public List<string> styleOptions;
    public List<string> subjectOptions;
    public List<string> moodOptions;
    public List<string> backgroundOptions;

    // 주사위 애니메이션 상수
    private const int FAST_ROLL_COUNT = 30;
    private const int SLOW_ROLL_COUNT = 7;
    private const float FAST_ROLL_DELAY = 0.03f;
    private const float SLOW_ROLL_START_DELAY = 0.05f;
    private const float SLOW_ROLL_DELAY_INCREMENT = 0.02f;

    // 난이도별 색상 (선택됨/기본)
    private readonly Color32[] selectedColors = {
        new Color32(0x85, 0xC8, 0x8C, 255), // EZ
        new Color32(0x6B, 0xC1, 0xD7, 255), // NM
        new Color32(0xDE, 0xC0, 0x85, 255), // HD
        new Color32(0xCD, 0x7F, 0x7F, 255)  // HL
    };

    private readonly Color32[] defaultColors = {
        new Color32(0xDF, 0xFF, 0xE2, 255), // EZ
        new Color32(0xC6, 0xF0, 0xFB, 255), // NM
        new Color32(0xFF, 0xF1, 0xD6, 255), // HD
        new Color32(0xFF, 0xE0, 0xE0, 255)  // HL
    };

    private readonly bool[] isRolling = new bool[4]; // 0: Style, 1: Subject, 2: Mood, 3: Background
    private int selectedDifficulty = 3; // 기본 3*3 선택

    private void Start()
    {
        SelectDifficulty(3);
        LoadAllTagOptions();
    }

    private void LoadAllTagOptions()
    {
        styleOptions = LoadTagOptions("TagRandom/Style");
        subjectOptions = LoadTagOptions("TagRandom/Subject");
        moodOptions = LoadTagOptions("TagRandom/Mood");
        backgroundOptions = LoadTagOptions("TagRandom/Background");

        // 로딩 실패 시 기본값 제공
        if (styleOptions.Count == 0) styleOptions.Add("Default Style");
        if (subjectOptions.Count == 0) subjectOptions.Add("Default Subject");
        if (moodOptions.Count == 0) moodOptions.Add("Default Mood");
        if (backgroundOptions.Count == 0) backgroundOptions.Add("Default Background");
    }

    private List<string> LoadTagOptions(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            var lines = textAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }

            return result;
        }

        Debug.LogWarning("⚠ Tag 파일을 찾을 수 없음: " + path);
        return new List<string>();
    }

    public void OnStartGame()
    {
        // 입력 필드 검증
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (string.IsNullOrEmpty(inputFields[i].text))
            {
                Debug.LogWarning($"태그 {i}가 비어있습니다.");
                return;
            }
        }

        // UserSession에 태그 저장
        var selectedTags = new List<string>();
        for (var i = 0; i < inputFields.Length; i++)
            selectedTags.Add(inputFields[i].text);

        UserSession.Instance.Tags = selectedTags;

        // GameData에도 저장 (기존 시스템 호환성)
        for (var i = 0; i < inputFields.Length; i++)
            GameData.tags[i] = inputFields[i].text;
        GameData.difficulty = selectedDifficulty;

        Debug.Log("[TagInputManager] 세션 태그: " + string.Join(", ", UserSession.Instance.Tags));
        Debug.Log("[TagInputManager] 선택된 난이도: " + selectedDifficulty);

        // 씬 전환
        fadeController.FadeToScene("G002_Game");
    }

    public void Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }

    public void SelectDifficulty(int level)
    {
        if (level < 2 || level > 5)
        {
            Debug.LogWarning("잘못된 난이도 레벨: " + level);
            return;
        }

        selectedDifficulty = level;

        for (var i = 0; i < difficultyButtons.Length; i++)
        {
            var img = difficultyButtons[i].GetComponent<Image>();
            if (img == null) continue;

            bool isSelected = (i == level - 2);
            img.color = isSelected ? selectedColors[i] : defaultColors[i];
        }
    }

    public void OnClickDice(int index)
    {
        if (index < 0 || index >= isRolling.Length)
        {
            Debug.LogWarning("잘못된 주사위 인덱스: " + index);
            return;
        }

        if (!isRolling[index])
            StartCoroutine(RollTag(index));
    }

    private IEnumerator RollTag(int index)
    {
        isRolling[index] = true;
        inputFields[index].interactable = false;

        var options = GetOptionList(index);
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("태그 옵션이 없습니다. 인덱스: " + index);
            isRolling[index] = false;
            inputFields[index].interactable = true;
            yield break;
        }

        // 1. 빠르게 돌리기
        for (var i = 0; i < FAST_ROLL_COUNT; i++)
        {
            inputFields[index].text = options[Random.Range(0, options.Count)];
            yield return new WaitForSeconds(FAST_ROLL_DELAY);
        }

        // 2. 느려지며 마무리
        var delay = SLOW_ROLL_START_DELAY;
        for (var i = 0; i < SLOW_ROLL_COUNT; i++)
        {
            inputFields[index].text = options[Random.Range(0, options.Count)];
            yield return new WaitForSeconds(delay);
            delay += SLOW_ROLL_DELAY_INCREMENT;
        }

        inputFields[index].interactable = true;
        isRolling[index] = false;
    }

    private List<string> GetOptionList(int index)
    {
        switch (index)
        {
            case 0: return styleOptions;
            case 1: return subjectOptions;
            case 2: return moodOptions;
            case 3: return backgroundOptions;
            default:
                Debug.LogWarning("알 수 없는 태그 인덱스: " + index);
                return null;
        }
    }
}