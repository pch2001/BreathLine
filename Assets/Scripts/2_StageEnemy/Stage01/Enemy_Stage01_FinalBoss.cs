using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
public class Enemy_Stage01_FinalBoss : EnemyBase
{


    Vector2 direction; // ���� �ٶ󺸴� ����
    private InputAction playerInteraction;
    public int angerAttackID = 0; // �г��� ���� �ߺ� �浹 ���� ����
    public int peaceAttackID = 1; // ��ȭ�� ���� �ߺ� �浹 ���� ����

    public GameObject attackArea2;
    public GameObject attackArea3;

    public GameObject[] boomArea; //3��°���� ���� ����
    private bool dontmove = true;
    public GameObject notePrefab;  // Inspector���� �Ҵ��� ��ǥ ������

    public GameObject[] attackAreas; // ���� ���� ������Ʈ �迭 
    public bool healmode = false; // ����� ����


    void Start()
    {
        maxHp = 100f; // �� ü�� ����
        currentHp = 50f; // �� ü�� �ʱ�ȭ
        damage = 20f; // �� ���ݷ� ���� 
        pollutionDegree = 20f; // �� óġ(�ǰ�)�� ������ ������ ����
        maxGroggyCnt = 3; // �ִ� �׷α� ������ 3���� ����
        currentGroggyCnt = 0; // ���� �׷α� ���� �ʱ�ȭ
        rigidBody.drag = 5f; // �⺻ ������ ����
        moveSpeed = defaultMoveSpeed;
        attackMode = false; // �⺻ ���ݸ�� false
        GameManager.Instance.isReturned = enemyIsReturn; // �� ȸ�� ���� ����

        if (GameManager.Instance.isReturned) // ȸ�� ��, �׷α� ���� �ʱ�ȭ 
        {
            groggyUI.SetupGroggySpriteGauge(maxGroggyCnt);  
        }


        boomArea[0].SetActive(false);
        boomArea[1].SetActive(false);
        boomArea[2].SetActive(false);
        boomArea[3].SetActive(false);

    }

    int count = 0;
    void FixedUpdate()
    {
        if (isPurifying && currentHp > 5f) // ���� ���� or ��ȭ�� ������ ������ ����(�ִ� 5���� ����)
            currentHp -= 5f * Time.deltaTime; // 1�ʿ� 5 Hp�� ����

        if (isReadyPeaceMelody && currentHp > 5f) // ��ȭ�� ���� �غ��ĵ� �ǰݽ� ������ ����(�ִ� 5����)
            currentHp -= 2f * Time.deltaTime; // 1�ʿ� 2Hp�� ����

        if (player == null || isStune || isDead) return;

        // ���� �÷��̾ �ٶ󺸰� ���� ���
        direction = (player.transform.position - transform.position).normalized;

        //�÷��̾� �ٶ󺸱�
        Vector3 scale = transform.localScale;
        scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        //if (!attackMode) return; // ���� ����� ���

        // ���������� ���� ��� �̵� ����
        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        rigidBody.velocity = velocity; // �� �̵�

        if (isAttacking) return;


        StartCoroutine(HealBoss());
        //if (count == 7)
        //{
        //    StartCoroutine(HealBoss());
        //}
        //else if (count > 5 && count < 7)
        //{
        //    count++;
        //    StartCoroutine(Attack3());
        //}
        //else if (count > 2 && count <= 4)
        //{
        //    count++;
        //    StartCoroutine(Attack2());
        //}
        //else
        //{
        //    count++;
        //    StartCoroutine(Attack1());

        //}


    }
    protected override void HandlerTriggerEnter(Collider2D collision) // �浹 ó�� ��� 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            // �ǰ� �ߺ� ���� ���� �ڵ�
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // �̹� ���� �浹 �Ϸ�� ����
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("�г��� ������ ���� �����մϴ�!!"); 


