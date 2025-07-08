using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Enemy_Stage_AncientBoss : BossBase // Mage ��ũ��Ʈ
{
    public int angerAttackID = 0; // �г��� ���� �ߺ� �浹 ���� ����
    public int peaceAttackID = 1; // ��ȭ�� ���� �ߺ� �浹 ���� ����

    private int specialPhaseCnt = 1;
    private int spawnEnemyCnt = 0; // ��ȯ�� ���� ��
    private bool isMoveAttackActived = false; // �̵� ���� ����������
    
    [SerializeField] private List<GameObject> phase3_Monster; // Ư�� ���Ͻ� ��ȯ�� 2�������� �� ����Ʈ
    [SerializeField] private List<GameObject> translatePos; // ����2�� �̵��� ��ġ
    [SerializeField] private GameObject AttackSpecialRange; // Ư�� ���Ͻ� ���� ����
    private Vector3 startTranslatePos; // ����2�� ���� ����
    private Vector3 targetTranslatePos; // ����2�� ��ǥ ����

    private void Start()
    {
        maxHp = 300f; // �� ü�� ����
        currentHp = 150f; // �� ü�� �ʱ�ȭ
        damage = 10f; // �� ���ݷ� ���� 
        bulletSpeed = 20; // źȯ �ӵ� ����
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

        foreach(GameObject obj in phase3_Monster)
    {
            EnemyBase enemy = obj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.RequestEnemyDie += CountSpawnEnemy;
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

        if (player == null || isStune || isSpecialPhase || isMoveAttackActived) return;

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
                attackTriggerRange.transform.localScale = new Vector3(1f, 0.2f, 0.1f);
                break;
            case 1:
                attackTriggerRange.transform.localScale = new Vector3(2f, 0.2f, 0.1f);
                break;
            case 2:
                attackTriggerRange.transform.localScale = new Vector3(1.2f, 0.2f, 0.1f);
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
        yield return new WaitForSeconds(0.3f); // ���� ���� 0.3��

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack"); // Attack �ִϸ��̼� ���� -> �ִϸ��̼ǿ��� �ڵ����� attackObject Ȱ��ȭ/��Ȱ��ȭ
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
        dirAttack.x = Mathf.Abs(dirAttack.x) * (direction.x > 0 ? 1 : -1);
        attackObjects[1].transform.localPosition = dirAttack;

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // ���� ��� UI ����
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(1f); // ���� ���� 1��

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack1"); // Attack �ִϸ��̼� ���� -> �ִϸ��̼ǿ��� �ڵ����� attackObject Ȱ��ȭ/��Ȱ��ȭ
        hitEffect_noGroggy.SetActive(false);
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
        isMoveAttackActived = true;

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
        hitEffect_noGroggy.SetActive(false);
        groggyUI.enabled = false;
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;
        yield return new WaitForSeconds(1f);

        // ���� ���� �̵� �� �̵� ���� ����
        if(Random.value < 0.5f) // ���� ���� ���� ����
        {
            startTranslatePos = translatePos[0].transform.position;
            targetTranslatePos = translatePos[1].transform.position;
        }
        else
        {
            startTranslatePos = translatePos[1].transform.position;
            targetTranslatePos = translatePos[0].transform.position;
        }
        transform.position = startTranslatePos;
        groggyUI.enabled = true;
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
        yield return new WaitForSeconds(3f);

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack2");
        attackObjects[2].SetActive(true);
        StartCoroutine(MoveToTarget(transform.position, targetTranslatePos, 2f)); // �� 3�� ���� �̵� ����
    }

    private IEnumerator SpecialPhaseRoutine() // Ư�� ���� ����
    {
        Debug.LogWarning("���� Ư�� ������ �����մϴ�! + �Ҹ� �߰�");
        
        moveSpeed = 0f;
        isSpecialPhase = true;
        specialPhaseCnt--;
        animator.SetTrigger("Damaged");
        attackMode = false; // ��� ����
        yield return new WaitForSeconds(3f);

        // ������� ����
        animator.SetTrigger("Charge"); 
        yield return new WaitForSeconds(0.3f);

        // ���� ����
        transform.position = startPos;
        animator.SetTrigger("Damaged");
        yield return new WaitForSeconds(0.5f);

        // ���� Ȱ��ȭ
        foreach (GameObject monster in phase3_Monster)
        {
            monster.SetActive(true);
        }
        spawnEnemyCnt = phase3_Monster.Count;

        // 5�ʿ� �ѹ� ���� ���� * 4 (�� 20��)
        for (int i = 0; i < 4; i++)
        {
            animator.SetTrigger("Charge");
            yield return new WaitForSeconds(5f);

            // ���� ���� ���� Ȱ��ȭ
            animator.SetTrigger("AttackSpecial");
            hitEffect_noGroggy.SetActive(true);
            yield return new WaitForSeconds(0.8f);

            hitEffect_noGroggy.SetActive(false);
            AttackSpecialRange.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            
            AttackSpecialRange.SetActive(false);
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(1f);
        
        // ���� ����� �� ���� ����
        animator.SetTrigger("Damaged");
        attackMode = true;
        isAttackRange = false;

        foreach (var monster in phase3_Monster)
        {
            if (monster != null)
            {
                monster.SetActive(false);
            }
        }
        isSpecialPhase = false; // Ư�� ���� ����
    }

    protected override IEnumerator MoveToTarget(Vector2 startPos, Vector2 targetPos, float duration)
    {
        float elapsed = 0f; // Ÿ�̸�
        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos; // ��ġ ����
        animator.SetTrigger("Attack2End");
        yield return new WaitForSeconds(1f);

        attackObjects[2].SetActive(false);
        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        SetAttackTriggerRange(nextAttackIndex); // ���� ���� �ν� ���� ����
        moveSpeed = defaultMoveSpeed;
        isMoveAttackActived = false;
        isAttacking = false;
        
        EvaluateCurrentState(); // ������Ƽ �� �ʱ�ȭ(���¿� �´� �ൿ ����)
    }

    private void CountSpawnEnemy()
    {
        spawnEnemyCnt--;
        Debug.LogWarning(spawnEnemyCnt);
        if(spawnEnemyCnt <= 0)
        {
            animator.SetTrigger("Transform");
            attackMode = true;
            isAttackRange = false;

            foreach(var monster in phase3_Monster)
            {
                if(monster != null)
                {
                    monster.SetActive(false);
                }
            }
            isSpecialPhase = false; // Ư�� ���� ����
        }
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
