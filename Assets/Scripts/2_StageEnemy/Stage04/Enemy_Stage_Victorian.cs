using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Enemy_Stage_Victorian : BossBase // Victorian ��ũ��Ʈ
{
    public int angerAttackID = 0; // �г��� ���� �ߺ� �浹 ���� ����
    public int peaceAttackID = 1; // ��ȭ�� ���� �ߺ� �浹 ���� ����

    [SerializeField] private GameObject bossShadow; // 4�������� ���� �׸��� ������Ʈ
    [SerializeField] private GameObject shadowAttackRange; // 4�������� ���� �׸��� ������Ʈ
    [SerializeField] private GameObject spiderPortalPrefab; // �Ź� �ٸ� ���� ��Ż ������
    [SerializeField] private List<GameObject> translatePos; // Ư�� ���Ͻ� ������ �̵��� ��ġ ����Ʈ
    [SerializeField] private List<GameObject> phase4_Monster; // Ư�� ���Ͻ� ��ȯ�� 4�������� �� ����Ʈ
    List<GameObject> portals = new List<GameObject>(); private float followDuration = 1.5f;

    private float delayBeforeAttack = 1f;
    private int spawnEnemyCnt = 0; // ��ȯ�� ���� ��
    private int specialPhaseCnt = 1; // Ư�� ���� ���� Ƚ�� 

    private void Start()
    {
        maxHp = 300f; // �� ü�� ����
        currentHp = 150f; // �� ü�� �ʱ�ȭ
        damage = 12f; // �� ���ݷ� ���� 
        bulletSpeed = 20; // źȯ �ӵ� ����
        maxGroggyCnt = 3; // �ִ� �׷α� ������ 3���� ����
        currentGroggyCnt = 0; // ���� �׷α� ���� �ʱ�ȭ
        rigidBody.drag = 5f; // �⺻ ������ ����
        startPos = transform.position;
        moveSpeed = defaultMoveSpeed;
        isPatrol = false;
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
                attackTriggerRange.transform.localScale = new Vector3(1f, 0.2f, 0.1f);
                break;
            case 1:
                attackTriggerRange.transform.localScale = new Vector3(1.2f, 0.2f, 0.1f);
                break;
            case 2:
                attackTriggerRange.transform.localScale = new Vector3(1.3f, 0.2f, 0.1f);
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

    private IEnumerator Attack0() // �Ϲ� ���� ����
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
        yield return new WaitForSeconds(0.4f); // ���� ���� 0.4��

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

    private IEnumerator Attack1() // �ٸ� Ÿ�̹� ���� + �׸��� ����
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
        yield return new WaitForSeconds(0.6f); // ���� ���� 1��

        // �Ź� �׸��� ��ġ �غ�
        Vector3 shadowPos = transform.transform.localPosition;
        shadowPos.x += 12f * Mathf.Sign(dirAttack.x);
        bossShadow.transform.localPosition = shadowPos;

        // �Ź� �׸��� ���� ���� ��ġ ����
        Vector3 shadowRange = shadowAttackRange.transform.localPosition;
        shadowRange.x = -Mathf.Abs(shadowRange.x) * Mathf.Sign(dirAttack.x);
        shadowAttackRange.transform.localPosition = shadowRange;

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack"); // Attack �ִϸ��̼� ���� -> �ִϸ��̼ǿ��� �ڵ����� attackObject Ȱ��ȭ/��Ȱ��ȭ
        hitEffect_noGroggy.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        bossShadow.SetActive(true);
        yield return new WaitForSeconds(0.6f);

        shadowAttackRange.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        shadowAttackRange.SetActive(false);
        StartCoroutine(ShadowFade(bossShadow, 1f)); // 1�ʵ��� ������� ȿ��
        yield return new WaitForSeconds(2f); // ���� �ൿ�� �ϴµ� ������ ��

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        SetAttackTriggerRange(nextAttackIndex); // ���� ���� �ν� ���� ����
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // ������Ƽ �� �ʱ�ȭ(���¿� �´� �ൿ ����)
    }

    private IEnumerator ShadowFade(GameObject targetObject, float duration) // ������� ȿ��
    {
        SpriteRenderer targetSpriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        Color defaultShadowColor = targetSpriteRenderer.color;
        float startAlpha = targetSpriteRenderer.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            targetSpriteRenderer.color = new Color(targetSpriteRenderer.color.r, targetSpriteRenderer.color.g, targetSpriteRenderer.color.b, newAlpha);
            yield return null;
        }
        targetSpriteRenderer.color = new Color(targetSpriteRenderer.color.r, targetSpriteRenderer.color.g, targetSpriteRenderer.color.b, 0);
        targetObject.SetActive(false);
        targetSpriteRenderer.color = defaultShadowColor; // ���� ���� ����
    }


    private IEnumerator Attack2()
    {
        isAttacking = true;
        attackCoroutine = null;
        currentAttack = attackObjects[2];
        moveSpeed = 0f;
        LockOnTargetPlayer();

        // ��� UI ����
        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition;
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);

        animator.SetTrigger("Charge");
        yield return new WaitForSeconds(0.5f);

        Debug.Log("���� �ҳฦ �����ϴ� �ٸ��� ��ȯ�մϴ�!");
        hitEffect_noGroggy.SetActive(false);
        for (int i = 0; i < 3; i++)
        {
            GameObject portal = Instantiate(spiderPortalPrefab, player.transform.position + Vector3.up * 5f, Quaternion.identity);
            portals.Add(portal);
            portal.SetActive(true);

            StartCoroutine(MoveWithPlayer(portal, followDuration));

            yield return new WaitForSeconds(1f); // ��Ż ���� ����
        }

        yield return new WaitForSeconds(2f);
        animator.SetTrigger("Damaged");

        yield return new WaitForSeconds(1.5f); // ���� �ൿ ���

        nextAttackIndex = Random.Range(0, attackPatterns.Length);
        SetAttackTriggerRange(nextAttackIndex);
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;
        EvaluateCurrentState();
    }

    private IEnumerator MoveWithPlayer(GameObject portal, float followDuration)
    {
        float elapsed = 0f;

        // �� ���� ���
        while (elapsed < followDuration)
        {
            if (portal == null)
                yield break;

            Vector3 pos = portal.transform.position;
            pos.x = player.transform.position.x;
            portal.transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ���� ���
        if (portal == null) yield break;

        GameObject bossLeg = portal.transform.Find("Victorian_Leg")?.gameObject;
        if (bossLeg != null)
        {
            bossLeg.SetActive(true);
            yield return new WaitForSeconds(0.5f);

            BoxCollider2D col = bossLeg.GetComponent<BoxCollider2D>();
            if (col != null && col.gameObject != null)
            {
                col.enabled = true;
                yield return new WaitForSeconds(0.2f);

                // �ٽ� �ı����� �ʾҴ��� Ȯ��
                if (col != null && col.gameObject != null)
                    col.enabled = false;
            }
            StartCoroutine(ShadowFade(bossLeg, 1f));
        }
        yield return new WaitForSeconds(0.5f);
        portal.SetActive(false);
    }

    public override void CancelAttack() // ���� ��ҽ� ���� �������̵�
    {

        if (attackCoroutine != null && currentAttack != null)
        {
            StopCoroutine(attackCoroutine);

            foreach (var attack in attackObjects) // ���� ���� ��� ��Ȱ��ȭ
            {
                if (attack != null)
                    attack.SetActive(false);
            }
            foreach (var portal in portals)
            {
                if (portal != null)
                    Destroy(portal);
            }

            hitEffect.SetActive(false);
            hitEffect_noGroggy.SetActive(false);
            spriteRenderer.enabled = true;
            boxCollider.enabled = true;
            attackCoroutine = null;
            nextAttackIndex = Random.Range(0, attackPatterns.Length);
            SetAttackTriggerRange(nextAttackIndex);
            isAttacking = false;

            EvaluateCurrentState();
            Debug.LogWarning("���� ���� ����!");
        }
    }

    private IEnumerator SpecialPhaseRoutine() // Ư�� ���� ����
    {
        Debug.LogWarning("������ Ư�� ������ �����մϴ�!");
        isSpecialPhase = true;
        moveSpeed = 0f;
        specialPhaseCnt--;
        animator.SetTrigger("Damaged");
        yield return new WaitForSeconds(3f);

        // ������� ����
        animator.SetTrigger("Attack2"); // �ö󰡴� �ִϸ��̼� 
        yield return new WaitForSeconds(0.3f);

        // ���� ����
        animator.SetTrigger("Attack2End"); // �������� �ִϸ��̼�
        int randIndex = Random.Range(0, translatePos.Count);
        transform.position = translatePos[randIndex].transform.position;
        yield return new WaitForSeconds(0.5f);

        // ���� Ȱ��ȭ
        foreach (GameObject monster in phase4_Monster)
        {
            monster.SetActive(true);
        }

        // ���� ���� ����
        while (isSpecialPhase)
        {
            // ���� ü�� Ȯ��
            float hpRatio = currentHp / maxHp;
            if (hpRatio <= 0f || hpRatio >= 1f) break; // �������� 0 or 100%�� ����

            StartCoroutine(Attack2()); // ������ ���Ÿ� ���� ����
            yield return new WaitForSeconds(7f); // ���� ���� ���
        }

        // ���� ��ġ�� �̵�
        transform.position = startPos;
        moveSpeed = defaultMoveSpeed;
        attackMode = false; // �ǰݸ�� ���� (���丮 ����)


        foreach (var monster in phase4_Monster) // ���� ����
        {
            if (monster != null)
                monster.SetActive(false);
        }
        bossShadow.SetActive(false);

        isSpecialPhase = false; // Ư�� ���� ����
        Debug.LogWarning("���� Ŭ����! ���丮 ���� ����");
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
