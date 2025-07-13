using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EnemyBase : MonoBehaviour
{
    protected GameObject player; // 플레이어 오브젝트 확인용
    protected Rigidbody2D rigidBody;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;
    protected AudioSource audioSource;

    public GameObject enemySight; // 적 경계 범위 
    public Transform enemyHpGauge; // 적 Hp(오염도) UI
    public GameObject echoGuardEffect; // 에코가드 성공 이펙트
    public GameObject hitEffect; // 공격 전조 이펙트(그로기 up)
    public GameObject hitEffect_noGroggy; // 공격 전조 이펙트(그로기x)
    public GameObject enemyFadeEffect; // 사라질 때 이펙트
    public GameObject dieEffect; // Die 이펙트
    public GameObject groggyUIObject; // 그로기 UI 오브젝트
    public EnemyGroggyUI groggyUI; // 그로기 UI 스크립트
    
    protected delegate IEnumerator AttackPattern(); // 공격 함수 종류 저장
    protected AttackPattern[] attackPatterns; // 저장할 Attack 코루틴 함수 배열
    public List<GameObject> attackObjects = new List<GameObject>(); // 공격패턴 오브젝트 리스트
    protected GameObject currentAttack;
    private Color originalColor; // 현재 SpriteRender 색상 저장
    private Color currentColor;
    protected Coroutine attackCoroutine; // 상시 공격 탈출 코루틴
    protected Coroutine rangeAttackCoroutine; // 범위 공격 탈출 코루틴

    protected float pollutionDegree; // 처치시 오염도 오르는 정도
    protected float pollutionResist = 1; // 오염도 감소 비율
    protected bool isAttacking = false; // 공격 간격 제한 변수
    protected Vector2 direction; // 적 이동 방향
    protected Vector2 startPos; // 시작 위치 저장
    public Vector2 targetPos; // 목표 위치 저장

    [SerializeField] private float patrolRange = 5f; // 패트롤 최대 반경
    [SerializeField] private float patrolMinRange = 3f; // 패트롤 최소 반경
    [SerializeField] protected AudioClip[] enemySounds; // 적 실행 음원 배열

    public float damage; // 적 공격력
    public int maxGroggyCnt; // 최대 그로기 게이지 개수
    public int currentGroggyCnt; // 현재 그로기 게이지 개수
    public bool attackMode = false; // 적 공격 상태 여부(경계 <-> 추격)
    public bool isStune = false; // 스턴 상태 여부
    public bool isDead = false; // 죽음 여부
    public bool enemyIsReturn; // // 적 회귀 상태 설정
    public bool isPurifying = false; // 정화 중인지(늑대 등장, 정화의 걸음)
    public bool isReadyPeaceMelody = false; // 평화의 멜로디 준비중인지 (준비파동 계산)
    public int nextAttackIndex; // 다음 실행할 공격 인덱스
    public bool isBoss; // 보스 몬스터인지 설정
    public float bulletSpeed; // 투사체 이동 속도
    public bool isBossCreated = false; // 보스로 생성된 적인지
    public event Action RequestEnemyDie; // 보스전에 소환된 적 사망 알림 이벤트

    public float maxHp; // 적 최대 HP (최대 오염도)
    [SerializeField] private float _currentHp;
    public float currentHp // 현재 적 HP (현재 오염도)
    {
        get => _currentHp;
        set
        {
            _currentHp = Mathf.Clamp(value, 0f, maxHp);
            UpdateHpGauge(); // hp 게이지 업데이트
        }
    }

    public float defaultMoveSpeed; // 적 기본 이동속도
    [SerializeField] private float _moveSpeed;
    public float moveSpeed // 적 현재 이동속도
    {
        get => _moveSpeed;
        set => _moveSpeed = Mathf.Clamp(value, 0f, defaultMoveSpeed);
    }


    // 적 이동 알고리즘 구현

    [SerializeField] protected bool _isPatrol; // 패트롤 모드인지 공격모드인지 여부
    public virtual bool isPatrol
    {
        get => _isPatrol;
        set
        {
            _isPatrol = value; // 변경된 값 적용

            if(isDead || isStune) return; // 죽음시 리턴

            if (_isPatrol) // Patrol Mode로 변경할 경우
            {
                isMoving = false; // 패트롤 모드 - 경계 상태부터 실행
            }
            else // Attack Mode로 변경할 경우
            {
                isAttackRange = false; // 공격 모드 - 플레이어 추격 상태부터 실행
            }
        }
    }

    [SerializeField] private bool _isMoving; // 패트롤 중 현재 이동중인지 경계중인지
    public bool isMoving
    {
        get => _isMoving;
        set
        {
            _isMoving = value; // 변경된 값 적용
            
            if (isDead || isStune) return; // 죽음시 리턴

            if (isMoving) // 패트롤 모드 - 이동 상태 함수 실행
            {
                MoveToTarget(); // 범위 내 랜덤 설정한 위치를 향해 이동
            }
            else // 패트롤 모드 - 경계 상태 함수 실행
            {
                StartCoroutine(LookAround()); // 일정 시간 멈춰 주변을 확인
            }
        }
    }

    [SerializeField] protected bool _isAttackRange; // 현재 공격 범위 안에 있는지 여부
    public virtual bool isAttackRange
    {
        get => _isAttackRange;
        set
        {
            _isAttackRange = value; // 변경된 값 적용

            if (isDead || isStune) return; // 죽음시 리턴

            if (_isAttackRange) // 공격 모드 - 공격 실행 함수
            {
                if (!isAttacking) // 공격 중복 실행 방지
                {
                    targetPos = transform.position; // 공격시 목적지를 자신으로 설정(이동x)

                    if (nextAttackIndex == 0)
                    {
                        attackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // 0번째 공격(파란색)은 공격 취소 가능하도록 설정
                    }
                    else if (nextAttackIndex == 2)
                    {
                        rangeAttackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // 2번째 공격(범위 공격)은 스크립트시 취소 가능하도록 설정(attackMode 변경 방지)
                    }
                    else
                        StartCoroutine(attackPatterns[nextAttackIndex]()); // 다른 공격(빨간색)은 공격 취소되지 않음
                }
                else
                {
                    moveSpeed = 0f;
                }
            }
        }
        // 공격 모드 - 적 추격 상태는 Update 함수에서 구현
    }


    protected void MoveToTarget() // 패트롤 모드 - 이동 상태 함수
    {
        moveSpeed = defaultMoveSpeed; // 이동 복구
    }

    protected IEnumerator LookAround() // 패트롤 모드 - 경계 상태 함수
    {
        moveSpeed = 0f;

        yield return new WaitForSeconds(1f); // 1초 대기
        ChooseNextPatrolPoint(); // 다음 목표 지점 설정

        isMoving = true;
    }

    protected void ChooseNextPatrolPoint() // 패트롤 다음 이동 목표지점 설정
    {
        float randomX;

        if (UnityEngine.Random.value < 0.5f) // 50%확률로 이동 방향 설정
        {
            // 왼쪽 방향: -patrolRange ~ -patrolMinRange
            randomX = UnityEngine.Random.Range(-patrolRange, -patrolMinRange);
        }
        else
        {
            // 오른쪽 방향: patrolMinRange ~ patrolRange
            randomX = UnityEngine.Random.Range(patrolMinRange, patrolRange);
        }

        targetPos = new Vector2(startPos.x + randomX, transform.position.y);
        direction = (targetPos - (Vector2)transform.position).normalized; // 패트롤 할 방향 설정

        if (direction.x > 0)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    protected void EvaluateCurrentState() // 프로퍼티 현재 상태 확인 함수
    {
        isMoving = isMoving;
        isAttackRange = isAttackRange;
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        groggyUI = GetComponentInChildren<EnemyGroggyUI>();
        player = GameObject.FindGameObjectWithTag("Player");

        originalColor = spriteRenderer.color;
    }

    // 적 공통 기능 구현

    public virtual IEnumerator Stunned(float delay) // 적 기절 반응 구현
    {
        moveSpeed = 0;
        isStune = true; // 잠시 스턴 상태
        currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(currentColor.r * 0.5f, currentColor.g * 0.5f, currentColor.b * 0.5f, currentColor.a);
        CancelAttack(); // 공격 중인 경우 취소
        animator.SetBool("isRun", false); // 잠시 Idle 모션

        yield return new WaitForSeconds(delay);

        moveSpeed = defaultMoveSpeed; // 이동속도 복구
        isStune = false; // 스턴 상태 해제
        spriteRenderer.color = originalColor; // 색상 복구
        EvaluateCurrentState(); // 적 상태 적용 함수
    }

    public virtual IEnumerator EnemyFade(float duration) // 평화의 악장으로 적 사라짐 함수
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

        gameObject.SetActive(false); // 적 비활성화
    }

    public virtual IEnumerator Damaged() // 피격시 반응 구현
    {
        if (currentHp >= maxHp) yield break; // 사망 이펙트와 충돌하여 hp 증가 방지

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
            if (isBossCreated)
                RequestEnemyDie?.Invoke(); // 보스가 만든 몬스터일 경우, 사망 알림

            Debug.Log("적이 고통스럽게 소멸합니다...");
            isDead = true;
            moveSpeed = 0f;
            animator.SetTrigger("Die");
            dieEffect.SetActive(true);
            GameManager.Instance.AddPolution(pollutionDegree); // 각 적의 pollutionDegree만큼 플레이어 오염도 증가
            yield return new WaitForSeconds(1.5f);

            dieEffect.SetActive(false);
            gameObject.SetActive(false); // 적 비활성화
        }
    }

    public virtual void PushBack(float dir, float duration = 5) // 밀격 반응 구현
    {
        if (dir > 0)
            rigidBody.AddForce(Vector2.right * 650);
        else
            rigidBody.AddForce(Vector2.left * 650); // 뒤로 일정 거리 밀격

        StartCoroutine(Stunned(duration)); // 지정한 시간 동안 기절

        if (GameManager.Instance.isReturned) // 늑대의 밀격이 아닌 에코가드로 인한 그로기 밀격일 때 추가기능 
        {
            groggyUI.ResetGroggyState(); // 그로기 내용 초기화
            currentGroggyCnt = 0;
        }
    }

    public virtual void EchoGuardPushBack(float dir) // 에코가드 밀격 반응 구현
    {
        if (dir > 0)
            rigidBody.AddForce(Vector2.right * 450);
        else
            rigidBody.AddForce(Vector2.left * 450); // 뒤로 일정 거리 밀격

        StartCoroutine(Stunned(1.5f)); // 1.5초간 기절
    }

    private void UpdateHpGauge() // 적 hp(오염도) 게이지 업데이트
    {
        float hpRatio = currentHp / maxHp;
        if (isBoss)
        {
            enemyHpGauge.GetComponent<Image>().fillAmount = hpRatio;
        }
        else
        {
            enemyHpGauge.localScale = new Vector2(hpRatio, enemyHpGauge.localScale.y);
        }

    }

    public void ActivateAttackRange() // 애니메이터상 충돌범위 활성화 함수
    {
        if (!isAttacking) return; // 공격 중이면만 실행


        attackObjects[nextAttackIndex].SetActive(true); // 현재 공격 범위 활성화

        if (nextAttackIndex + 1 < enemySounds.Length)
        {
            audioSource.clip = enemySounds[nextAttackIndex + 1];
            audioSource.Play(); // 현재 실행중인 공격 음원 실행
        }
        else
        {
            //만약 루프면 다시 0으로 만들기
        }

    }

    public void DeActivateAttackRange() // 애니메이터상 충돌범위 비활성화 함수 
    {
        attackObjects[nextAttackIndex].SetActive(false); // 현재 공격 범위 비활성화
    }

    public void ActivateProjectile(GameObject projectile)
    {
        GameObject enemyProjectile = Instantiate(projectile, transform.position, Quaternion.identity); // 탄환생성

        Rigidbody2D rb = enemyProjectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 bulletDirection = new Vector2(direction.x, 0f).normalized;
            Vector2 spawnPosition = (Vector2)transform.position + bulletDirection * 1f;
            enemyProjectile.transform.position = spawnPosition;
            rb.velocity = bulletDirection * bulletSpeed;
        }
    }

    public virtual void CancelAttack() // 공격 취소시
    {
        if (attackCoroutine != null && currentAttack != null)
        {
            isAttacking = false;
            StopCoroutine(attackCoroutine);

            foreach (var attack in attackObjects) // 공격 범위 모두 비활성화
            {
                if(attack != null)
                {
                    attack.SetActive(false); 
                    Debug.LogWarning("공격 지웠어용");
                }
            }

            hitEffect.SetActive(false);
            hitEffect_noGroggy.SetActive(false);
            spriteRenderer.enabled = true;
            boxCollider.enabled = true;
            attackCoroutine = null;
            nextAttackIndex = UnityEngine.Random.Range(0, attackPatterns.Length);
            Debug.LogWarning("공격 강제 종료!");
        }
    }

    protected virtual IEnumerator MoveToTarget(Vector2 startPos, Vector2 targetPos, float duration) // 특정위치로 이동하는 함수
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 force = targetPos - startPos;

        rb.velocity = Vector2.zero;
        rb.AddForce(force * 10, ForceMode2D.Impulse);
        yield return new WaitForSeconds(duration);

        rb.velocity = Vector2.zero; // 대시 종료 후 정지
    }

    public IEnumerator EchoGuardSuccess(Collider2D collision) // EnemyAttack 공격 방어시
    {
        if(currentAttack != null)
            currentAttack.SetActive(false); // 최근 적 공격 범위 off (에코가드시 범위 취소)

        nextAttackIndex = UnityEngine.Random.Range(0, attackPatterns.Length); // 에코가드 성공시, 현재 공격 초기화

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
            PushBack(pushBackDir, 15f);

            echoGuardEffect.SetActive(true); // 에코가드 성공 이펙트 활성화
            yield return new WaitForSeconds(0.4f);

            echoGuardEffect.SetActive(false); // 에코가드 이펙트 비활성화
        }
    }

    public void EchoGuardSuccess_NoGloogy() // EnemyAttack_NoGroggy 공격 방어시
    {
        Debug.LogWarning("해당 공격은 그로기가 올라기지 않습니다!");

        if (currentAttack != null)
            currentAttack.SetActive(false); // 최근 적 공격 범위 off (에코가드시 범위 취소)

        nextAttackIndex = UnityEngine.Random.Range(0, attackPatterns.Length); // 에코가드 성공시, 현재 공격 초기화

        StartCoroutine(Stunned(0.5f)); // 0.5초 기절
    }

    protected virtual void InitializeAttackPatterns() //공격 함수 구성 초기화 함수
    {
        // 자식 오브젝트에 따라 공격 함수 초기화 구현
    }

    // 충돌 관련 함수

    protected abstract void HandlerTriggerEnter(Collider2D collision); // 충돌시 범위 주변(Enter) 담당 함수
    protected abstract void HandlerTriggerStay(Collider2D collision); // 충돌시 범위 내(Stay) 담당 함수

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return; // 사망시 충돌체크 X

        HandlerTriggerEnter(collision); // 구체적인 충돌 처리과정은 자식 스크립트에게 맡김!
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead) return;

        HandlerTriggerStay(collision); // 위에와 동일하지만 이때는 범위 오브젝트 생성시 적이 이미 범위 내부에 있을 경우 (내용 동일)
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WolfAppear") || collision.gameObject.CompareTag("PurifyStep"))
        {
            Debug.Log("적이 범위를 벗어납니다");
            moveSpeed = defaultMoveSpeed; // 기존 속도
        }
    }
}