            if (!attackMode) // attackMode�� ��Ȱ��ȭ �Ǿ����� �� �ǰݽ�
            {
                AttackMode(); // ���ݸ�� Ȱ��ȭ
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            healmode = false; // ����� ����
            if (!attackMode) return; // ���ݸ�尡 �ƴ� ��� ��ȭ�� ���� ����

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // ���� �������� ���� �� �ǰ� + �̹� ���� �浹 �Ϸ�� ����
            peaceAttackID = attackArea.attackGlobalID;

            currentHp -= 20f;
            if (currentHp <= 0) 
            {
                StartCoroutine(EnemyFade(3f)); // �� �����
            }
            else
                StartCoroutine(Stunned(3f)); // �� 3�� ����
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

            Debug.Log("������ ������ ���� ������ŵ�ϴ�!");
            if (currentHp - 20f >= 5) // �ִ� 5���� ������ ����
                currentHp -= 20f; // �� ������ ��� 20 ���� 
            else
                currentHp = 5f;

            float pushBackDir = transform.position.x - collision.transform.position.x; // ���� �аݵ� ���� ���
            PushBack(pushBackDir);
        }
        else if (collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep"))
        {
            if (!attackMode) return;

            Debug.Log("���� ������ŵ�ϴ�");
            isPurifying = true;
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            if (!attackMode) return;

            Debug.Log("��ȭ�� ������ �غ��մϴ�! �ֺ��� ������ �ĵ��� �����ϴ�.");
            isReadyPeaceMelody = true; // ��ȭ�� ���� �غ� �ĵ� ����
        }
        else if (collision.gameObject.CompareTag("EchoGuard"))
        {
            if (!attackMode || isStune) return;

            StartCoroutine(EchoGuardSuccess(collision));
        }
    }

    protected override void HandlerTriggerStay(Collider2D collision) // �浹 ó�� ��� 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // �̹� ���� �浹 �Ϸ�� ����
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("�г��� ������ ���� �����մϴ�!!");

