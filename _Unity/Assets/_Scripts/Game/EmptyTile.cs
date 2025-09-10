using System;
using UnityEngine;

public class EmptyTile : MonoBehaviour
{
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private Material mat;

    public event Action OnClick;

    private void OnMouseDown()
    {
        OnClick?.Invoke();
    }

    public void Init(Texture2D puzzleImage, int width, int height, int x, int y)
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            mat = renderer.material;

            if (puzzleImage != null)
            {
                mat.mainTexture = puzzleImage;
                mat.mainTextureScale = new Vector2(1f / width, 1f / height);
                mat.mainTextureOffset = new Vector2(
                    1f / width * x,
                    1f - 1f / height * (y + 1)
                );
            }

            // 초기 색상 설정 (완전 투명)
            if (mat.HasProperty(ColorID))
            {
                mat.SetColor(ColorID, new Color(1f, 1f, 1f, 0f));
            }
        }
        else
        {
            Debug.LogError("EmptyTile에 Renderer가 없습니다.");
        }
    }

    public void SetAlpha(float alpha)
    {
        if (mat == null || !mat.HasProperty(ColorID)) return;

        Color c = mat.color;
        c.a = Mathf.Clamp01(alpha);
        mat.color = c;
    }

    public void SetFadeOut(float t)
    {
        if (mat == null || !mat.HasProperty(ColorID)) return;

        Color c = mat.color;
        c.a = Mathf.Lerp(1f, 0f, t);
        mat.SetColor(ColorID, c);
    }

    public void Restore()
    {
        // 빈 칸은 복원되지 않으므로 별다른 동작 필요 없음
        // 필요하다면 여기에 로직 추가
    }
}