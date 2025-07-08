using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class PlayerCtrl_R : PlayerCtrlBase
{
    private Animator animator;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer; // Sprite 반전용
    public PlayerInputAction_R playerinputAction; // 강화된 Input 방식 사용
    private PlayerSkill_R playerSkill;
    
    [SerializeField] private AnimatorOverrideController playPiriContorller; // 변경할 애니메이터(피리 모션)
    private RuntimeAnimatorController defaultController; // 기본 애니메이터
    [SerializeField] private Transform groundCheck; // 캐릭터 발바닥 위치
    [SerializeField] private LayerMask groundLayer; // 바닥으로 간주할 Layer
    public Image echoGuardCool; // 에코가드 쿨타임 UI 
    public Image purifyStepCool; // 정화의 걸음 쿨타임 UI 
    public GameObject SealUI; // 소녀 스킬 봉인 표시 UI
    public Vector3 savePoint; // 현재 스테이지에서 사용할 임시 세이브 포인트

    float h; // 플레이어 좌우 이동값
    public float moveSpeed = 5f; // 이동속도
    private float jumpForce = 14f; // 점프력
    private bool isGrounded = true; // 착지 여부
    private bool isReadyPiri = true; // 피리 연주 가능 여부 
    public bool isPeaceMelody = false; // 평화의 악장 연주 중인지
    private bool isDamaged = false; // 현재 피격상태 여부
    public float damagedTime = 1f; // 피격 반응 유지 시간
    private float blinkInterval = 0.15f; // 피격시 한번 깜빡하는 시간
    private Color originColor; // 소녀 스프라이트 색상
    private Color damagedColor = new Color(0.2f, 0.2f, 0.2f); // 소녀 피격시 깜빡일 때 색상
    private Coroutine echoGuardCoolRoutine; // 에코가드 쿨타임 코루틴
    private Coroutine purifySteoCoolRoutine; // 정화의 걸음 쿨타임 코루틴
    private Coroutine SealAttackRoutine; // 봉인 공격 실행 코루틴
    private Coroutine speedRoutine; // 이동속도 변경 코루틴
    public bool isLocked; // 상호작용시 행동 제한
    private bool isSealed; // 현재 능력 봉인 상태인지
    public bool isCovered = false; // 엄폐물에 있을 경우

    public GameObject saveButton;
    public GameObject MainButton;

    // 피리 연주 여부 프로퍼티
    [SerializeField] private bool _isPressingPiri = false;
    public override bool isPressingPiri // 피리 연주 여부
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

    // 정화의 걸음 여부 프로퍼티
    [SerializeField] private bool _isPurifying = false;
    public bool isPurifying // 정화의 걸음 여부
    {
        get => _isPurifying;
        set
        {
            if (_isPurifying == value) return;
            _isPurifying = value;

            // 정화의 걸음 애니메이션 전환
            animator.SetBool("isPurifying", value);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        defaultController = animator.runtimeAnimatorController;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerinputAction = new PlayerInputAction_R();
        playerSkill = GetComponent<PlayerSkill_R>();
    }

    public void OnEnable()
    {
        isLocked = false; // 움직임 제한 해제

        // inputAction 활성화
        playerinputAction.Enable();

        // PlayerCtrl 변수 변경 이벤트
        playerSkill.RequestSetMoveSpeedAndTime += OnSetMoveSpeedAndTime;
        playerSkill.RequestSetMoveSpeed += OnSetMoveSpeed;
        playerSkill.RequestAnimTrigger += OnSetAnimTrigger;
        playerSkill.RequestEchoGuardStart += SetEchoGuardCoolTime;
        playerSkill.RequestPuriFyStepStart += SetPurifyStepCoolTime;
        playerSkill.RequestisPurifing += SetisPurifing;
        playerSkill.RequestSetSpriteColor += OnSetSpriteColor;
        playerSkill.RequestPressingPiriState += OnSetPressingPiri;
        playerSkill.RequestPeaceMelodyActived += OnPeaceMelodyActived;
        
        // PlayerInputAction 이벤트
        playerinputAction.Player.Jump.performed += OnJump;
        playerinputAction.Player.PlayPiri.started += OnStartPiri;
        playerinputAction.Player.PlayPiri.canceled += OnReleasePiri;
        playerinputAction.Player.EchoGuard.performed +=     OnEchoGuard;
        playerinputAction.Player.PurifyingStep.started += OnPurifyStepStart;
        playerinputAction.Player.PurifyingStep.canceled += OnPurifyStepStop;
        playerinputAction.MenuUI.ESC.performed += OnESC;
    }
    public void OnDisable()
    {
        animator.SetBool("isMove", false); // 대화시 Idle 상태로 전환

        isLocked = true;// 대화시 움직임 제한 해제

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
    
    private void Start()
    {
        isLocked = false;

        playerSkill.OnUpdateStageData(); // 연결된 음원 딕셔너리에 초기화
        GameManager.Instance.isReturned = true; // 회귀 후로 설정 변경
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer); // 바닥에 착지 여부 확인

        if (isLocked || isPushed) return; // 행동 제한 변수 활성화시 제한

        OnPlaySoftPiri(); // 평화의 악장 연주 차징 확인
        UpdatePurifyStep(); // 정화의 걸음시 방향 갱신
        OnPlayerMove(); // 이동 구현
    }

    private void OnDrawGizmosSelected() // 바닥 충돌 확인 기즈모 표시
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }

    private void OnPlayerMove()
    {
        if (!isPurifying) // 정화의 걸음시 입력 기준 변경 및 겹치는 애니메이션 방지
        {
            // 좌우 방향 기준
            h = Input.GetAxisRaw("Horizontal");

            // 애니메이션 변경
            if (h != 0)
                animator.SetBool("isMove", true);
            else
                animator.SetBool("isMove", false);
        }

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
        saveButton.SetActive(!saveButton.activeSelf); // 세이브 버튼 토글
        MainButton.SetActive(!MainButton.activeSelf); // 메인 버튼 토글
    }
    private void OnJump(InputAction.CallbackContext context) // 점프
    {
        if (isGrounded && !isPressingPiri) // 착지시 + 연주가 아닐때만 점프 가능
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetTrigger("isJump");
        }
    }

    private void OnJumpEnd() // 점프 애니메이션 종료시, 잠시 그 상태로 멈춤
    {
        animator.speed = 0f;
    }

    private void OnSetSpriteColor(float brightness) // 오염도 변경시 기존 색상 저장 및 sprite 변경
    {
        spriteRenderer.color = new Color(brightness, brightness, brightness, spriteRenderer.color.a);
        originColor = spriteRenderer.color; // 현재 색상을 변경
    }

    private void OnStartPiri(InputAction.CallbackContext context) // 연주버튼을 눌렀을 때 실행
    {
        if (isGrounded && isReadyPiri) // 착지, 피리 준비시에만 가능
        {
            animator.runtimeAnimatorController = playPiriContorller; // 피리부는 애니메이터로 변경
            StartCoroutine(StartPiriCool()); // 피리 연주 쿨타임 시작(0.2f);
            playerSkill.StartPiri();
            isPressingPiri = true; // 피리 연주 시작
        }
    }

    private IEnumerator StartPiriCool()
    {
        isReadyPiri = false;

        yield return new WaitForSeconds(0.6f);
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
        if (isPressingPiri) // 피리 연주중일 때
        {
            playerSkill.CheckSoftPiri();
        }
    }

    private void OnEchoGuard(InputAction.CallbackContext context) // 마우스 우클릭시 에코가드 실행
    {
        if (!isPressingPiri) // 피리 연주시가 아닐 경우
        {
            StartCoroutine(playerSkill.EchoGuard());
        }
    }

    private void SetEchoGuardCoolTime(float duration)
    {
        // 중복 실행 방지
        if (echoGuardCoolRoutine != null)
            StopCoroutine(echoGuardCoolRoutine);

        echoGuardCoolRoutine = StartCoroutine(EchoGuardCooldownRoutine(duration));
    }

    private IEnumerator EchoGuardCooldownRoutine(float duration) // duration은 에코가드 쿨타임
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            echoGuardCool.fillAmount = fill;
            yield return null;
        }
        echoGuardCool.fillAmount = 1f;
    }

    private void SetisPurifing(bool state) // 정화의 걸음 실행 여부 설정
    {
        isPurifying = state;
    }
    private void OnPurifyStepStart(InputAction.CallbackContext context) // 정화의 걸음 시작 함수
    {
        if (!playerSkill.purifyStepReady) return; // 정화의 걸음 준비 상태에서만 실행

        playerSkill.PurifyStepStart();
        playerinputAction.Player.PlayPiri.Disable(); // 정화의 걸음 중에는 피리 사용x
        playerinputAction.Player.EchoGuard.Disable(); // 정화의 걸음 중에는 에코가드 사용x
    }

    public void PurifyStepStop() // 정화의 걸음이 자동으로 종료되었을때 호출할 수 있도록 따로 구성
    {
        if (!isPurifying) return; // 정화의 걸음 종료 중복실행 방지

        playerSkill.PurifyStepStop();
        playerinputAction.Player.PlayPiri.Enable(); // 피리 사용 복구
        playerinputAction.Player.EchoGuard.Enable(); // 에코가드 복구
    }

    private void OnPurifyStepStop(InputAction.CallbackContext context) // 정화의 걸음 종료 함수
    {
        PurifyStepStop();
    }

    private void UpdatePurifyStep() // 정화의 걸음 갱신 함수
    {
        if (!isPurifying) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        h = (mousePos.x - transform.position.x) > 0 ? 1 : -1;

    }

    private void SetPurifyStepCoolTime(float duration)
    {
        // 중복 실행 방지
        if (purifySteoCoolRoutine != null)
            StopCoroutine(purifySteoCoolRoutine);

        purifySteoCoolRoutine = StartCoroutine(PurifyStepCooldownRoutine(duration));
    }

    private IEnumerator PurifyStepCooldownRoutine(float duration) // duration은 정화의 걸음 쿨타임
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            purifyStepCool.fillAmount = fill;
            yield return null;
        }
        purifyStepCool.fillAmount = 1f;
    }


    // 충돌 관련 기능 구현
    private IEnumerator OnEnemySealAttack(float enemyDamage, float enemyPosX)
    {
        if (isPurifying) // 정화의 걸음시 봉인 공격 무시
        {
            Debug.Log("소녀가 능력 봉인을 저항합니다."); 
            yield break;
        }

        SealUI.SetActive(true); // 스킬 봉인 UI 표시
        isPushed = true; // 밀격상태 시작
        animator.SetTrigger(PlayerAnimTrigger.Hit);
        rb.AddForce(Vector2.right * -Mathf.Sign(enemyPosX - transform.position.x) * 30, ForceMode2D.Impulse); // 피격시 반대방향으로 살짝 밀격됨
        
        yield return new WaitForSeconds(0.1f);
        isPushed = false; // 밀격상태 해제

        if (GameManager.Instance.Pollution < 100f) // 오염도가 다 차지 않았을 경우
        {
            // 피격시 연주, 에코가드, 정화의 걸음 비활성화
            playerinputAction.Player.PlayPiri.Disable();
            playerinputAction.Player.EchoGuard.Disable();
            if (!isPurifying) // 정화의 걸음 사용중 피격시 무시 
            {
                playerinputAction.Player.PurifyingStep.Disable();
            }

            yield return new WaitForSeconds(5f); // 5초 동안 소녀 능력 제한

            // 피격시 연주, 에코가드, 정화의 걸음 복구
            playerinputAction.Player.PlayPiri.Enable();
            playerinputAction.Player.EchoGuard.Enable();
            playerinputAction.Player.PurifyingStep.Enable();

            SealUI.SetActive(false); // 스킬 봉인 UI 제거
        }
    }


    private IEnumerator OnDamagedStart(float enemyDamage, float enemyPosX) // 소녀 피격 시작 함수
    {
        Debug.Log("소녀 피격! 소녀의 오염도가 증가합니다!");

        isPeaceMelody = false;
        isPressingPiri = false;
        if (isPurifying) // 정화의 걸음시 피격 반응 무시
        {
            isDamaged = true; // 피격상태 시작
        }
        else // 정화의 걸음이 아닐 경우
        {
            isDamaged = true; // 피격상태 시작
            isPushed = true; // 밀격상태 시작

            animator.SetTrigger(PlayerAnimTrigger.Hit);
            rb.AddForce(Vector2.right * ((enemyPosX - transform.position.x > 0) ? -1 : 1) * 13, ForceMode2D.Impulse); // 피격시 반대방향으로 살짝 밀격됨
        }

        GameManager.Instance.AddPolution(enemyDamage); // 적 공격력만큼 오염도 증가
        yield return new WaitForSeconds(0.1f);
        isPushed = false; // 밀격상태 해제

        if (GameManager.Instance.Pollution < 100f) // 오염도가 다 차지 않았을 경우
        {
            // Player 레이어(7번)와 Enemy 레이어(6번) 사이 충돌을 무시
            Physics2D.IgnoreLayerCollision(7, 6, true);

            // 피격시 연주, 에코가드, 정화의 걸음 비활성화
            playerinputAction.Player.PlayPiri.Disable();
            playerinputAction.Player.EchoGuard.Disable();
            if (!isPurifying) // 정화의 걸음 사용중 피격시 무시 
            {
                playerinputAction.Player.PurifyingStep.Disable();
            }

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
        else // 오염도가 가득 찼을 경우 
        {
            StartCoroutine(GameOver()); // 게임 오버
        }
    }

    public void OnDamagedEnd() // 소녀 피격 종료 함수
    {
        // Player 레이어(7번)와 Enemy 레이어(6번) 사이 충돌을 다시 허용
        Physics2D.IgnoreLayerCollision(7, 6, false);

        spriteRenderer.color = originColor;

        if (!isSealed) // 능력 봉인 상태가 아닐 때만
        {
            // 피격시 연주, 에코가드, 정화의 걸음 복구
            playerinputAction.Player.PlayPiri.Enable();
            playerinputAction.Player.PurifyingStep.Enable();
        }
        playerinputAction.Player.EchoGuard.Enable();
        
        if(!isPurifying)
            moveSpeed = 5f;
        
        isDamaged = false; // 피격상태 종료
    }

    private IEnumerator GameOver() // GameOver시 함수
    {
        Debug.LogWarning("소녀가 쓰러집니다.. GameOver");
        Debug.LogWarning("GameOver표시 + 게임오버 UI 표시 추가");

        Time.timeScale = 0.5f; // 시간 느려지는 연출
        yield return new WaitForSeconds(1f);

        Time.timeScale = 1f;
        GameManager.Instance.Pollution = 0f;
        GameManager.Instance.AddPolution(0f); // 오염도 UI 초기화
    }

    public void ActivatedSealMode(bool state) // 플레이어 능력 봉인 상태 구현
    {
        if (state)
        {
            isSealed = true;
            // 피격시 연주, 정화의 걸음 제한
            playerinputAction.Player.PlayPiri.Disable();
            playerinputAction.Player.PurifyingStep.Disable();

            SealUI.SetActive(true); // 스킬 봉인 UI 제거
        }
        else
        {
            isSealed = false;
            // 피격시 연주, 정화의 걸음 복구
            playerinputAction.Player.PlayPiri.Enable();
            playerinputAction.Player.PurifyingStep.Enable();

            SealUI.SetActive(false); // 스킬 봉인 UI 제거
        }
    }


    private void OnCollisionEnter2D(Collision2D collision) // 물리 충돌만 구현
    {
        if (collision.gameObject.CompareTag("Ground")) // 바닥과 충돌시 값 초기화
        {
            animator.speed = 1f; // 다시 애니메이션 동작
        }
        else if (collision.gameObject.CompareTag("Fall")) // 추락 판정시
        {
            Debug.Log("최근 세이브 포인트로 이동");
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
        if (collision.gameObject.CompareTag("Enemy")) // 적과 충돌시 데미지 or 가드
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>(); // Enemy 기본 클래스 가져옴
            if (enemy != null)
            {
                if (!enemy.attackMode || enemy.isStune || enemy.isDead || isDamaged) return; // 적의 공격 모드가 false or 스턴, 사망 상태일 경우 or 소녀 피격 상태시 충돌 X

                if (isPeaceMelody)
                {
                    playerSkill.PlaySoftPiriCanceled(); // 평화의 연주중이었을 경우 캔슬
                }
                StartCoroutine(OnDamagedStart(enemy.damage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // 피격 반응 구현 (정화의 걸음시 데미지 20%로 반감)
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }
        else if (collision.gameObject.CompareTag("EnemyAttack")) // 에코가드 성공시 그로기 게이지를 높일 수 있는 Attack
        {
            if (isDamaged) return; // 소녀 피격 상태시 충돌 X

            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();

            if (enemyAttack != null)
            {

                if (playerSkill.isEchoGuarding) // 에코가드 상태시 적 그로그 게이지 증가
                {
                    Debug.Log("[EnemyAttack] 공격을 방어해 적의 그로기 게이지를 높입니다!");
                    StartCoroutine(enemyAttack.enemyOrigin.GetComponent<EnemyBase>().EchoGuardSuccess(collision));
                    enemyAttack.enemyOrigin.GetComponent<EnemyBase>().CancelAttack();
                    return;
                }

                if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
                {
                    isPeaceMelody = false;
                    isPressingPiri = false;
                    playerSkill.PlaySoftPiriCanceled();
                }
                StartCoroutine(OnDamagedStart(enemyAttack.attackDamage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // 피격 반응 구현 (정화의 걸음시 데미지 20%로 반감)
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }

        else if (collision.gameObject.CompareTag("EnemyAttack_NoGroggy")) // 에코가드 성공시에도 그로기 게이지를 높일 수 없는 Attack
        {
            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();
            if (enemyAttack != null)
            {
                if (isDamaged) return; // 소녀 피격 상태시 충돌 X

                if (playerSkill.isEchoGuarding) // 에코가드 상태시 적 그로그 게이지 증가
                {
                    Debug.Log("[EnemyAttack_NoGroggy] 공격을 방어해 적이 잠시 기절합니다!");
                    if(enemyAttack.enemyOrigin != null)
                    {
                        enemyAttack.enemyOrigin.GetComponent<EnemyBase>().EchoGuardSuccess_NoGloogy();
                        enemyAttack.enemyOrigin.GetComponent<EnemyBase>().CancelAttack();   
                    }
                    return;
                }

                if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
                {
                    isPeaceMelody = false;
                    isPressingPiri = false;
                    playerSkill.PlaySoftPiriCanceled();
                }
                Debug.LogWarning("Enemy 노 그로기와 충돌했슈");
                StartCoroutine(OnDamagedStart(enemyAttack.attackDamage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // 피격 반응 구현 (정화의 걸음시 데미지 20%로 반감)
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }

        else if (collision.gameObject.CompareTag("EnemySealAttack")) // 플레이어 능력 봉인 Attack
        {
            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();

            if (enemyAttack != null)
            {
                if (isDamaged) return; // 소녀 피격 상태시 충돌 X

                if (playerSkill.isEchoGuarding) // 에코가드 상태시 적 그로그 게이지 증가
                {
                    Debug.Log("[EnemyAttack] 공격을 방어해 적의 그로기 게이지를 높입니다!");
                    StartCoroutine(enemyAttack.enemyOrigin.GetComponent<EnemyBase>().EchoGuardSuccess(collision));
                    enemyAttack.enemyOrigin.GetComponent<EnemyBase>().CancelAttack();
                    moveSpeed = 5f;
                    return;
                }

                if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
                {
                    isPeaceMelody = false;
                    isPressingPiri = false;
                    playerSkill.PlaySoftPiriCanceled();
                }
                if(SealAttackRoutine != null)
                {
                    StopCoroutine(SealAttackRoutine); // 기존 실행중인 봉인 공격 중지
                }
                SealAttackRoutine = StartCoroutine(OnEnemySealAttack(enemyAttack.attackDamage * (isPurifying ? 0.2f : 1), enemyAttack.enemyOrigin.transform.position.x)); // 봉인공격 반응 구현 (정화의 걸음시 데미지 20%로 반감)
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }
        else if (collision.gameObject.CompareTag("EnemyProjectile")) // 에코 가드 성공시 막기만 하는 Attack
        {
            Debug.Log("소녀가 적의 투사체에 데미지를 입습니다.");
            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();

            if (isPeaceMelody) // 평화의 연주중이었을 경우 캔슬
            {
                isPeaceMelody = false;
                isPressingPiri = false;
                playerSkill.PlaySoftPiriCanceled();
            }
            StartCoroutine(OnDamagedStart(enemyAttack.attackDamage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // 피격 반응 구현 (정화의 걸음시 데미지 20%로 반감)

            Destroy(collision.gameObject); // 투사체 삭제
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
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("EnemyAttackRange"))
        {
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.isAttackRange = true; // 적 공격 실행 함수 수행
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
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
    }
}