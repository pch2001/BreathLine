﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class PlayerCtrl : PlayerCtrlBase
{
    private Animator animator;
    private Rigidbody2D rb;
    public SpriteRenderer spriteRenderer; // Sprite 반전용
    public PlayerInputAction playerinputAction; // 강화된 Input 방식 사용
    private PlayerSkill playerSkill;

    [SerializeField] private AnimatorOverrideController playPiriContorller; // 변경할 애니메이터(피리 모션)
    private RuntimeAnimatorController defaultController; // 기본 애니메이터
    [SerializeField] private Transform groundCheck; // 캐릭터 발바닥 위치
    [SerializeField] private LayerMask groundLayer; // 바닥으로 간주할 Layer
    public GameObject SealUI; // 소녀 스킬 봉인 표시 UI
    public Image wolfAttackCoolBG; // 늑대 공격 쿨타임 UI(배경)
    public Image wolfAttack; // 늑대 공격 UI
    public Image wolfAttackCool; // 늑대 공격 쿨타임 UI(아이콘)
    public Vector3 savePoint; // 현재 스테이지에서 사용할 임시 세이브 포인트

    // 소녀 관련 변수

    float h; // 플레이어 좌우 이동값
    public float moveSpeed = 5f; // 이동속도
    private Coroutine speedRoutine; // 이동속도 변경 코루틴
    private float jumpForce = 14f; // 점프력
    private bool isGrounded = true; // 착지 여부
    private bool isReadyPiri = true; // 피리 연주 가능 여부 
    public bool isPeaceMelody = false; // 평화의 악장 연주 중인지
    private bool isDamaged = false; // 현재 피격상태 여부
    public float damagedTime = 1f; // 피격 반응 유지 시간
    private float blinkInterval = 0.15f; // 피격시 한번 깜빡하는 시간
    private Color originColor = new Color(1f, 1f, 1f); // 소녀 스프라이트 색상
    private Color damagedColor = new Color(0.5f, 0.5f, 0.5f); // 소녀 피격시 깜빡일 때 색상
    private Coroutine SealAttackRoutine; // 봉인 공격 실행 코루틴
    public bool isLocked; // 상호작용시 행동 제한
    public bool isCovered = false; // 엄폐물에 있을 경우
    public bool is4BossStage = false; // 현재 회귀전 4스테이지 보스전인지

    public Story_four story4;
    public Story_note storyVideo;
    public GameObject uiChange;
    public GameObject stage4PlayerPos;

    // 늑대 관련 변수

    public GameObject wolf; // 늑대 게임 오브젝트
    public Animator wolfAnimator; // 늑대 애니메이터
    private float wolfExitTimer = 0f; // 늑대 Hide 타이머 / 일정 시간 이후 Hide실행
    public float defaultWolfExitTime = 10f; // 늑대 자동 퇴장 시간
    public WolfState currentWolfState = WolfState.Idle; // 현재 늑대 상태 확인 (WolfState 클래스)
    private Coroutine wolfAttackCoolRoutine; // 늑대 공격 쿨타임 코루틴
    public bool isWolfRange; // 늑대의 범위 내에 있는지(WolfAppear 영역 / 피해x)

    // 소녀 오염도 최대치 알림 이벤트
    public event Action RequestPlayerPolluted; // 소녀 오염도 최대치 알림 이벤트

    // 피리 연주 여부 프로퍼티
    private bool _isPressingPiri = false;

    public GameObject saveButton;
    public GameObject MainButton;
    public override bool isPressingPiri
    {
        get => _isPressingPiri;
        set
        {
            if (_isPressingPiri == value) return;
            _isPressingPiri = value;

            // 피리 애니메이션 전환
            animator.runtimeAnimatorController = value ? playPiriContorller : defaultController;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        defaultController = animator.runtimeAnimatorController;
        wolfAnimator = wolf.GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerinputAction = new PlayerInputAction();
        playerSkill = GetComponent<PlayerSkill>();
    }

    public void OnEnable()
    {
        isLocked = false; // 대화시 움직임 제한 해제
        playerSkill.SetWolfEyesVisible(true); // 늑대 눈 UI 활성화

        // inputAction 활성화
        playerinputAction.Enable();

        // PlayerCtrl 변수 변경 이벤트
        playerSkill.RequestSetMoveSpeedAndTime += OnSetMoveSpeedAndTime;
        playerSkill.RequestSetMoveSpeed += OnSetMoveSpeed;
        playerSkill.RequestAnimTrigger += OnSetAnimTrigger;
        playerSkill.RequestWolfAnimTrigger += OnSetWolfAnimTrigger;
        playerSkill.RequestPressingPiriState += OnSetPressingPiri;
        playerSkill.RequestPeaceMelodyActived += OnPeaceMelodyActived;

        playerSkill.RequestWolfState += OnSetWolfState;
        playerSkill.RequestWolfStartAttack += SetWolfAttackCoolTime;

        // PlayerInputAction 이벤트
        playerinputAction.Player.Jump.performed += OnJump;
        playerinputAction.Player.PlayPiri.started += OnStartPiri;
        playerinputAction.Player.PlayPiri.canceled += OnReleasePiri;
        playerinputAction.Wolf.Move.performed += OnWolfMove;
        playerinputAction.Wolf.Attack.performed += OnWolfAttack;
        playerinputAction.MenuUI.ESC.performed += OnESC;
    }

    public void OnDisable()
    {
        isLocked = true;// 대화시 움직임 제한
        playerSkill.SetWolfEyesVisible(false); // 늑대 눈 UI 비활성화

        animator.SetBool("isMove", false); // 대화시 Idle 상태로 전환

        // inputAction 비활성화
        playerinputAction.Disable();
    }

    // 소녀 연결 이벤트
    public void OnSetMoveSpeed(float speed)
    {
        if (speedRoutine != null)
        {
            StopCoroutine(speedRoutine);
            speedRoutine = null;
        }
        moveSpeed = speed;
    }
    public void OnSetMoveSpeedAndTime(float speed, float duration) // 이동속도 변경 함수
    {
        if (speedRoutine != null)
            StopCoroutine(speedRoutine);

        moveSpeed = speed;
        speedRoutine = StartCoroutine(RestoreMoveSpeed(duration));
    }

    private IEnumerator RestoreMoveSpeed(float delay) // 이동속도 복귀 코루틴
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = 5f;
        speedRoutine = null;
    }

    private void OnSetAnimTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    private void OnSetPressingPiri(bool state)
    {
        isPressingPiri = state;
    }

    private void OnPeaceMelodyActived(bool state)
    {
        isPeaceMelody = state;
    }

    // 늑대 연결 이벤트
    private void OnSetWolfAnimTrigger(string triggerName)
    {
        wolfAnimator.SetTrigger(triggerName);
    }

    private void OnSetWolfState(WolfState state)
    {
        currentWolfState = state;
    }

    private void Start()
    {
        isLocked = false;

        playerSkill.OnUpdateStageData(); // 연결된 음원 딕셔너리에 초기화
        GameManager.Instance.isReturned = false; // 회귀 후로 설정 변경
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer); // 바닥에 착지 여부 확인

        if (isLocked || isPushed) return; // 행동 제한 변수 활성화시 제한

        OnPlaySoftPiri(); // 평화의 악장 연주 차징 확인
        OnPlayerMove(); // 이동 구현
        OnWolfHide(); // 늑대 숨김 구현
    }

    // 소녀 기능 구현
    private void OnDrawGizmosSelected() // 바닥 충돌 확인(GroundCheck) 기즈모 표시
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }

    private void OnPlayerMove()
    {
        // 좌우 방향 기준
        h = Input.GetAxisRaw("Horizontal");

        // 애니메이션 변경
        if (h != 0)
            animator.SetBool("isMove", true);
        else
            animator.SetBool("isMove", false);

        // 이동 구현
        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        // 좌우 반전
        if (h > 0)
            spriteRenderer.flipX = false;
        else if (h < 0)
            spriteRenderer.flipX = true;
    }

    private void OnESC(InputAction.CallbackContext context)
    {
        saveButton.SetActive(!saveButton.activeSelf); // 세이브 버튼 표시/숨김 토글
        MainButton.SetActive(!MainButton.activeSelf); // 메인 버튼 표시/숨김 토글
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && !isPressingPiri)
        {
            animator.SetTrigger("isJump");
            StartCoroutine(DelayedJump(0.1f));
        }
    }

    private IEnumerator DelayedJump(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnJumpEnd() // 점프 애니메이션 종료시, 잠시 그 상태로 멈춤
    {
        animator.speed = 0f;
    }

    private void OnStartPiri(InputAction.CallbackContext context) // 연주버튼을 눌렀을 때 실행
    {
        if (isGrounded && isReadyPiri) // 착지, 피리 준비시에만 가능
        {
            animator.runtimeAnimatorController = playPiriContorller; // 피리부는 애니메이터로 변경
            StartCoroutine(StartPiriCool()); // 피리 연주 쿨타임 시작(1f);
            playerSkill.StartPiri();
            isPressingPiri = true; // 피리 연주 시작
        }
    }

    private IEnumerator StartPiriCool()
    {
        isReadyPiri = false;

        yield return new WaitForSeconds(1f);
        isReadyPiri = true;
    }

    private void OnReleasePiri(InputAction.CallbackContext context) // 연주버튼을 떼었을 때 실행
    {
        if (isGrounded && isPressingPiri) // 착지 + 연주시에만 가능
        {
            playerSkill.ReleasePiri();
        }
    }

    private void OnPlaySoftPiri() // 평화의 악장 연주 차징 확인
    {
        if (isPressingPiri) // 피리 연주시에 확인
        {
            playerSkill.CheckSoftPiri();
        }
    }

    // 늑대 기능 구현
    private void OnWolfMove(InputAction.CallbackContext context) // 늑대 움직임 구현, 마우스 좌클릭 시 실행
    {
        if (currentWolfState == WolfState.Damaged) return; // 늑대 부상시 조작 불가능

        wolfExitTimer = 0f;

        if (currentWolfState == WolfState.Hide) // 늑대가 Hide 상태일 때, 늑대 등장
        {
            StartCoroutine(playerSkill.WolfAppear(false));
        }
        else if (currentWolfState == WolfState.Idle)// 늑대가 Hide상태x, 기존 위치 -> 늑대 새로운 위치 등장
        {
            StartCoroutine(playerSkill.WolfAppear(true));
        }
    }

    private void OnWolfGuard() // 늑대 가드 구현
    {
        isWolfRange = false; // 늑대 보호 범위에서 벗어남
        playerSkill.WolfGuard();
    }

    private void OnWolfAttack(InputAction.CallbackContext context) // 늑대 공격 구현, 마우스 우클릭 시 실행
    {
        if (currentWolfState == WolfState.Idle) // Hide상태가 아닐때만 실행
        {
            StartCoroutine(playerSkill.WolfAttack());
            wolfExitTimer = 0f;
        }
    }
    private void OnWolfHide() // 늑대 Hide 구현
    {
        if (currentWolfState != WolfState.Idle) return;

        if (wolfExitTimer >= defaultWolfExitTime) // 아무런 동작 없이 defaultWolfExitTime 이상 흐르면 실행
        {
            playerSkill.hideCoroutine = StartCoroutine(playerSkill.WolfHide(false)); // 늑대 자동 퇴장
        }
        else if (currentWolfState == WolfState.Idle) // 늑대 등장 후, wolfExitTimer 계속 증가
        {
            wolfExitTimer += Time.deltaTime;
        }
    }

    private void SetWolfAttackCoolTime(float duration)
    {
        // 중복 실행 방지
        if (wolfAttackCoolRoutine != null)
            StopCoroutine(wolfAttackCoolRoutine);

        wolfAttackCoolRoutine = StartCoroutine(WolfAttackCooldownRoutine(duration));
    }

    private IEnumerator WolfAttackCooldownRoutine(float duration) // duration은 늑대 공격 쿨타임
    {
        float elapsed = 0f;
        wolfAttackCool.gameObject.SetActive(true);
        wolfAttack.gameObject.SetActive(false);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            wolfAttackCoolBG.fillAmount = fill;
            yield return null;
        }
        wolfAttackCoolBG.fillAmount = 1f;
        wolfAttackCool.gameObject.SetActive(false);
        wolfAttack.gameObject.SetActive(true);
    }

    // 충돌 관련 기능 구현
    private IEnumerator OnEnemySealAttack(float enemyPosX)
    {
        Debug.Log("소녀의 능력이 봉인됩니다.");

        SealUI.SetActive(true); // 스킬 봉인 UI 표시
        isPushed = true; // 밀격상태 시작
        animator.SetTrigger(PlayerAnimTrigger.Hit);
        rb.AddForce(Vector2.right * ((enemyPosX - transform.position.x > 0) ? -1 : 1) * 30, ForceMode2D.Impulse); // 피격시 반대방향으로 살짝 밀격됨

        yield return new WaitForSeconds(0.1f);
        isPushed = false; // 밀격상태 해제

        if (GameManager.Instance.Pollution < 100f) // 오염도가 다 차지 않았을 경우
        {
            // 피격시 연주, 늑대 호출, 늑대 울음소리 봉인
            playerinputAction.Player.PlayPiri.Disable();
            playerinputAction.Wolf.Disable();

            yield return new WaitForSeconds(5f); // 5초 동안 소녀 능력 제한

            // 피격시 연주, 늑대 호출, 늑대 울음소리 봉인
            playerinputAction.Player.PlayPiri.Enable();
            playerinputAction.Wolf.Enable();
            SealUI.SetActive(false); // 스킬 봉인 UI 제거
        }
    }

    private IEnumerator OnDamagedStart(float enemyDamage, float enemyPosX, bool isWolfGuarding) // 소녀 피격 시작 함수
    {
        Debug.Log("소녀 피격! 소녀의 오염도가 증가합니다!");

        isDamaged = true; // 피격상태 시작
        isPushed = true; // 밀격상태 시작
        animator.SetTrigger(PlayerAnimTrigger.Hit);
        rb.AddForce(Vector2.right * ((enemyPosX - transform.position.x > 0) ? -1 : 1) * 13, ForceMode2D.Impulse); // 피격시 반대방향으로 살짝 밀격됨

        GameManager.Instance.AddPolution(enemyDamage * (isWolfGuarding ? 0.5f : 1)); // 적 공격력만큼 오염도 증가
        yield return new WaitForSeconds(0.1f);
        isPushed = false; // 밀격상태 해제

        if (is4BossStage && GameManager.Instance.Pollution >= 100) // 보스전에서 오염도가 가득 찼을 경우, 스토리 진행
        {
            OnDisable();
            Time.timeScale = 0.4f;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            gameObject.transform.position = stage4PlayerPos.transform.position;
            animator.SetTrigger("isSad");
            RequestPlayerPolluted?.Invoke();

            OnWolfGuard();
            StopCoroutine(playerSkill.hideCoroutine);
            wolf.transform.position = stage4PlayerPos.transform.position + Vector3.up * 0.6f;
            wolf.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
            wolf.GetComponent<SpriteRenderer>().sortingOrder = 7; // 잠시 소녀보다 앞에 나오게 설정
            wolfAnimator.SetTrigger("isDead");

            yield return new WaitForSeconds(1.5f);

            storyVideo.PlayVideo();
            Time.timeScale = 1f;
            yield break;
        }

        // 피격시 연주 비활성화
        playerinputAction.Player.PlayPiri.Disable();

        // Player 레이어(7번)와 Enemy 레이어(6번) 사이 충돌을 무시
        Physics2D.IgnoreLayerCollision(7, 6, true);

        // 깜빡이는 효과
        float elapsed = 0f; // 경과된 정도
        while (elapsed < damagedTime)
        {
            spriteRenderer.color = damagedColor;
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.color = originColor;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval * 2;
        }

        OnDamagedEnd();
    }

    public IEnumerator PlayWolfDie() // 늑대 희생 장면 실행 함수
    {
        uiChange.SetActive(true); // UI 변경 표시
        yield return new WaitForSeconds(2f);

        StartCoroutine(story4.TypingText(3));
        Time.timeScale = 1f;

        yield break;
    }

    public void OnDamagedEnd() // 소녀 피격 종료 함수
    {
        // Player 레이어(7번)와 Enemy 레이어(6번) 사이 충돌을 다시 허용
        Physics2D.IgnoreLayerCollision(7, 6, false);

        spriteRenderer.color = originColor;

        // 피격시 연주, 늑대 등장, 울음소리 복구
        playerinputAction.Player.PlayPiri.Enable();
        moveSpeed = 5f;
        isDamaged = false; // 피격상태 종료
    }

    private IEnumerator GameOver() // GameOver시 함수
    {
        Debug.LogWarning("소녀가 쓰러집니다.. GameOver");
        Debug.LogWarning("GameOver표시 + 게임오버 UI 표시 추가");

        animator.SetTrigger("isDead"); // 사망 애니메이션 실행
        Time.timeScale = 0.5f; // 시간 느려지는 연출
        OnDisable(); // 조작 중지

        yield return new WaitForSeconds(4f);

        Time.timeScale = 1f;
        OnEnable(); // 조작 복구
        GameManager.Instance.Pollution = 0f;
        GameManager.Instance.AddPolution(0f); // 오염도 UI 초기화
    }

    private void OnCollisionEnter2D(Collision2D collision) // 물리 충돌만 구현
    {
        if (collision.gameObject.CompareTag("Ground")) // 바닥과 충돌시 값 초기화
        {
            animator.speed = 1f; // 다시 애니메이션 동작
        }
        else if (collision.gameObject.CompareTag("Fall")) // 추락 판정시
        {
            Debug.Log("최근 세이브 포인트로 이동, 오염도 증가");
            GameManager.Instance.AddPolution(10f);
            transform.position = savePoint;
        }
    }

    private void OnCollisionStay2D(Collision2D collision) // 발판 충돌시 동작 중지 방지
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            animator.speed = 1f; // 다시 애니메이션 동작
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // 주된 충돌은 Trigger에서 계산
    {
        if (isLocked) return; // 스크립트 중일때는 충돌 리턴

        if (collision.gameObject.CompareTag("Enemy")) // 적과 충돌시 데미지 or 가드
        {
            if (isDamaged) return; // 소녀 피격 상태, 늑대 영역에 있을 경우 충돌 X

            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>(); // Enemy 기본 클래스 가져옴

            if (enemy != null)
            {
                if (!enemy.attackMode || enemy.isStune || enemy.isDead) return; // 적의 공격 모드가 false or 스턴, 사망 상태일 경우 충돌 X

                if (currentWolfState != WolfState.Damaged) // 늑대 보호 가능
                {
                    OnWolfGuard(); // 가드 실행
                    StartCoroutine(OnDamagedStart(enemy.damage, collision.transform.position.x, true)); // 소녀 피격 상태 구현 (늑대 o)
                }
                else // 늑대 보호 불가능
                {
                    StartCoroutine(OnDamagedStart(enemy.damage, collision.transform.position.x, false)); // 소녀 피격 상태 구현 (늑대 x)
                }
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }
        else if (collision.gameObject.CompareTag("EnemyAttack") || collision.gameObject.CompareTag("EnemyAttack_NoGroggy"))
        {
            if (isDamaged || isWolfRange) return; // 소녀 피격 상태, 늑대 영역에 있을 경우 충돌 X

            Debug.Log("소녀가 적의 공격에 피해를 입습니다!");

            if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
            {
                isPeaceMelody = false;
                isPressingPiri = false;
                playerSkill.PlaySoftPiriCanceled();
            }

            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();
            if (enemyAttack != null)
            {
                float damage = enemyAttack.attackDamage;
                float position = collision.transform.position.x;
                bool isGuarding = currentWolfState != WolfState.Damaged;

                if (isGuarding) // 늑대 보호 가능
                {
                    OnWolfGuard(); // 가드 실행
                    StartCoroutine(OnDamagedStart(damage, position, true)); // 소녀 피격 상태 구현
                }
                else
                {
                    StartCoroutine(OnDamagedStart(damage, position, false)); // 소녀 피격 상태 구현
                }
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }
        else if (collision.gameObject.CompareTag("EnemyPiercingAttack"))
        {
            if (isDamaged || isWolfRange || isCovered) return; // 소녀 피격 상태, 늑대 영역에 있을 경우 충돌 X

            Debug.Log("소녀가 적의 공격에 피해를 입습니다!");

            if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
            {
                isPeaceMelody = false;
                isPressingPiri = false;
                playerSkill.PlaySoftPiriCanceled();
            }

            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();
            if (enemyAttack != null)
            {
                float damage = enemyAttack.attackDamage;
                float position = collision.transform.position.x;
                bool isGuarding = currentWolfState != WolfState.Damaged;

                if (isGuarding) // 늑대 보호 가능
                {
                    OnWolfGuard(); // 가드 실행
                    StartCoroutine(OnDamagedStart(damage, position, true)); // 소녀 피격 상태 구현
                }
                else
                {
                    StartCoroutine(OnDamagedStart(damage, position, false)); // 소녀 피격 상태 구현
                }
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }
        else if (collision.gameObject.CompareTag("EnemySealAttack")) // 플레이어 능력 봉인 Attack
        {
            if (isDamaged || isWolfRange) return; // 소녀 피격 상태, 늑대 영역에 있을 경우 충돌 X

            if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
            {
                isPeaceMelody = false;
                isPressingPiri = false;
                playerSkill.PlaySoftPiriCanceled();
            }

            if (SealAttackRoutine != null)
            {
                StopCoroutine(SealAttackRoutine); // 기존 실행중인 봉인 공격 중지
            }
            SealAttackRoutine = StartCoroutine(OnEnemySealAttack(collision.transform.position.x)); // 봉인공격 반응 구현

            moveSpeed = 5f;
        }
        else if (collision.gameObject.CompareTag("EnemyProjectile")) // 에코 가드 성공시 막기만 하는 Attack
        {
            if (isDamaged) return; // 소녀 피격 상태시 충돌 X

            Debug.Log("소녀가 적의 공격에 피해를 입습니다!");

            if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
            {
                isPeaceMelody = false;
                isPressingPiri = false;
                playerSkill.PlaySoftPiriCanceled();
            }

            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();
            if (enemyAttack != null)
            {
                float damage = enemyAttack.attackDamage;
                float position = collision.transform.position.x;
                bool isGuarding = currentWolfState != WolfState.Damaged;

                if (isGuarding) // 늑대 보호 가능
                {
                    OnWolfGuard(); // 가드 실행
                    StartCoroutine(OnDamagedStart(damage, position, true)); // 소녀 피격 상태 구현
                }
                else
                {
                    StartCoroutine(OnDamagedStart(damage, position, false)); // 소녀 피격 상태 구현
                }
            }
        }
        else if (collision.gameObject.CompareTag("EnemySight"))
        {
            Debug.Log("적이 소녀를 발견했습니다!!");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                //enemyBase.enemySight.GetComponent<SpriteRenderer>().enabled = false; // 적 경계 범위 표시 off
                enemyBase.isPatrol = false; // 적 공격모드로 전환
            }
        }
        else if (collision.gameObject.CompareTag("EnemyAttackRange"))
        {
            Debug.Log("소녀가 적의 공격 범위 안에 들어옵니다!");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.isAttackRange = true; // 적 공격 실행 함수 수행
            }
        }
        else if (collision.gameObject.CompareTag("SavePoint"))
        {
            savePoint = collision.transform.position; // 세이브 포인트 저장
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("WolfAppear"))
        {
            isWolfRange = true; // 늑대 범위 안에 있을 경우, 피해x 상태
        }
        else if (collision.gameObject.CompareTag("CoverObject"))
        {
            Debug.Log("소녀가 공격을 피해 주변 잔해에 숨습니다");
            isCovered = true;
        }


    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isLocked) return; // 스크립트 중일때는 충돌 리턴

        if (collision.gameObject.CompareTag("WolfAppear"))
        {
            isWolfRange = true; // 늑대 범위 안에 있을 경우, 피해x 상태
        }
        else if (collision.gameObject.CompareTag("EnemyAttackRange"))
        {
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.isAttackRange = true; // 적 공격 실행 함수 수행
            }
        }
        else if (collision.gameObject.CompareTag("EnemyPiercingAttack"))
        {
            if (isDamaged || isWolfRange || isCovered) return; // 소녀 피격 상태, 늑대 영역에 있을 경우 충돌 X

            Debug.Log("소녀가 적의 공격에 피해를 입습니다!");

            if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
            {
                isPeaceMelody = false;
                isPressingPiri = false;
                playerSkill.PlaySoftPiriCanceled();
            }

            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();
            if (enemyAttack != null)
            {
                float damage = enemyAttack.attackDamage;
                float position = collision.transform.position.x;
                bool isGuarding = currentWolfState != WolfState.Damaged;

                if (isGuarding) // 늑대 보호 가능
                {
                    OnWolfGuard(); // 가드 실행
                    StartCoroutine(OnDamagedStart(damage, position, true)); // 소녀 피격 상태 구현
                }
                else
                {
                    StartCoroutine(OnDamagedStart(damage, position, false)); // 소녀 피격 상태 구현
                }
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isLocked) return; // 스크립트 중일때는 충돌 리턴

        if (collision.gameObject.CompareTag("EnemyAttackRange"))
        {
            Debug.Log("소녀가 적의 공격 범위 밖으로 벗어납니다!");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.isAttackRange = false; // 적 플레이어 추격 함수 실행
            }
            Debug.Log("적이 소녀를 추격합니다!");
        }
        else if (collision.gameObject.CompareTag("EnemySight"))
        {
            Debug.Log("소녀가 적의 시야 밖으로 벗어납니다! 적이 추격을 멈춥니다");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                //enemyBase.enemySight.GetComponent<SpriteRenderer>().enabled = true; // 적 경계 범위 표시 on
                enemyBase.isPatrol = true; // 적 플레이어 추격 중단
            }
        }
        else if (collision.gameObject.CompareTag("WolfAppear"))
        {
            isWolfRange = false; // 늑대 범위 밖에 있을 경우, 피해 입을 수 있는 상태
        }
        else if (collision.gameObject.CompareTag("CoverObject"))
        {
            Debug.Log("소녀가 주변 잔해에서 빠져나옵니다.");
            isCovered = false;
        }
    }
}