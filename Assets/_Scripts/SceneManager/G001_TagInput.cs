using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TagInputManager : MonoBehaviour
{
    [Header("Fade Controller")]
    public FadeController fadeController;

    [Header("Tag Input Fields")]
    public TMP_InputField[] inputFields; // 0: Style, 1: Subject, 2: Mood, 3: Background

    [Header("Dice Buttons")]
    public Button[] diceButtons;

    [Header("Difficulty Buttons")]
    public Button[] difficultyButtons;

    [Header("Tag Options")]
    public List<string> styleOptions;
    public List<string> subjectOptions;
    public List<string> moodOptions;
    public List<string> backgroundOptions;

    // Dice animation constants
    private const int FAST_ROLL_COUNT = 30;
    private const int SLOW_ROLL_COUNT = 7;
    private const float FAST_ROLL_DELAY = 0.03f;
    private const float SLOW_ROLL_START_DELAY = 0.05f;
    private const float SLOW_ROLL_DELAY_INCREMENT = 0.02f;

    // Difficulty colors (selected/default)
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

    private readonly bool[] isRolling = new bool[4];
    private int selectedDifficulty = 3; // Default 3x3

    private void Start()
    {
        SelectDifficulty(3);
        LoadAllTagOptions();

        // If not logged in, create guest session
        EnsureGuestSession();
    }

    /// <summary>
    /// Ensure guest session exists if user is not logged in
    /// </summary>
    private void EnsureGuestSession()
    {
        if (UserSession.Instance != null)
        {
            if (!UserSession.Instance.IsLoggedIn && !UserSession.Instance.IsGuest)
            {
                StartCoroutine(CreateGuestSession());
            }
        }
    }

    /// <summary>
    /// Create guest account and get temporary token
    /// </summary>
    private IEnumerator CreateGuestSession()
    {
        // Generate guest credentials
        string guestUsername = $"guest_{System.Guid.NewGuid().ToString("N").Substring(0, 8)}";
        string guestPassword = System.Guid.NewGuid().ToString("N");
        string guestNickname = $"Guest_{UnityEngine.Random.Range(1000, 9999)}";

        Debug.Log($"[TagInput] Creating guest account: {guestNickname}");

        // Sign up guest account
        SignupRequest signupRequest = new SignupRequest
        {
            username = guestUsername,
            password = guestPassword,
            nickname = guestNickname
        };

        bool signupComplete = false;
        bool signupSuccess = false;

        yield return APIManager.Instance.Post(
            "/users/signup",
            signupRequest,
            onSuccess: (response) =>
            {
                Debug.Log("[TagInput] Guest account created successfully");
                signupSuccess = true;
                signupComplete = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[TagInput] Guest account creation failed: {error}");
                signupSuccess = false;
                signupComplete = true;
            }
        );

        yield return new WaitUntil(() => signupComplete);

        if (!signupSuccess)
        {
            Debug.LogError("[TagInput] Failed to create guest account");
            yield break;
        }

        // FIXED: Login with guest account using userId field
        LoginRequest loginRequest = new LoginRequest
        {
            userId = guestUsername,  // FIXED: API spec uses "userId" not "username"
            password = guestPassword
        };

        bool loginComplete = false;

        yield return APIManager.Instance.Post(
            "/users/login",
            loginRequest,
            onSuccess: (response) =>
            {
                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);

                // Save token
                if (!string.IsNullOrEmpty(loginResponse.token))
                {
                    APIManager.Instance.SetToken(loginResponse.token);
                    Debug.Log("[TagInput] Guest token saved");
                }

                // Set guest session
                UserSession.Instance.SetUserInfo(guestUsername, guestNickname, true);

                Debug.Log($"[TagInput] Guest login success: {guestNickname}");
                loginComplete = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[TagInput] Guest login failed: {error}");
                loginComplete = true;
            }
        );

        yield return new WaitUntil(() => loginComplete);
    }

    private void LoadAllTagOptions()
    {
        styleOptions = LoadTagOptions("TagRandom/Style");
        subjectOptions = LoadTagOptions("TagRandom/Subject");
        moodOptions = LoadTagOptions("TagRandom/Mood");
        backgroundOptions = LoadTagOptions("TagRandom/Background");

        // Fallback values if loading fails
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

        Debug.LogWarning($"[TagInput] Tag file not found: {path}");
        return new List<string>();
    }

    public void OnStartGame()
    {
        // Validate input fields
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (string.IsNullOrEmpty(inputFields[i].text))
            {
                Debug.LogWarning($"[TagInput] Tag {i} is empty");
                return;
            }
        }

        // Save tags to UserSession
        var selectedTags = new List<string>();
        for (var i = 0; i < inputFields.Length; i++)
            selectedTags.Add(inputFields[i].text);

        UserSession.Instance.Tags = selectedTags;

        // Also save to GameData (for compatibility)
        for (var i = 0; i < inputFields.Length; i++)
            GameData.tags[i] = inputFields[i].text;
        GameData.difficulty = selectedDifficulty;

        Debug.Log("[TagInput] Session tags: " + string.Join(", ", UserSession.Instance.Tags));
        Debug.Log("[TagInput] Selected difficulty: " + selectedDifficulty);

        // Transition to game scene
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
            Debug.LogWarning($"[TagInput] Invalid difficulty level: {level}");
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
            Debug.LogWarning($"[TagInput] Invalid dice index: {index}");
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
            Debug.LogWarning($"[TagInput] No tag options for index: {index}");
            isRolling[index] = false;
            inputFields[index].interactable = true;
            yield break;
        }

        // Fast roll
        for (var i = 0; i < FAST_ROLL_COUNT; i++)
        {
            inputFields[index].text = options[Random.Range(0, options.Count)];
            yield return new WaitForSeconds(FAST_ROLL_DELAY);
        }

        // Slow down
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
                Debug.LogWarning($"[TagInput] Unknown tag index: {index}");
                return null;
        }
    }
}