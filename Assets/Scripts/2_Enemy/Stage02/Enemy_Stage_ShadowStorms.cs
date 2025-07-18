using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage_ShadowStorms : BossBase // Mage 스크립트
{
    public int angerAttackID = 0; // 분노의 악장 중복 충돌 판정 방지
    public int peaceAttackID = 1; // 평화의 악장 중복 충돌 판정 방지

    private int specialPhaseCnt = 1;
    private int spawnEnemyCnt = 0; // 소환할 몬스터 수
    private bool isHealMode = false; // 힐 상태인지 여부
    
    [SerializeField] private List<GameObject> beamAttacks; // 공격2의 빔 오브젝트 리스트
    [SerializeField] private List<GameObject> phase2_Monster; // 특수 패턴시 소환할 2스테이지 적 리스트

    private void Start()
    {
        maxHp = 300f; // 적 체력 설정
        currentHp = 150f; // 적 체력 초기화
        damage = 10f; // 적 공격력 설정 
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

        foreach(GameObject obj in phase2_Monster)
    {
            EnemyBase enemy = obj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.RequestEnemyDie += CountSpawnEnemy;
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
        // 특수 패턴시 회복 구현
        float hpRatio = currentHp / maxHp;
        if (isHealMode)
        {    
            if (hpRatio > 0.5f && hpRatio <= 1f) // 오염도를 낮추는 회복 실행
            {
                if (hpRatio <= 0.51f)
                {
                    isHealMode = false;
                    spawnEnemyCnt = 0;
                    CountSpawnEnemy();
                }
                else
                    currentHp -= 5f * Time.deltaTime; // 오염도가 75%이상에서 실행시 1초에 5씩 오염도 감소
            }
            else // 오염도를 높이는 회복 실행
            {
                if (hpRatio >= 0.5f)
                {
                    isHealMode = false;
                    spawnEnemyCnt = 0;
                    CountSpawnEnemy();
                }
                else
                    currentHp += 5f * Time.deltaTime; // 오염도가 25%이하에서 실행시 1초에 5씩 오염도 증가
            }
        }

        // 죽음시 리턴
        if (isDead || !attackMode) return;

        // 특수 패턴 시작 확인
        if (specialPhaseCnt >= 1 && !isSpecialPhase && (hpRatio < 0.25f || hpRatio > 0.75f))
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
                attackTriggerRange.transform.localScale = new Vector3(2f, 0.2f, 0.1f);
                break;
            case 2:
                attackTriggerRange.transform.localScale = new Vector3(1.2f, 0.2f, 0.1f);
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

        audioSource.clip = enemySounds[0]; // 음원[경고]
        audioSource.Play(); // 음원 실행

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
        yield return new WaitForSeconds(0.8f); // 공격 간격 0.8초

        Debug.Log("적이 소녀를 공격합니다!");
        animator.SetTrigger("Attack"); // Attack 애니메이션 실행 -> 애니메이션에서 자동으로 attackObject 활성화/비활성화
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

        audioSource.clip = enemySounds[0]; // 음원[경고]
        audioSource.Play(); // 음원 실행   

        Debug.Log("적이 [공격 0]을 준비합니다!"); 
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        currentAttack = null;
        moveSpeed = 0f;
        animator.SetTrigger("Charge");
        LockOnTargetPlayer(); // 플레이어를 바라보게 설정

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);
        yield return new WaitForSeconds(1f); // 공격 간격 1초

        audioSource.clip = enemySounds[2]; // 음원[경고]
        audioSource.Play(); // 음원 실행

        Debug.Log("적이 탄환을 발사합니다!");
        animator.SetTrigger("Attack1"); // Attack 애니메이션 실행 -> 애니메이션에서 자동으로 attackObject 활성화/비활성화
        hitEffect_noGroggy.SetActive(false);
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

        audioSource.clip = enemySounds[0]; // 음원[경고]
        audioSource.Play(); // 음원 실행

        Debug.Log("적이 [공격 2]을 준비합니다!");
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        currentAttack = null;
        moveSpeed = 0f;
        animator.SetTrigger("Charge");
        LockOnTargetPlayer(); // 플레이어를 바라보게 설정

        Vector3 dirReady = hitEffect_noGroggy.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect_noGroggy.transform.localPosition = dirReady;
        hitEffect_noGroggy.SetActive(true);

        foreach(var beam in beamAttacks)
        {
            Vector3 pos = beam.transform.localPosition;
            pos.x = Mathf.Abs(pos.x) * (direction.x > 0 ? 1 : -1);
            beam.transform.localPosition = pos; 
        }
        yield return new WaitForSeconds(1.2f); // 공격 간격 1.2초

        Debug.Log("적이 공격을 시작합니다!");

        audioSource.clip = enemySounds[3]; // 음원[경고]
        audioSource.Play(); // 음원 실행

        animator.SetTrigger("Attack2"); // Attack 애니메이션 실행 -> 애니메이션에서 자동으로 attackObject 활성화/비활성화
        hitEffect_noGroggy.SetActive(false);
        yield return new WaitForSeconds(2f); // 다음 행동을 하는데 간격을 둠

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        SetAttackTriggerRange(nextAttackIndex); // 다음 공격 인식 범위 설정
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    public void ActiveBeamAttack(int index)
    {
        beamAttacks[index].SetActive(true);
    }

    public void DeActiveBeamAttack(int index)
    {
        beamAttacks[index].SetActive(false);
    }

    private IEnumerator SpecialPhaseRoutine() // 특수 패턴 구현
    {
        Debug.LogWarning("적이 특수 패턴을 시작합니다! + 소리 추가");
        
        moveSpeed = 0f;
        isSpecialPhase = true;
        specialPhaseCnt--;
        animator.SetTrigger("Damaged");
        attackMode = false; // 잠시 무적
        yield return new WaitForSeconds(3f);

        // 사라지는 연출
        animator.SetTrigger("Transform"); 
        yield return new WaitForSeconds(0.3f);

        // 등장 연출

        audioSource.clip = enemySounds[0]; // 음원[경고]
        audioSource.Play(); // 음원 실행

        transform.position = startPos;
        animator.SetTrigger("Transform");
        yield return new WaitForSeconds(0.5f);

        // 회복 모드 + 몬스터 활성화
        animator.SetTrigger("Charge");
        isHealMode = true; // 회복모드 실행

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
            isSpecialPhase = false; // 특수 패턴 종료
        }
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
        
        storyObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false); // 적 비활성화
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
            {
                audioSource.clip = enemySounds[4]; // 음원[사라짐]
                audioSource.Play(); // 음원 실행

                StartCoroutine(EnemyFade(3f)); // 적 사라짐
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

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            if (currentHp - 20f >= 5) // 최대 5까지 오염도 감소
                currentHp -= 20f; // 적 오염도 즉시 20 감소 
            else
                currentHp = 5f;

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
            {
                audioSource.clip = enemySounds[4]; // 음원[사라짐]
                audioSource.Play(); // 음원 실행

                StartCoroutine(EnemyFade(3f)); // 적 사라짐
            }
            else
                StartCoroutine(Stunned(3f)); // 적 3초 기절
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (isStune) return;

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
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
