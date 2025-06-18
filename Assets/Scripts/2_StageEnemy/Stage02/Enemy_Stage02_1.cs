using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy_Stage02_1 : EnemyBase // Mage 스크립트
{
    private InputAction playerInteraction;
    public bool isLooking = false; // 플레이어가 적의 시야에 들어왔는지
    public int angerAttackID = 0; // 분노의 악장 중복 충돌 판정 방지
    public int peaceAttackID = 1; // 평화의 악장 중복 충돌 판정 방지

    [SerializeField] private float patrolRange = 5f; // 패트롤 최대 반경
    [SerializeField] private float patrolMinRange = 3f; // 패트롤 최소 반경
    [SerializeField] private float patrolSpeed = 3f; // 패트롤 속도

    [SerializeField] private GameObject projectile; // 적 탄환 프리팹
    [SerializeField] private float fireDelay = 2f; // 발사 간격
    private float bulletSpeed = 15f; // 탄환 속도

    private Vector2 direction; // 적 이동 방향
    private Vector2 startPos; // 시작 위치 저장
    public Vector2 targetPos; // 목표 위치 저장
    private bool isMoving = true; // 이동중인지 확인(패트롤 중 멈춤 표현)

    private void Start()
    {
        maxHp = 100f; // 적 체력 설정
        currentHp = 50f; // 적 체력 초기화
        damage = 20f; // 적 공격력 설정 
        maxGroggyCnt = 3; // 최대 그로기 게이지 3개로 설정
        currentGroggyCnt = 0; // 현재 그로기 개수 초기화
        rigidBody.drag = 5f; // 기본 마찰력 설정
        moveSpeed = defaultMoveSpeed;
        attackMode = false; // 기본 공격모드 false
        GameManager.Instance.isReturned = enemyIsReturn; // 적 회귀 상태 설정

        startPos = transform.position;
        ChooseNextPatrolPoint(); // 다음 목적지 설정

        if (GameManager.Instance.isReturned) // 회귀 후, 그로기 슬롯 초기화 
        {
            groggyUI.SetupGroggySpriteGauge(maxGroggyCnt);
        }
    }

    private void Update()
    {
        // 디버프 확인
        if ((moveSpeed == 2 || isReadyPeaceMelody) && currentHp > 5f) // 늑대 등장 or 정화의 걸음시 오염도 감소(최대 5까지 감소)
            currentHp -= 5f * Time.deltaTime; // 1초에 5 Hp씩 감소

        if (isReadyPeaceMelody && currentHp > 5f) // 평화의 악장 준비파동 피격시 오염도 감소(최대 5까지)
            currentHp -= 2f * Time.deltaTime; // 1초에 2Hp씩 감소
        
        // 적의 인식 범위 설정
        if (Vector2.Distance(transform.position, player.transform.position) < 4f)
            AttackMode(); // 공격모드 활성화

        // 패트롤 중 이동시 or 사망시 이동 업데이트 X
        if (!isMoving || player == null || isStune || isDead) return;

        // 패트롤 구현
        if (!attackMode) // 공격모드가 아닐 때 패트롤 실행
        {
            Vector2 velocity = new Vector2(direction.x * patrolSpeed, rigidBody.velocity.y);
            rigidBody.velocity = velocity; // 적 이동
            animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // 속도에 따른 애니메이션 제어


            if (Vector2.Distance(transform.position, targetPos) < 1f)
            {
                StartCoroutine(PauseBeforeNextMove()); // 다음 패트롤 전 잠시 멈춤
            }
        }
        else // 공격 모드 중 
        {
            if (!isAttacking) // 발사 중이지 않을 때 실행(중복 실행 방지)
            {
                direction = (player.transform.position - transform.position).normalized;
                if (direction.x > 0)
                    spriteRenderer.flipX = false;
                else
                    spriteRenderer.flipX = true;

                attackCoroutine = StartCoroutine(EnemyAttack()); // 공격함수 실행
            }
        }


    }

    protected override void HandlerTriggerEnter(Collider2D collision) // 충돌 처리 담당 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == angerAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            angerAttackID = attackArea.attackGlobalID;

            Debug.Log("분노의 악장이 적을 공격합니다!!");
            attackMode = true;
            StartCoroutine(Damaged());


        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (!attackMode) return; 

            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            peaceAttackID = attackArea.attackGlobalID;

            currentHp -= 10f;
            if (currentHp <= 0)
                StartCoroutine(EnemyFade(3f)); // 적 사라짐
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

            Debug.Log("적을 진정시킵니다");
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {

            isReadyPeaceMelody = true; // 평화의 악장 준비 파동 시작
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (!isLooking)
            {
                isLooking = true; // 플레이어가 적의 시야에 들어왔을때 반응
                Debug.Log("적이 플레이어를 바라봅니다.");
            }
        }
        else if (collision.gameObject.CompareTag("EchoGuard"))
        {
            if (!attackMode || isStune) return;

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
                groggyUI.AddGroggyState(); // 그로기 스택 증가
                audioSource.Play(); // 패링 소리 재생
                float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
                PushBack(pushBackDir);
            }
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
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
                AttackMode(); // 공격모드 활성화
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (!attackMode) return;

            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == peaceAttackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
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
            if (!attackMode || isStune) return;

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir);
            StartCoroutine(Stunned(3f));
        }
        else if (collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep"))
        {
            if (!attackMode) return;

            moveSpeed = 2f;
        }
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep")) && attackMode)
        {
            Debug.Log("적이 범위를 벗어납니다");
            moveSpeed = defaultMoveSpeed; // 기존 속도
        }
        else if (collision.gameObject.CompareTag("PeaceWaiting"))
        {
            if (attackMode) return;

            Debug.Log("평화의 악장을 준비를 마칩니다!");
            isReadyPeaceMelody = false; // 평화의 악장 준비 해제
        }else if (collision.gameObject.CompareTag("Player"))
        {
            DeActivateAttackMode(); // 공격모드 비활성화
            targetPos = startPos;
        }
    }

    public IEnumerator EnemyAttack() // 적 공격 함수
    {
        isAttacking = true; // 중복 공격 방지 플래그

        Debug.Log("적이 공격을 준비합니다.");
        animator.SetBool("isRun", false); // Idle 상태로 변경
        yield return new WaitForSeconds(1f); // 1초 대기

        Debug.Log("적이 탄환을 발사합니다!");
        animator.SetTrigger("Attack"); // Attack 애니메이션 실행
        GameObject enemyProjectile = Instantiate(projectile, transform.position, Quaternion.identity);

        Rigidbody2D rb = enemyProjectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 bulletDirection = new Vector2(direction.x, 0f).normalized;
            Vector2 spawnPosition = (Vector2)transform.position + bulletDirection * 1f;
            enemyProjectile.transform.position = spawnPosition;
            rb.velocity = bulletDirection * bulletSpeed;
        }
        yield return new WaitForSeconds(1f);

        isAttacking = false; // 다시 공격 가능
        attackCoroutine = null; // 코루틴 정리
    }

    public IEnumerator Die() // 1스테이지 일반 몬스터의 특성(자폭)에 필요
    {
        Debug.Log("적이 고통스럽게 소멸합니다...");
        moveSpeed = 0f;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        animator.SetTrigger("Die");
        dieEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        dieEffect.SetActive(false);
        gameObject.SetActive(false); // 적 비활성화
    }

    private void AttackMode() // 공격 모드 활성화
    {
        Debug.Log("적이 플레이어에게 달려듭니다!");
        isLooking = true;
        attackMode = true;
        boxCollider.size = new Vector2(0.03f, 0.23f); // 피격 범위 크기 변경 (플레이어 인식 범위 -> 적 충돌 범위)
        boxCollider.offset = new Vector2(0f, 0f); // 피격 범위 위치 변경
    }

    private void DeActivateAttackMode() // 공격 모드 해제 구현
    {
        Debug.Log("적이 진정됩니다...");

        attackMode = false;
        animator.SetBool("isRun", false);
        boxCollider.size = new Vector2(1.5f, 1f); // 피격 범위 크기 변경 (적 충돌 범위 -> 플레이어 인식 범위)
        boxCollider.offset = new Vector2(0f, 0f); // 피격 범위 위치 변경
    }

    private void ChooseNextPatrolPoint() // 패트롤 다음 이동 목표지점 설정
    {
        float randomX;

        if (Random.value < 0.5f) // 50%확률로 이동 방향 설정
        {
            // 왼쪽 방향: -patrolRange ~ -patrolMinRange
            randomX = Random.Range(-patrolRange, -patrolMinRange);
        }
        else
        {
            // 오른쪽 방향: patrolMinRange ~ patrolRange
            randomX = Random.Range(patrolMinRange, patrolRange);
        }

        targetPos = new Vector2(startPos.x + randomX, transform.position.y);
        direction = (targetPos - (Vector2)transform.position).normalized; // 패트롤 할 방향 설정

        if (direction.x > 0)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;

        Debug.Log("StartPos: " + startPos);
        Debug.Log("Direction: " + direction);
        Debug.Log("TargetPos: " + targetPos);
    }

    private IEnumerator PauseBeforeNextMove()
    {
        isMoving = false;
        patrolSpeed = 0f;
        yield return new WaitForSeconds(1f); // 1초 대기
        ChooseNextPatrolPoint(); // 다음 목표 지점 설정
        isMoving = true;
        patrolSpeed = 2f;
    }
}
