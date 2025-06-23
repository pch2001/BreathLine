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

public class PlayerCtrl_R : MonoBehaviour, PlayerCtrlBase
{
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerSkill_R playerSkill;
    public SpriteRenderer spriteRenderer; // Sprite 반전용
    public PlayerInputAction_R playerinputAction; // 강화된 Input 방식 사용

    private Coroutine echoGuardCoolRoutine; // 에코가드 쿨타임 코루틴
    public Image echoGuardCool; // 에코가드 쿨타임 UI 

    private Coroutine purifySteoCoolRoutine; // 정화의 걸음 쿨타임 코루틴
    public Image purifyStepCool; // 정화의 걸음 쿨타임 UI 

    public Vector3 savePoint; // 현재 스테이지에서 사용할 임시 세이브 포인트

    private Color originColor; // 소녀 스프라이트 색상
    private Color damagedColor = new Color(0.2f, 0.2f, 0.2f); // 소녀 피격시 깜빡일 때 색상

    float h; // 플레이어 좌우 이동값
    public float moveSpeed = 5f; // 이동속도
    public float jumpForce = 12f; // 점프력
    private bool isGrounded = true; // 착지 여부
    public bool isPressingPiri { get; private set; } = false; // 피리 연주 여부
    public bool isPurifying = false; // 정화의 걸음 여부
    public bool isDamaged = false; // 현재 피격상태 여부
    public float damagedTime = 1f; // 피격 반응 유지 시간
    private float blinkInterval = 0.15f; // 피격시 한번 깜빡하는 시간

    public bool isLocked; // 상호작용시 행동 제한

