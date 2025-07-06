using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Linq;
using static UnityEngine.UI.Image;

public class Enemy_Stage_GirlBoss : BossBase // Victorian ��ũ��Ʈ
{
    public int angerAttackID = 0; // �г��� ���� �ߺ� �浹 ���� ����
    public int peaceAttackID = 1; // ��ȭ�� ���� �ߺ� �浹 ���� ����

    [SerializeField] private GameObject bossAngerEffect; // ���� ���� ������Ʈ
    [SerializeField] private GameObject bossShadow; // 4�������� ���� �׸��� ������Ʈ
    [SerializeField] private GameObject shadowAttackRange; // 4�������� ���� �׸��� ������Ʈ
    [SerializeField] private GameObject spiderPortalPrefab; // �Ź� �ٸ� ���� ��Ż ������
    [SerializeField] private List<GameObject> phase4_Monster; // Ư�� ���Ͻ� ��ȯ�� 4�������� �� ����Ʈ
    [SerializeField] private List<GameObject> thunderPositions; // ���� ���ݽ� Ȱ��ȭ�� ���� ������Ʈ ����Ʈ

    [SerializeField] private GameObject thunderPrefab; // ���� ������Ʈ ������

    List<GameObject> portals = new List<GameObject>(); 
    private float followDuration = 1.5f;

    public bool isRangeMode = false; // ���Ÿ� ���� ��� ����
    private bool rangeModeStarted = false; // ���Ÿ� ���ݰ� ���۵Ǿ�����
    
    [SerializeField] private List<float> pollutionPhases; // �̵� �� �� ������ ������ ������ ��
    [SerializeField] private List<int> pollutionPhaseIndex; // ������ ������ �ܰ� ���� 
    [SerializeField] private List<Transform> pollutionPhasePos; // ������ �̵��� ������ �ܰ� ��ġ
    [SerializeField] private List<Transform> playerPhasePos; // �÷��̾ �̵��� ������ �ܰ� ��ġ 
    private bool isChangingPos = false; // ������ �ܰ� ���� �ߺ� ���� ����
    
    public GameObject storyObj1; // �������� �� ���丮 ������Ʈ

    private ChangeMap changMap;
    public float hpRatio; // ���� ������ ��
    public int currnetPhase; // ���� ������ �ܰ�

    public override bool isAttackRange
    {
        get => _isAttackRange;
        set
        {
            if (!attackMode || isRangeMode) return;

            _isAttackRange = value; // ����� �� ����

            if (isDead || isStune || isSpecialPhase) return; // ����, ����, Ư�� ���Ͻ� ����

            if (isAttackRange) // ���� ��� - ���� ���� �Լ�
            {
                if (!isAttacking) // ���� �ߺ� ���� ����
                {
                    targetPos = transform.position; // ���ݽ� �������� �ڽ����� ����(�̵�x)
                    attackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // ���� ���� �� �������� ����
                }
                else
                {
                    moveSpeed = 0f;
                }
            }
        }
    }

