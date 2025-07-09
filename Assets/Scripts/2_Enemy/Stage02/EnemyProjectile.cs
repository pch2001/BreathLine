using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float destroyTime = 1f; // �ı����� �ɸ��� �ð�
    private float currentTimer = 0f;

    void Start()
    {
        Destroy(gameObject, 1f);
        currentTimer = destroyTime; // Ÿ�̸� �ʱ�ȭ
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
            gameObject.layer = LayerMask.NameToLayer("PlayerAttack"); // Enemy�� �浹�ϴ� ���̾�� ����
            currentTimer = destroyTime; // Ÿ�̸� �ʱ�ȭ
        }
        else if (collision.gameObject.CompareTag("EchoGuard") || collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("WolfAttack"))
        {
            Debug.Log("���� ������ ����մϴ�!");
            currentTimer = 0f;
        }else if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("���� �ݻ�� ���ݿ� ���ظ� �Խ��ϴ�!");
            currentTimer = 0f;
        }
    }
}