    public bool isPase4 = false; // 최종 보스 페이즈4 여부
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerinputAction = new PlayerInputAction_R();
        playerSkill = GetComponent<PlayerSkill_R>();
    }

    public void OnEnable()
    {
        isLocked = true; // 움직임 제한 해제

        // inputAction 활성화
        playerinputAction.Enable();

        // PlayerCtrl 변수 변경 이벤트
        playerSkill.RequestMoveSpeed += OnSetMoveSpeed;
        playerSkill.RequestAnimTrigger += OnSetAnimTrigger;
        playerSkill.RequestAnimSpeed += OnSetAnimSpeed;
        playerSkill.RequestEchoGuardStart += SetEchoGuardCoolTime;
        playerSkill.RequestPuriFyStepStart += SetPurifyStepCoolTime;
        playerSkill.RequestisPurifing += SetisPurifing;
        playerSkill.RequestSetSpriteColor += OnSetSpriteColor;

        // PlayerInputAction 이벤트
        playerinputAction.Player.Jump.performed += OnJump;
        playerinputAction.Player.PlayPiri.started += OnStartPiri;
        playerinputAction.Player.PlayPiri.canceled += OnReleasePiri;
        playerinputAction.Player.EchoGuard.performed += OnEchoGuard;
        playerinputAction.Player.PurifyingStep.started += OnPurifyStepStart;
        playerinputAction.Player.PurifyingStep.canceled += OnPurifyStepStop;

    }

    public void OnDisable()
    {
        isLocked = false;// 대화시 움직임 못 움직이는 상태

        // inputAction 비활성화
        playerinputAction.Disable();
    }

    // 소녀 연결 이벤트
    private void OnSetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    private void OnSetAnimTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    private void OnSetAnimSpeed(float speed)
    {
        animator.speed = speed;
    }

    private void Start()
    {
        isLocked = false;

        playerSkill.OnUpdateStageData(); // 연결된 음원 딕셔너리에 초기화
        GameManager.Instance.isReturned = true; // 회귀 후로 설정 변경
    }

    void Update()
    {
        if (isLocked) return; // 행동 제한 변수 활성화시 제한
        if (!isPase4) return; // 페이즈4가 활성화되면 공격제한

        OnPlayerMove(); // 이동 구현

        OnPlaySoftPiri(); // 평화의 악장 연주 차징 확인
        UpdatePurifyStep(); // 정화의 걸음시 방향 갱신
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

    private void OnJump(InputAction.CallbackContext context) // 점프
    {
        if (isGrounded && !isPressingPiri) // 착지시 + 연주가 아닐때만 점프 가능
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
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

    private void OnStartInteract(InputAction.CallbackContext context)
    {
        moveSpeed = 0f; // 이동 제한
        isLocked = true;
        playerinputAction.Player.PlayPiri.Disable(); // 피리 사용 제한
        playerinputAction.Player.EchoGuard.Disable(); // 에코가드 사용 제한
        playerinputAction.Player.PurifyingStep.Disable(); // 정화의 걸음 사용 제한
    }

    private void OnStopInteract(InputAction.CallbackContext context)
    {
        moveSpeed = 5f; // 이동 제한 해제
        isLocked = false;
        playerinputAction.Player.PlayPiri.Enable(); // 피리 사용 제한 해제
        playerinputAction.Player.EchoGuard.Enable(); // 에코가드 사용 제한 해제
        playerinputAction.Player.PurifyingStep.Enable(); // 정화의 걸음 사용 제한 해제
    }

    private void OnStartPiri(InputAction.CallbackContext context) // 연주버튼을 눌렀을 때 실행
    {
        if (isGrounded) // 착지시에만 가능
        {
            playerSkill.StartPiri();
            isPressingPiri = true; // 피리 연주 시작
        }
    }

    private void OnReleasePiri(InputAction.CallbackContext context) // 연주버튼을 떼었을 때 실행
    {
        if (isGrounded && isPressingPiri) // 착지 + 연주시에만 가능
        {
            playerSkill.ReleasePiri();
            isPressingPiri = false; // 피리 연주 종료
        }
    }

    private void OnPlaySoftPiri() // 평화의 악장 연주 차징 확인
    {
        if (isPressingPiri) // 피리 연주시에 확인
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

    private IEnumerator PurifyStepCooldownRoutine(float duration) // duration은 에코가드 쿨타임
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

    private IEnumerator OnDamagedStart(float enemyDamage) // 소녀 피격 시작 함수
    {
        isDamaged = true; // 피격상태 시작

        Debug.Log("소녀 피격! 소녀의 오염도가 증가합니다!");
        GameManager.Instance.AddPolution(enemyDamage); // 적 공격력만큼 오염도 증가

        if (GameManager.Instance.Pollution < 100f) // 오염도가 다 차지 않았을 경우
        {
            // Player 레이어(7번)와 Enemy 레이어(6번) 사이 충돌을 무시
            Physics2D.IgnoreLayerCollision(7, 6, true);

            animator.SetTrigger(PlayerAnimTrigger.Hit);

            // 피격시 연주, 에코가드, 정화의 걸음 비활성화
            playerinputAction.Player.PlayPiri.Disable();
            playerinputAction.Player.EchoGuard.Disable();
            playerinputAction.Player.PurifyingStep.Disable();

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

        // 피격시 연주, 에코가드, 정화의 걸음 복구
        playerinputAction.Player.PlayPiri.Enable();
        playerinputAction.Player.EchoGuard.Enable();
        playerinputAction.Player.PurifyingStep.Enable();

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

    private void OnCollisionEnter2D(Collision2D collision) // 물리 충돌만 구현
    {
        if (collision.gameObject.CompareTag("Ground")) // 바닥과 충돌시 값 초기화
        {
            animator.speed = 1f; // 다시 애니메이션 동작
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Fall")) // 추락 판정시
        {
            Debug.Log("최근 세이브 포인트로 이동");
            transform.position = savePoint;
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

                if (isPressingPiri)
                {
                    playerSkill.PlaySoftPiriCanceled(); // 평화의 연주중이었을 경우 캔슬
                }
                StartCoroutine(OnDamagedStart(enemy.damage * (isPurifying ? 0.2f : 1))); // 피격 반응 구현 (정화의 걸음시 데미지 20%로 반감)
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }
        else if (collision.gameObject.CompareTag("EnemyAttack"))
        {
            EnemyAttackBase enemy = collision.gameObject.GetComponent<EnemyAttackBase>(); // EnemyAttack 기본 클래스 가져옴
            if (enemy != null)
            {
                if (isDamaged) return; // 소녀 피격 상태시 충돌 X

                if (isPressingPiri) // 평화의 연주중이었을 경우 캔슬
                {
                    playerSkill.PlaySoftPiriCanceled();
                }
                StartCoroutine(OnDamagedStart(enemy.attackDamage * (isPurifying ? 0.2f : 1))); // 피격 반응 구현 (정화의 걸음시 데미지 20%로 반감)
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }

        else if (collision.gameObject.CompareTag("SavePoint"))
        {
            savePoint = collision.transform.position; // 세이브 포인트 저장
        }
    }
}