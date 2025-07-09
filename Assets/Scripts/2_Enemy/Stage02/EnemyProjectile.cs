using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float destroyTime = 1f; // 파괴까지 걸리는 시간
    private float currentTimer = 0f;

    void Start()
    {
        Destroy(gameObject, 1f);
        currentTimer = destroyTime; // 타이머 초기화
    }

    void Update()
    {
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            gameObject.GetComponent<Rigidbody2D>().velocity *= -1f;
            gameObject.layer = LayerMask.NameToLayer("PlayerAttack"); // Enemy와 충돌하는 레이어로 변경
            currentTimer = destroyTime; // 타이머 초기화
        }
        else if (collision.gameObject.CompareTag("EchoGuard") || collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("WolfAttack"))
        {
            Debug.Log("적의 공격을 방어합니다!");
            currentTimer = 0f;
        }else if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("적이 반사된 공격에 피해를 입습니다!");
            currentTimer = 0f;
        }
    }
}
