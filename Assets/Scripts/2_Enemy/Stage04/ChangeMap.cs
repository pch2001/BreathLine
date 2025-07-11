using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeMap : MonoBehaviour
{
    public GameObject[] pase;
    public int paseIndex = 0;

    public Image flashImage;

    private Camera mainCam;
    private Vector3 originalCamPos;

    private PlayerCtrl_R playerCtrl;

    // Start is called before the first frame update
    void Start()
    {
        playerCtrl = GameObject.FindWithTag("Player").GetComponent<PlayerCtrl_R>();

        mainCam = Camera.main;
        if (mainCam != null)
        {
            originalCamPos = mainCam.transform.localPosition;
        }
    }

    public void Pase(int number)
    {
        switch(number)
        {
            case 0: // ªÁ∏∑
                Time.timeScale = 1f;
                StartCoroutine(ShakeCamera(1.5f, 1f));
                break;
            case 1: // ±‚∞Ë
                Time.timeScale = 1.0f;
                StartCoroutine(ShakeCamera(1.5f, 1f));
                break;
            case 2: // end
                StartCoroutine(ShakeCamera(1.5f, 1f));
                break;
            case 3: // ±‚¡∏
                StartCoroutine(Pase4());
                break;
            default:
                Debug.LogWarning("Invalid pase number: " + number);
                break;
        }
    }
  
    public void Pase3()
    {
        StartCoroutine(ShakeCamera(1.5f, 1f));
        Debug.Log("∞·«Ã(∫¿¿Œ¥Á«œ¥¬ ƒ⁄µÂ)");
        //playerCtrl.isPase4 = true;
    }

    IEnumerator Pase4()
    {   
        yield return StartCoroutine(ShakeCamera(1.5f, 0.8f));

        yield return StartCoroutine(ShakeCamera(1.5f, 0.8f));
        
        yield return StartCoroutine(ShakeCamera(1.5f, 0.8f));
        
        yield return StartCoroutine(ShakeCamera(1.5f, 0.8f));

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

        // π‡æ∆¡¸
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

        // æÓµŒøˆ¡¸
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
