using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 逐渐恢复颜色
    public void FadeIn()
    {
        var targetColor = new Color(1, 1, 1, 1);
        _spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }

    // 逐渐半透明
    public void FadeOut()
    {
        var targetColor = new Color(1, 1, 1, Settings.targetAlpha);
        _spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }
}
