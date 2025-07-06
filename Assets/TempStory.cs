using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempStory : MonoBehaviour
{
    public GameObject girlBoss;
    public ChangeMap changeMap;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(PlayStroy(collision));
        }
    }

    private IEnumerator PlayStroy(Collider2D collision)
    {
        Debug.Log("�̾߱⸦ �����մϴ�.");

        collision.GetComponent<PlayerCtrl_R>().OnDisable();
        yield return new WaitForSeconds(10f);

        Debug.Log("�̾߱⸦ ��Ĩ�ϴ�.");
        changeMap.Pase(3); // �������� ȿ�� ����

        yield return new WaitForSeconds(8f);
        girlBoss.GetComponent<EnemyBase>().attackMode = true; // ���� ����
        collision.GetComponent<PlayerCtrl_R>().OnEnable();
        gameObject.SetActive(false);
        
    }
}
