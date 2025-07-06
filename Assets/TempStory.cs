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
        Debug.Log("이야기를 시작합니다.");

        collision.GetComponent<PlayerCtrl_R>().OnDisable();
        yield return new WaitForSeconds(10f);

        Debug.Log("이야기를 마칩니다.");
        changeMap.Pase(3); // 정신착란 효과 시작

        yield return new WaitForSeconds(8f);
        girlBoss.GetComponent<EnemyBase>().attackMode = true; // 공격 실행
        collision.GetComponent<PlayerCtrl_R>().OnEnable();
        gameObject.SetActive(false);
        
    }
}
