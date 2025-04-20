using System;
using UnityEngine;

public class EmptyTile : MonoBehaviour
{
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private Material mat;

    private void OnMouseDown()
    {
        OnClick?.Invoke();
    }

    public event Action OnClick;


    public void SetAlpha(float alpha)
    {
        Color c = mat.color;
        c.a = Mathf.Clamp01(alpha);
        mat.color = c;
    }



    // Init 메서드로 이미지를 받아와서 처리
    public void Init(Texture2D puzzleImage, int width, int height, int x, int y)
    {
        var renderer = GetComponent<Renderer>();
        mat = renderer.material;

        mat.mainTexture = puzzleImage;
        mat.mainTextureScale = new Vector2(1f / width, 1f / height);
        mat.mainTextureOffset = new Vector2(
            1f / width * x,
            1f - 1f / height * (y + 1)
        );

        // 초기 색상 설정
        if (mat.HasProperty(ColorID))
        {
            mat.SetColor(ColorID, new Color(1f, 1f, 1f, 0f)); // 완전 투명
        }
    }


    // 빈 칸은 복원되지 않으므로 별다른 동작은 필요 없음
    public void Restore()
    {
    }
}