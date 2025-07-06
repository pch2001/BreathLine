using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Linq;
using static UnityEngine.UI.Image;

public class Enemy_Stage_GirlBoss : BossBase // Victorian 스크립트
{
    public int angerAttackID = 0; // 분노의 악장 중복 충돌 판정 방지
    public int peaceAttackID = 1; // 평화의 악장 중복 충돌 판정 방지

    [SerializeField] private GameObject bossAngerEffect; // 보스 음파 오브젝트
    [SerializeField] private GameObject bossShadow; // 4스테이지 보스 그림자 오브젝트
    [SerializeField] private GameObject shadowAttackRange; // 4스테이지 보스 그림자 오브젝트
    [SerializeField] private GameObject spiderPortalPrefab; // 거미 다리 생성 포탈 프리팹
    [SerializeField] private List<GameObject> phase4_Monster; // 특수 패턴시 소환할 4스테이지 적 리스트
    [SerializeField] private List<GameObject> thunderPositions; // 번개 공격시 활성화할 범위 오브젝트 리스트

    [SerializeField] private GameObject thunderPrefab; // 번개 오브젝트 프리팹

    List<GameObject> portals = new List<GameObject>(); 
    private float followDuration = 1.5f;

    public bool isRangeMode = false; // 원거리 공격 모드 인지
    private bool rangeModeStarted = false; // 원거리 공격가 시작되었는지
    
    [SerializeField] private List<float> pollutionPhases; // 이동 및 맵 변경을 실행할 오염도 값
    [SerializeField] private List<int> pollutionPhaseIndex; // 실행할 오염도 단계 순서 
    [SerializeField] private List<Transform> pollutionPhasePos; // 보스가 이동할 오염도 단계 위치
    [SerializeField] private List<Transform> playerPhasePos; // 플레이어가 이동할 오염도 단계 위치 
    private bool isChangingPos = false; // 오염도 단계 변경 중복 실행 방지
    
    public GameObject storyObj1; // 정신착란 전 스토리 오브젝트

    private ChangeMap changMap;
    public float hpRatio; // 현재 오염도 값
    public int currnetPhase; // 현재 페이즈 단계

