using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderStop : MonoBehaviour
{
    public GameObject thunderPrefab;

    public AudioSource audioSource;
    public AudioClip[] audioClipKnock;

    private int numm = 0; // ����� ����� �ε���


    private PlayerCtrl playerCtrl; // ����ĥ�� ���߰� �Ϸ��� �ڵ� ��������


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
            playerCtrl.enabled = false; // �Ͻ������� ����
            yield return new WaitForSeconds(1f); // 0.5�� ���� ����
            playerCtrl.enabled = true;
        }
    }
}
