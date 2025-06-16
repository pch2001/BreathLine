using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeMap : MonoBehaviour
{
    public GameObject[] pase;
    private int paseIndex = 0;

    public Image flashImage;

    private Camera mainCam;
    private Vector3 originalCamPos;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            originalCamPos = mainCam.transform.localPosition;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine(ShakeCamera(1.5f, 1f));
        }
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        float elapsed = 0f;

        if (flashImage != null)
        {
            StartCoroutine(FlashEffect(0.2f));
        }

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-3f, 3f) * magnitude;
            float offsetY = Random.Range(-3f, 3f) * magnitude;

            mainCam.transform.localPosition = originalCamPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.localPosition = originalCamPos;
    }



    IEnumerator FlashEffect(float flashDuration)
    {
        float timer = 0f;
        Color color = flashImage.color;

        // ¹à¾ÆÁü
        while (timer < flashDuration / 2f)
        {
            float alpha = Mathf.Lerp(0, 1, timer / (flashDuration / 2f));
            color.a = alpha;
            flashImage.color = color;
            timer += Time.deltaTime;
            yield return null;
        }

        pase[paseIndex].SetActive(false);
        pase[++paseIndex].SetActive(true);


        // ¾îµÎ¿öÁü
        timer = 0f;
        while (timer < flashDuration / 2f)
        {
            float alpha = Mathf.Lerp(1, 0, timer / (flashDuration / 2f));
            color.a = alpha;
            flashImage.color = color;
            timer += Time.deltaTime;
            yield return null;
        }

        color.a = 0;
        flashImage.color = color;
    }

}