    private void Start()
    {
        changMap = FindObjectOfType<ChangeMap>();
        maxHp = 300f; // �� ü�� ����
        currentHp = 290f; // �� ü�� �ʱ�ȭ
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

        // ����� Ȯ��
        if (isPurifying && currentHp > 5f) // ���� ���� or ��ȭ�� ������ ������ ����(�ִ� 5���� ����)
            currentHp -= 40f * Time.deltaTime; // 1�ʿ� 5 Hp�� ����

        if (isReadyPeaceMelody && currentHp > 5f) // ��ȭ�� ���� �غ��ĵ� �ǰݽ� ������ ����(�ִ� 5����)
            currentHp -= 2f * Time.deltaTime; // 1�ʿ� 2Hp�� ����

        // ���Ÿ� ��� ���� Ȯ��
        if (isRangeMode && !rangeModeStarted)
        {
            StartCoroutine(SpecialPhaseRoutine());
        }

        // �� ���� �Լ� ���� Ȯ��
        hpRatio = currentHp / maxHp;
        CheckPollutionPhase(hpRatio);

        if (player == null || isStune || isRangeMode || isSpecialPhase) return;

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

    protected override void SetAttackTriggerRange(int index)
    {
        switch (index)
        {
            case 0:
                attackTriggerRange.transform.localScale = new Vector3(8, 2.5f, 0.1f);
                break;
            case 1:
                attackTriggerRange.transform.localScale = new Vector3(10f, 2.5f, 0.1f);
                break;
            case 2:
                attackTriggerRange.transform.localScale = new Vector3(14f, 2.5f, 0.1f);
                break;
        }
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
        
        Vector3 dirScale = attackObjects[0].transform.localScale;
        dirScale.x = Mathf.Abs(dirScale.x) * (direction.x > 0 ? 1 : -1);
        attackObjects[0].transform.localScale = dirScale;

        Vector3 dirReady = hitEffect.transform.localPosition; // ���� ��� UI ����
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect.transform.localPosition = dirReady;
        hitEffect.SetActive(true);
        yield return new WaitForSeconds(0.4f); // ���� ���� 0.4��

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack"); // Attack �ִϸ��̼� ���� -> �ִϸ��̼ǿ��� �ڵ����� attackObject Ȱ��ȭ/��Ȱ��ȭ
        bossAngerEffect.SetActive(true); // ���� ���� Ȱ��ȭ
        hitEffect.SetActive(false);

        yield return new WaitForSeconds(1.5f); // ���� �ൿ�� �ϴµ� ������ ��
        bossAngerEffect.SetActive(false); // ���� ���� ��Ȱ��ȭ

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
        Vector3 shadowPos = transform.transform.localPosition + Vector3.up;
        shadowPos.x += 10f * (direction.x > 0 ? 1 : -1);
        bossShadow.transform.localPosition = shadowPos;

        // �Ź� �׸��� ���� �غ�
        Vector3 shadowScale = bossShadow.transform.localScale;
        shadowScale.x = -Mathf.Abs(shadowScale.x) * (direction.x > 0 ? 1 : -1);
        bossShadow.transform.localScale = shadowScale;

        Debug.Log("���� �ҳฦ �����մϴ�!");
        animator.SetTrigger("Attack1"); // Attack �ִϸ��̼� ���� -> �ִϸ��̼ǿ��� �ڵ����� attackObject Ȱ��ȭ/��Ȱ��ȭ
        bossAngerEffect.SetActive(true); // ���� ���� Ȱ��ȭ
        hitEffect_noGroggy.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        bossShadow.SetActive(true);
        yield return new WaitForSeconds(0.6f);

        shadowAttackRange.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        shadowAttackRange.SetActive(false);

        bossAngerEffect.SetActive(false); // ���� ���� ��Ȱ��ȭ
        StartCoroutine(ShadowFade(bossShadow, 1f)); // 1�ʵ��� ������� ȿ��
        yield return new WaitForSeconds(2f); // ���� �ൿ�� �ϴµ� ������ ��

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // ���� ���� ����
        SetAttackTriggerRange(nextAttackIndex); // ���� ���� �ν� ���� ����
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // ������Ƽ �� �ʱ�ȭ(���¿� �´� �ൿ ����)
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
        yield return new WaitForSeconds(1f);

        // ���� ��� �� �������� ����
        Debug.Log("���� �ֺ��� �������� ������ ����Ĩ�ϴ�!");
        animator.SetTrigger("Attack2");
        attackMode = false; // ���� ����
        bossAngerEffect.SetActive(true); // ���� ���� Ȱ��ȭ
        hitEffect_noGroggy.SetActive(false);

        rigidBody.velocity = Vector3.zero;
        rigidBody.gravityScale = 0f;

        transform.DOMoveY(transform.position.y + 2f, 1f).SetEase(Ease.OutSine); // ���� �ε巴�� �������� ���

        // ���� ��� Ȱ��ȭ
        List<GameObject> shuffled = thunderPositions.OrderBy(x => Random.value).ToList(); // ����Ʈ ������ �������� ����
        List<GameObject> slected = shuffled.Take(5).ToList(); // ó�� 4���� ���
        foreach (var pos in slected)
        {
            yield return new WaitForSeconds(0.3f);
            pos.SetActive(true);
            Vector3 thunderPos = new Vector3(pos.transform.position.x, transform.position.y+6, 0);
            StartCoroutine(ActivateThunder(pos, thunderPos));
        }

        yield return new WaitForSeconds(1f);
        transform.DOMoveY(transform.position.y - 2f, 0.8f).SetEase(Ease.OutSine); // ���� �ε巴�� �������� ���
        bossAngerEffect.SetActive(false); // ���� ���� ��Ȱ��ȭ

        yield return new WaitForSeconds(2.5f); // ���� �ൿ���� ���
        nextAttackIndex = Random.Range(0, attackPatterns.Length);
        SetAttackTriggerRange(nextAttackIndex);
        moveSpeed = defaultMoveSpeed;
        rigidBody.gravityScale = 1f; // �߷� ������� ����

        if(!player.GetComponent<PlayerCtrl_R>().isLocked) // ��ũ��Ʈ ���������� ���� ���� AttackMode true�� �ʱ�ȭ
            attackMode = true; // ���� ���� ����
        
        isAttacking = false;
        EvaluateCurrentState();
    }

    private IEnumerator SpecialPhaseRoutine() // ���Ÿ� ��� ����
    {
        Debug.LogWarning("������ ���Ÿ� ������ �����մϴ�!");
        rangeModeStarted = true; // ���Ÿ� ���� ���۵�
        moveSpeed = 0f;
        animator.SetTrigger("Push");
        
        foreach (var attack in attackObjects) // ���� ���� ��� ��Ȱ��ȭ
        {
            if (attack != null)
                attack.SetActive(false);
        }

        yield return new WaitForSeconds(3f);

        // ���� ����
        animator.SetTrigger("Appear");
        transform.position = pollutionPhasePos[currnetPhase].position; // ������ ��ġ�� �̵�
        yield return new WaitForSeconds(0.5f);

        // ���� ���� ����
        animator.SetBool("isRun", false); // �⺻������ Idle �ִϸ��̼� ����
        while (isRangeMode)
        {
            // ���� ü�� Ȯ��
            float hpRatio = currentHp / maxHp;

            StartCoroutine(AttackSpecial()); // ������ ���Ÿ� ���� ����
            yield return new WaitForSeconds(7f); // ���� ���� ���
        }
        moveSpeed = defaultMoveSpeed;
        rangeModeStarted = false; // ���Ÿ� ���� �����
    }

    private IEnumerator AttackSpecial()
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
        yield return new WaitForSeconds(1f);

        Debug.Log("���� �ҳฦ �����ϴ� �ٸ��� ��ȯ�մϴ�!");
        attackMode = false; // ���� ����
        animator.SetTrigger("AttackSpecial");
        bossAngerEffect.SetActive(true); // ���� ���� Ȱ��ȭ
        hitEffect_noGroggy.SetActive(false);

        rigidBody.velocity = Vector3.zero;
        rigidBody.gravityScale = 0f;
       
        transform.DOMoveY(transform.position.y + 2f, 1f).SetEase(Ease.OutSine); // ���� �ε巴�� �������� ���

        for (int i = 0; i < 4; i++)
        {
            GameObject portal = Instantiate(spiderPortalPrefab, player.transform.position + Vector3.up * 5f, Quaternion.identity);
            portals.Add(portal);
            portal.SetActive(true);

            StartCoroutine(MoveWithPlayer(portal, followDuration));

            yield return new WaitForSeconds(0.7f); // ��Ż ���� ����
        }
        yield return new WaitForSeconds(1f);
        rigidBody.gravityScale = 1f; // �߷� ������� ����
        bossAngerEffect.SetActive(false); // ���� ���� ��Ȱ��ȭ

        yield return new WaitForSeconds(2.5f); // ���� �ൿ ���

        nextAttackIndex = Random.Range(0, attackPatterns.Length);
        SetAttackTriggerRange(nextAttackIndex);
        moveSpeed = defaultMoveSpeed;
        attackMode = true; // ���� ���� ����
        isAttacking = false;
        EvaluateCurrentState();
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

    private IEnumerator ShadowFade(GameObject targetObject, float duration) // ������� ȿ��
    {
        SpriteRenderer targetSpriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        Color defaultShadowColor = targetSpriteRenderer.color;
        float startAlpha = targetSpriteRenderer.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (targetObject == null) yield break;

            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            targetSpriteRenderer.color = new Color(targetSpriteRenderer.color.r, targetSpriteRenderer.color.g, targetSpriteRenderer.color.b, newAlpha);
            yield return null;
        }
        targetSpriteRenderer.color = new Color(targetSpriteRenderer.color.r, targetSpriteRenderer.color.g, targetSpriteRenderer.color.b, 0);
        targetObject.SetActive(false);
        targetSpriteRenderer.color = defaultShadowColor; // ���� ���� ����
    }

