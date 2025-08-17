using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCtrl : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        var c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, 0f);
    }

    void Start()
    {
        StartCoroutine(FadeCoroutine(1f, 0.5f));
    }

    public IEnumerator FadeCoroutine(float targetAlpha, float duration) // 음표 fade 효과 코루틴 (targetAlpha: 목표 투명도, duration: 실행시간)
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, targetAlpha);
    }
}
