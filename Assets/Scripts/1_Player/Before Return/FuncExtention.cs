using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FuncExtention
{
    // * SpriteRenderer ��� Ȯ��

    // Fade ȿ�� ���(��ǥ ������, �ɸ� �ð�)
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