            if (!attackMode) // attackMode�� ��Ȱ��ȭ �Ǿ����� �� �ǰݽ�
            {
                AttackMode(); // ���ݸ�� Ȱ��ȭ
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            healmode = false; // ����� ����

            if (!attackMode) return; // ���ݸ�尡 �ƴ� ��� ��ȭ�� ���� ����

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // ���� �������� ���� �� �ǰ� + �̹� ���� �浹 �Ϸ�� ����
            peaceAttackID = attackArea.attackGlobalID;

            currentHp -= 20f;
            if (currentHp <= 0)
            {
                StartCoroutine(EnemyFade(3f)); // �� �����
            }
            else
                StartCoroutine(Stunned(3f)); // �� 3�� ����
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

            Debug.Log("������ ������ ���� ������ŵ�ϴ�!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // ���� �аݵ� ���� ���
            PushBack(pushBackDir);
            StartCoroutine(Stunned(3f));
        }
        else if (collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep"))
        {
            if (!attackMode) return;

            isPurifying = true;
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            if (!attackMode) return;

            isReadyPeaceMelody = true;
        }
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep")) && attackMode)
        {
            Debug.Log("���� ������ ����ϴ�");
            moveSpeed = defaultMoveSpeed; // ���� �ӵ�
            isPurifying = false;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            if (attackMode) return;

            Debug.Log("��ȭ�� ������ �غ� ��Ĩ�ϴ�!");
            isReadyPeaceMelody = false; // ��ȭ�� ���� �غ� ����
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

    public void AttackMode() // ���� ��� Ȱ��ȭ
    {
        Debug.Log("���� �÷��̾�� �޷���ϴ�!");
        animator.SetTrigger("Damaged");
        attackMode = true;
        boxCollider.size = new Vector2(0.03f, 0.23f); // �ǰ� ���� ũ�� ���� (�÷��̾� �ν� ���� -> �� �浹 ����)
        boxCollider.offset = new Vector2(0f, 0f); // �ǰ� ���� ��ġ ����
        moveSpeed = defaultMoveSpeed;
    }

    private void DeActivateAttackMode() // ���� ��� ���� ����
    {
        Debug.Log("���� �����˴ϴ�...");

        attackMode = false;
        animator.SetBool("isRun", false);
        boxCollider.size = new Vector2(1.5f, 1f); // �ǰ� ���� ũ�� ���� (�� �浹 ���� -> �÷��̾� �ν� ����)
        boxCollider.offset = new Vector2(0f, 0f); // �ǰ� ���� ��ġ ����

        StartCoroutine(Stunned(3f)); // 3�ʰ� ����
    }

    private IEnumerator EchoGuardSuccess(Collider2D collision) 
    {

        if (currentGroggyCnt < maxGroggyCnt - 1) // �׷α� �������� 2�� �̻� ������ ���
        {
            Debug.Log("�ҳడ ���� ������ ����س��ϴ�!");
            groggyUI.AddGroggyState(); // �׷α� ���� ����
            currentGroggyCnt++;
            audioSource.Play(); // �и� �Ҹ� ���
            float pushBackDir = transform.position.x - collision.transform.position.x; // ���� �аݵ� ���� ���
            EchoGuardPushBack(pushBackDir);
        }
        else // ���� �׷α� �������� 1���� ���
        {
            Debug.Log("���� ��� �׷α� ���¿� �����ϴ�!");

            if (currentHp - 20f >= 5) // �ִ� 5���� ������ ����
                currentHp -= 15f; // �� ������ ��� 20 ���� 
            else
                currentHp = 5f;
            groggyUI.AddGroggyState(); // �׷α� ���� ����
            audioSource.Play(); // �и� �Ҹ� ���
            float pushBackDir = transform.position.x - collision.transform.position.x; // ���� �аݵ� ���� ���
            PushBack(pushBackDir);

            echoGuardEffect.SetActive(true); // ���ڰ��� ���� ����Ʈ Ȱ��ȭ
            yield return new WaitForSeconds(0.4f);

            echoGuardEffect.SetActive(false); // ���ڰ��� ����Ʈ ��Ȱ��ȭ
        }
    }

    public void SetAttack(int attacknum)
    {
        attackAreas[attacknum].SetActive(true); // ���� ���� Ȱ��ȭ

        Collider2D col = attackAreas[attacknum].GetComponent<Collider2D>();
        col.enabled = false;
        col.enabled = true;

        Debug.Log("���� ��ȣ: " + attacknum);



       // GameObject targetArea = attackAreas[attacknum];
        //attack attackScript = targetArea.GetComponent<attack>();

        //if (attackScript != null)
        //{
        //    attackScript.isAttacking = true; // ���� ���·� ����
        //}


    }

    public void SetNoAttack(int attacknum)
    {
        //Debug.Log("���� ��ȣ: " + attacknum);
        attackAreas[attacknum].SetActive(false); // ���� ���� Ȱ��ȭ

        //GameObject targetArea = attackAreas[attacknum];
        //attack attackScript = targetArea.GetComponent<attack>();

        //if (attackScript != null)
        //{
        //    attackScript.isAttacking = false; // ���� ���·� ����
        //}
    }

    IEnumerator Attack1()
    {
        isAttacking = true;
        animator.SetTrigger("attack1");

        if (dontmove)
        {

        }
        yield return new WaitForSeconds(1.2f);
        isAttacking = false;
    }

    IEnumerator Attack2()
    {

        isAttacking = true;
        animator.SetTrigger("attack2");
        yield return new WaitForSeconds(0.7f);

        // ���� ���� �ð� ���
        if (dontmove)
        {
            //attackArea2.SetActive(true);
            Collider2D col = attackArea2.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    IEnumerator Attack3()
    {
        isAttacking = true;
        animator.SetTrigger("attack3");

        yield return new WaitForSeconds(2.5f);


        isAttacking = false;
    }

    IEnumerator HealBoss()
    {
        isAttacking = true;
        healmode = true;
        animator.SetTrigger("heal");

        while (healmode)
        {
            yield return new WaitForSeconds(0.5f);

        }
        isAttacking = false;
        animator.SetTrigger("idle");

    }

    public void ActiveBoom(int num)
    {
        boomArea[num].SetActive(true);

        GameObject targetArea = boomArea[num];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = true; // ���� ���·� ����
        }
        if (dontmove)
        {
            //attackArea2.SetActive(true);
            Collider2D col = attackArea2.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }
    }
    public void PassiveBoom(int num)
    {

        GameObject targetArea = boomArea[num];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = false; // ���� ���·� ����
        }

        boomArea[num].SetActive(false);

    }

}
