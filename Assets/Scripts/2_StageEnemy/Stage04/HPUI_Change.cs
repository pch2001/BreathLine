using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
public class HPUI_Change : MonoBehaviour
{
    public Image pollutionImage;              // ������ UI Image
    public GameObject HPImage;                   // HP UI Image

    public GameObject sineEffect;

    public Vector2 centerPosition = Vector2.zero; // ȭ�� �߾� ��ġ
    public float moveDuration = 1f;
    public float floatDuration = 2f;
    public float rotateDuration = 0.5f;

    private RectTransform rect;
    //��ġ, ũ��, ȸ��, ��Ŀ ���� �����ϴ� ���� Ʈ������

    void Start()
    {
        rect = pollutionImage.rectTransform;
        StartCoroutine(PollutionEffectSequence());
        HPImage.SetActive(false);

    }

    IEnumerator PollutionEffectSequence()
    {

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = centerPosition;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        Camera.main.GetComponent<CameraShake>()?.StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(0.5f, 1f));

        //ȸ�� 
        StartCoroutine(BlinkAlpha());
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rect.position);

        // ȭ�� ��ǥ�� ���� ��ǥ�� ��ȯ (z �� ����)
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f)); // z�� ī�޶���� �Ÿ�

        Instantiate(sineEffect, worldPos, Quaternion.identity);

        pollutionImage.color = new Color(1f, 0.3f, 0.3f, 0f);

        t = 0;
        Quaternion startRot = Quaternion.Euler(0, 0, 0); // ���� ����
        Quaternion endRot = Quaternion.Euler(0, 0, 90);    // ���� ����
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
            yield return null;
        }

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
        HPImage.SetActive(true);
    }
}
