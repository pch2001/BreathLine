using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage04_2 : EnemyBase // Mage ��ũ��Ʈ
{
    public int angerAttackID = 0; // �г��� ���� �ߺ� �浹 ���� ����
    public int peaceAttackID = 1; // ��ȭ�� ���� �ߺ� �浹 ���� ����

    private void Start()
    {
        maxHp = 70f; // �� ü�� ����
        currentHp = 35f; // �� ü�� �ʱ�ȭ
        damage = 7f; // �� ���ݷ� ���� 
        maxGroggyCnt = 2; // �ִ� �׷α� ������ 3���� ����
        currentGroggyCnt = 0; // ���� �׷α� ���� �ʱ�ȭ
        rigidBody.drag = 5f; // �⺻ ������ ����
        startPos = transform.position;
        moveSpeed = defaultMoveSpeed;
        isPatrol = true;
        attackMode = true;

        foreach (GameObject attackObj in attackObjects) // ���� ���� ���ΰ� �ʱ�ȭ
        {
            EnemyAttackBase attackBase = attackObj.GetComponent<EnemyAttackBase>();
            if (attackBase != null)
            {
                attackBase.enemyOrigin = this.gameObject;
            }
        }

        ChooseNextPatrolPoint(); // ���� ������ ����
        InitializeAttackPatterns(); // ���� �Լ� ���� �ʱ�ȭ

        GameManager.Instance.isReturned = enemyIsReturn; // �� ȸ�� ���� ����
        if (GameManager.Instance.isReturned) // ȸ�� ��, �׷α� ���� �ʱ�ȭ 
        {
            groggyUI.SetupGroggySpriteGauge(maxGroggyCnt);
        }
    }

    private void Update()
    {
        // ������ ����
        if (isDead) return;

        // ����� Ȯ��
        if (isPurifying && currentHp > 5f) // ���� ���� or ��ȭ�� ������ ������ ����(�ִ� 5���� ����)
            currentHp -= 5f * Time.deltaTime; // 1�ʿ� 5 Hp�� ����

        if (isReadyPeaceMelody && currentHp > 5f) // ��ȭ�� ���� �غ��ĵ� �ǰݽ� ������ ����(�ִ� 5����)
            currentHp -= 2f * Time.deltaTime; // 1�ʿ� 2Hp�� ����

        if (!isMoving || player == null || isStune) return;

        if (isPatrol && isMoving && Vector2.Distance(transform.position, targetPos) < 1f) // ��Ʈ�� ��� �� ������ ������ 
            isMoving = false; // ��� ���·� ��ȯ

        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // �ӵ��� ���� �ִϸ��̼� ����

        if (!isPatrol && isAttackRange || isAttacking) return; // ���� ��� - ���� ���� ���� or �̹� ���� ����� �̵� ����

        if (!isPatrol && !isAttackRange) // ���� ��� - �� �߰� ���� ����
        {
            LockOnTargetPlayer(); // �������� �÷��̾� ��ġ�� ����
        }

        rigidBody.velocity = velocity; // �� �̵�
    }

    protected override void InitializeAttackPatterns() // ���� �Լ� ���� �ʱ�ȭ
    {
        attackPatterns = new AttackPattern[] // ���� ���� ���� �ʱ�ȭ
        {
                Attack0,
                Attack1,
        };
    }

    private void LockOnTargetPlayer() // �÷��̾� ��ġ�� ��ǥ�������� ����
    {
        targetPos = player.transform.position;
        direction = (targetPos - (Vector2)transform.position).normalized; // ��Ʈ�� �� ���� ����

        if (direction.x > 0)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    private IEnumerator Attack0()
    {
        isAttacking = true;

        Debug.Log("���� [���� 0]�� �غ��մϴ�!");
        attackCoroutine = null; // ���� ���̾��� �ڷ�ƾ ����
        currentAttack = attackObjects[0];
        moveSpeed = 0f;
        LockOnTargetPlayer(); // �÷��̾ �ٶ󺸰� ����

        Vector3 dirAttack = attackObjects[0].transform.localPosition; // �� ���� ���� �غ�
        dirAttack.x = Mathf.Abs(dirAttack.x) * (direction.x > 0 ? 1 : -1);
        attackObjects[0].transform.localPosition = dirAttack;

        Vector3 dirReady = hitEffect.transform.localPosition; // ���� ��� UI ����
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect.transform.localPosition = dirReady;
        hitEffect.SetActive(true);
        yield return new WaitForSeconds(0.3f); // ���� ���� 0.3��

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack"); // Attack �ִϸ��̼� ����
        hitEffect.SetActive(false);
        yield return new WaitForSeconds(0.1f);

        attackObjects[0].SetActive(true); // ���ݹ��� Ȱ��ȭ
        yield return new WaitForSeconds(0.1f);

        attackObjects[0].SetActive(false); // ���ݹ��� ��Ȱ��ȭ
        yield return new WaitForSeconds(1f); // ���� �ൿ�� �ϴµ� ������ ��

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // ������Ƽ �� �ʱ�ȭ(���¿� �´� �ൿ ����)
    }

    private IEnumerator Attack1()
    {
        isAttacking = true;

        Debug.Log("���� [���� 1]�� �غ��մϴ�!");
        attackCoroutine = null; // ���� ���̾��� �ڷ�ƾ ����
        currentAttack = attackObjects[1];
        moveSpeed = 0f;
        LockOnTargetPlayer(); // �÷��̾ �ٶ󺸰� ����

        Vector3 dirAttack = attackObjects[1].transform.localPosition; // �� ���� ���� �غ�
        dirAttack.x = Mathf.Abs(dirAttack.x) * -(direction.x > 0 ? 1 : -1);
        attackObjects[1].transform.localPosition = dirAttack;

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // ���� ��� UI ����
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(0.3f); // ���� ���� 0.3��

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack1"); // Attack1 �ִϸ��̼� ����
        hitEffect_noGroggy.SetActive(false);

        Vector3 targetPos = transform.position + new Vector3(direction.x * 7f, 0f, 0f); // ���� ������ ����
        StartCoroutine(MoveToTarget(transform.position, targetPos, 0.1f)); // �� 0.1�� ���� �̵� ����
        attackObjects[1].SetActive(true); // ���ݹ��� Ȱ��ȭ
        yield return new WaitForSeconds(0.1f);

        attackObjects[1].SetActive(false); // ���ݹ��� ��Ȱ��ȭ
        attackCoroutine = null; // �ڷ�ƾ ����
        yield return new WaitForSeconds(1f); // ���� �ൿ�� �ϴµ� ������ ��

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // ������Ƽ �� �ʱ�ȭ(���¿� �´� �ൿ ����)
    }

    protected override void HandlerTriggerEnter(Collider2D collision) // �浹 ó�� ��� 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // ���� �������� ���� �� �ǰ� + �̹� ���� �浹 �Ϸ�� ����
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("�г��� ������ ���� �����մϴ�!!");
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // ���� �������� ���� �� �ǰ� + �̹� ���� �浹 �Ϸ�� ����
            peaceAttackID = attackArea.attackGlobalID;

            currentHp -= 10f;
            if (currentHp <= 0)
                StartCoroutine(EnemyFade(3f)); // �� �����
            else
                StartCoroutine(Stunned(3f)); // �� 3�� ����
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (isStune) return;

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
            Debug.Log("���� ������ŵ�ϴ�");
            isPurifying = true;
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {

            isReadyPeaceMelody = true; // ��ȭ�� ���� �غ� �ĵ� ����
        }
        else if (collision.gameObject.CompareTag("EnemyProjectile"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyAttackBase>(); // EnemyAttack �⺻ Ŭ���� ������
            Debug.Log("���� �ݻ�� ���ݿ� ���ظ� �Խ��ϴ�!");
            StartCoroutine(Damaged());
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

            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // ���� �������� ���� �� �ǰ� + �̹� ���� �浹 �Ϸ�� ����
            peaceAttackID = attackArea.attackGlobalID;

            currentHp -= 10f;
            if (currentHp <= 0)
                StartCoroutine(EnemyFade(3f)); // �� �����
            else
                StartCoroutine(Stunned(3f)); // �� 3�� ����
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (isStune) return;

            Debug.Log("������ ������ ���� ������ŵ�ϴ�!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // ���� �аݵ� ���� ���
            PushBack(pushBackDir);
            StartCoroutine(Stunned(3f));
        }
        else if (collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep"))
        {
            isPurifying = true;
            moveSpeed = 2f;
        }
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep")))
        {
            Debug.Log("���� ������ ����ϴ�");
            moveSpeed = defaultMoveSpeed; // ���� �ӵ�
            isPurifying = false;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            Debug.Log("��ȭ�� ������ �غ� ��Ĩ�ϴ�!");
            isReadyPeaceMelody = false; // ��ȭ�� ���� �غ� ����
        }
    }

}
