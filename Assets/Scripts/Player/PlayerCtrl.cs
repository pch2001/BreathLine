using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PlayerCtrl : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerSkill playerSkill;
    private GhoulCamp ghoulCamp; //@구울 캠프 스크립트
    public SpriteRenderer spriteRenderer; // Sprite 반전용
    private PlayerInputAction playerinputAction; // 강화된 Input 방식 사용
    private float moveSpeed = 5f; // 이동속도
    private float jumpForce = 12f; // 점프력
    private bool isGrounded = true; // 착지 여부
    private bool isPressingPiri = false; // 피리 연주 여부
    private bool isBow = false; //@숙임 여부
    private float campTimer = 0f; //@CampTimer
    private GameObject savePoint; //@savePoint

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

    private void OnEnable()
    {
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

    private void OnDisable()
    {
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
        OnPlaySoftPiri(); // 평화의 악장 연주 차징 확인
        OnPlayerMove(); // 이동 구현
        OnWolfHide(); // 늑대 숨김 구현
    }

    private void OnPlayerMove()
    {
        float h = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.S)) //@@
        {
            h = 0f;
            isBow = true;
            //Debug.Log("숙임!");
        }
        else
        {
            isBow = false;
        }

        // 좌우 이동
        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        if (h != 0)
            animator.SetBool("isMove", true);
        else
            animator.SetBool("isMove", false);

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
        if (currentWolfState == WolfState.Hide) // 늑대가 Hide 상태일 때, 늑대 등장
        {
            StartCoroutine(playerSkill.WolfAppear()); // 늑대 등장 구현
            wolfExitTimer = 0f;
            currentWolfState = WolfState.Idle;
        }
        else if (currentWolfState == WolfState.Idle)// 늑대가 Hide상태x, 기존 위치 -> 늑대 새로운 위치 등장
        {
            StartCoroutine(playerSkill.WolfAppear()); // 늑대 등장 구현
            wolfExitTimer = 0f;
            currentWolfState = WolfState.Idle;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // 바닥과 충돌시 값 초기화
        {
            animator.speed = 1f; // 다시 애니메이션 동작
            isGrounded = true;
        }
        if (collision.gameObject.CompareTag("Enemy")) // 적과 충돌시 데미지 or 가드
        {
            if (currentWolfState != WolfState.Damaged) // 늑대 보호 가능
            {
                OnWolfGuard(); // 가드 실행
            }
            else
            {
                Debug.Log("소녀 피격! GameOver...");
                this.transform.position = savePoint.transform.position; //@@
            }
        }
        if (collision.gameObject.CompareTag("Fall")) //@@
        {
            Debug.Log("소녀 낙사! GameOver...");
            this.transform.position = savePoint.transform.position; //@@
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("GhoulCamp") && isBow)
        {
            campTimer += Time.deltaTime;
            if (campTimer >= 2f)
            {
                ghoulCamp = other.gameObject.GetComponent<GhoulCamp>();
                StartCoroutine(ghoulCamp.Cure(true));
                campTimer = 0f;
            }
        }
        else
        {
            campTimer = 0f;
        }

        if (other.gameObject.CompareTag("Ignore"))
        {
            ghoulCamp = other.gameObject.transform.parent.GetComponent<GhoulCamp>();
            StartCoroutine(ghoulCamp.Cure(false));
        }
        if (other.gameObject.CompareTag("Seed") && Input.GetKeyDown(KeyCode.S))
        {
            GameManager.Instance.curedSeed++;
            other.gameObject.SetActive(false);
        }
        if (other.gameObject.CompareTag("SavePoint"))
        {
            savePoint = other.gameObject;
        }
    }
}