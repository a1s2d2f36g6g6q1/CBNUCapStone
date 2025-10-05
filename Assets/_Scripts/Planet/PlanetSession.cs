using UnityEngine;

public class PlanetSession : MonoBehaviour
{
    public static PlanetSession Instance;

    public string CurrentPlanetOwnerID; // 현재 보고 있는 행성 주인의 ID

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 후에도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }
}