    public override bool isAttackRange
    {
        get => _isAttackRange;
        set
        {
            if (!attackMode || isRangeMode) return;

            _isAttackRange = value; // 변경된 값 적용

            if (isDead || isStune || isSpecialPhase) return; // 죽음, 기절, 특수 패턴시 리턴

            if (isAttackRange) // 공격 모드 - 공격 실행 함수
            {
                if (!isAttacking) // 공격 중복 실행 방지
                {
                    targetPos = transform.position; // 공격시 목적지를 자신으로 설정(이동x)
                    attackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // 공격 패턴 중 랜덤으로 실행
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
        maxHp = 300f; // 적 체력 설정
        currentHp = 290f; // 적 체력 초기화
        damage = 12f; // 적 공격력 설정 
        bulletSpeed = 20; // 탄환 속도 설정
        maxGroggyCnt = 3; // 최대 그로기 게이지 3개로 설정
        currentGroggyCnt = 0; // 현재 그로기 개수 초기화
        rigidBody.drag = 5f; // 기본 마찰력 설정
        startPos = transform.position;
        moveSpeed = defaultMoveSpeed;
        isPatrol = false;
        attackMode = true;

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
    }

    private void Update()
    {
        // 죽음시 리턴
        if (isDead || !attackMode) return;

        // 디버프 확인
        if (isPurifying && currentHp > 5f) // 늑대 등장 or 정화의 걸음시 오염도 감소(최대 5까지 감소)
            currentHp -= 40f * Time.deltaTime; // 1초에 5 Hp씩 감소

        if (isReadyPeaceMelody && currentHp > 5f) // 평화의 악장 준비파동 피격시 오염도 감소(최대 5까지)
            currentHp -= 2f * Time.deltaTime; // 1초에 2Hp씩 감소

        // 원거리 모드 상태 확인
        if (isRangeMode && !rangeModeStarted)
        {
            StartCoroutine(SpecialPhaseRoutine());
        }

        // 맵 변경 함수 실행 확인
        hpRatio = currentHp / maxHp;
        CheckPollutionPhase(hpRatio);

        if (player == null || isStune || isRangeMode || isSpecialPhase) return;

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

    private IEnumerator Attack0() // 일반 근접 공격
    {
        isAttacking = true;

        Debug.Log("적이 [공격 0]을 준비합니다!");
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        currentAttack = attackObjects[0];
        moveSpeed = 0f;
        LockOnTargetPlayer(); // 플레이어를 바라보게 설정

        Vector3 dirAttack = attackObjects[0].transform.localPosition; // 적 공격 범위 준비
        dirAttack.x = Mathf.Abs(dirAttack.x) * (direction.x > 0 ? 1 : -1);
        attackObjects[0].transform.localPosition = dirAttack;
        
        Vector3 dirScale = attackObjects[0].transform.localScale;
        dirScale.x = Mathf.Abs(dirScale.x) * (direction.x > 0 ? 1 : -1);
        attackObjects[0].transform.localScale = dirScale;

        Vector3 dirReady = hitEffect.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect.transform.localPosition = dirReady;
        hitEffect.SetActive(true);
        yield return new WaitForSeconds(0.4f); // 공격 간격 0.4초

        Debug.Log("적이 소녀를 공격합니다!");
        animator.SetTrigger("Attack"); // Attack 애니메이션 실행 -> 애니메이션에서 자동으로 attackObject 활성화/비활성화
        bossAngerEffect.SetActive(true); // 보스 음파 활성화
        hitEffect.SetActive(false);

        yield return new WaitForSeconds(1.5f); // 다음 행동을 하는데 간격을 둠
        bossAngerEffect.SetActive(false); // 보스 음파 비활성화

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        SetAttackTriggerRange(nextAttackIndex); // 다음 공격 인식 범위 설정
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    private IEnumerator Attack1() // 다른 타이밍 공격 + 그림자 공격
    {
        isAttacking = true;

        Debug.Log("적이 [공격 1]을 준비합니다!");
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
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
        Vector3 shadowPos = transform.transform.localPosition + Vector3.up;
        shadowPos.x += 10f * (direction.x > 0 ? 1 : -1);
        bossShadow.transform.localPosition = shadowPos;

        // 거미 그림자 방향 준비
        Vector3 shadowScale = bossShadow.transform.localScale;
        shadowScale.x = -Mathf.Abs(shadowScale.x) * (direction.x > 0 ? 1 : -1);
        bossShadow.transform.localScale = shadowScale;

        Debug.Log("적이 소녀를 공격합니다!");
        animator.SetTrigger("Attack1"); // Attack 애니메이션 실행 -> 애니메이션에서 자동으로 attackObject 활성화/비활성화
        bossAngerEffect.SetActive(true); // 보스 음파 활성화
        hitEffect_noGroggy.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        bossShadow.SetActive(true);
        yield return new WaitForSeconds(0.6f);

        shadowAttackRange.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        shadowAttackRange.SetActive(false);

        bossAngerEffect.SetActive(false); // 보스 음파 비활성화
        StartCoroutine(ShadowFade(bossShadow, 1f)); // 1초동안 사라지는 효과
        yield return new WaitForSeconds(2f); // 다음 행동을 하는데 간격을 둠

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        SetAttackTriggerRange(nextAttackIndex); // 다음 공격 인식 범위 설정
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    private IEnumerator Attack2()
    {
        isAttacking = true;
        attackCoroutine = null;
        currentAttack = attackObjects[2];
        moveSpeed = 0f;
        LockOnTargetPlayer();

        // 경고 UI 연출
        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition;
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(1f);

        // 공격 모션 및 떠오르는 연출
        Debug.Log("적이 주변에 무작위로 번개를 내리칩니다!");
        animator.SetTrigger("Attack2");
        attackMode = false; // 무적 상태
        bossAngerEffect.SetActive(true); // 보스 음파 활성화
        hitEffect_noGroggy.SetActive(false);

        rigidBody.velocity = Vector3.zero;
        rigidBody.gravityScale = 0f;

        transform.DOMoveY(transform.position.y + 2f, 1f).SetEase(Ease.OutSine); // 위로 부드럽게 떠오르는 기능

        // 번개 경고 활성화
        List<GameObject> shuffled = thunderPositions.OrderBy(x => Random.value).ToList(); // 리스트 순서를 무작위로 섞음
        List<GameObject> slected = shuffled.Take(5).ToList(); // 처음 4개만 사용
        foreach (var pos in slected)
        {
            yield return new WaitForSeconds(0.3f);
            pos.SetActive(true);
            Vector3 thunderPos = new Vector3(pos.transform.position.x, transform.position.y+6, 0);
            StartCoroutine(ActivateThunder(pos, thunderPos));
        }

        yield return new WaitForSeconds(1f);
        transform.DOMoveY(transform.position.y - 2f, 0.8f).SetEase(Ease.OutSine); // 위로 부드럽게 떠오르는 기능
        bossAngerEffect.SetActive(false); // 보스 음파 비활성화

        yield return new WaitForSeconds(2.5f); // 다음 행동까지 대기
        nextAttackIndex = Random.Range(0, attackPatterns.Length);
        SetAttackTriggerRange(nextAttackIndex);
        moveSpeed = defaultMoveSpeed;
        rigidBody.gravityScale = 1f; // 중력 원래대로 복구

        if(!player.GetComponent<PlayerCtrl_R>().isLocked) // 스크립트 진행중이지 않을 때만 AttackMode true로 초기화
            attackMode = true; // 무적 상태 종료
        
        isAttacking = false;
        EvaluateCurrentState();
    }

    private IEnumerator SpecialPhaseRoutine() // 원거리 모드 구현
    {
        Debug.LogWarning("보스가 원거리 공격을 시작합니다!");
        rangeModeStarted = true; // 원거리 공격 시작됨
        moveSpeed = 0f;
        animator.SetTrigger("Push");
        
        foreach (var attack in attackObjects) // 공격 범위 모두 비활성화
        {
            if (attack != null)
                attack.SetActive(false);
        }

        yield return new WaitForSeconds(3f);

        // 등장 연출
        animator.SetTrigger("Appear");
        transform.position = pollutionPhasePos[currnetPhase].position; // 지정한 위치로 이동
        yield return new WaitForSeconds(0.5f);

        // 공격 루프 실행
        animator.SetBool("isRun", false); // 기본적으로 Idle 애니메이션 실행
        while (isRangeMode)
        {
            // 보스 체력 확인
            float hpRatio = currentHp / maxHp;

            StartCoroutine(AttackSpecial()); // 기존의 원거리 공격 재사용
            yield return new WaitForSeconds(7f); // 다음 공격 대기
        }
        moveSpeed = defaultMoveSpeed;
        rangeModeStarted = false; // 원거리 공격 종료됨
    }

    private IEnumerator AttackSpecial()
    {
        isAttacking = true;
        attackCoroutine = null;
        currentAttack = attackObjects[2];
        moveSpeed = 0f;
        LockOnTargetPlayer();

        // 경고 UI 연출
        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition;
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(1f);

        Debug.Log("적이 소녀를 추적하는 다리를 소환합니다!");
        attackMode = false; // 무적 상태
        animator.SetTrigger("AttackSpecial");
        bossAngerEffect.SetActive(true); // 보스 음파 활성화
        hitEffect_noGroggy.SetActive(false);

        rigidBody.velocity = Vector3.zero;
        rigidBody.gravityScale = 0f;
       
        transform.DOMoveY(transform.position.y + 2f, 1f).SetEase(Ease.OutSine); // 위로 부드럽게 떠오르는 기능

        for (int i = 0; i < 4; i++)
        {
            GameObject portal = Instantiate(spiderPortalPrefab, player.transform.position + Vector3.up * 5f, Quaternion.identity);
            portals.Add(portal);
            portal.SetActive(true);

            StartCoroutine(MoveWithPlayer(portal, followDuration));

            yield return new WaitForSeconds(0.7f); // 포탈 생성 간격
        }
        yield return new WaitForSeconds(1f);
        rigidBody.gravityScale = 1f; // 중력 원래대로 복구
        bossAngerEffect.SetActive(false); // 보스 음파 비활성화

        yield return new WaitForSeconds(2.5f); // 다음 행동 대기

        nextAttackIndex = Random.Range(0, attackPatterns.Length);
        SetAttackTriggerRange(nextAttackIndex);
        moveSpeed = defaultMoveSpeed;
        attackMode = true; // 무적 상태 종료
        isAttacking = false;
        EvaluateCurrentState();
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

    private IEnumerator ActivateThunder(GameObject pos, Vector3 thunderPos) // 번개 실행 함수
    {
        yield return new WaitForSeconds(1f);

        GameObject thunder = Instantiate(thunderPrefab, thunderPos, Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        thunder.GetComponent<BoxCollider2D>().enabled = false;
        
        yield return new WaitForSeconds(0.5f);
        Destroy(thunder);
        pos.SetActive(false); // 경고 범위 비활성화
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
            bossAngerEffect.SetActive(false);
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

    void CheckPollutionPhase(float hpRatio) // 오염도에 따른 맵 변경 상태 확인 함수
    {
        if(hpRatio <= pollutionPhases[0] && !isChangingPos)
        {
            isChangingPos = true;
            ActivePollutionPhase(pollutionPhaseIndex[0]);
        }
    }

    private void ActivePollutionPhase(int pollutionPhase) // 오염도 변경 내용 실행 함수
    {
        Debug.LogWarning("[" + pollutionPhase + "] 적이 이동 or 맵을 변경합니다!");
        switch(pollutionPhase)
        {
            case 0:
                // 이동 + 맵 변경, 원거리 모드 x
                StartCoroutine(ChangeMap(0, 0, true));
                break;

            case 1:
                // 이동 only, 원거리 모드 o
                StartCoroutine(TransLateTarget(1, true));
                break;

            case 2:
                // 이동 only, 원거리 모드 x
                StartCoroutine(TransLateTarget(2, false));
                break;

            case 3:
                // 이동 + 맵 변경, 원거리 모드 x
                StartCoroutine(ChangeMap(3, 1, true));
                break;

            case 4:
                // 이동 only, 원거리 모드 o
                StartCoroutine(TransLateTarget(4, true));
                break;

            case 5:
                // 이동 only, 원거리 모드 x
                StartCoroutine(TransLateTarget(5, false));
                break;

            case 6:
                // 이동 + 맵 변경, 원거리 모드 x
                StartCoroutine(ChangeMap(6, 2, true));
                player.GetComponent<PlayerCtrl_R>().ActivatedSealMode(true); // 능력 봉인 (에코가드만 가능)
                break;

            case 7:
                attackMode = false; // 스크립트 진행
                animator.SetBool("isRun", false); // Idle 상태로 변경
                player.GetComponent<PlayerCtrl_R>().ActivatedSealMode(false); // 능력 봉인 해제
                storyObj1.SetActive(true); // 스토리 오브젝트 활성화
                break;
        }
    }

    private IEnumerator TransLateTarget(int pollutionPhase, bool rangeMode) // 다음 위치 이동 함수
    {
        Debug.LogWarning("보스가 [" + pollutionPhaseIndex + "] 위치로 이동합니다!");

        // 사용한 데이터 삭제
        currnetPhase = pollutionPhaseIndex[0]; // 현재 페이즈 저장
        pollutionPhases.RemoveAt(0);
        pollutionPhaseIndex.RemoveAt(0);

        isSpecialPhase = true;

        attackMode = false; // 무적 상태
        moveSpeed = 0f;
        animator.SetTrigger("Push");
        animator.SetTrigger("Disaapear");

        foreach (var attack in attackObjects) // 공격 범위 모두 비활성화
        {
            if (attack != null)
                attack.SetActive(false);
        }
        yield return new WaitForSeconds(1.5f);
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(1.5f);

        // 보스 위치 이동 및 공격 모드 설정
        spriteRenderer.enabled = true;
        animator.SetTrigger("Appear");
        transform.position = pollutionPhasePos[pollutionPhase].position;
        isRangeMode = rangeMode; // 기본 모드 vs 원거리 모드 설정 

        yield return new WaitForSeconds(1.5f);

        moveSpeed = defaultMoveSpeed;
        attackMode = true;
        isChangingPos = false;
        isSpecialPhase = false;
    }

    private IEnumerator ChangeMap(int pollutionPhase, int mapPhase, bool isAttackMode) // 다음 위치 이동 함수
    {
        Debug.LogWarning("보스가 맵을 변경합니다!");

        isSpecialPhase = true;

        // 사용한 데이터 삭제
        currnetPhase = pollutionPhaseIndex[0]; // 현재 페이즈 저장
        pollutionPhases.RemoveAt(0);
        pollutionPhaseIndex.RemoveAt(0);

        attackMode = false; // 무적 상태
        StartCoroutine(PushAttack(0f));
        moveSpeed = 0f;
        animator.SetTrigger("Disaapear");

        yield return new WaitForSeconds(1f);

        // 맵 변경
        transform.position = startPos;
        changMap.Pase(mapPhase);
        yield return new WaitForSeconds(0.1f);
        player.transform.position = playerPhasePos[mapPhase].position;

        // 보스 위치 이동 및 공격 모드 설정
        yield return new WaitForSeconds(1.5f);
        animator.SetTrigger("Appear");
        transform.position = pollutionPhasePos[pollutionPhase].position;
        isRangeMode = false; // 기본 모드 
        yield return new WaitForSeconds(1.5f);


        foreach (var attack in attackObjects) // 공격 범위 모두 비활성화
        {
            if (attack != null)
                attack.SetActive(false);
        }

        if(isAttackMode) // 공격모드일 경우
            attackMode = true; // 무적 상태 해제
        
        isAttackRange = false;
        moveSpeed = defaultMoveSpeed;
        isChangingPos = false;
        isSpecialPhase = false;
    }

    void OnDrawGizmos() // 밀격 범위 표시
    {
        Gizmos.color = Color.red; // 색상 지정
        Gizmos.DrawWireSphere(transform.position, 3); // 원형 범위
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
        else // 사망 반응 
        {
            Debug.LogWarning("적이 포효합니다!");
            StartCoroutine(PushAttack(0.5f));
            hitEffect.SetActive(true); // 피격 이펙트 활성화

            yield return new WaitForSeconds(0.2f);

            hitEffect.SetActive(false); // 피격 이펙트 비활성화
        }
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
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            peaceAttackID = attackArea.attackGlobalID;

            currentHp -= 15f;
            if (currentHp <= 0)
                StartCoroutine(EnemyFade(3f)); // 적 사라짐
            else
            {
                StartCoroutine(Stunned(3f)); // 적 3초 기절
                StartCoroutine(PushAttack(3f)); // 3초후 밀격 공격
            }
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (isStune) return;

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            if (currentHp - 20f >= 5) // 최대 5까지 오염도 감소
                currentHp -= 20f; // 적 오염도 즉시 20 감소 
            else
                currentHp = 5f;

            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir);
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

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
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
