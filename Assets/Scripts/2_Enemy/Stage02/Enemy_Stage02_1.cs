using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage02_1 : EnemyBase // Mage 스크립트
{
    [SerializeField] private GameObject projectile; // 적 탄환 프리팹

    public int angerAttackID = 0; // 분노의 악장 중복 충돌 판정 방지
    public int peaceAttackID = 1; // 평화의 악장 중복 충돌 판정 방지

    private void Start()
    {
        maxHp = 60f; // 적 체력 설정
        currentHp = 30f; // 적 체력 초기화
        damage = 5f; // 적 공격력 설정 
        bulletSpeed = 10f; // 탄환 속도 설정
        maxGroggyCnt = 3; // 최대 그로기 게이지 3개로 설정
        currentGroggyCnt = 0; // 현재 그로기 개수 초기화
        rigidBody.drag = 5f; // 기본 마찰력 설정
        startPos = transform.position;
        moveSpeed = defaultMoveSpeed;
        isPatrol = true;
        attackMode = true;

        ChooseNextPatrolPoint(); // 다음 목적지 설정
        InitializeAttackPatterns(); // 공격 함수 구성 초기화
        
        GameManager.Instance.isReturned = enemyIsReturn; // 적 회귀 상태 설정
        if (GameManager.Instance.isReturned) // 회귀 후, 그로기 슬롯 초기화 
        {
            groggyUI.SetupGroggySpriteGauge(maxGroggyCnt);
        }
    }

    private void Update()
    {
        // 죽음시 리턴
        if (isDead) return;

        // 디버프 확인
        if (isPurifying && currentHp > 5f) // 늑대 등장 or 정화의 걸음시 오염도 감소(최대 5까지 감소)
            currentHp -= 5f * Time.deltaTime; // 1초에 5 Hp씩 감소

        if (isReadyPeaceMelody && currentHp > 5f) // 평화의 악장 준비파동 피격시 오염도 감소(최대 5까지)
            currentHp -= 2f * Time.deltaTime; // 1초에 2Hp씩 감소

        if (!isMoving || player == null || isStune) return;

        if (isPatrol && isMoving && Vector2.Distance(transform.position, targetPos) < 1f) // 패트롤 모드 중 목적지 도착시 
            isMoving = false; // 경계 상태로 전환

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
        };
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

        Debug.Log("적이 공격을 준비합니다!");
        attackCoroutine = null; // 실행 중이었던 코루틴 정리
        currentAttack = attackObjects[0];
        moveSpeed = 0f;
        LockOnTargetPlayer(); // 플레이어를 바라보게 설정

        Vector3 dirReady = hitEffect.transform.localPosition; // 공격 경고 UI 구현
        dirReady.x = Mathf.Abs(dirReady.x) * (direction.x > 0 ? 1 : -1);
        hitEffect.transform.localPosition = dirReady;
        hitEffect.SetActive(true);
        yield return new WaitForSeconds(1f); // 공격 간격 1초

        Debug.Log("적이 탄환을 발사합니다!");

        audioSource.clip = enemySounds[1]; // 음원[탄환]
        audioSource.Play(); // 음원 실행

        animator.SetTrigger("Attack"); // Attack 애니메이션 실행
        hitEffect.SetActive(false);
        yield return new WaitForSeconds(3f); // 다음 행동을 하는데 간격을 둠

        nextAttackIndex = Random.Range(0, attackPatterns.Length); // 다음 공격 결정
        moveSpeed = defaultMoveSpeed;
        isAttacking = false;

        EvaluateCurrentState(); // 프로퍼티 값 초기화(상태에 맞는 행동 수행)
    }

    protected override void HandlerTriggerEnter(Collider2D collision) // 충돌 처리 담당 
    {
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

            currentHp -= 10f;
            if (currentHp <= 0)
            {
                audioSource.clip = enemySounds[2]; // 음원[사라짐]
                audioSource.Play(); // 음원 실행

                StartCoroutine(EnemyFade(3f)); // 적 사라짐
            }
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

            StartCoroutine(Stunned(4f));
        }
        else if (collision.gameObject.CompareTag("WolfPush"))
        {
            if (isStune) return;

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.LogWarning("늑대가 적을 밀쳐냅니다!");
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

            Debug.LogWarning("여러번 나오면 안되는데");
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

            StartCoroutine(Stunned(4f));
        }
        else if (collision.gameObject.CompareTag("WolfPush"))
        {
            if (isStune) return;

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.LogWarning("늑대가 적을 밀쳐냅니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir);
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
