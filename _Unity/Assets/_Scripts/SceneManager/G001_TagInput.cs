using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TagInputManager : MonoBehaviour
{
    [Header("페이드 전환용")] public FadeController fadeController;

    [Header("태그 입력 필드")] public TMP_InputField[] inputFields; // 0: Style, 1: Subject, 2: Mood, 3: Background

    [Header("주사위 버튼들")] public Button[] diceButtons;

    [Header("난이도 버튼들")] public Button[] difficultyButtons;

    [Header("태그 데이터 옵션들")] public List<string> styleOptions;

    public List<string> subjectOptions;
    public List<string> moodOptions;
    public List<string> backgroundOptions;

    private readonly bool[] isRolling = new bool[4]; // 0: Style, 1: Subject, 2: Mood, 3: Background
    private int selectedDifficulty = 3; // 기본 3*3 선택

    private void Start()
    {
        // 기본 난이도 설정
        SelectDifficulty(3);

        // (랜덤요소) 텍스트파일
        styleOptions = LoadTagOptions("TagRandom/Style");
        subjectOptions = LoadTagOptions("TagRandom/Subject");
        moodOptions = LoadTagOptions("TagRandom/Mood");
        backgroundOptions = LoadTagOptions("TagRandom/Background");
    }

    // (랜덤요소) 텍스트파일 각 항목에 넣기
    private List<string> LoadTagOptions(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            var lines = textAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(lines);
        }

        Debug.LogWarning("⚠ Tag 파일을 찾을 수 없음: " + path);
        return new List<string>();
    }


public void OnStartGame()
{
    // 태그 4개 리스트로 저장
    var selectedTags = new List<string>();
    for (var i = 0; i < 4; i++)
        selectedTags.Add(inputFields[i].text);

    // UserSession에 태그 저장
    UserSession.Instance.Tags = selectedTags;

    // 기존 GameData에도 저장
    for (var i = 0; i < 4; i++)
        GameData.tags[i] = inputFields[i].text;
    GameData.difficulty = selectedDifficulty;

Debug.Log("[TagInputManager] 세션 태그: " + string.Join(", ", UserSession.Instance.Tags));
    // 씬 전환
    fadeController.FadeToScene("G002_Game");
}

    // 뒤로가기 버튼 클릭 시 호출
    public void Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }

    // 난이도 버튼 클릭 시 호출 (2~5 전달)
    public void SelectDifficulty(int level)
    {
        selectedDifficulty = level;

        for (var i = 0; i < difficultyButtons.Length; i++)
        {
            var img = difficultyButtons[i].GetComponent<Image>();

            if (i == level - 2)
                // 강조된 색상
                switch (i)
                {
                    case 0:
                        img.color = new Color32(0x85, 0xC8, 0x8C, 255);
                        break; // EZ
                    case 1:
                        img.color = new Color32(0x6B, 0xC1, 0xD7, 255);
                        break; // NM
                    case 2:
                        img.color = new Color32(0xDE, 0xC0, 0x85, 255);
                        break; // HD
                    case 3:
                        img.color = new Color32(0xCD, 0x7F, 0x7F, 255);
                        break; // HL
                }
            else
                // 기본 색상으로 복귀
                switch (i)
                {
                    case 0:
                        img.color = new Color32(0xDF, 0xFF, 0xE2, 255);
                        break;
                    case 1:
                        img.color = new Color32(0xC6, 0xF0, 0xFB, 255);
                        break;
                    case 2:
                        img.color = new Color32(0xFF, 0xF1, 0xD6, 255);
                        break;
                    case 3:
                        img.color = new Color32(0xFF, 0xE0, 0xE0, 255);
                        break;
                }
        }
    }


    // 각 주사위 버튼 클릭 시 index 전달 (0: Style, 1: Subject, ...)
    public void OnClickDice(int index)
    {
        if (!isRolling[index])
            StartCoroutine(RollTag(index));
    }


    // 주사위 애니메이션 코루틴
    private IEnumerator RollTag(int index)
    {
        isRolling[index] = true;
        inputFields[index].interactable = false;

        var options = GetOptionList(index);

        // 1. 빠르게 30번
        for (var i = 0; i < 30; i++)
        {
            inputFields[index].text = options[Random.Range(0, options.Count)];
            yield return new WaitForSeconds(0.03f);
        }

        // 2. 느려지며 5~15번
        var delay = 0.05f;
        for (var i = 0; i < 7; i++)
        {
            inputFields[index].text = options[Random.Range(0, options.Count)];
            yield return new WaitForSeconds(delay);
            delay += 0.02f;
        }

        inputFields[index].interactable = true;
        isRolling[index] = false;
    }


    // 인덱스에 따라 태그 리스트 반환
    private List<string> GetOptionList(int index)
    {
        switch (index)
        {
            case 0: return styleOptions;
            case 1: return subjectOptions;
            case 2: return moodOptions;
            case 3: return backgroundOptions;
        }

        return null;
    }


}