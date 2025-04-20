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
        puzzleManager.TryMove(this);
    }

    public void Init(PuzzleManager manager, int x, int y, int correctX, int correctY, Texture2D puzzleImage, int width,
        int height)
    {
        puzzleManager = manager;
        gridPosition = new Vector2Int(x, y);
        correctPosition = new Vector2Int(correctX, correctY);

        var renderer = GetComponent<Renderer>();
        mat = renderer.material;
        renderer.material = mat;

        // 텍스처 세팅
        mat.mainTexture = puzzleImage;
        mat.mainTextureScale = new Vector2(1f / width, 1f / height);
        mat.mainTextureOffset = new Vector2(
            1f / width * correctX,
            1f - 1f / height * (correctY + 1)
        );

        // 초기 색상 = 완전 흰색 불투명
        if (mat.HasProperty(ColorID))
            mat.SetColor(ColorID, new Color(1f, 1f, 1f, 1f));
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

    public void SetFadeGray(float t)
    {
        if (!mat.HasProperty(ColorID)) return;

        // 흰색에서 검정으로 직접 변화 (회색도 포함되지만, 최종적으로 검정까지 확실히 변경)
        var from = Color.white; // 흰색
        var to = new Color(0.5f, 0.5f, 0.5f); // 중간 회색으로 설정
        var c = Color.Lerp(from, to, t); // 색상 보간 (회색 범위까지 확장)

        // 회색에서 검정으로 확실히 변경
        var black = new Color(0f, 0f, 0f); // 검정색으로 가는 최종 목표

        // 회색에서 검정으로 최종적으로 가기 위한 보간 추가
        c = Color.Lerp(c, black, t);

        // 최종 색상과 알파 값 설정
        c.a = 1f; // 알파는 완전 불투명
        mat.SetColor(ColorID, c);
    }

    public void MoveToFinal(Vector3 target)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveSmooth(target));
    }


    // 📕 완전 페이드아웃: 검정색으로 고정
    public void FadeToBlack()
    {
        if (!mat.HasProperty(ColorID)) return;

        mat.SetColor(ColorID, new Color(0f, 0f, 0f, 1f));
    }

    // 🟢 복원: 다시 흰색 + 텍스처 보이게
    public void Restore()
    {
        if (!mat.HasProperty(ColorID)) return;

        mat.SetColor(ColorID, new Color(1f, 1f, 1f, 1f));
    }
}