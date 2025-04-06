using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public Vector2Int correctPosition;
    public bool isEmpty = false;

    private PuzzleManager puzzleManager;

    public void Init(PuzzleManager manager, int x, int y, int correctX, int correctY, Texture2D puzzleImage, int width, int height)
    {
        puzzleManager = manager;
        gridPosition = new Vector2Int(x, y);
        correctPosition = new Vector2Int(correctX, correctY);

        Material mat = GetComponent<Renderer>().material;
        mat.mainTexture = puzzleImage;

        mat.mainTextureScale = new Vector2(1f / width, 1f / height);
        mat.mainTextureOffset = new Vector2(
            1f / width * correctX,
            1f / height * (height - 1 - correctY) // 🔄 이미지 위아래 반전
        );
    }

    void OnMouseDown()
    {
        if (!isEmpty)
        {
            puzzleManager.TryMove(this);
        }
    }

    public void MoveTo(Vector2Int newPosition)
    {
        gridPosition = newPosition;
        transform.localPosition = puzzleManager.GetTilePosition(newPosition.x, newPosition.y);
    }

    public bool IsCorrect()
    {
        return gridPosition == correctPosition;
    }
}