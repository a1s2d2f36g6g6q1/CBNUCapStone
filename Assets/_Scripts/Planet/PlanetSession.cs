using UnityEngine;

public class PlanetSession : MonoBehaviour
{
    public static PlanetSession Instance;

    public string CurrentPlanetOwnerID;     // 행성 주인 username
    public string CurrentPlanetId;          // 행성 planetId (username과 동일)

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
}