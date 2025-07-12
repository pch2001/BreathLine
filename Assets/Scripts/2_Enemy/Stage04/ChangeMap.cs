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
    public Story_four_R story4R_2; // 정신착란 스토리 오브젝트
    public GameObject BossR4; // 보스 오브젝트


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
            case 0: // 사막
                Time.timeScale = 1f;
                StartCoroutine(ShakeCamera(1.5f, 1f));
                break;
            case 1: // 기계
                Time.timeScale = 1.0f;
                StartCoroutine(ShakeCamera(1.5f, 1f));
                break;
            case 2: // 기존
                StartCoroutine(ShakeCamera(1.5f, 1f));
                break;
            case 3: // 기존
                StartCoroutine(Pase4());
                break;
            default:
                Debug.LogWarning("Invalid pase number: " + number);
                break;
        }
    }

    IEnumerator Pase4()
    {
        BossR4.GetComponent<Animator>().SetBool("isRun", false);// Idle 상태로 변경

        yield return StartCoroutine(ShakeCamera(1.5f, 0.8f));

        yield return StartCoroutine(ShakeCamera(1.5f, 0.8f));
        
        yield return StartCoroutine(ShakeCamera(1.5f, 0.8f));
        
        yield return StartCoroutine(ShakeCamera(1.5f, 0.8f));

        StartCoroutine(story4R_2.TypingText(2));
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

        // 밝아짐
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

        // 어두워짐
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
