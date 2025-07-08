using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage_BloodKing : BossBase // Mage 스크립트
{
    public int angerAttackID = 0; // 분노의 악장 중복 충돌 판정 방지
    public int peaceAttackID = 1; // 평화의 악장 중복 충돌 판정 방지

    private int specialPhaseCnt = 1;

    [SerializeField] private List<GameObject> phase1_Ghouls; // 특수 패턴시 소환할 구울 리스트
    [SerializeField] private List<GameObject> phase1_Spitters; // 특수 패턴시 소환할 스피터 리스트

    private void Start()
    {
        maxHp = 300f; // 적 체력 설정
        currentHp = 150f; // 적 체력 초기화
        damage = 15f; // 적 공격력 설정 
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
    }

    private void Update()
    {
        // 죽음시 리턴
        if (isDead || !attackMode) return;

        // 특수 패턴 시작 확인
        float hpRatio = currentHp / maxHp;
        if (specialPhaseCnt >= 1 && !isSpecialPhase && (hpRatio <= 0.25f || hpRatio >= 0.75f))
        {
            StartCoroutine(SpecialPhaseRoutine()); return;
        }

        // 디버프 확인
        if (isPurifying && currentHp > 5f) // 늑대 등장 or 정화의 걸음시 오염도 감소(최대 5까지 감소)
            currentHp -= 5f * Time.deltaTime; // 1초에 5 Hp씩 감소

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

    private void LockOnTargetPlayer() // 플레이어 위치를 목표지점으로 설정
    {
        targetPos = player.transform.position;
        direction = (targetPos - (Vector2)transform.position).normalized; // 패트롤 할 방향 설정

        if (direction.x > 0)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    private IEnumerator Attack0()
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

        Vector3 dirReady = hitEffect.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect.transform.localPosition = dirReady;
        hitEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f); // 공격 간격 0.5초

        Debug.Log("적이 소녀를 공격합니다!");
        animator.SetTrigger("Attack"); // Attack 애니메이션 실행
        hitEffect.SetActive(false);
        yield return new WaitForSeconds(1.5f); // 다음 행동을 하는데 간격을 둠

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        SetAttackTriggerRange(nextAttackIndex); // 다음 공격 인식 범위 설정
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;
        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    private IEnumerator Attack1()
    {
        isAttacking = true;

        Debug.Log("적이 [공격 1]을 준비합니다!");
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        currentAttack = attackObjects[1];
        moveSpeed = 0f;
        LockOnTargetPlayer(); // 플레이어를 바라보게 설정

        Vector3 dirAttack = attackObjects[1].transform.localPosition; // 적 공격 범위 준비
        dirAttack.x = Mathf.Abs(dirAttack.x) * -(direction.x > 0 ? 1 : -1);
        attackObjects[1].transform.localPosition = dirAttack;

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(0.7f); // 공격 간격 0.7초

        Debug.Log("적이 소녀를 공격합니다!");
        animator.SetTrigger("Attack1"); // Attack1 애니메이션 실행
        hitEffect_noGroggy.SetActive(false);

        Vector2 targetPos = transform.position + new Vector3(direction.x * 7f, 0f); // 공격 목적지 설정
        StartCoroutine(MoveToTarget(transform.position, targetPos, 0.1f)); // 적 0.1초 동안 이동 공격
        attackCoroutine = null; // 코루틴 정리
        yield return new WaitForSeconds(1.5f); // 다음 행동을 하는데 간격을 둠

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        SetAttackTriggerRange(nextAttackIndex); // 다음 공격 인식 범위 설정
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;
        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    private IEnumerator Attack2()
    {
        isAttacking = true;
        attackMode = false; // 잠시 무적 상태
        Debug.Log("적이 [공격 2]을 준비합니다!");

        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        currentAttack = attackObjects[2];
        moveSpeed = 0f;
        animator.SetTrigger("Charge"); // 이동 공격 전 차징
        LockOnTargetPlayer(); // 플레이어를 바라보게 설정

        Vector3 dirAttack = attackObjects[2].transform.localPosition; // 적 공격 범위 준비
        dirAttack.x = Mathf.Abs(dirAttack.x) * -(direction.x > 0 ? 1 : -1);
        attackObjects[2].transform.localPosition = dirAttack;

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(1.8f);

        // 적 사라짐 구현
        attackMode = true; // 무적 상태 해제
        hitEffect_noGroggy.SetActive(false);
        groggyUIObject.SetActive(false);
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;

        yield return new WaitForSeconds(3f);
        groggyUIObject.SetActive(true);
        Debug.Log("적이 소녀를 공격합니다!");
        animator.SetTrigger("Attack2"); // Attack2 애니메이션 실행
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;

        transform.position = player.transform.position + Vector3.up * 7f;
        yield return new WaitForSeconds(0.3f);

        Vector3 targetPos = new Vector3(transform.position.x, player.transform.position.y, transform.position.z); // 공격 목적지 설정
        StartCoroutine(MoveToTarget(transform.position, targetPos, 0.1f)); // 적 0.1초 동안 이동 공격

        attackCoroutine = null; // 코루틴 정리
        yield return new WaitForSeconds(1.5f); // 다음 행동을 하는데 간격을 둠

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        SetAttackTriggerRange(nextAttackIndex); // 다음 공격 인식 범위 설정
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;
        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    private IEnumerator SpecialPhaseRoutine() // 특수 패턴 구현
    {
        Debug.LogWarning("적이 특수 패턴을 시작합니다! + 소리 추가");
        moveSpeed = 0f;
        isSpecialPhase = true;
        specialPhaseCnt--;

        yield return new WaitForSeconds(3f);

        // 사라지는 연출
        animator.SetTrigger("Transform"); 
        yield return new WaitForSeconds(0.7f);
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;

        // 몬스터 활성화
        foreach(GameObject monster in phase1_Ghouls)
        {
            monster.SetActive(true);
        }
        foreach (GameObject monster in phase1_Spitters)
        {
            monster.SetActive(true);
        }
        yield return new WaitForSeconds(20f); // 몬스터 20초간 등장
        
        // 보스 재등장 및 자폭 명령
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
        foreach (GameObject monster in phase1_Ghouls)
        {
            if(monster.activeSelf) // 해당 적이 켜져있을 경우 공격 모드 실행
                StartCoroutine(monster.GetComponent<Enemy_Stage01_1>().AttackMode()); // 연결된 구울 AttackMode 실행
        }
        animator.SetTrigger("Appear");
        moveSpeed = defaultMoveSpeed;
        yield return new WaitForSeconds(2f);
        
        isSpecialPhase = false; // 특수 패턴 종료
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
