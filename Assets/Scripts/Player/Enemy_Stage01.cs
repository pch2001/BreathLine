using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class Enemy_Stage01 : EnemyBase
{
    private Rigidbody2D rigidBody;
    private Animator animator;
    private GameObject player;
    private SpriteRenderer spriteRenderer;

    public GameObject hitEffect; // �ǰ� ����Ʈ
    public GameObject dieEffect; // Die ����Ʈ


    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");

        hp = 30f; // �� ü�� ����
        attackPoint = 10f; // �� ���ݷ� ���� 
        rigidBody.drag = 5f; // �⺻ ������ ����
        moveSpeed = 2f;
    }

    private void Update()
    {
        if (!attackMode || player == null) return; // ���ݸ�� �ƴ� ��� �̵�x

        Vector2 direction = (player.transform.position - transform.position).normalized;
        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        rigidBody.velocity = velocity; // �� �̵�

        if (direction.x > 0) 
            spriteRenderer.flipX = true;
        else 
            spriteRenderer.flipX = false;

        animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // �ӵ��� ���� �ִϸ��̼� ����

    }

    private IEnumerator Die() // ��� ���� ����
    {
        animator.SetTrigger("Die");
        dieEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        dieEffect.SetActive(false);
        gameObject.SetActive(false); // �� ��Ȱ��ȭ
    }

    IEnumerator PushBack(float dir) // �а� ���� ����
    {
        float originalDrag = rigidBody.drag;
        rigidBody.drag = 1f; // �аݽ� ��� ������ ����

        if (dir > 0)
            rigidBody.AddForce(Vector2.right * 650);
        else
            rigidBody.AddForce(Vector2.left * 650); // �ڷ� ���� �Ÿ� �а�

        yield return new WaitForSeconds(0.2f);
        rigidBody.drag = originalDrag;
    }

    IEnumerator Damaged() // �ǰ� ���� ����
    {
        hp -= 10;
        attackMode = true;

        StartCoroutine(Stunned(0.5f)); // 0.5�� ����
        animator.SetTrigger("Damaged"); // �ǰ� �ִϸ��̼� ����
        hitEffect.SetActive(true); // �ǰ� ����Ʈ Ȱ��ȭ

        yield return new WaitForSeconds(0.2f);

        hitEffect.SetActive(false); // �ǰ� ����Ʈ ��Ȱ��ȭ
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            Debug.Log("�г��� ������ ���� �����մϴ�!!");
            if(hp <= 0)
            {
                Debug.Log("���� ���뽺���� �Ҹ��մϴ�...");
                StartCoroutine(Die());
            }
            else
            {
                Debug.Log("���� �������� ���� ���ظ� �Խ��ϴ�.");
                StartCoroutine(Damaged());
            }
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            Debug.Log("��ȭ�� ������ ���� �Ƚɽ�ŵ�ϴ�.");
            attackMode = false; 
            animator.SetBool("isRun", false);
            StartCoroutine(EnemyFade(4f)); // ���� õõ�� �����
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            Debug.Log("������ ������ ���� ������ŵ�ϴ�!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // ���� �аݵ� ���� ���
            StartCoroutine(PushBack(pushBackDir));
            StartCoroutine(Stunned(3f));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!attackMode) return; // ���ݸ�尡 �ƴ� ��Ȳ���� �浹�� ����

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("�� �÷��̾�� ���ظ� �����ϴ�!");
            StartCoroutine(Die());
        }
    }
}
