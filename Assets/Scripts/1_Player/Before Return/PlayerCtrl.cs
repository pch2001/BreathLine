using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PlayerCtrl : MonoBehaviour, PlayerCtrlBase
{
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerSkill playerSkill;
    public SpriteRenderer spriteRenderer; // Sprite 반전용
    public PlayerInputAction playerinputAction; // 강화된 Input 방식 사용

    float h; // 플레이어 좌우 이동값
    public float moveSpeed = 5f; // 이동속도
    public float jumpForce = 12f; // 점프력
    private bool isGrounded = true; // 착지 여부
    public bool isPressingPiri { get; private set; } = false; // 피리 연주 여부
    public bool isDamaged = false; // 현재 피격상태 여부
    public float damagedTime = 1f; // 피격 반응 유지 시간
    private float blinkInterval = 0.15f; // 피격시 한번 깜빡하는 시간

    private Color originColor = new Color(1f, 1f, 1f); // 소녀 스프라이트 색상
    private Color damagedColor = new Color(0.5f, 0.5f, 0.5f); // 소녀 피격시 깜빡일 때 색상

    public Vector3 savePoint; // 현재 스테이지에서 사용할 임시 세이브 포인트
    public bool isLocked = false; // 상호작용시 행동 제한

    public GameObject wolf; // 늑대 게임 오브젝트
    public Animator wolfAnimator; // 늑대 애니메이터
    public float wolfExitTimer = 0f; // 늑대 Hide 타이머, 5f가 되면 Hide실행
    public WolfState currentWolfState = WolfState.Idle; // 현재 늑대 상태 확인 (WolfState 클래스)

    public Image wolfAttackCool; // 늑대 공격 쿨타임 UI 
    private Coroutine wolfAttackCoolRoutine; // 늑대 공격 쿨타임 코루틴

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        wolfAnimator = wolf.GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerinputAction = new PlayerInputAction();
        playerSkill = GetComponent<PlayerSkill>();
    }

    public void OnEnable()
    {
        isLocked = false; // 움직임 제한 해제

        // inputAction 활성화
        playerinputAction.Enable();

        // PlayerCtrl 변수 변경 이벤트
        playerSkill.RequestMoveSpeed += OnSetMoveSpeed;
        playerSkill.RequestAnimTrigger += OnSetAnimTrigger;
        playerSkill.RequestAnimSpeed += OnSetAnimSpeed;

        playerSkill.RequestWolfAnimTrigger += OnSetWolfAnimTrigger;
        playerSkill.RequestWolfState += OnSetWolfState;
        playerSkill.RequestWolfStartAttack += SetWolfAttackCoolTime;

        // PlayerInputAction 이벤트
        playerinputAction.Player.Jump.performed += OnJump;
        playerinputAction.Player.PlayPiri.started += OnStartPiri;
        playerinputAction.Player.PlayPiri.canceled += OnReleasePiri;
        playerinputAction.Wolf.Move.performed += OnWolfMove;
        playerinputAction.Wolf.Attack.performed += OnWolfAttack;
    }

    public void OnDisable()
    {
        isLocked = true;// 대화시 움직임 못 움직이는 상태

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
        playerSkill.OnUpdateStageData(); // 연결된 음원 딕셔너리에 초기화
    }

    void Update()
    {
        if (isLocked) return; // 행동 제한 변수 활성화시 제한

        OnPlaySoftPiri(); // 평화의 악장 연주 차징 확인
        OnPlayerMove(); // 이동 구현
        OnWolfHide(); // 늑대 숨김 구현
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

    private void OnStartInteract(InputAction.CallbackContext context)
    {
        moveSpeed = 0f; // 이동 제한
        isLocked = true;
        playerinputAction.Player.PlayPiri.Disable(); // 피리 사용 제한
    }

    private void OnStopInteract(InputAction.CallbackContext context)
    {
        moveSpeed = 5f; // 이동 제한 해제
        isLocked = false;
        playerinputAction.Player.PlayPiri.Enable(); // 피리 사용 제한 해제
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

    //여기서부터 늑대 선언부
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

        if (wolfExitTimer >= 4.0f) // 아무런 동작 없이 4초 이상 흐르면 실행
        {
            StartCoroutine(playerSkill.WolfHide(false)); // 늑대 자동 퇴장
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

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            wolfAttackCool.fillAmount = fill;
            yield return null;
        }
        wolfAttackCool.fillAmount = 1f;
    }

    private IEnumerator OnDamagedStart() // 소녀 피격 시작 함수
    {
        isDamaged = true; // 피격상태 시작

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

    public void OnDamagedEnd() // 소녀 피격 종료 함수
    {
        // Player 레이어(7번)와 Enemy 레이어(6번) 사이 충돌을 다시 허용
        Physics2D.IgnoreLayerCollision(7, 6, false);

        spriteRenderer.color = originColor;

        // 피격시 연주, 에코가드, 정화의 걸음 복구
        playerinputAction.Player.PlayPiri.Enable();

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
                if (!enemy.attackMode || enemy.isStune || enemy.isDead) return; // 적의 공격 모드가 false or 스턴, 사망 상태일 경우 충돌 X

                if (currentWolfState != WolfState.Damaged) // 늑대 보호 가능
                {
                    StartCoroutine(OnDamagedStart()); // 소녀 피격 상태 구현
                    OnWolfGuard(); // 가드 실행
                }
                else
                {
                    GameManager.Instance.StartCoroutine(GameOver()); // 게임 오버 실행
                }
            }
            else
            {
                Debug.Log("해당 적은 EnemyBase 클래스를 상속하지 않았습니다! 연결해유");
            }
        }
        else if (collision.gameObject.CompareTag("EnemyAttack"))
        {
            if (isDamaged) return; // 소녀 피격 상태시 충돌 X

            if (isPressingPiri) // 평화의 연주중이었을 경우 캔슬
            {
                playerSkill.PlaySoftPiriCanceled();
            }

            if (currentWolfState != WolfState.Damaged) // 늑대 보호 가능
            {
                StartCoroutine(OnDamagedStart()); // 소녀 피격 상태 구현
                OnWolfGuard(); // 가드 실행
            }
            else
            {
                GameManager.Instance.StartCoroutine(GameOver()); // 게임 오버 실행
            }
        }
        else if (collision.gameObject.CompareTag("SavePoint"))
        {
            savePoint = collision.transform.position; // 세이브 포인트 저장
        }
    }
}