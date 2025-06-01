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
    private GhoulCamp ghoulCamp; //@���� ķ�� ��ũ��Ʈ
    public SpriteRenderer spriteRenderer; // Sprite ������
    private PlayerInputAction playerinputAction; // ��ȭ�� Input ��� ���
    private float moveSpeed = 5f; // �̵��ӵ�
    private float jumpForce = 12f; // ������
    private bool isGrounded = true; // ���� ����
    private bool isPressingPiri = false; // �Ǹ� ���� ����
    private bool isBow = false; //@���� ����
    private float campTimer = 0f; //@CampTimer
    private GameObject savePoint; //@savePoint

    public GameObject wolf; // ���� ���� ������Ʈ
    public Animator wolfAnimator; // ���� �ִϸ�����
    public float wolfExitTimer = 0f; // ���� Hide Ÿ�̸�, 5f�� �Ǹ� Hide����
    public WolfState currentWolfState = WolfState.Idle; // ���� ���� ���� Ȯ�� (WolfState Ŭ����)

    public Image wolfAttackCool; // ���� ���� ��Ÿ�� UI 
    private Coroutine wolfAttackCoolRoutine; // ���� ���� ��Ÿ�� �ڷ�ƾ

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
        // inputAction Ȱ��ȭ
        playerinputAction.Enable();

        // PlayerCtrl ���� ���� �̺�Ʈ
        playerSkill.RequestMoveSpeed += OnSetMoveSpeed;
        playerSkill.RequestAnimTrigger += OnSetAnimTrigger;
        playerSkill.RequestAnimSpeed += OnSetAnimSpeed;

        playerSkill.RequestWolfAnimTrigger += OnSetWolfAnimTrigger;
        playerSkill.RequestWolfState += OnSetWolfState;
        playerSkill.RequestWolfStartAttack += SetWolfAttackCoolTime;

        // PlayerInputAction �̺�Ʈ
        playerinputAction.Player.Jump.performed += OnJump;
        playerinputAction.Player.PlayPiri.started += OnStartPiri;
        playerinputAction.Player.PlayPiri.canceled += OnReleasePiri;

        playerinputAction.Wolf.Move.performed += OnWolfMove;
        playerinputAction.Wolf.Attack.performed += OnWolfAttack;
    }

    private void OnDisable()
    {
        // inputAction ��Ȱ��ȭ
        playerinputAction.Disable();
    }

    // �ҳ� ���� �̺�Ʈ
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

    // ���� ���� �̺�Ʈ
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
        playerSkill.OnUpdateStageData(); // ����� ���� ��ųʸ��� �ʱ�ȭ
    }

    void Update()
    {
        OnPlaySoftPiri(); // ��ȭ�� ���� ���� ��¡ Ȯ��
        OnPlayerMove(); // �̵� ����
        OnWolfHide(); // ���� ���� ����
    }

    private void OnPlayerMove()
    {
        float h = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.S)) //@@
        {
            h = 0f;
            isBow = true;
            //Debug.Log("����!");
        }
        else
        {
            isBow = false;
        }

        // �¿� �̵�
        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        if (h != 0)
            animator.SetBool("isMove", true);
        else
            animator.SetBool("isMove", false);

        // �¿� ����
        if (h > 0)
            spriteRenderer.flipX = false;
        else if (h < 0)
            spriteRenderer.flipX = true;
    }

    private void OnJump(InputAction.CallbackContext context) // ����
    {
        if (isGrounded && !isPressingPiri) // ������ + ���ְ� �ƴҶ��� ���� ����
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            animator.SetTrigger("isJump");
        }
    }

    private void OnJumpEnd() // ���� �ִϸ��̼� �����, ��� �� ���·� ����
    {
        animator.speed = 0f;
    }

    private void OnStartPiri(InputAction.CallbackContext context) // ���ֹ�ư�� ������ �� ����
    {
        if (isGrounded) // �����ÿ��� ����
        {
            playerSkill.StartPiri();
            isPressingPiri = true; // �Ǹ� ���� ����
        }
    }

    private void OnReleasePiri(InputAction.CallbackContext context) // ���ֹ�ư�� ������ �� ����
    {
        if (isGrounded && isPressingPiri) // ���� + ���ֽÿ��� ����
        {
            playerSkill.ReleasePiri();
            isPressingPiri = false; // �Ǹ� ���� ����
        }
    }

    private void OnPlaySoftPiri() // ��ȭ�� ���� ���� ��¡ Ȯ��
    {
        if (isPressingPiri) // �Ǹ� ���ֽÿ� Ȯ��
        {
            playerSkill.CheckSoftPiri();
        }
    }

    //���⼭���� ���� �����
    private void OnWolfMove(InputAction.CallbackContext context) // ���� ������ ����, ���콺 ��Ŭ�� �� ����
    {
        if (currentWolfState == WolfState.Hide) // ���밡 Hide ������ ��, ���� ����
        {
            StartCoroutine(playerSkill.WolfAppear()); // ���� ���� ����
            wolfExitTimer = 0f;
            currentWolfState = WolfState.Idle;
        }
        else if (currentWolfState == WolfState.Idle)// ���밡 Hide����x, ���� ��ġ -> ���� ���ο� ��ġ ����
        {
            StartCoroutine(playerSkill.WolfAppear()); // ���� ���� ����
            wolfExitTimer = 0f;
            currentWolfState = WolfState.Idle;
        }
    }

    private void OnWolfGuard() // ���� ���� ����
    {
        playerSkill.WolfGuard();
    }

    private void OnWolfAttack(InputAction.CallbackContext context) // ���� ���� ����, ���콺 ��Ŭ�� �� ����
    {
        if (currentWolfState == WolfState.Idle) // Hide���°� �ƴҶ��� ����
        {
            StartCoroutine(playerSkill.WolfAttack());
            wolfExitTimer = 0f;
        }
    }
    private void OnWolfHide() // ���� Hide ����
    {
        if (currentWolfState != WolfState.Idle) return;

        if (wolfExitTimer >= 4.0f) // �ƹ��� ���� ���� 4�� �̻� �帣�� ����
        {
            StartCoroutine(playerSkill.WolfHide(false)); // ���� �ڵ� ����
        }
        else if (currentWolfState == WolfState.Idle) // ���� ���� ��, wolfExitTimer ��� ����
        {
            wolfExitTimer += Time.deltaTime;
        }
    }

    private void SetWolfAttackCoolTime(float duration)
    {
        // �ߺ� ���� ����
        if (wolfAttackCoolRoutine != null)
            StopCoroutine(wolfAttackCoolRoutine);

        wolfAttackCoolRoutine = StartCoroutine(WolfAttackCooldownRoutine(duration));
    }

    private IEnumerator WolfAttackCooldownRoutine(float duration) // duration�� ���� ���� ��Ÿ��
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
        if (collision.gameObject.CompareTag("Ground")) // �ٴڰ� �浹�� �� �ʱ�ȭ
        {
            animator.speed = 1f; // �ٽ� �ִϸ��̼� ����
            isGrounded = true;
        }
        if (collision.gameObject.CompareTag("Enemy")) // ���� �浹�� ������ or ����
        {
            if (currentWolfState != WolfState.Damaged) // ���� ��ȣ ����
            {
                OnWolfGuard(); // ���� ����
            }
            else
            {
                Debug.Log("�ҳ� �ǰ�! GameOver...");
                this.transform.position = savePoint.transform.position; //@@
            }
        }
        if (collision.gameObject.CompareTag("Fall")) //@@
        {
            Debug.Log("�ҳ� ����! GameOver...");
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