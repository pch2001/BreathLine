using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PlayerCtrl_R : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerSkill_R playerSkill;
    public SpriteRenderer spriteRenderer; // Sprite ������
    private PlayerInputAction_R playerinputAction; // ��ȭ�� Input ��� ���

    private Coroutine echoGuardCoolRoutine; // ���ڰ��� ��Ÿ�� �ڷ�ƾ
    public Image echoGuardCool; // ���ڰ��� ��Ÿ�� UI 

    private Coroutine purifySteoCoolRoutine; // ��ȭ�� ���� ��Ÿ�� �ڷ�ƾ
    public Image purifyStepCool; // ��ȭ�� ���� ��Ÿ�� UI 

    float h; // �÷��̾� �¿� �̵���
    public float moveSpeed = 5f; // �̵��ӵ�
    public float jumpForce = 12f; // ������
    private bool isGrounded = true; // ���� ����
    private bool isPressingPiri = false; // �Ǹ� ���� ����
    private bool isDamaged = false; // �÷��̾� �ǰ� ���½�
    public bool isPurifying = false; // ��ȭ�� ���� ����

    private bool dontmove = true;//�÷��� ������

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
        dontmove = true;// ��ȭ�� ������ ���߱� ���� true = �����̴� ����
        
        // inputAction Ȱ��ȭ
        playerinputAction.Enable(); 

        // PlayerCtrl ���� ���� �̺�Ʈ
        playerSkill.RequestMoveSpeed += OnSetMoveSpeed;
        playerSkill.RequestAnimTrigger += OnSetAnimTrigger;
        playerSkill.RequestAnimSpeed += OnSetAnimSpeed;
        playerSkill.RequestEchoGuardStart += SetEchoGuardCoolTime;
        playerSkill.RequestPuriFyStepStart += SetPurifyStepCoolTime;
        playerSkill.RequestisPurifing += SetisPurifing;

        // PlayerInputAction �̺�Ʈ
        playerinputAction.Player.Jump.performed += OnJump;
        playerinputAction.Player.PlayPiri.started += OnStartPiri; 
        playerinputAction.Player.PlayPiri.canceled += OnReleasePiri;
        playerinputAction.Player.EchoGuard.performed += OnEchoGuard;
        playerinputAction.Player.PurifyingStep.started += OnPurifyStepStart;
        playerinputAction.Player.PurifyingStep.canceled += OnPurifyStepStop;

    }

    public void OnDisable()
    {
        dontmove = false;// ��ȭ�� ������ ���߱� ���� faalse = �� �����̴� ����

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
    
    private void Start()
    {
        playerSkill.OnUpdateStageData(); // ����� ���� ��ųʸ��� �ʱ�ȭ
    }

    void Update()
    {
        OnPlaySoftPiri(); // ��ȭ�� ���� ���� ��¡ Ȯ��
        UpdatePurifyStep(); // ��ȭ�� ������ ���� ����
        OnPlayerMove(); // �̵� ����
    }

    private void OnPlayerMove()
    {
        if(!dontmove) return; // ��ũ��Ʈ �߻��� �̵� X

        if (!isPurifying) // ��ȭ�� ������ �Է� ���� ���� �� ��ġ�� �ִϸ��̼� ����
        {
            // �¿� ���� ����
            h = Input.GetAxisRaw("Horizontal");

            // �ִϸ��̼� ����
            if (h != 0)
                animator.SetBool("isMove", true);
            else
                animator.SetBool("isMove", false);
        }

        // �̵� ����
        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        // �¿� ����
        if (h > 0) 
            spriteRenderer.flipX = false;
        else if(h < 0) 
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

    private void OnEchoGuard(InputAction.CallbackContext context) // ���콺 ��Ŭ���� ���ڰ��� ����
    {
        if (!isPressingPiri) // �Ǹ� ���ֽð� �ƴ� ���
        {
            StartCoroutine(playerSkill.EchoGuard());
        } 
    }

    private void SetEchoGuardCoolTime(float duration)
    {
        // �ߺ� ���� ����
        if (echoGuardCoolRoutine != null)
            StopCoroutine(echoGuardCoolRoutine);

        echoGuardCoolRoutine = StartCoroutine(EchoGuardCooldownRoutine(duration));
    }

    private IEnumerator EchoGuardCooldownRoutine(float duration) // duration�� ���ڰ��� ��Ÿ��
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

    private void SetisPurifing(bool state) // ��ȭ�� ���� ���� ���� ����
    {
        isPurifying = state;
    }
    private void OnPurifyStepStart(InputAction.CallbackContext context) // ��ȭ�� ���� ���� �Լ�
    {
        if (!playerSkill.purifyStepReady) return; // ��ȭ�� ���� �غ� ���¿����� ����

        playerSkill.PurifyStepStart();
        playerinputAction.Player.PlayPiri.Disable(); // ��ȭ�� ���� �߿��� �Ǹ� ���x
        playerinputAction.Player.EchoGuard.Disable(); // ��ȭ�� ���� �߿��� ���ڰ��� ���x
    }

    public void PurifyStepStop() // ��ȭ�� ������ �ڵ����� ����Ǿ����� ȣ���� �� �ֵ��� ���� ����
    {
        if (!isPurifying) return; // ��ȭ�� ���� ���� �ߺ����� ����

        playerSkill.PurifyStepStop();
        playerinputAction.Player.PlayPiri.Enable(); // �Ǹ� ��� ����
        playerinputAction.Player.EchoGuard.Enable(); // ���ڰ��� ����
    }

    private void OnPurifyStepStop(InputAction.CallbackContext context) // ��ȭ�� ���� ���� �Լ�
    {
        PurifyStepStop();
    }

    private void UpdatePurifyStep() // ��ȭ�� ���� ���� �Լ�
    {
        if(!isPurifying) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        h = (mousePos.x - transform.position.x) > 0 ? 1 : -1;

    }

    private void SetPurifyStepCoolTime(float duration)
    {
        // �ߺ� ���� ����
        if (purifySteoCoolRoutine != null)
            StopCoroutine(purifySteoCoolRoutine);

        purifySteoCoolRoutine = StartCoroutine(PurifyStepCooldownRoutine(duration));
    }

    private IEnumerator PurifyStepCooldownRoutine(float duration) // duration�� ���ڰ��� ��Ÿ��
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

    private IEnumerator OnDamaged() // �ҳ� �ǰ� ���� �Լ�
    {
        isDamaged = true; // �ҳ� �ǰ� ���� Ȱ��ȭ

        // �����̴� ȿ��
        while (isDamaged) 
        {

            spriteRenderer.color = new Color(0.2f, 0.2f, 0.2f, spriteRenderer.color.a);
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.color = new Color(0.2f, 0.2f, 0.2f, spriteRenderer.color.a);
        }

        GameManager.Instance.AddPolution(0f); // �÷��̾� ���� ����
    }

    public void OnDamagedEnd() // �ҳ� �ǰ� ���� �Լ�
    {
        isDamaged = false; // �ҳ� �ǰ� ���� ����
    }

    private void OnCollisionEnter2D(Collision2D collision) // ���� �浹�� ����
    {
        if (collision.gameObject.CompareTag("Ground")) // �ٴڰ� �浹�� �� �ʱ�ȭ
        {
            animator.speed = 1f; // �ٽ� �ִϸ��̼� ����
            isGrounded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // �ֵ� �浹�� Trigger���� ���
    {
        if (collision.gameObject.CompareTag("Enemy")) // ���� �浹�� ������ or ����
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>(); // Enemy �⺻ Ŭ���� ������
            if (enemy != null)
            {
                if (!enemy.attackMode || enemy.isStune || enemy.isDead) return; // ���� ���� ��尡 false or ����, ��� ������ ��� �浹 X

                Debug.Log("�ҳ� �ǰ�! �ҳ��� �������� �����մϴ�!");
                animator.SetTrigger(PlayerAnimTrigger.Hit);
            }
            else
            {
                Debug.Log("�ش� ���� EnemyBase Ŭ������ ������� �ʾҽ��ϴ�! ��������");
            }
        }
    }
}