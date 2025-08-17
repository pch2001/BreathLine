using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FuncExtention
{
    // * SpriteRenderer 기능 확장

    // Fade 효과 기능(목표 투명도값, 걸린 시간)
    public static IEnumerator FadeTo(this SpriteRenderer sr, float targetAlpha, float duration)
    {
        float startAlpha = sr.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);

            Color c = sr.color;
            c.a = newAlpha;
            sr.color = c;

            yield return null;
        }

        Color final = sr.color;
        final.a = targetAlpha;
        sr.color = final;
    }
}