    private IEnumerator ActivateThunder(GameObject pos, Vector3 thunderPos) // ���� ���� �Լ�
    {
        yield return new WaitForSeconds(1f);

        GameObject thunder = Instantiate(thunderPrefab, thunderPos, Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        thunder.GetComponent<BoxCollider2D>().enabled = false;
        
        yield return new WaitForSeconds(0.5f);
        Destroy(thunder);
        pos.SetActive(false); // ��� ���� ��Ȱ��ȭ
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
        Destroy(portal);
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

            bossShadow.SetActive(false);
            shadowAttackRange.SetActive(false);
            hitEffect.SetActive(false);
            hitEffect_noGroggy.SetActive(false);
            bossAngerEffect.SetActive(false);
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

    void CheckPollutionPhase(float hpRatio) // �������� ���� �� ���� ���� Ȯ�� �Լ�
    {
        if(hpRatio <= pollutionPhases[0] && !isChangingPos)
        {
            isChangingPos = true;
            ActivePollutionPhase(pollutionPhaseIndex[0]);
        }
    }

    private void ActivePollutionPhase(int pollutionPhase) // ������ ���� ���� ���� �Լ�
    {
        Debug.LogWarning("[" + pollutionPhase + "] ���� �̵� or ���� �����մϴ�!");
        switch(pollutionPhase)
        {
            case 0:
                // �̵� + �� ����, ���Ÿ� ��� x
                StartCoroutine(ChangeMap(0, 0, true));
                break;

            case 1:
                // �̵� only, ���Ÿ� ��� o
                StartCoroutine(TransLateTarget(1, true));
                break;

            case 2:
                // �̵� only, ���Ÿ� ��� x
                StartCoroutine(TransLateTarget(2, false));
                break;

            case 3:
                // �̵� + �� ����, ���Ÿ� ��� x
                StartCoroutine(ChangeMap(3, 1, true));
                break;

            case 4:
                // �̵� only, ���Ÿ� ��� o
                StartCoroutine(TransLateTarget(4, true));
                break;

            case 5:
                // �̵� only, ���Ÿ� ��� x
                StartCoroutine(TransLateTarget(5, false));
                break;

            case 6:
                // �̵� + �� ����, ���Ÿ� ��� x
                StartCoroutine(ChangeMap(6, 2, true));
                player.GetComponent<PlayerCtrl_R>().ActivatedSealMode(true); // �ɷ� ���� (���ڰ��常 ����)
                break;

            case 7:
                attackMode = false; // ��ũ��Ʈ ����
                animator.SetBool("isRun", false); // Idle ���·� ����
                player.GetComponent<PlayerCtrl_R>().ActivatedSealMode(false); // �ɷ� ���� ����
                storyObj1.SetActive(true); // ���丮 ������Ʈ Ȱ��ȭ
                break;
        }
    }

    private IEnumerator TransLateTarget(int pollutionPhase, bool rangeMode) // ���� ��ġ �̵� �Լ�
    {
        Debug.LogWarning("������ [" + pollutionPhaseIndex + "] ��ġ�� �̵��մϴ�!");

        // ����� ������ ����
        currnetPhase = pollutionPhaseIndex[0]; // ���� ������ ����
        pollutionPhases.RemoveAt(0);
        pollutionPhaseIndex.RemoveAt(0);

        isSpecialPhase = true;

        attackMode = false; // ���� ����
        moveSpeed = 0f;
        animator.SetTrigger("Push");
        animator.SetTrigger("Disaapear");

        foreach (var attack in attackObjects) // ���� ���� ��� ��Ȱ��ȭ
        {
            if (attack != null)
                attack.SetActive(false);
        }
        yield return new WaitForSeconds(1.5f);
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(1.5f);

        // ���� ��ġ �̵� �� ���� ��� ����
        spriteRenderer.enabled = true;
        animator.SetTrigger("Appear");
        transform.position = pollutionPhasePos[pollutionPhase].position;
        isRangeMode = rangeMode; // �⺻ ��� vs ���Ÿ� ��� ���� 

        yield return new WaitForSeconds(1.5f);

        moveSpeed = defaultMoveSpeed;
        attackMode = true;
        isChangingPos = false;
        isSpecialPhase = false;
    }

    private IEnumerator ChangeMap(int pollutionPhase, int mapPhase, bool isAttackMode) // ���� ��ġ �̵� �Լ�
    {
        Debug.LogWarning("������ ���� �����մϴ�!");

        isSpecialPhase = true;

        // ����� ������ ����
        currnetPhase = pollutionPhaseIndex[0]; // ���� ������ ����
        pollutionPhases.RemoveAt(0);
        pollutionPhaseIndex.RemoveAt(0);

        attackMode = false; // ���� ����
        StartCoroutine(PushAttack(0f));
        moveSpeed = 0f;
        animator.SetTrigger("Disaapear");

        yield return new WaitForSeconds(1f);

        // �� ����
        transform.position = startPos;
        changMap.Pase(mapPhase);
        yield return new WaitForSeconds(0.1f);
        player.transform.position = playerPhasePos[mapPhase].position;

        // ���� ��ġ �̵� �� ���� ��� ����
        yield return new WaitForSeconds(1.5f);
        animator.SetTrigger("Appear");
        transform.position = pollutionPhasePos[pollutionPhase].position;
        isRangeMode = false; // �⺻ ��� 
        yield return new WaitForSeconds(1.5f);


        foreach (var attack in attackObjects) // ���� ���� ��� ��Ȱ��ȭ
        {
            if (attack != null)
                attack.SetActive(false);
        }

        if(isAttackMode) // ���ݸ���� ���
            attackMode = true; // ���� ���� ����
        
        isAttackRange = false;
        moveSpeed = defaultMoveSpeed;
        isChangingPos = false;
        isSpecialPhase = false;
    }

    void OnDrawGizmos() // �а� ���� ǥ��
    {
        Gizmos.color = Color.red; // ���� ����
        Gizmos.DrawWireSphere(transform.position, 3); // ���� ����
    }

    public override IEnumerator Damaged() // �ǰݽ� ���� �������̵�
    {
        if (GameManager.Instance.isReturned) // ȸ�� �� �÷��̾� ������ ����
            currentHp += player.GetComponent<PlayerSkill_R>().playerDamage;
        else // ȸ�� �� �÷��̾� ������ ����
            currentHp += player.GetComponent<PlayerSkill>().playerDamage;

        Debug.Log("���� ������ : " + currentHp);

        if (currentHp < maxHp) // �ǰ� ����
        {
            Debug.Log("���� �������� ���� ���ظ� �Խ��ϴ�.");
            StartCoroutine(Stunned(0.3f)); // 0.3�� ����
            animator.SetTrigger("Damaged"); // �ǰ� �ִϸ��̼� ����
            hitEffect.SetActive(true); // �ǰ� ����Ʈ Ȱ��ȭ

            yield return new WaitForSeconds(0.2f);

            hitEffect.SetActive(false); // �ǰ� ����Ʈ ��Ȱ��ȭ
        }
        else // ��� ���� 
        {
            Debug.LogWarning("���� ��ȿ�մϴ�!");
            StartCoroutine(PushAttack(0.5f));
            hitEffect.SetActive(true); // �ǰ� ����Ʈ Ȱ��ȭ

            yield return new WaitForSeconds(0.2f);

            hitEffect.SetActive(false); // �ǰ� ����Ʈ ��Ȱ��ȭ
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
