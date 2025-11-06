public static class GameData
{
    public static string[] tags = new string[4];
    public static int difficulty = 3;

    // Game session data
    public static string gameCode = "";
    public static string imageUrl = "";
    public static bool isMultiplay = false;

    // Clear time for API submission
    public static string startTime = "";
    public static string endTime = "";

    public static void Reset()
    {
        gameCode = "";
        imageUrl = "";
        isMultiplay = false;
        startTime = "";
        endTime = "";
    }
}