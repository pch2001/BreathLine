using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
public class HPUI_Change : MonoBehaviour
{
    public Image pollutionImage;              // ������ UI Image
    public GameObject HPImage;                   // HP UI Image
    public GameObject sineEffect;
    public GameObject returnHP; // ����� HP ������

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
        Vector2 endPos = startPos + new Vector2(-850f, 0f); // ���������� �̵�
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        Camera.main.GetComponent<CameraShake>()?.StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(0.5f, 1f));

        //ȸ�� 
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rect.position);

        // ȭ�� ��ǥ�� ���� ��ǥ�� ��ȯ (z �� ����)
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f)); // z�� ī�޶���� �Ÿ�

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

    private IEnumerator ReturnHpAppear(CanvasGroup returnHp, float duration) // ȸ�� �� Hp ���� �Լ�
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
