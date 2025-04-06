using UnityEngine;

public class Tile : MonoBehaviour
{
    private PuzzleManager manager;
    private int correctX, correctY;
    private int currentX, currentY;

    public void Init(PuzzleManager _manager, int x, int y, Texture2D texture, int w, int h)
    {
        manager = _manager;
        correctX = x;
        correctY = y;
        currentX = x;
        currentY = y;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = texture;

        Vector2 offset = new Vector2((float)x / w, (float)y / h);
        Vector2 scale = new Vector2(1f / w, 1f / h);

        mat.mainTextureOffset = offset;
        mat.mainTextureScale = scale;

        renderer.material = mat;
    }

    public void MoveTo(int x, int y)
    {
        currentX = x;
        currentY = y;

        Vector3 targetPos = new Vector3(
            -(manager.width - 1) / 2f + x * (1 + manager.spacing),
            0,
            -(manager.height - 1) / 2f + y * (1 + manager.spacing)
        );

        StopAllCoroutines();
        StartCoroutine(MoveSmooth(targetPos));
    }

    System.Collections.IEnumerator MoveSmooth(Vector3 target)
    {
        while (Vector3.Distance(transform.localPosition, target) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, 10f * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = target;
    }

    public bool IsCorrect()
    {
        return currentX == correctX && currentY == correctY;
    }

    void OnMouseDown()
    {
        manager.TryMove(currentX, currentY);
    }
}