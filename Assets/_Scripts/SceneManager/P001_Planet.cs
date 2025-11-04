using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class P001_Planet : MonoBehaviour
{
    [Header("Planet List")]
    public Transform planetListContainer;
    public GameObject planetCardPrefab;

    [Header("Sort Buttons")]
    public Button recentSortButton;
    public Button recommendSortButton;

    [Header("Search")]
    public TMP_InputField searchInputField;
    public Button searchButton;

    private List<PlanetListItem> allPlanets = new List<PlanetListItem>();
    private List<FavoriteItem> favoritePlanets = new List<FavoriteItem>();
    private bool isRecentSort = true;

    private void Start()
    {
        // Button listeners
        recentSortButton?.onClick.AddListener(() => OnClick_SortRecent());
        recommendSortButton?.onClick.AddListener(() => OnClick_SortRecommend());
        searchButton?.onClick.AddListener(() => OnClick_Search());

        // Load planet list
        StartCoroutine(LoadPlanetListCoroutine());
    }

    #region Load Data
    private IEnumerator LoadPlanetListCoroutine()
    {
        // Get all planet list
        yield return APIManager.Instance.Get(
            "/planets",
            onSuccess: (response) =>
            {
                // FIXED: Parse with wrapper structure
                PlanetListResponse planetResponse = JsonUtility.FromJson<PlanetListResponse>(response);

                if (planetResponse.isSuccess && planetResponse.result != null)
                {
                    // FIXED: result.planets is the array
                    allPlanets = new List<PlanetListItem>(planetResponse.result.planets);
                    Debug.Log($"Planet list loaded: {allPlanets.Count} planets");

                    // If logged in, load favorites too
                    if (UserSession.Instance.IsLoggedIn)
                    {
                        StartCoroutine(LoadFavoriteListCoroutine());
                    }
                    else
                    {
                        RefreshPlanetList();
                    }
                }
                else
                {
                    Debug.LogError($"Planet list load failed: {planetResponse.message}");
                }
            },
            onError: (error) =>
            {
                Debug.LogError("Planet list load failed: " + error);
            }
        );
    }

    private IEnumerator LoadFavoriteListCoroutine()
    {
        yield return APIManager.Instance.Get(
            "/planets/favorites/me",
            onSuccess: (response) =>
            {
                // FIXED: Parse with wrapper structure
                FavoriteListResponse favoriteResponse = JsonUtility.FromJson<FavoriteListResponse>(response);

                if (favoriteResponse.isSuccess && favoriteResponse.result != null)
                {
                    // FIXED: result.favorites is the array
                    favoritePlanets = new List<FavoriteItem>(favoriteResponse.result.favorites);
                    Debug.Log($"Favorite list loaded: {favoritePlanets.Count} favorites");
                }

                RefreshPlanetList();
            },
            onError: (error) =>
            {
                Debug.LogError("Favorite list load failed: " + error);
                RefreshPlanetList();
            }
        );
    }

    private void RefreshPlanetList()
    {
        // Clear existing cards
        foreach (Transform child in planetListContainer)
        {
            Destroy(child.gameObject);
        }

        // Create sorted list
        List<PlanetListItem> sortedList = new List<PlanetListItem>(allPlanets);

        if (isRecentSort)
        {
            // FIXED: Sort by username (in real case, should sort by createdAt)
            sortedList.Sort((a, b) => string.Compare(b.username, a.username));
        }
        else
        {
            // Sort by visit count (recommendation)
            sortedList.Sort((a, b) => b.visitCount.CompareTo(a.visitCount));
        }

        // Put favorites first
        List<PlanetListItem> favoriteFirst = new();
        List<PlanetListItem> others = new();

        foreach (var planet in sortedList)
        {
            // FIXED: Use username to check favorites
            if (IsFavorite(planet.username))
                favoriteFirst.Add(planet);
            else
                others.Add(planet);
        }

        favoriteFirst.AddRange(others);

        // Create cards
        foreach (var planet in favoriteFirst)
        {
            var card = Instantiate(planetCardPrefab, planetListContainer);
            var planetCard = card.GetComponent<PlanetCard>();
            bool isFav = IsFavorite(planet.username);
            planetCard.Init(planet, isFav, this);
        }
    }

    private bool IsFavorite(string username)
    {
        // FIXED: FavoriteItem has 'username' field
        return favoritePlanets.Exists(p => p.username == username);
    }
    #endregion

    #region Sort Buttons
    public void OnClick_SortRecent()
    {
        isRecentSort = true;
        RefreshPlanetList();
        Debug.Log("Sort: Recent");
    }

    public void OnClick_SortRecommend()
    {
        isRecentSort = false;
        RefreshPlanetList();
        Debug.Log("Sort: Recommend");
    }
    #endregion

    #region Search
    public void OnClick_Search()
    {
        string query = searchInputField.text.Trim().ToLower();

        if (string.IsNullOrEmpty(query))
        {
            RefreshPlanetList();
            return;
        }

        // Clear existing cards
        foreach (Transform child in planetListContainer)
        {
            Destroy(child.gameObject);
        }

        // FIXED: Search by username or title (no ownerNickname in API)
        var filtered = allPlanets.FindAll(p =>
            (p.username?.ToLower().Contains(query) ?? false) ||
            (p.title?.ToLower().Contains(query) ?? false)
        );

        // Create cards
        foreach (var planet in filtered)
        {
            var card = Instantiate(planetCardPrefab, planetListContainer);
            var planetCard = card.GetComponent<PlanetCard>();
            bool isFav = IsFavorite(planet.username);
            planetCard.Init(planet, isFav, this);
        }
    }
    #endregion

    #region Toggle Favorite (called from PlanetCard)
    public void ToggleFavorite(string username, bool currentState)
    {
        if (!UserSession.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Login required");
            return;
        }

        StartCoroutine(ToggleFavoriteCoroutine(username, currentState));
    }

    private IEnumerator ToggleFavoriteCoroutine(string username, bool currentState)
    {
        if (currentState)
        {
            // Remove from favorites
            yield return APIManager.Instance.Delete(
                $"/planets/{username}/favorite",
                onSuccess: (response) =>
                {
                    Debug.Log("Favorite removed successfully");
                    favoritePlanets.RemoveAll(p => p.username == username);
                    RefreshPlanetList();
                },
                onError: (error) =>
                {
                    Debug.LogError("Favorite removal failed: " + error);
                }
            );
        }
        else
        {
            // Add to favorites
            yield return APIManager.Instance.Post(
                $"/planets/{username}/favorite",
                new { },
                onSuccess: (response) =>
                {
                    Debug.Log("Favorite added successfully");

                    // Create FavoriteItem and add to list
                    var planet = allPlanets.Find(p => p.username == username);
                    if (planet != null)
                    {
                        var favoriteItem = new FavoriteItem
                        {
                            planetId = planet.planetId,
                            username = planet.username,
                            title = planet.title,
                            visitCount = planet.visitCount,
                            createdAt = planet.createdAt,
                            profileImageUrl = planet.profileImageUrl,
                            favoritedAt = System.DateTime.UtcNow.ToString("o")
                        };
                        favoritePlanets.Add(favoriteItem);
                    }

                    RefreshPlanetList();
                },
                onError: (error) =>
                {
                    Debug.LogError("Favorite addition failed: " + error);
                }
            );
        }
    }
    #endregion
}