using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public Vector2Int correctPosition;

    private PuzzleManager puzzleManager;
    private Coroutine moveCoroutine;

    private Material mat;
    private Texture2D storedTexture;
    private Vector2 textureScale;
    private Vector2 textureOffset;

    public void Init(PuzzleManager manager, int x, int y, int correctX, int correctY, Texture2D puzzleImage, int width, int height)
    {
        puzzleManager = manager;
        gridPosition = new Vector2Int(x, y);
        correctPosition = new Vector2Int(correctX, correctY);

        mat = GetComponent<Renderer>().material;
        storedTexture = puzzleImage;
        textureScale = new Vector2(1f / width, 1f / height);
        textureOffset = new Vector2(
            1f / width * correctX,
            1f - 1f / height * (correctY + 1)
        );

        ApplyImage();
    }

    public void ApplyImage()
    {
        if (mat != null && storedTexture != null)
        {
            mat.mainTexture = storedTexture;
            mat.mainTextureScale = textureScale;
            mat.mainTextureOffset = textureOffset;
            mat.color = Color.white;
        }
    }

    void OnMouseDown()
    {
        puzzleManager.OnTileClicked(this);
    }

    public void MoveTo(Vector2Int newPos)
    {
        gridPosition = newPos;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveSmooth(puzzleManager.GetTilePosition(newPos.x, newPos.y)));
    }

    private IEnumerator MoveSmooth(Vector3 target)
    {
        while (Vector3.Distance(transform.localPosition, target) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * 8f);
            yield return null;
        }

        transform.localPosition = target;
    }

    public bool IsCorrect()
    {
        return gridPosition == correctPosition;
    }
}