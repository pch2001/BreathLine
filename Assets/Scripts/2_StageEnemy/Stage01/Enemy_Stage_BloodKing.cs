using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage_BloodKing : BossBase // Mage ��ũ��Ʈ
{
    public int angerAttackID = 0; // �г��� ���� �ߺ� �浹 ���� ����
    public int peaceAttackID = 1; // ��ȭ�� ���� �ߺ� �浹 ���� ����

    private int specialPhaseCnt = 1;

    [SerializeField] private List<GameObject> phase1_Ghouls; // Ư�� ���Ͻ� ��ȯ�� ���� ����Ʈ
    [SerializeField] private List<GameObject> phase1_Spitters; // Ư�� ���Ͻ� ��ȯ�� ������ ����Ʈ

    private void Start()
    {
        maxHp = 300f; // �� ü�� ����
        currentHp = 150f; // �� ü�� �ʱ�ȭ
        damage = 15f; // �� ���ݷ� ���� 
        maxGroggyCnt = 3; // �ִ� �׷α� ������ 3���� ����
        currentGroggyCnt = 0; // ���� �׷α� ���� �ʱ�ȭ
        rigidBody.drag = 5f; // �⺻ ������ ����
        startPos = transform.position;
        moveSpeed = defaultMoveSpeed;
        isPatrol = false;

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
        MoveToTarget();

        GameManager.Instance.isReturned = enemyIsReturn; // �� ȸ�� ���� ����
        if (GameManager.Instance.isReturned) // ȸ�� ��, �׷α� ���� �ʱ�ȭ 
        {
            groggyUI.SetupGroggySpriteGauge(maxGroggyCnt);
        }
    }

    private void Update()
    {
        // ������ ����
        if (isDead || !attackMode) return;

        // Ư�� ���� ���� Ȯ��
        float hpRatio = currentHp / maxHp;
        if (specialPhaseCnt >= 1 && !isSpecialPhase && (hpRatio <= 0.25f || hpRatio >= 0.75f))
        {
            StartCoroutine(SpecialPhaseRoutine()); return;
        }

        // ����� Ȯ��
        if (isPurifying && currentHp > 5f) // ���� ���� or ��ȭ�� ������ ������ ����(�ִ� 5���� ����)
            currentHp -= 5f * Time.deltaTime; // 1�ʿ� 5 Hp�� ����

        if (isReadyPeaceMelody && currentHp > 5f) // ��ȭ�� ���� �غ��ĵ� �ǰݽ� ������ ����(�ִ� 5����)
            currentHp -= 2f * Time.deltaTime; // 1�ʿ� 2Hp�� ����

        if (player == null || isStune || isSpecialPhase) return;

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
                Attack2,
        };
    }

    protected override void SetAttackTriggerRange(int index)
    {
        switch (index)
        {
            case 0:
                attackTriggerRange.transform.localScale = new Vector3(0.6f, 0.2f, 0.1f);
                break;
            case 1:
                attackTriggerRange.transform.localScale = new Vector3(1f, 0.2f, 0.1f);
                break;
            case 2:
                attackTriggerRange.transform.localScale = new Vector3(0.4f, 0.2f, 0.1f);
                break;
        }
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
        yield return new WaitForSeconds(0.5f); // ���� ���� 0.5��

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack"); // Attack �ִϸ��̼� ����
        hitEffect.SetActive(false);
        yield return new WaitForSeconds(1.5f); // ���� �ൿ�� �ϴµ� ������ ��

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        SetAttackTriggerRange(nextAttackIndex); // ���� ���� �ν� ���� ����
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
        yield return new WaitForSeconds(0.7f); // ���� ���� 0.7��

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack1"); // Attack1 �ִϸ��̼� ����
        hitEffect_noGroggy.SetActive(false);

        Vector2 targetPos = transform.position + new Vector3(direction.x * 7f, 0f); // ���� ������ ����
        StartCoroutine(MoveToTarget(transform.position, targetPos, 0.1f)); // �� 0.1�� ���� �̵� ����
        attackCoroutine = null; // �ڷ�ƾ ����
        yield return new WaitForSeconds(1.5f); // ���� �ൿ�� �ϴµ� ������ ��

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        SetAttackTriggerRange(nextAttackIndex); // ���� ���� �ν� ���� ����
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;
        EvaluateCurrentState(); // ������Ƽ �� �ʱ�ȭ(���¿� �´� �ൿ ����)
    }

    private IEnumerator Attack2()
    {
        isAttacking = true;
        attackMode = false; // ��� ���� ����
        Debug.Log("���� [���� 2]�� �غ��մϴ�!");

        attackCoroutine = null; // ���� ���̾��� �ڷ�ƾ ����
        currentAttack = attackObjects[2];
        moveSpeed = 0f;
        animator.SetTrigger("Charge"); // �̵� ���� �� ��¡
        LockOnTargetPlayer(); // �÷��̾ �ٶ󺸰� ����

        Vector3 dirAttack = attackObjects[2].transform.localPosition; // �� ���� ���� �غ�
        dirAttack.x = Mathf.Abs(dirAttack.x) * -(direction.x > 0 ? 1 : -1);
        attackObjects[2].transform.localPosition = dirAttack;

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // ���� ��� UI ����
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(1.8f);

        // �� ����� ����
        attackMode = true; // ���� ���� ����
        hitEffect_noGroggy.SetActive(false);
        groggyUIObject.SetActive(false);
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;

        yield return new WaitForSeconds(3f);
        groggyUIObject.SetActive(true);
        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack2"); // Attack2 �ִϸ��̼� ����
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;

        transform.position = player.transform.position + Vector3.up * 7f;
        yield return new WaitForSeconds(0.3f);

        Vector3 targetPos = new Vector3(transform.position.x, player.transform.position.y, transform.position.z); // ���� ������ ����
        StartCoroutine(MoveToTarget(transform.position, targetPos, 0.1f)); // �� 0.1�� ���� �̵� ����

        attackCoroutine = null; // �ڷ�ƾ ����
        yield return new WaitForSeconds(1.5f); // ���� �ൿ�� �ϴµ� ������ ��

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        SetAttackTriggerRange(nextAttackIndex); // ���� ���� �ν� ���� ����
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;
        EvaluateCurrentState(); // ������Ƽ �� �ʱ�ȭ(���¿� �´� �ൿ ����)
    }

    private IEnumerator SpecialPhaseRoutine() // Ư�� ���� ����
    {
        Debug.LogWarning("���� Ư�� ������ �����մϴ�! + �Ҹ� �߰�");
        moveSpeed = 0f;
        isSpecialPhase = true;
        specialPhaseCnt--;

        yield return new WaitForSeconds(3f);

        // ������� ����
        animator.SetTrigger("Transform"); 
        yield return new WaitForSeconds(0.7f);
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;

        // ���� Ȱ��ȭ
        foreach(GameObject monster in phase1_Ghouls)
        {
            monster.SetActive(true);
        }
        foreach (GameObject monster in phase1_Spitters)
        {
            monster.SetActive(true);
        }
        yield return new WaitForSeconds(20f); // ���� 20�ʰ� ����
        
        // ���� ����� �� ���� ���
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
        foreach (GameObject monster in phase1_Ghouls)
        {
            if(monster.activeSelf) // �ش� ���� �������� ��� ���� ��� ����
                StartCoroutine(monster.GetComponent<Enemy_Stage01_1>().AttackMode()); // ����� ���� AttackMode ����
        }
        animator.SetTrigger("Appear");
        moveSpeed = defaultMoveSpeed;
        yield return new WaitForSeconds(2f);
        
        isSpecialPhase = false; // Ư�� ���� ����
    }

    protected override void HandlerTriggerEnter(Collider2D collision) // �浹 ó�� ��� 
    {
        if (!attackMode) return;

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

            currentHp -= 15f;
            if (currentHp <= 0)
                StartCoroutine(EnemyFade(3f)); // �� �����
            else
            {
                StartCoroutine(Stunned(3f)); // �� 3�� ����
                StartCoroutine(PushAttack(3f)); // 3���� �а� ����
            }
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
        if (!attackMode) return;

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
