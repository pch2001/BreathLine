using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage_ShadowStorms : BossBase // Mage ��ũ��Ʈ
{
    public int angerAttackID = 0; // �г��� ���� �ߺ� �浹 ���� ����
    public int peaceAttackID = 1; // ��ȭ�� ���� �ߺ� �浹 ���� ����

    private int specialPhaseCnt = 1;
    private int spawnEnemyCnt = 0; // ��ȯ�� ���� ��
    private bool isHealMode = false; // �� �������� ����
    
    [SerializeField] private List<GameObject> beamAttacks; // ����2�� �� ������Ʈ ����Ʈ
    [SerializeField] private List<GameObject> phase2_Monster; // Ư�� ���Ͻ� ��ȯ�� 2�������� �� ����Ʈ

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

        foreach(GameObject obj in phase2_Monster)
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
        // Ư�� ���Ͻ� ȸ�� ����
        float hpRatio = currentHp / maxHp;
        if (isHealMode)
        {    
            if (hpRatio > 0.5f && hpRatio <= 1f) // �������� ���ߴ� ȸ�� ����
            {
                if (hpRatio <= 0.51f)
                {
                    isHealMode = false;
                    spawnEnemyCnt = 0;
                    CountSpawnEnemy();
                }
                else
                    currentHp -= 5f * Time.deltaTime; // �������� 75%�̻󿡼� ����� 1�ʿ� 5�� ������ ����
            }
            else // �������� ���̴� ȸ�� ����
            {
                if (hpRatio >= 0.5f)
                {
                    isHealMode = false;
                    spawnEnemyCnt = 0;
                    CountSpawnEnemy();
                }
                else
                    currentHp += 5f * Time.deltaTime; // �������� 25%���Ͽ��� ����� 1�ʿ� 5�� ������ ����
            }
        }

        // ������ ����
        if (isDead || !attackMode) return;

        // Ư�� ���� ���� Ȯ��
        if (specialPhaseCnt >= 1 && !isSpecialPhase && (hpRatio < 0.25f || hpRatio > 0.75f))
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
        
        Debug.Log("���� [���� 0]�� �غ��մϴ�!"); 
        attackCoroutine = null; // ���� ���̾��� �ڷ�ƾ ����
        currentAttack = null;
        moveSpeed = 0f;
        animator.SetTrigger("Charge");
        LockOnTargetPlayer(); // �÷��̾ �ٶ󺸰� ����

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // ���� ��� UI ����
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(0.8f); // ���� ���� 0.8��

        Debug.Log("���� źȯ�� �߻��մϴ�!");
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

        Debug.Log("���� [���� 2]�� �غ��մϴ�!");
        attackCoroutine = null; // ���� ���̾��� �ڷ�ƾ ����
        currentAttack = null;
        moveSpeed = 0f;
        animator.SetTrigger("Charge");
        LockOnTargetPlayer(); // �÷��̾ �ٶ󺸰� ����

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // ���� ��� UI ����
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);

        foreach(var beam in beamAttacks)
        {
            Vector3 pos = beam.transform.localPosition;
            pos.x = Mathf.Abs(pos.x) * (direction.x > 0 ? 1 : -1);
            beam.transform.localPosition = pos; 
        }
        yield return new WaitForSeconds(0.8f); // ���� ���� 0.8��

        Debug.Log("���� ������ �����մϴ�!");
        animator.SetTrigger("Attack2"); // Attack �ִϸ��̼� ���� -> �ִϸ��̼ǿ��� �ڵ����� attackObject Ȱ��ȭ/��Ȱ��ȭ
        hitEffect_noGroggy.SetActive(false);
        yield return new WaitForSeconds(1.5f); // ���� �ൿ�� �ϴµ� ������ ��

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        SetAttackTriggerRange(nextAttackIndex); // ���� ���� �ν� ���� ����
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // ������Ƽ �� �ʱ�ȭ(���¿� �´� �ൿ ����)
    }

    public void ActiveBeamAttack(int index)
    {
        beamAttacks[index].SetActive(true);
    }

    public void DeActiveBeamAttack(int index)
    {
        beamAttacks[index].SetActive(false);
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
        animator.SetTrigger("Transform"); 
        yield return new WaitForSeconds(0.3f);

        // ���� ����
        transform.position = startPos;
        animator.SetTrigger("Transform");
        yield return new WaitForSeconds(0.5f);

        // ȸ�� ��� + ���� Ȱ��ȭ
        animator.SetTrigger("Charge");
        isHealMode = true; // ȸ����� ����

        foreach (GameObject monster in phase2_Monster)
        {
            monster.SetActive(true);
        }
        spawnEnemyCnt = phase2_Monster.Count;
    }

    private void CountSpawnEnemy()
    {
        spawnEnemyCnt--;
        Debug.LogWarning(spawnEnemyCnt);
        if(spawnEnemyCnt <= 0)
        {
            animator.SetTrigger("Transform");
            isHealMode = false;
            attackMode = true;
            isAttackRange = false;

            foreach(var monster in phase2_Monster)
            {
                if(monster != null)
                {
                    monster.SetActive(false);
                }
            }
            isSpecialPhase = false; // Ư�� ���� ����
        }
    }

    public override IEnumerator EnemyFade(float duration) // ��ȭ�� �������� �� ����� �Լ�
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;

        isDead = true; // ���� ���·� ����
        enemyFadeEffect.SetActive(true);
        defaultMoveSpeed = 0f; // �̵� �Ұ���
        animator.SetBool("isRun", false);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        
        storyObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false); // �� ��Ȱ��ȭ
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
            PushBack(pushBackDir, 2f);
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
            PushBack(pushBackDir, 2f);
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
