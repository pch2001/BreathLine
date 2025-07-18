using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Enemy_Stage_Victorian : BossBase // Victorian 스크립트
{
    public int angerAttackID = 0; // 분노의 악장 중복 충돌 판정 방지
    public int peaceAttackID = 1; // 평화의 악장 중복 충돌 판정 방지

    [SerializeField] private GameObject bossShadow; // 4스테이지 보스 그림자 오브젝트
    [SerializeField] private GameObject shadowAttackRange; // 4스테이지 보스 그림자 오브젝트
    [SerializeField] private GameObject spiderPortalPrefab; // 거미 다리 생성 포탈 프리팹
    [SerializeField] private List<GameObject> translatePos; // 특수 패턴시 보스가 이동할 위치 리스트
    [SerializeField] private List<GameObject> phase4_Monster; // 특수 패턴시 소환할 4스테이지 적 리스트
    [SerializeField] private GameObject storyR4; // 거미 사망시 진행할 대화 스크립트
    List<GameObject> portals = new List<GameObject>(); private float followDuration = 1.5f;
    public int specialPhaseCnt = 10; // 특수 패턴 남은 횟수 

    public GameObject boss4R; // 소녀 보스 오브젝트
    public GameObject playerPos; // 플레이어 이동 시킬 위치
    public GameObject spawnThunder; // 번개 생성 오브젝트

    private void Start()
    {
        maxHp = 300f; // 적 체력 설정
        currentHp = 150f; // 적 체력 초기화
        damage = 12f; // 적 공격력 설정 
        bulletSpeed = 20; // 탄환 속도 설정
        maxGroggyCnt = 3; // 최대 그로기 게이지 3개로 설정
        currentGroggyCnt = 0; // 현재 그로기 개수 초기화
        rigidBody.drag = 5f; // 기본 마찰력 설정
        startPos = transform.position;
        moveSpeed = defaultMoveSpeed;
        isPatrol = false;

        foreach (GameObject attackObj in attackObjects) // 공격 패턴 내부값 초기화
        {
            EnemyAttackBase attackBase = attackObj.GetComponent<EnemyAttackBase>();
            if (attackBase != null)
            {
                attackBase.enemyOrigin = this.gameObject;
            }
        }

        ChooseNextPatrolPoint(); // 다음 목적지 설정
        InitializeAttackPatterns(); // 공격 함수 구성 초기화
        MoveToTarget();

        GameManager.Instance.isReturned = enemyIsReturn; // 적 회귀 상태 설정
        if (GameManager.Instance.isReturned) // 회귀 후, 그로기 슬롯 초기화 
        {
            groggyUI.SetupGroggySpriteGauge(maxGroggyCnt);
        }

        if (!GameManager.Instance.isReturned)
        {
            player.GetComponent<PlayerCtrl>().RequestPlayerPolluted += OnRequestPlayerPolluted;
        }
    }

    private void Update()
    {
        // 죽음시 리턴
        if (isDead || !attackMode) return;

        // 특수 패턴 시작 확인
        float hpRatio = currentHp / maxHp;
        if (!isSpecialPhase && specialPhaseCnt <= 0)
        {
            StartCoroutine(SpecialPhaseRoutine()); return;
        }

        // 디버프 확인
        if (isPurifying && currentHp > 5f) // 늑대 등장 or 정화의 걸음시 오염도 감소(최대 5까지 감소)
            currentHp -= 40f * Time.deltaTime; // 1초에 5 Hp씩 감소

        if (isReadyPeaceMelody && currentHp > 5f) // 평화의 악장 준비파동 피격시 오염도 감소(최대 5까지)
            currentHp -= 2f * Time.deltaTime; // 1초에 2Hp씩 감소

        if (player == null || isStune || isSpecialPhase) return;

        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // 속도에 따른 애니메이션 제어

        if (!isPatrol && isAttackRange || isAttacking) return; // 공격 모드 - 공격 실행 상태 or 이미 공격 실행시 이동 제한

        if (!isPatrol && !isAttackRange) // 공격 모드 - 적 추격 상태 구현
        {
            LockOnTargetPlayer(); // 목적지를 플레이어 위치로 갱신
        }

        rigidBody.velocity = velocity; // 적 이동

    }

    protected override void InitializeAttackPatterns() // 공격 함수 구성 초기화
    {
        attackPatterns = new AttackPattern[] // 공격 패턴 종류 초기화
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

    private void LockOnTargetPlayer() // 플레이어 위치를 목표지점으로 설정
    {
        targetPos = player.transform.position;
        direction = (targetPos - (Vector2)transform.position).normalized; // 패트롤 할 방향 설정

        if (direction.x > 0)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    private IEnumerator Attack0() // 일반 근접 공격
    {
        isAttacking = true;

        Debug.Log("적이 [공격 0]을 준비합니다!");
        currentAttack = attackObjects[0];
        specialPhaseCnt--;
        moveSpeed = 0f;
        LockOnTargetPlayer(); // 플레이어를 바라보게 설정

        Vector3 dirAttack = attackObjects[0].transform.localPosition; // 적 공격 범위 준비
        dirAttack.x = Mathf.Abs(dirAttack.x) * (direction.x > 0 ? 1 : -1);
        attackObjects[0].transform.localPosition = dirAttack;

        Vector3 dirReady = hitEffect.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect.transform.localPosition = dirReady;
        hitEffect.SetActive(true);
        yield return new WaitForSeconds(0.4f); // 공격 간격 0.4초

        Debug.Log("적이 소녀를 공격합니다!");
        animator.SetTrigger("Attack"); // Attack 애니메이션 실행 -> 애니메이션에서 자동으로 attackObject 활성화/비활성화
        hitEffect.SetActive(false);
        yield return new WaitForSeconds(1.5f); // 다음 행동을 하는데 간격을 둠

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        SetAttackTriggerRange(nextAttackIndex); // 다음 공격 인식 범위 설정
        moveSpeed = defaultMoveSpeed;
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        isAttacking = false;

        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    private IEnumerator Attack1() // 다른 타이밍 공격 + 그림자 공격
    {
        isAttacking = true;

        Debug.Log("적이 [공격 1]을 준비합니다!");
        specialPhaseCnt--;
        currentAttack = attackObjects[1];
        moveSpeed = 0f;
        LockOnTargetPlayer(); // 플레이어를 바라보게 설정

        Vector3 dirAttack = attackObjects[1].transform.localPosition; // 적 공격 범위 준비
        dirAttack.x = Mathf.Abs(dirAttack.x) * (direction.x > 0 ? 1 : -1);
        attackObjects[1].transform.localPosition = dirAttack;

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(0.6f); // 공격 간격 1초

        // 거미 그림자 위치 준비
        Vector3 shadowPos = transform.transform.localPosition;
        shadowPos.x += 12f * (direction.x > 0 ? 1 : -1);
        bossShadow.transform.localPosition = shadowPos;

        // 거미 그림자 방향 준비
        Vector3 shadowScale = bossShadow.transform.localScale;
        shadowScale.x = -Mathf.Abs(shadowScale.x) * (direction.x > 0 ? 1 : -1);
        bossShadow.transform.localScale = shadowScale;

        Debug.Log("적이 소녀를 공격합니다!");
        animator.SetTrigger("Attack"); // Attack 애니메이션 실행 -> 애니메이션에서 자동으로 attackObject 활성화/비활성화
        hitEffect_noGroggy.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        bossShadow.SetActive(true);
        yield return new WaitForSeconds(0.6f);

        shadowAttackRange.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        shadowAttackRange.SetActive(false);
        StartCoroutine(ShadowFade(bossShadow, 1f)); // 1초동안 사라지는 효과
        yield return new WaitForSeconds(2f); // 다음 행동을 하는데 간격을 둠

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        SetAttackTriggerRange(nextAttackIndex); // 다음 공격 인식 범위 설정
        moveSpeed = defaultMoveSpeed;
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        isAttacking = false;

        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    private IEnumerator ShadowFade(GameObject targetObject, float duration) // 사라지는 효과
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
        targetSpriteRenderer.color = defaultShadowColor; // 기존 색상 복원
    }


    private IEnumerator Attack2()
    {
        isAttacking = true;
        attackMode = false; // 무적 판정

        currentAttack = attackObjects[2];
        moveSpeed = 0f;
        LockOnTargetPlayer();

        // 경고 UI 연출
        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition;
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);

        animator.SetTrigger("Charge");
        yield return new WaitForSeconds(0.5f);

        Debug.Log("적이 소녀를 추적하는 다리를 소환합니다!");
        hitEffect_noGroggy.SetActive(false);
        for (int i = 0; i < (isSpecialPhase ? 5 : 3); i++)
        {
            GameObject portal = Instantiate(spiderPortalPrefab, player.transform.position + Vector3.up * 5f, Quaternion.identity);
            portals.Add(portal);
            portal.SetActive(true);

            StartCoroutine(MoveWithPlayer(portal, followDuration));

            yield return new WaitForSeconds((isSpecialPhase ? 0.7f : 1f)); // 포탈 생성 간격
        }

        yield return new WaitForSeconds(2f);
        animator.SetTrigger("Damaged");

        yield return new WaitForSeconds(1.5f); // 다음 행동 대기

        nextAttackIndex = Random.Range(0, attackPatterns.Length);
        SetAttackTriggerRange(nextAttackIndex);
        moveSpeed = defaultMoveSpeed;
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        attackMode = true; // 무적 해제
        isAttacking = false;
        EvaluateCurrentState();
    }

    private IEnumerator MoveWithPlayer(GameObject portal, float followDuration)
    {
        float elapsed = 0f;

        // 적 추적 기능
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

        // 공격 기능
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

                // 다시 파괴되지 않았는지 확인
                if (col != null && col.gameObject != null)
                    col.enabled = false;
            }
            StartCoroutine(ShadowFade(bossLeg, 1f));
        }
        yield return new WaitForSeconds(0.5f);
        Destroy(portal);
    }

    public override void CancelAttack() // 공격 취소시 내용 오버라이딩
    {

        if (attackCoroutine != null && currentAttack != null)
        {
            StopCoroutine(attackCoroutine);

            foreach (var attack in attackObjects) // 공격 범위 모두 비활성화
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
            spriteRenderer.enabled = true;
            boxCollider.enabled = true;
            attackCoroutine = null;
            nextAttackIndex = Random.Range(0, attackPatterns.Length);
            SetAttackTriggerRange(nextAttackIndex);
            isAttacking = false;

            EvaluateCurrentState();
            Debug.LogWarning("공격 강제 종료!");
        }
    }

    private IEnumerator SpecialPhaseRoutine() // 특수 패턴 구현
    {
        Debug.LogWarning("보스가 특수 패턴을 시작합니다!");
        isSpecialPhase = true;
        specialPhaseCnt = 10;
        moveSpeed = 0f;
        specialPhaseCnt--;
        animator.SetTrigger("Damaged");
        yield return new WaitForSeconds(3f);

        // 사라지는 연출
        animator.SetTrigger("Attack2"); // 올라가는 애니메이션 
        yield return new WaitForSeconds(0.3f);

        // 등장 연출
        animator.SetTrigger("Attack2End"); // 내려가는 애니메이션
        int randIndex = Random.Range(0, translatePos.Count);
        transform.position = translatePos[randIndex].transform.position;
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SpecialPatternCool()); // 특수 패턴 유지 쿨타임 시작

        // 몬스터 활성화
        foreach (GameObject monster in phase4_Monster)
        {
            monster.SetActive(true);
        }

        // 공격 루프 실행
        while (isSpecialPhase)
        {
            // 보스 체력 확인
            float hpRatio = currentHp / maxHp;

            attackCoroutine = StartCoroutine(Attack2()); // 기존의 원거리 공격 재사용
            yield return new WaitForSeconds(10f); // 다음 공격 대기
        }

        animator.SetTrigger("Attack2"); // 올라가는 애니메이션 
        yield return new WaitForSeconds(0.3f); // 잠시 대기
        // 일반몬스터 삭제 및 원래 위치로 이동

        foreach (GameObject monster in phase4_Monster)
        {
            monster.SetActive(false);
        }
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        animator.SetTrigger("Attack2End"); // 내려가는 애니메이션
        transform.position = startPos;
    }

    private IEnumerator SpecialPatternCool() // 특수 패턴 쿨타임 확인 함수
    {
        yield return new WaitForSeconds(20f);
        isSpecialPhase = false;
    }


    public override IEnumerator Damaged() // 피격시 반응 오버라이딩
    {
        if (GameManager.Instance.isReturned) // 회귀 후 플레이어 데미지 연결
            currentHp += player.GetComponent<PlayerSkill_R>().playerDamage;
        else // 회귀 전 플레이어 데미지 연결
            currentHp += player.GetComponent<PlayerSkill>().playerDamage;

        Debug.Log("현재 오염도 : " + currentHp);

        if (currentHp < maxHp) // 피격 반응
        {
            Debug.Log("적이 공격으로 인해 피해를 입습니다.");
            StartCoroutine(Stunned(0.3f)); // 0.3초 경직
            animator.SetTrigger("Damaged"); // 피격 애니메이션 실행
            hitEffect.SetActive(true); // 피격 이펙트 활성화

            yield return new WaitForSeconds(0.2f);

            hitEffect.SetActive(false); // 피격 이펙트 비활성화
        }
        else // 오염도 최대치 반응
        {
            Debug.LogWarning("적이 포효합니다!");
            StartCoroutine(PushAttack(0.5f));
            hitEffect.SetActive(true); // 피격 이펙트 활성화

            yield return new WaitForSeconds(0.2f);

            hitEffect.SetActive(false); // 피격 이펙트 비활성화
        }
    }

    private void OnRequestPlayerPolluted() // 공격 모드 비활성화 함수
    {
        if(attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        if (rangeAttackCoroutine != null)
        {
            StopCoroutine(rangeAttackCoroutine);
            attackMode = false;
        }
        isSpecialPhase = false;
        attackMode = false;
        transform.position = startPos;
        spriteRenderer.flipX = true;
        animator.SetTrigger("Attack1");
    }


    private IEnumerator StartTextStage4R() // 거미 소멸시 재생할 스크립트
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        if (rangeAttackCoroutine != null)
        {
            StopCoroutine(rangeAttackCoroutine);
            attackMode = false;
        }

        player.GetComponent<PlayerCtrl_R>().OnDisable(); // 플레이어 조작 비활성화
        Time.timeScale = 0.5f;
        attackMode = false;

        gameObject.transform.position = startPos;
        player.transform.position = playerPos.transform.position;
        StartCoroutine(EnemyFade(1.5f));
        yield return new WaitForSeconds(1.5f);

        boss4R.SetActive(true); // 소녀 보스 활성화
        yield return new WaitForSeconds(0.2f);
        
        Time.timeScale = 1f;
        player.GetComponent<SpriteRenderer>().flipX = false;
        yield return new WaitForSeconds(0.2f);

        boss4R.GetComponent<SpriteRenderer>().flipX = true;
        StartCoroutine(storyR4.GetComponent<Story_four_R>().TypingText(1));
    }

    public override IEnumerator EnemyFade(float duration) // 평화의 악장으로 적 사라짐 함수
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;

        isDead = true; // 죽음 상태로 변경
        enemyFadeEffect.SetActive(true);
        defaultMoveSpeed = 0f; // 이동 불가능
        animator.SetBool("isRun", false);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }


    protected override void HandlerTriggerEnter(Collider2D collision) // 충돌 처리 담당 
    {
        if (!attackMode) return;

        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("분노의 악장이 적을 공격합니다!!");
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            Debug.Log("평화의 악장이 적을 진정시킵니다");
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            peaceAttackID = attackArea.attackGlobalID;

            currentHp -= 15f;
            if (currentHp <= 0 && enemyIsReturn)
            {
                audioSource.clip = enemySounds[4]; // 음원[사라짐]
                audioSource.Play(); // 음원 실행

                StartCoroutine(StartTextStage4R()); // 스크립트 재생
                Debug.LogWarning("스크립트를 진행합니다");
            }
            else
            {
                StartCoroutine(Stunned(3f)); // 적 3초 기절
                StartCoroutine(PushAttack(3f)); // 3초후 밀격 공격
            }
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (isStune) return;

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.LogWarning("늑대의 공격이 적을 기절시킵니다!");
            if (currentHp - 20f >= 5) // 최대 5까지 오염도 감소
                currentHp -= 20f; // 적 오염도 즉시 20 감소 
            else
                currentHp = 5f;

            StartCoroutine(Stunned(2f));
        }
        else if (collision.gameObject.CompareTag("WolfPush"))
        {
            if (isStune) return;

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.LogWarning("늑대가 적을 밀쳐냅니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir, 2f);
        }
        else if (collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep"))
        {
            Debug.Log("적을 진정시킵니다");
            isPurifying = true;
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {

            isReadyPeaceMelody = true; // 평화의 악장 준비 파동 시작
        }
        else if (collision.gameObject.CompareTag("EnemyProjectile"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyAttackBase>(); // EnemyAttack 기본 클래스 가져옴
            Debug.Log("적이 반사된 공격에 피해를 입습니다!");
            StartCoroutine(Damaged());
        }
    }

    protected override void HandlerTriggerStay(Collider2D collision) // 충돌 처리 담당 
    {
        if (!attackMode) return;

        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("분노의 악장이 적을 공격합니다!!");

            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            peaceAttackID = attackArea.attackGlobalID;

            currentHp -= 10f;
            if (currentHp <= 0)
                StartCoroutine(EnemyFade(3f)); // 적 사라짐
            else
                StartCoroutine(Stunned(3f)); // 적 3초 기절
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (isStune) return;
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.LogWarning("늑대의 공격이 적을 기절시킵니다!");
            if (currentHp - 20f >= 5) // 최대 5까지 오염도 감소
                currentHp -= 20f; // 적 오염도 즉시 20 감소 
            else
                currentHp = 5f;

            StartCoroutine(Stunned(2f));
        }
        else if (collision.gameObject.CompareTag("WolfPush"))
        {
            if (isStune) return;

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.LogWarning("늑대가 적을 밀쳐냅니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir, 2f);
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
            Debug.Log("적이 범위를 벗어납니다");
            moveSpeed = defaultMoveSpeed; // 기존 속도
            isPurifying = false;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            Debug.Log("평화의 악장을 준비를 마칩니다!");
            isReadyPeaceMelody = false; // 평화의 악장 준비 해제
        }
    }
}
