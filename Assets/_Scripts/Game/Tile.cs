using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    public Vector2Int gridPosition;
    public Vector2Int correctPosition;
    private Material mat;
    private Coroutine moveCoroutine;
    private PuzzleManager puzzleManager;

    private void OnMouseDown()
    {
        if (puzzleManager != null)
            puzzleManager.TryMove(this);
    }

    public void Init(PuzzleManager manager, int x, int y, int correctX, int correctY, Texture2D puzzleImage, int width, int height)
    {
        puzzleManager = manager;
        gridPosition = new Vector2Int(x, y);
        correctPosition = new Vector2Int(correctX, correctY);

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            mat = renderer.material;

            // 텍스처 세팅
            if (puzzleImage != null)
            {
                mat.mainTexture = puzzleImage;
                mat.mainTextureScale = new Vector2(1f / width, 1f / height);
                mat.mainTextureOffset = new Vector2(
                    1f / width * correctX,
                    1f - 1f / height * (correctY + 1)
                );
            }

            // 초기 색상 = 완전 흰색 불투명
            if (mat.HasProperty(ColorID))
                mat.SetColor(ColorID, new Color(1f, 1f, 1f, 1f));
        }
        else
        {
            Debug.LogError("Tile에 Renderer가 없습니다.");
        }
    }

    public void MoveTo(Vector2Int newPos)
    {
        gridPosition = newPos;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        if (puzzleManager != null)
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

    public void SetFadeGray(float t)
    {
        if (mat == null || !mat.HasProperty(ColorID)) return;

        // 흰색에서 검정색으로 직접 보간 (단순화)
        var c = Color.Lerp(Color.white, Color.black, t);
        c.a = 1f;
        mat.SetColor(ColorID, c);
    }

    public void FadeToBlack()
    {
        if (mat == null || !mat.HasProperty(ColorID)) return;
        mat.SetColor(ColorID, new Color(0f, 0f, 0f, 1f));
    }

    public void Restore()
    {
        if (mat == null || !mat.HasProperty(ColorID)) return;
        mat.SetColor(ColorID, new Color(1f, 1f, 1f, 1f));
    }
}