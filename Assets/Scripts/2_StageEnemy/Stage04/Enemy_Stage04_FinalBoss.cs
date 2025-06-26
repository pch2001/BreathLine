using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage04_FinalBoss : EnemyBase // Ghowl 스크립트
{
    [SerializeField] private GameObject AttackRange0; // Enemy 공격 0

    Vector2 direction; // 적이 바라보는 방향
    private InputAction playerInteraction; 
    public int angerAttackID = 0; // 분노의 악장 중복 충돌 판정 방지
    public int peaceAttackID = 1; // 평화의 악장 중복 충돌 판정 방지

    public GameObject baseattack; // 기본 공격

    public Transform possionPoint; // 원거리 공격 위치 지점
    public GameObject arrow; // 원거리 공격용 화살 프리팹

    public GameObject[] attackPoints; //거미 다리공격 위치
    public GameObject[] attackPoints_attack;
    public Transform[] teleportPositions; // 순간이동 위치

    public GameObject thunderObj; // 번개 공격 프리팹
    public GameObject warningZonePrefab; // 경고 구역 프리팹


    public GameObject spawnBoss;


    private void Start()
    {
        maxHp = 100f; // 적 체력 설정
        currentHp = 50f; // 적 체력 초기화
        damage = 20f; // 적 공격력 설정 
        pollutionDegree = 20f; // 적 처치(피격)시 오르는 오염도 설정
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
    }
    int count = 0;
    private void FixedUpdate()
    {
        if (isPurifying && currentHp > 5f) // 늑대 등장 or 정화의 걸음시 오염도 감소(최대 5까지 감소)
            currentHp -= 5f * Time.deltaTime; // 1초에 5 Hp씩 감소

        if (isReadyPeaceMelody && currentHp > 5f) // 평화의 악장 준비파동 피격시 오염도 감소(최대 5까지)
            currentHp -= 2f * Time.deltaTime; // 1초에 2Hp씩 감소

        if (player == null || isStune || isDead) return;

        // 적이 플레이어를 바라보고 있을 경우
        direction = (player.transform.position - transform.position).normalized;
        //if (direction.x > 0)
        //    spriteRenderer.flipX = false;
        //else
        //    spriteRenderer.flipX = true;
        //플레이어 바라보기
        Vector3 scale = transform.localScale;
        scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        //if (!attackMode) return; // 공격 모드일 경우

        // 공격중이지 않을 경우 이동 실행
        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        rigidBody.velocity = velocity; // 적 이동
        //animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // 속도에 따른 애니메이션 제어

        if (isAttacking) return;
        //StartCoroutine("TeleportAndAOE");

        if (count == 7)
        {
            StartCoroutine(TeleportAndAOE());
        }
        else if (count > 5 && count < 7)
        {
            count++;
            StartCoroutine(Attack3());
        }
        else if (count > 2 && count <= 4)
        {
            count++;
            StartCoroutine(Attack2());
        }
        else
        {
            count++;
            StartCoroutine(Attack1());

        }

        // 일정 범위에 도달시 공격 실해
        //if (Vector2.Distance(gameObject.transform.position, player.transform.position) < 3f)
        //{
        //    // attackCoroutine = StartCoroutine(Attack1()); // 실행중 피격시 중지
        //    
        //}

    }

    protected override void HandlerTriggerEnter(Collider2D collision) // 충돌 처리 담당 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            // 피격 중복 판정 방지 코드
            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("분노의 악장이 적을 공격합니다!!"); 
            AttackRange0.SetActive(false);

            if (!attackMode) // attackMode가 비활성화 되어있을 때 피격시
            {
                AttackMode(); // 공격모드 활성화
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (!attackMode) return; // 공격모드가 아닐 경우 평화의 악장 무시

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            peaceAttackID = attackArea.attackGlobalID;

            AttackRange0.SetActive(false);
            currentHp -= 20f;
            if (currentHp <= 0) 
            {
                StartCoroutine(EnemyFade(3f)); // 적 사라짐
            }
            else
                StartCoroutine(Stunned(3f)); // 적 3초 기절
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            AttackRange0.SetActive(false);
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

            Debug.Log("적을 진정시킵니다");
            isPurifying = true;
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            if (!attackMode) return;

            Debug.Log("평화의 악장을 준비합니다! 주변에 잔잔한 파동이 퍼집니다.");
            isReadyPeaceMelody = true; // 평화의 악장 준비 파동 시작
        }
        else if (collision.gameObject.CompareTag("EchoGuard"))
        {
            if (!attackMode || isStune) return;

            StartCoroutine(EchoGuardSuccess(collision));
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
            AttackRange0.SetActive(false);

            if (!attackMode) // attackMode가 비활성화 되어있을 때 피격시
            {
                AttackMode(); // 공격모드 활성화
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (!attackMode) return; // 공격모드가 아닐 경우 평화의 악장 무시

            var attackArea = collision.GetComponent<AttackArea>();
            if (attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            peaceAttackID = attackArea.attackGlobalID;

            AttackRange0.SetActive(false);
            currentHp -= 20f;
            if (currentHp <= 0)
            {
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
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            if (!attackMode) return;

            isReadyPeaceMelody = true;
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

    public void AttackMode() // 공격 모드 활성화
    {
        Debug.Log("적이 플레이어에게 달려듭니다!");
        animator.SetTrigger("Damaged");
        attackMode = true;
        boxCollider.size = new Vector2(0.03f, 0.23f); // 피격 범위 크기 변경 (플레이어 인식 범위 -> 적 충돌 범위)
        boxCollider.offset = new Vector2(0f, 0f); // 피격 범위 위치 변경
        moveSpeed = defaultMoveSpeed;
    }

    private void DeActivateAttackMode() // 공격 모드 해제 구현
    {
        Debug.Log("적이 진정됩니다...");

        attackMode = false;
        animator.SetBool("isRun", false);
        boxCollider.size = new Vector2(1.5f, 1f); // 피격 범위 크기 변경 (적 충돌 범위 -> 플레이어 인식 범위)
        boxCollider.offset = new Vector2(0f, 0f); // 피격 범위 위치 변경

        StartCoroutine(Stunned(3f)); // 3초간 기절
    }

    private IEnumerator EchoGuardSuccess(Collider2D collision) 
    {

        if (currentGroggyCnt < maxGroggyCnt - 1) // 그로기 게이지가 2개 이상 남았을 경우
        {
            Debug.Log("소녀가 적의 공격을 방어해냅니다!");
            groggyUI.AddGroggyState(); // 그로기 스택 증가
            currentGroggyCnt++;
            audioSource.Play(); // 패링 소리 재생
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            EchoGuardPushBack(pushBackDir);
        }
        else // 남은 그로기 게이지가 1개일 경우
        {
            Debug.Log("적이 잠시 그로기 상태에 빠집니다!");

            if (currentHp - 20f >= 5) // 최대 5까지 오염도 감소
                currentHp -= 15f; // 적 오염도 즉시 20 감소 
            else
                currentHp = 5f;
            groggyUI.AddGroggyState(); // 그로기 스택 증가
            audioSource.Play(); // 패링 소리 재생
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir);

            echoGuardEffect.SetActive(true); // 에코가드 성공 이펙트 활성화
            yield return new WaitForSeconds(0.4f);

            echoGuardEffect.SetActive(false); // 에코가드 이펙트 비활성화
        }
    }
    //===============================================================공격 패턴 구현

    public void Attack1Start()
    {
        baseattack.SetActive(true);
        attack attackScript = baseattack.GetComponent<attack>();
        if (attackScript != null)
        {
            attackScript.isAttacking = true; // 공격 상태로 설정
        }
        Debug.Log("공격 실행");
    }

    public void Attack1End()
    {
        baseattack.SetActive(false);
        attack attackScript = baseattack.GetComponent<attack>();
        if (attackScript != null)
        {
            attackScript.isAttacking = false; // 공격 상태로 설정
        }
        Debug.Log("공격 종료");

    }

    IEnumerator Attack1()
    {
        isAttacking = true; // 공격 시작
        moveSpeed = 0f;

        animator.SetTrigger("attack1");
        yield return new WaitForSeconds(1.5f); // 공격 시간

        isAttacking = false;
    }

    IEnumerator Attack2()
    {
        isAttacking = true;
        animator.SetTrigger("attack2");
        yield return new WaitForSeconds(0.1f);

        Instantiate(arrow, possionPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    IEnumerator Attack3()
    {
        isAttacking = true;

        Debug.Log("공격3 시작");
        animator.SetTrigger("Die");
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2 / 5;
            Vector3 spawnOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 2;
            Vector3 spawnPos = transform.position + spawnOffset;

            Instantiate(spawnBoss, spawnPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(4f);

        animator.SetTrigger("Idle");

        yield return new WaitForSeconds(1f);

        isAttacking = false;

    }
    IEnumerator TeleportAndAOE()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);

        animator.SetTrigger("attack3");


        yield return new WaitForSeconds(0.8f);

        // 순간이동 위치 중 랜덤 선택
        Transform targetPos = teleportPositions[0];
        yield return new WaitForSeconds(0.5f);

        transform.position = targetPos.position;

        // 다시 나타나기
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;

        animator.SetTrigger("attack3_1");
        yield return new WaitForSeconds(1.5f);

        //============================================

        attackPoints[0].SetActive(true);
        attackPoints[2].SetActive(true); 
        attackPoints[4].SetActive(true);
        attackPoints[1].SetActive(false);
        attackPoints[3].SetActive(false);
        attackPoints[5].SetActive(false);

        yield return new WaitForSeconds(1f);

        attackPoints_attack[0].SetActive(true);
        attackPoints_attack[2].SetActive(true);
        attackPoints_attack[4].SetActive(true);


        yield return new WaitForSeconds(0.5f);
        attackPoints_attack[0].SetActive(false);
        attackPoints_attack[2].SetActive(false);
        attackPoints_attack[4].SetActive(false);

        yield return new WaitForSeconds(0.3f);

        // 짝수 공격

        attackPoints[0].SetActive(false);
        attackPoints[2].SetActive(false);
        attackPoints[4].SetActive(false);
        attackPoints[1].SetActive(true);
        attackPoints[3].SetActive(true);
        attackPoints[5].SetActive(true);

        yield return new WaitForSeconds(1f);

        attackPoints_attack[1].SetActive(true);
        attackPoints_attack[3].SetActive(true);
        attackPoints_attack[5].SetActive(true);


        yield return new WaitForSeconds(0.5f);
        attackPoints_attack[1].SetActive(false);
        attackPoints_attack[3].SetActive(false);
        attackPoints_attack[5].SetActive(false);

        yield return new WaitForSeconds(0.3f);

        //============================================

        yield return new WaitForSeconds(1f);
        animator.SetTrigger("attack3_2");

        for (int i = 0; i < attackPoints.Length; i++)
        {
            attackPoints[i].SetActive(false);
        }

        yield return new WaitForSeconds(2f);

        isAttacking = false;
    }

}
