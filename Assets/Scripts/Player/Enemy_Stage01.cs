using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class Enemy_Stage01 : EnemyBase
{
    public bool isLooking = false; // �÷��̾ ���� �þ߿� ���Դ���
    public int attackID = 0; // �� �ߺ� �浹 ���� ����

    private void Start()
    {
        maxHp = 50f; // �� ü�� ����
        currentHp = 0f; // �� ü�� �ʱ�ȭ
        damage = 10f; // �� ���ݷ� ���� 
        rigidBody.drag = 5f; // �⺻ ������ ����
        moveSpeed = defaultMoveSpeed; 
        attackMode = false; // �⺻ ���ݸ�� false
    }

    private void Update()
    {
        if(moveSpeed == 2) // ���� ����� ������ ����
            currentHp -= 5f * Time.deltaTime; // 1�ʿ� 5 Hp�� ����
        
        if (!isLooking || player == null || isStune || isDead ) return;
        // ���� �÷��̾ �ٶ󺸰� ���� ���
        Vector2 direction = (player.transform.position - transform.position).normalized;
        if (direction.x > 0) 
            spriteRenderer.flipX = false;
        else 
            spriteRenderer.flipX = true;
        
        if (!attackMode) return; 
        // ���� ��� or ���� ���°� �ƴ� ���
        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        rigidBody.velocity = velocity; // �� �̵�

        animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // �ӵ��� ���� �ִϸ��̼� ����
    }

    protected override void HandlerTriggerEnter(Collider2D collision) // �浹 ó�� ��� 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == attackID) return; // ���� �������� ���� �� �ǰ� + �̹� ���� �浹 �Ϸ�� ����
            attackID = attackArea.attackGlobalID;

            Debug.Log("�г��� ������ ���� �����մϴ�!!");
            
            if (!attackMode) // attackMode�� ��Ȱ��ȭ �Ǿ����� �� �ǰݽ�
            {
                AttackMode(); // ���ݸ�� Ȱ��ȭ
            }
            StartCoroutine(Damaged()); 
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (!attackMode) return; // ���ݸ�尡 �ƴ� ��� ��ȭ�� ���� ����

            if(currentHp > 0) // �� �������� ������ ���, ������ �ʱ�ȭ
            {
                currentHp = 0;
                DeActivateAttackMode(); // �� �ʱ� ���·� �ǵ���
            }
            else
            {
                Debug.Log("��ȭ�� ������ ���� �Ƚɽ�ŵ�ϴ�.");
                StartCoroutine(EnemyFade(3f)); // ���� õõ�� �����
            }

        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

            Debug.Log("������ ������ ���� ������ŵ�ϴ�!");
            currentHp -= 20f; // �� ������ ��� 20 ���� 
            float pushBackDir = transform.position.x - collision.transform.position.x; // ���� �аݵ� ���� ���
            PushBack(pushBackDir);
        }
        else if (collision.gameObject.CompareTag("WolfAppear"))
        {
            if (!attackMode) return;

            Debug.Log("���밡 ���� ������ŵ�ϴ�");
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (!isLooking) 
            {
                isLooking = true; // �÷��̾ ���� �þ߿� �������� ����
                Debug.Log("���� �÷��̾ �ٶ󺾴ϴ�.");
            } 

            if (!attackMode || isStune) return; // ���ݸ�尡 �ƴ� ��Ȳ or ���� ��Ȳ���� �浹�� ����

            Debug.Log("�� �÷��̾�� ���ظ� �����ϴ�!");
            StartCoroutine(Die());
        }
    }

    protected override void HandlerTriggerStay(Collider2D collision) // �浹 ó�� ��� 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == attackID) return; // �̹� ���� �浹 �Ϸ�� ����
            attackID = attackArea.attackGlobalID;

            Debug.Log("�г��� ������ ���� �����մϴ�!!");

            if (!attackMode) // attackMode�� ��Ȱ��ȭ �Ǿ����� �� �ǰݽ�
            {
                AttackMode(); // ���ݸ�� Ȱ��ȭ
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (!attackMode) return; // ���ݸ�尡 �ƴ� ��� ��ȭ�� ���� ����

            Debug.Log("��ȭ�� ������ ���� �Ƚɽ�ŵ�ϴ�.");
            
            isLooking = false;
            StartCoroutine(EnemyFade(3f)); // ���� õõ�� �����
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

            Debug.Log("������ ������ ���� ������ŵ�ϴ�!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // ���� �аݵ� ���� ���
            PushBack(pushBackDir);
            StartCoroutine(Stunned(3f));
        }
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WolfAppear") && attackMode)
        {
            Debug.Log("���� ������ ������ ����ϴ�");
            moveSpeed = defaultMoveSpeed; // ���� �ӵ�
        }
        else if(collision.gameObject.CompareTag("Player") && isLooking)
        {
            AttackMode(); // ���ݸ�� Ȱ��ȭ
        }
    }

    private IEnumerator Die() // 1�������� �Ϲ� ������ Ư��(����)�� �ʿ�
    {
        Debug.Log("���� ���뽺���� �Ҹ��մϴ�...");
        moveSpeed = 0f;
        animator.SetTrigger("Die");
        dieEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        dieEffect.SetActive(false);
        gameObject.SetActive(false); // �� ��Ȱ��ȭ
    }

    private void AttackMode() // ���� ��� Ȱ��ȭ
    {
        Debug.Log("���� �÷��̾�� �޷���ϴ�!");
        attackMode = true;
        boxCollider.size = new Vector2(0.03f, 0.23f); // �ǰ� ���� ũ�� ���� (�÷��̾� �ν� ���� -> �� �浹 ����)
        boxCollider.offset = new Vector2(0f, 0.125f); // �ǰ� ���� ��ġ ����
        moveSpeed = defaultMoveSpeed;
    }

    private void DeActivateAttackMode() // ���� ��� ���� ����
    {
        Debug.Log("���� �����˴ϴ�...");

        isLooking = false;
        attackMode = false;
        animator.SetBool("isRun", false);
        boxCollider.size = new Vector2(1.5f, 1f); // �ǰ� ���� ũ�� ���� (�� �浹 ���� -> �÷��̾� �ν� ����)
        boxCollider.offset = new Vector2(0f, 0.51f); // �ǰ� ���� ��ġ ����

        StartCoroutine(Stunned(3f)); // 3�ʰ� ����
    }
}
