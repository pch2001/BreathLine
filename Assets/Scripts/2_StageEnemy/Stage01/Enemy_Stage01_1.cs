using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage01_1 : EnemyBase // Ghowl 스크립트
{
    private InputAction playerInteraction;
    public bool isLooking = false; // 플레이어가 적의 시야에 들어왔는지
    public int angerAttackID = 0; // 분노의 악장 중복 충돌 판정 방지
    public int peaceAttackID = 1; // 평화의 악장 중복 충돌 판정 방지

    [SerializeField] private Enemy_Stage01_2 linkedSpitter; // 스피터 연결

    private void Start()
    {
        maxHp = 60; // 적 체력 설정
        currentHp = 30f; // 적 체력 초기화
        damage = 10f; // 적 공격력 설정 
        pollutionDegree = 5f; // 적 처치(피격)시 오르는 오염도 설정
        maxGroggyCnt = 3; // 최대 그로기 게이지 3개로 설정
        currentGroggyCnt = 0; // 현재 그로기 개수 초기화
        rigidBody.drag = 5f; // 기본 마찰력 설정
        moveSpeed = defaultMoveSpeed;
        attackMode = false; // 기본 공격모드 false
        GameManager.Instance.isReturned = enemyIsReturn; // 적 회귀 상태 설정

        if (GameManager.Instance.isReturned) // 회귀 후, 그로기 슬롯 초기화 
        {
            groggyUI.SetupGroggySpriteGauge(maxGroggyCnt);
        }
        InitializeAttackPatterns();
    }

    private void Update()
    {
        if (isPurifying && currentHp > 5f) // 늑대 등장 or 정화의 걸음시 오염도 감소(최대 5까지 감소)
            currentHp -= 5f * Time.deltaTime; // 1초에 5 Hp씩 감소

        if (isReadyPeaceMelody && currentHp > 5f) // 평화의 악장 준비파동 피격시 오염도 감소(최대 5까지)
            currentHp -= 2f * Time.deltaTime; // 1초에 2Hp씩 감소

        if (!isLooking || player == null || isStune || isDead) return;
        // 적이 플레이어를 바라보고 있을 경우
        Vector2 direction = (player.transform.position - transform.position).normalized;
        if (direction.x > 0)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;

        if (!attackMode) return;
        // 공격 모드 or 스턴 상태가 아닐 경우
        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        rigidBody.velocity = velocity; // 적 이동

        animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // 속도에 따른 애니메이션 제어
    }

    protected override void InitializeAttackPatterns() // 공격 함수 구성 초기화
    {
        attackPatterns = new AttackPattern[] // 공격 패턴 종류 초기화
        {
            // 해당 몬스터는 공격 패턴x / 자폭 공격
        };
    }

    protected override void HandlerTriggerEnter(Collider2D collision) // 충돌 처리 담당 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("분노의 악장이 적을 공격합니다!!");

            if (!attackMode) // attackMode가 비활성화 되어있을 때 피격시
            {
                StartCoroutine(AttackMode()); // 공격모드 활성화
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (attackMode) return; // 공격모드가 아닐 경우 평화의 악장 무시

            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            peaceAttackID = attackArea.attackGlobalID;

            Debug.Log("평화의 악장이 적을 안심시킵니다.");

            currentHp -= 20f;
            if (currentHp <= 0) 
            {
                StartCoroutine(linkedSpitter.EnemyFade(3f)); // 연결된 Spitter 제거
                StartCoroutine(EnemyFade(3f)); // 적 사라짐
            }
            else
                StartCoroutine(Stunned(3f)); // 적 3초 기절
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

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
            if (!attackMode) return;
            isPurifying = true;
            Debug.Log("적을 진정시킵니다");
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            if (attackMode) return;

            Debug.Log("평화의 악장을 준비합니다! 주변에 잔잔한 파동이 퍼집니다.");
            isReadyPeaceMelody = true; // 평화의 악장 준비 파동 시작
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (!isLooking)
            {
                isLooking = true; // 플레이어가 적의 시야에 들어왔을때 반응
                Debug.Log("적이 플레이어를 바라봅니다.");
            }

            if (!attackMode || isStune) return; // 공격모드가 아닌 상황 or 스턴 상황에서 충돌시 무시

            if (GameManager.Instance.isReturned && player.GetComponent<PlayerSkill_R>().isEchoGuarding) // 플레이어 에코가드시 그로기 증가
            {
                Debug.LogWarning("에코 가드 함수 실행하려고 하는디?");
                StartCoroutine(EchoGuardSuccess(collision));
                return;
            }
            Debug.Log("적 플레이어에게 피해를 입힙니다!");
            StartCoroutine(Die());
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
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("분노의 악장이 적을 공격합니다!!");

            if (!attackMode) // attackMode가 비활성화 되어있을 때 피격시
            {
                StartCoroutine(AttackMode()); // 공격모드 활성화
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (attackMode) return; // 공격모드가 아닐 경우 평화의 악장 무시

            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            peaceAttackID = attackArea.attackGlobalID;

            Debug.Log("평화의 악장이 적을 안심시킵니다.");

            currentHp -= 20f;
            if (currentHp <= 0)
            {
                StartCoroutine(linkedSpitter.EnemyFade(3f)); // 연결된 Spitter 제거
                StartCoroutine(EnemyFade(3f)); // 적 사라짐
            }
            else
                StartCoroutine(Stunned(3f)); // 적 3초 기절
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir);
            StartCoroutine(Stunned(3f));
        }
        else if (collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep"))
        {
            if (!attackMode) return;
            isPurifying = true;
            moveSpeed = 2f;
        }
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep")) && attackMode)
        {
            Debug.Log("적이 범위를 벗어납니다");
            moveSpeed = defaultMoveSpeed; // 기존 속도
            isPurifying = false;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            if (attackMode) return;

            Debug.Log("평화의 악장을 준비를 마칩니다!");
            isReadyPeaceMelody = false; // 평화의 악장 준비 해제
        }
        else if (!isBossCreated && collision.gameObject.CompareTag("Player") && isLooking && !attackMode && !isDead)
        {
            isDead = true;
            StartCoroutine(linkedSpitter.Die()); // 연결된 Spitter 제거
            StartCoroutine(DieAfterDelay(2f)); // 2초 지연후 Die 함수 실행
        }
    }

    private IEnumerator Die() // 1스테이지 일반 몬스터의 특성(자폭)에 필요
    {
        Debug.Log("적이 고통스럽게 소멸합니다...");
        moveSpeed = 0f;
        animator.SetTrigger("Die");
        dieEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        dieEffect.SetActive(false);
        gameObject.SetActive(false); // 적 비활성화
    }

    public IEnumerator AttackMode() // 공격 모드 활성화
    {
        Debug.Log("적이 울부짖습니다!");
        animator.SetTrigger("Attack");
        linkedSpitter.moveSpeed = 0f;
        isLooking = true;
        attackMode = true;
        moveSpeed = 0f;
        yield return new WaitForSeconds(1f);

        Debug.Log("적이 플레이어에게 달려듭니다!");
        gameObject.SetActive(false);
        gameObject.SetActive(true); // 오브젝트를 껐다 켜 충돌 판정 초기화

        StartCoroutine(linkedSpitter.Die()); // 연결된 Spitter 제거
        boxCollider.size = new Vector2(0.03f, 0.23f); // 피격 범위 크기 변경 (플레이어 인식 범위 -> 적 충돌 범위)
        boxCollider.offset = new Vector2(0f, 0f); // 피격 범위 위치 변경
        moveSpeed = defaultMoveSpeed;
    }

    private void DeActivateAttackMode() // 공격 모드 해제 구현
    {
        Debug.Log("적이 진정됩니다...");

        isLooking = false;
        attackMode = false;
        animator.SetBool("isRun", false);
        boxCollider.size = new Vector2(1.5f, 1f); // 피격 범위 크기 변경 (적 충돌 범위 -> 플레이어 인식 범위)
        boxCollider.offset = new Vector2(0f, 0f); // 피격 범위 위치 변경

        StartCoroutine(Stunned(3f)); // 3초간 기절
    }

    private IEnumerator DieAfterDelay(float delay) // 지연 사망시
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(Die());
    }
}
