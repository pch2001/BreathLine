using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderStop : MonoBehaviour
{
    public GameObject thunderPrefab;

    public AudioSource audioSource;
    public AudioClip[] audioClipKnock;

    private int numm = 0; // 재생할 오디오 인덱스


    private PlayerCtrl playerCtrl; // 번개칠때 멈추게 하려고 코드 가져오기


    void Start()
    {
        GameObject playerCode = GameObject.FindWithTag("Player");
        if (playerCode != null)
        {
            playerCtrl = playerCode.GetComponent<PlayerCtrl>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameObject obj = Instantiate(thunderPrefab, transform.position, Quaternion.identity);
            Destroy(obj, 3f); 
            if (audioClipKnock.Length > 0 && numm < audioClipKnock.Length)
            {
                audioSource.PlayOneShot(audioClipKnock[numm]);
                StartCoroutine(PlayerStun());
                numm++;
            }
            else
            {
                numm = 0;
            }
        }

    }

    IEnumerator PlayerStun()
    {
        if (playerCtrl != null)
        {
            playerCtrl.enabled = false; // 일시적으로 멈춤
            yield return new WaitForSeconds(1f); // 0.5초 동안 정지
            playerCtrl.enabled = true;
        }
    }
}
