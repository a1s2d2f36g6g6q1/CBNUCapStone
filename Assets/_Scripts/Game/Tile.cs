using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    private int correctX, correctY;
    private int currentX, currentY;

    private PuzzleManager puzzleManager;
    private Image image;

    public void Init(PuzzleManager manager, int x, int y, Texture2D texture, int width, int height)
    {
        puzzleManager = manager;

        correctX = x;
        correctY = y;
        currentX = x;
        currentY = y;

        image = GetComponent<Image>();

        Rect rect = new Rect(
            (float)x / width * texture.width,
            (float)y / height * texture.height,
            texture.width / width,
            texture.height / height
        );

        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        image.sprite = sprite;

        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => puzzleManager.TryMove(currentX, currentY));
    }

    public void MoveTo(int newX, int newY)
    {
        currentX = newX;
        currentY = newY;

        // 자동으로 GridLayoutGroup 내에서 정렬되므로 별도 위치이동 불필요!
    }

    public bool IsCorrect()
    {
        return currentX == correctX && currentY == correctY;
    }
}