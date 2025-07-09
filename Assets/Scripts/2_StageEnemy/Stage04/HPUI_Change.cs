using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
public class HPUI_Change : MonoBehaviour
{
    public Image pollutionImage;              // 오염도 UI Image
    public GameObject HPImage;                   // HP UI Image
    public GameObject sineEffect;
    public GameObject returnHP; // 변경된 HP 게이지

    public Vector2 centerPosition = Vector2.zero; // 화면 중앙 위치
    public float moveDuration = 1f;
    public float floatDuration = 2f;
    public float rotateDuration = 0.5f;

    private RectTransform rect;
    //위치, 크기, 회전, 앵커 등을 제어하는 전용 트랜스폼

    void Start()
    {
        rect = pollutionImage.rectTransform;
        StartCoroutine(PollutionEffectSequence());
        HPImage.SetActive(false);

    }

    IEnumerator PollutionEffectSequence()
    {
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(-850f, 0f); // 오른쪽으로 이동
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        Camera.main.GetComponent<CameraShake>()?.StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(0.5f, 1f));

        //회전 
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rect.position);

        // 화면 좌표를 월드 좌표로 변환 (z 값 주의)
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f)); // z는 카메라와의 거리

        Instantiate(sineEffect, worldPos, Quaternion.identity);

        Image[] allImages = pollutionImage.GetComponentsInChildren<Image>();
        Color[] startColors = new Color[allImages.Length];
        Color[] endColors = new Color[allImages.Length];

        for (int i = 0; i < allImages.Length; i++)
        {
            startColors[i] = allImages[i].color;
            endColors[i] = new Color(startColors[i].r, startColors[i].g, startColors[i].b, 0f);
        }

        t = 0;
        Quaternion startRot = Quaternion.Euler(0, 0, 0); // 세로 방향
        Quaternion endRot = Quaternion.Euler(0, 0, 90);    // 가로 방향
        rect.rotation = startRot;

        while (t < 1f)
        {
            t += Time.deltaTime / rotateDuration;
            rect.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        Vector2 floatStart = rect.anchoredPosition;
        Vector2 floatEnd = floatStart + new Vector2(0, -450);

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / floatDuration;
            rect.anchoredPosition = Vector2.Lerp(floatStart, floatEnd, t);

            for (int i = 0; i < allImages.Length; i++)
            {
                allImages[i].color = Color.Lerp(startColors[i], endColors[i], t);
            }

            yield return null;
        }

        returnHP.SetActive(true);
        StartCoroutine(ReturnHpAppear(returnHP.GetComponent<CanvasGroup>(), 2f));
    }

     IEnumerator BlinkAlpha()
    {
        float t=0;
        while (t<3)
        {
            t += Time.deltaTime;
            float alpha = Mathf.PingPong(Time.time * 2f, 1f);
            Color c = pollutionImage.color;
            pollutionImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
    }

    private IEnumerator ReturnHpAppear(CanvasGroup returnHp, float duration) // 회귀 후 Hp 등장 함수
    {
        float time = 0f;
        returnHp.alpha = 0f;
        returnHp.interactable = false;
        returnHp.blocksRaycasts = false;

        while (time < duration)
        {
            time += Time.deltaTime;
            returnHp.alpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }

        returnHp.alpha = 1f;
        returnHp.interactable = true;
        returnHp.blocksRaycasts = true;
    }
}
