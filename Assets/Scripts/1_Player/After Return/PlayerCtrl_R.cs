using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Build;
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
    public SpriteRenderer spriteRenderer; // Sprite ������
    public PlayerInputAction_R playerinputAction; // ��ȭ�� Input ��� ���
    private PlayerSkill_R playerSkill;
    
    [SerializeField] private AnimatorOverrideController playPiriContorller; // ������ �ִϸ�����(�Ǹ� ���)
    private RuntimeAnimatorController defaultController; // �⺻ �ִϸ�����
    [SerializeField] private Transform groundCheck; // ĳ���� �߹ٴ� ��ġ
    [SerializeField] private LayerMask groundLayer; // �ٴ����� ������ Layer
    public Image echoGuardCoolBG; // ���ڰ��� ��Ÿ�� UI 
    public Image echoGuard;
    public Image echoGuardCool;
    public Image purifyStepCoolBG; // ��ȭ�� ���� ��Ÿ�� UI 
    public Image purifyStep;
    public Image purifyStepCool;
    public Image gameoverFade;
    public Image gameoverBase;
    public Text gameoverText;
    public Text retryText;
    public Text homeText;
    public Text encourageText;

    public GameObject SealUI; // �ҳ� ��ų ���� ǥ�� UI
    public Vector3 savePoint; // ���� ������������ ����� �ӽ� ���̺� ����Ʈ

    float h; // �÷��̾� �¿� �̵���
    public float moveSpeed = 5f; // �̵��ӵ�
    private float jumpForce = 14f; // ������
    private bool isGrounded = true; // ���� ����
    private bool isReadyPiri = true; // �Ǹ� ���� ���� ���� 
    public bool isPeaceMelody = false; // ��ȭ�� ���� ���� ������
    private bool isDamaged = false; // ���� �ǰݻ��� ����
    public float damagedTime = 1f; // �ǰ� ���� ���� �ð�
    private float blinkInterval = 0.15f; // �ǰݽ� �ѹ� �����ϴ� �ð�
    private Color originColor; // �ҳ� ��������Ʈ ����
    private Color damagedColor = new Color(0.2f, 0.2f, 0.2f); // �ҳ� �ǰݽ� ������ �� ����
    private Coroutine echoGuardCoolRoutine; // ���ڰ��� ��Ÿ�� �ڷ�ƾ
    private Coroutine purifySteoCoolRoutine; // ��ȭ�� ���� ��Ÿ�� �ڷ�ƾ
    private Coroutine SealAttackRoutine; // ���� ���� ���� �ڷ�ƾ
    private Coroutine speedRoutine; // �̵��ӵ� ���� �ڷ�ƾ
    public bool isLocked; // ��ȣ�ۿ�� �ൿ ����
    private bool isSealed; // ���� �ɷ� ���� ��������
    public bool isCovered = false; // ���󹰿� ���� ���

    public GameObject saveButton;
    public GameObject MainButton;
    public GameObject AttackEffect; // ���� ���� ����Ʈ ������Ʈ

    // �Ǹ� ���� ���� ������Ƽ
    [SerializeField] private bool _isPressingPiri = false;
    public override bool isPressingPiri // �Ǹ� ���� ����
    {
        get => _isPressingPiri;
        set
        {
            if (_isPressingPiri == value) return;
            _isPressingPiri = value;

            // �Ǹ� �ִϸ��̼� ��ȯ
            animator.runtimeAnimatorController = value ? playPiriContorller : defaultController;
        }
    }

    // ���� ���� ���� ������Ƽ
    [SerializeField] private bool _isEchoGuarding = false;
    public bool IsEchoGiarding // ���� ���� ����
    {
        get => _isEchoGuarding;
        set
        {
            if (_isEchoGuarding == value) return;
            _isEchoGuarding = value;

            // �Ǹ� �ִϸ��̼� ��ȯ
            if (!isPressingPiri)
            {
                animator.runtimeAnimatorController = value ? playPiriContorller : defaultController;
            }
        }
    }

    // ��ȭ�� ���� ���� ������Ƽ
    [SerializeField] private bool _isPurifying = false;
    public bool isPurifying // ��ȭ�� ���� ����
    {
        get => _isPurifying;
        set
        {
            if (_isPurifying == value) return;
            _isPurifying = value;

            // ��ȭ�� ���� �ִϸ��̼� ��ȯ
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
        isLocked = false; // ������ ���� ����

        // inputAction Ȱ��ȭ
        playerinputAction.Enable();

        // PlayerCtrl ���� ���� �̺�Ʈ
        playerSkill.RequestSetMoveSpeedAndTime += OnSetMoveSpeedAndTime;
        playerSkill.RequestSetMoveSpeed += OnSetMoveSpeed;
        playerSkill.RequestAnimTrigger += OnSetAnimTrigger;
        playerSkill.RequestEchoGuardStart += SetEchoGuardCoolTime;
        playerSkill.RequestEchoGuardingState += OnSetEchoGuarding;
        playerSkill.RequestPuriFyStepStart += SetPurifyStepCoolTime;
        playerSkill.RequestisPurifing += SetisPurifing;
        playerSkill.RequestSetSpriteColor += OnSetSpriteColor;
        playerSkill.RequestPressingPiriState += OnSetPressingPiri;
        playerSkill.RequestPeaceMelodyActived += OnPeaceMelodyActived;
        
        // PlayerInputAction �̺�Ʈ
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
        isLocked = true;// ��ȭ�� ������ ���� ����

        animator.SetBool("isMove", false); // ��ȭ�� Idle ���·� ��ȯ

        // inputAction ��Ȱ��ȭ
        playerinputAction.Disable();
    }

    // �ҳ� ���� �̺�Ʈ
    public void OnSetMoveSpeed(float speed)
    {
        if (speedRoutine != null)
        {
            StopCoroutine(speedRoutine);
            speedRoutine = null;
        }
        moveSpeed = speed;
    }

    public void OnSetMoveSpeedAndTime(float speed, float duration) // �̵��ӵ� ���� �Լ�
    {
        if (speedRoutine != null)
            StopCoroutine(speedRoutine);

        moveSpeed = speed;
        speedRoutine = StartCoroutine(RestoreMoveSpeed(duration));
    }

    private IEnumerator RestoreMoveSpeed(float delay) // �̵��ӵ� ���� �ڷ�ƾ
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

    private void OnSetEchoGuarding(bool state)
    {
        IsEchoGiarding = state;
    }

    private void OnPeaceMelodyActived(bool state)
    {
        isPeaceMelody = state;
    }
    
    private void Start()
    {
        isLocked = false;

        playerSkill.OnUpdateStageData(); // ����� ���� ��ųʸ��� �ʱ�ȭ
        GameManager.Instance.isReturned = true; // ȸ�� �ķ� ���� ����
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer); // �ٴڿ� ���� ���� Ȯ��

        if (isLocked || isPushed) return; // �ൿ ���� ���� Ȱ��ȭ�� ����

        OnPlaySoftPiri(); // ��ȭ�� ���� ���� ��¡ Ȯ��
        UpdatePurifyStep(); // ��ȭ�� ������ ���� ����
        OnPlayerMove(); // �̵� ����
    }

    private void OnDrawGizmosSelected() // �ٴ� �浹 Ȯ�� ����� ǥ��
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }

    private void OnPlayerMove()
    {
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
        else if (h < 0)
            spriteRenderer.flipX = true;
    }

    private void OnESC(InputAction.CallbackContext context)
    {
        saveButton.SetActive(!saveButton.activeSelf); // ���̺� ��ư ���
        MainButton.SetActive(!MainButton.activeSelf); // ���� ��ư ���
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

    private void OnJumpEnd() // ���� �ִϸ��̼� �����, ��� �� ���·� ����
    {
        animator.speed = 0f;
    }

    private void OnSetSpriteColor(float brightness) // ������ ����� ���� ���� ���� �� sprite ����
    {
        spriteRenderer.color = new Color(brightness, brightness, brightness, spriteRenderer.color.a);
        originColor = spriteRenderer.color; // ���� ������ ����
    }

    private void OnStartPiri(InputAction.CallbackContext context) // ���ֹ�ư�� ������ �� ����
    {
        if (isGrounded && isReadyPiri) // ����, �Ǹ� �غ�ÿ��� ����
        {
            animator.runtimeAnimatorController = playPiriContorller; // �Ǹ��δ� �ִϸ����ͷ� ����
            StartCoroutine(StartPiriCool()); // �Ǹ� ���� ��Ÿ�� ����(0.2f);
            playerSkill.StartPiri();
            isPressingPiri = true; // �Ǹ� ���� ����
        }
    }

    private IEnumerator StartPiriCool()
    {
        isReadyPiri = false;

        yield return new WaitForSeconds(1f);
        isReadyPiri = true;
    }

    private void OnReleasePiri(InputAction.CallbackContext context) // ���ֹ�ư�� ������ �� ����
    {
        if (isGrounded && isPressingPiri) // ���� + ���ֽÿ��� ����
        {
            playerSkill.ReleasePiri();
        }
    }

    private void OnPlaySoftPiri() // ��ȭ�� ���� ���� ��¡ Ȯ��
    {
        if (isPressingPiri) // �Ǹ� �������� ��
        {
            playerSkill.CheckSoftPiri();
        }
    }

    private void OnEchoGuard(InputAction.CallbackContext context) // ���콺 ��Ŭ���� ���ڰ��� ����
    {
        StartCoroutine(playerSkill.EchoGuard());
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
        echoGuard.gameObject.SetActive(false);
        echoGuardCool.gameObject.SetActive(true);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            echoGuardCoolBG.fillAmount = fill;
            yield return null;
        }
        echoGuardCoolBG.fillAmount = 1f;
        echoGuard.gameObject.SetActive(true);
        echoGuardCool.gameObject.SetActive(false);
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
        if (!isPurifying) return;

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

    private IEnumerator PurifyStepCooldownRoutine(float duration) // duration�� ��ȭ�� ���� ��Ÿ��
    {
        float elapsed = 0f;
        purifyStepCool.gameObject.SetActive(true);
        purifyStep.gameObject.SetActive(false);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            purifyStepCoolBG.fillAmount = fill;
            yield return null;
        }
        purifyStepCoolBG.fillAmount = 1f;
        purifyStepCool.gameObject.SetActive(false);
        purifyStep.gameObject.SetActive(true);
    }


    // �浹 ���� ��� ����
    private IEnumerator OnEnemySealAttack(float enemyDamage, float enemyPosX)
    {
        if (isPurifying) // ��ȭ�� ������ ���� ���� ����
        {
            Debug.Log("�ҳడ �ɷ� ������ �����մϴ�."); 
            yield break;
        }

        SealUI.SetActive(true); // ��ų ���� UI ǥ��
        isPushed = true; // �аݻ��� ����
        animator.SetTrigger(PlayerAnimTrigger.Hit);
        rb.AddForce(Vector2.right * -Mathf.Sign(enemyPosX - transform.position.x) * 30, ForceMode2D.Impulse); // �ǰݽ� �ݴ�������� ��¦ �аݵ�
        
        yield return new WaitForSeconds(0.1f);
        isPushed = false; // �аݻ��� ����

        if (GameManager.Instance.Pollution < 100f) // �������� �� ���� �ʾ��� ���
        {
            // �ǰݽ� ����, ���ڰ���, ��ȭ�� ���� ��Ȱ��ȭ
            playerinputAction.Player.PlayPiri.Disable();
            playerinputAction.Player.EchoGuard.Disable();
            if (!isPurifying) // ��ȭ�� ���� ����� �ǰݽ� ���� 
            {
                playerinputAction.Player.PurifyingStep.Disable();
            }

            yield return new WaitForSeconds(5f); // 5�� ���� �ҳ� �ɷ� ����

            // �ǰݽ� ����, ���ڰ���, ��ȭ�� ���� ����
            playerinputAction.Player.PlayPiri.Enable();
            playerinputAction.Player.EchoGuard.Enable();
            playerinputAction.Player.PurifyingStep.Enable();

            SealUI.SetActive(false); // ��ų ���� UI ����
        }
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
    private IEnumerator OnDamagedStart(float enemyDamage, float enemyPosX) // �ҳ� �ǰ� ���� �Լ�
    {
        Debug.Log("�ҳ� �ǰ�! �ҳ��� �������� �����մϴ�!");
        StartCoroutine(Shake(0.2f, 0.1f));
        Instantiate(AttackEffect, transform.position, Quaternion.identity); // �ǰ� ����Ʈ ����

        isPeaceMelody = false;
        isPressingPiri = false;
        if (isPurifying) // ��ȭ�� ������ �ǰ� ���� ����
        {
            isDamaged = true; // �ǰݻ��� ����
        }
        else // ��ȭ�� ������ �ƴ� ���
        {
            isDamaged = true; // �ǰݻ��� ����
            isPushed = true; // �аݻ��� ����

            animator.SetTrigger(PlayerAnimTrigger.Hit);
            rb.AddForce(Vector2.right * ((enemyPosX - transform.position.x > 0) ? -1 : 1) * 13, ForceMode2D.Impulse); // �ǰݽ� �ݴ�������� ��¦ �аݵ�
        }

        GameManager.Instance.AddPolution(enemyDamage); // �� ���ݷ¸�ŭ ������ ����
        yield return new WaitForSeconds(0.1f);
        isPushed = false; // �аݻ��� ����

        if (GameManager.Instance.Pollution < 100f) // �������� �� ���� �ʾ��� ���
        {
            // Player ���̾�(7��)�� Enemy ���̾�(6��) ���� �浹�� ����
            Physics2D.IgnoreLayerCollision(7, 6, true);

            // �ǰݽ� ����, ���ڰ���, ��ȭ�� ���� ��Ȱ��ȭ
            playerinputAction.Player.PlayPiri.Disable();
            playerinputAction.Player.EchoGuard.Disable();
            if (!isPurifying) // ��ȭ�� ���� ����� �ǰݽ� ���� 
            {
                playerinputAction.Player.PurifyingStep.Disable();
            }

            // �����̴� ȿ��
            float elapsed = 0f; // ����� ����
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
        else // �������� ���� á�� ��� 
        {
            StartCoroutine(GameOver()); // ���� ����
        }
    }

    public void OnDamagedEnd() // �ҳ� �ǰ� ���� �Լ�
    {
        // Player ���̾�(7��)�� Enemy ���̾�(6��) ���� �浹�� �ٽ� ���
        Physics2D.IgnoreLayerCollision(7, 6, false);

        spriteRenderer.color = originColor;

        if (!isSealed) // �ɷ� ���� ���°� �ƴ� ����
        {
            // �ǰݽ� ����, ���ڰ���, ��ȭ�� ���� ����
            playerinputAction.Player.PlayPiri.Enable();
            playerinputAction.Player.PurifyingStep.Enable();
        }
        playerinputAction.Player.EchoGuard.Enable();
        
        if(!isPurifying)
            moveSpeed = 5f;
        
        isDamaged = false; // �ǰݻ��� ����
    }

    private IEnumerator GameOver() // GameOver�� �Լ�
    {
        Debug.LogWarning("�ҳడ �������ϴ�.. GameOver");
        Debug.LogWarning("GameOverǥ�� + ���ӿ��� UI ǥ�� �߰�");
        gameoverFade.gameObject.SetActive(true);

        StartCoroutine(FadeImage(gameoverFade, 0.8f, 1f, true));
        


        Time.timeScale = 0.5f; // �ð� �������� ����
        yield return new WaitForSeconds(1f);

        Time.timeScale = 1f;
        GameManager.Instance.Pollution = 0f;
        GameManager.Instance.AddPolution(0f); // ������ UI �ʱ�ȭ
    }

    public void ActivatedSealMode(bool state) // �÷��̾� �ɷ� ���� ���� ����
    {
        if (state)
        {
            isSealed = true;
            // �ǰݽ� ����, ��ȭ�� ���� ����
            playerinputAction.Player.PlayPiri.Disable();
            playerinputAction.Player.PurifyingStep.Disable();

            SealUI.SetActive(true); // ��ų ���� UI ����
        }
        else
        {
            isSealed = false;
            // �ǰݽ� ����, ��ȭ�� ���� ����
            playerinputAction.Player.PlayPiri.Enable();
            playerinputAction.Player.PurifyingStep.Enable();

            SealUI.SetActive(false); // ��ų ���� UI ����
        }
    }


    private void OnCollisionEnter2D(Collision2D collision) // ���� �浹�� ����
    {
        if (collision.gameObject.CompareTag("Ground")) // �ٴڰ� �浹�� �� �ʱ�ȭ
        {
            animator.speed = 1f; // �ٽ� �ִϸ��̼� ����
        }
        else if (collision.gameObject.CompareTag("Fall")) // �߶� ������
        {
            Debug.Log("�ֱ� ���̺� ����Ʈ�� �̵�, ������ ����");
            GameManager.Instance.AddPolution(10f);
            transform.position = savePoint;
        }
    }

    private void OnCollisionStay2D(Collision2D collision) // ���� �浹�� ���� ���� ����
    {
        if (collision.gameObject.CompareTag("Ground")) 
        {
            animator.speed = 1f; // �ٽ� �ִϸ��̼� ����
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // �ֵ� �浹�� Trigger���� ���
    {
        if (isLocked) return; // ��ũ��Ʈ ���϶��� �浹 ����


        //if (collision.gameObject.CompareTag("Enemy")) // ���� �浹�� ������ or ����
        //{
        //    EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>(); // Enemy �⺻ Ŭ���� ������
        //    if (enemy != null)
        //    {
        //        if (!enemy.attackMode || enemy.isStune || enemy.isDead || isDamaged) return; // ���� ���� ��尡 false or ����, ��� ������ ��� or �ҳ� �ǰ� ���½� �浹 X


        //        if (isPeaceMelody)
        //        {
        //            playerSkill.PlaySoftPiriCanceled(); // ��ȭ�� �������̾��� ��� ĵ��
        //        }
        //        float damage = enemy.damage;
        //        float position = collision.transform.position.x;

        //        StartCoroutine(OnDamagedStart(damage * (isPurifying ? 0.2f : 1), position)); // �ǰ� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
        //    }
        //    else
        //    {
        //        Debug.Log("�ش� ���� EnemyBase Ŭ������ ������� �ʾҽ��ϴ�! ��������");
        //    }
        //}
        //else 
        if (collision.gameObject.CompareTag("EnemyAttack")) // ���ڰ��� ������ �׷α� �������� ���� �� �ִ� Attack
        {
            if (isDamaged) return; // �ҳ� �ǰ� ���½� �浹 X

            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();
            if (enemyAttack != null)
            {
                float damage = enemyAttack.attackDamage;
                float position = collision.transform.position.x;
                GameObject origin = enemyAttack.enemyOrigin;
                Collider2D attack = collision;

                if (playerSkill.isEchoGuarding) // ���ڰ��� ���½� �� �׷α� ������ ����
                {
                    Debug.Log("[EnemyAttack] ������ ����� ���� �׷α� �������� ���Դϴ�!");
                    StartCoroutine(origin.GetComponent<EnemyBase>().EchoGuardSuccess(attack));
                    origin.GetComponent<EnemyBase>().CancelAttack();
                    return;
                }

                if (isPeaceMelody) // ��ȭ�� �������̾��� ��� ĵ��
                {
                    isPeaceMelody = false;
                    isPressingPiri = false;
                    playerSkill.PlaySoftPiriCanceled();
                }
                StartCoroutine(OnDamagedStart(damage * (isPurifying ? 0.2f : 1), position)); // �ǰ� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
            }
            else
            {
                Debug.Log("�ش� ���� EnemyBase Ŭ������ ������� �ʾҽ��ϴ�! ��������");
            }
        }

        else if (collision.gameObject.CompareTag("EnemyAttack_NoGroggy")) // ���ڰ��� �����ÿ��� �׷α� �������� ���� �� ���� Attack
        {
            if (isDamaged) return; // �ҳ� �ǰ� ���½� �浹 X

            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();
            if (enemyAttack != null)
            {
                float damage = enemyAttack.attackDamage;
                float position = collision.transform.position.x;
                GameObject origin = enemyAttack.enemyOrigin;
                Collider2D attack = collision;

                if (playerSkill.isEchoGuarding) // ���ڰ��� ���½� �� �׷α� ������ ����
                {
                    Debug.Log("[EnemyAttack_NoGroggy] ������ ����� ���� ��� �����մϴ�!");
                    if (origin != null)
                    {
                        origin.GetComponent<EnemyBase>().EchoGuardSuccess_NoGloogy();
                        origin.GetComponent<EnemyBase>().CancelAttack();
                    }
                    return;
                }

                if (isPeaceMelody) // ��ȭ�� �������̾��� ��� ĵ��
                {
                    isPeaceMelody = false;
                    isPressingPiri = false;
                    playerSkill.PlaySoftPiriCanceled();
                }
                Debug.LogWarning("Enemy �� �׷α�� �浹�߽�");
                StartCoroutine(OnDamagedStart(damage * (isPurifying ? 0.2f : 1), position)); // �ǰ� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
            }
            else
            {
                Debug.Log("�ش� ���� EnemyBase Ŭ������ ������� �ʾҽ��ϴ�! ��������");
            }
        }
        else if (collision.gameObject.CompareTag("EnemySealAttack")) // �÷��̾� �ɷ� ���� Attack
        {
            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();

            if (enemyAttack != null)
            {
                if (isDamaged) return; // �ҳ� �ǰ� ���½� �浹 X

                float damage = enemyAttack.attackDamage;
                float position = collision.transform.position.x;
                GameObject origin = enemyAttack.enemyOrigin;
                Collider2D attack = collision;

                if (playerSkill.isEchoGuarding) // ���ڰ��� ���½� �� �׷α� ������ ����
                {
                    Debug.Log("[EnemyAttack] ������ ����� ���� �׷α� �������� ���Դϴ�!");
                    StartCoroutine(origin.GetComponent<EnemyBase>().EchoGuardSuccess(attack));
                    origin.GetComponent<EnemyBase>().CancelAttack();
                    moveSpeed = 5f;
                    return;
                }

                if (isPeaceMelody) // ��ȭ�� �������̾��� ��� ĵ��
                {
                    isPeaceMelody = false;
                    isPressingPiri = false;
                    playerSkill.PlaySoftPiriCanceled();
                }
                if (SealAttackRoutine != null)
                {
                    StopCoroutine(SealAttackRoutine); // ���� �������� ���� ���� ����
                }
                SealAttackRoutine = StartCoroutine(OnEnemySealAttack(damage * (isPurifying ? 0.2f : 1), origin.transform.position.x)); // ���ΰ��� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
            }
            else
            {
                Debug.Log("�ش� ���� EnemyBase Ŭ������ ������� �ʾҽ��ϴ�! ��������");
            }
        }
        else if (collision.gameObject.CompareTag("EnemyProjectile")) // ���� ���� ������ ���⸸ �ϴ� Attack
        {
            Debug.Log("�ҳడ ���� ����ü�� �������� �Խ��ϴ�.");

            if (isPeaceMelody) // ��ȭ�� �������̾��� ��� ĵ��
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

                StartCoroutine(OnDamagedStart(damage * (isPurifying ? 0.2f : 1), position)); // �ǰ� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
            }
        }
        else if (collision.gameObject.CompareTag("EnemySight"))
        {
            Debug.Log("���� �ҳฦ �߰��߽��ϴ�!!");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                //enemyBase.enemySight.GetComponent<SpriteRenderer>().enabled = false; // �� ��� ���� ǥ�� off
                enemyBase.isPatrol = false; // �� ���ݸ��� ��ȯ
            }
        }
        else if (collision.gameObject.CompareTag("EnemyAttackRange"))
        {
            Debug.Log("�ҳడ ���� ���� ���� �ȿ� ���ɴϴ�!");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.isAttackRange = true; // �� ���� ���� �Լ� ����
            }
        }
        else if (collision.gameObject.CompareTag("SavePoint"))
        {
            savePoint = collision.transform.position; // ���̺� ����Ʈ ����
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isLocked) return; // ��ũ��Ʈ ���϶��� �浹 ����

        if (collision.gameObject.CompareTag("EnemyAttackRange"))
        {
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.isAttackRange = true; // �� ���� ���� �Լ� ����
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isLocked) return; // ��ũ��Ʈ ���϶��� �浹 ����

        if (collision.gameObject.CompareTag("EnemyAttackRange"))
        {
            Debug.Log("�ҳడ ���� ���� ���� ������ ����ϴ�!");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.isAttackRange = false; // �� �÷��̾� �߰� �Լ� ����
            }
            Debug.Log("���� �ҳฦ �߰��մϴ�!");
        }
        else if (collision.gameObject.CompareTag("EnemySight"))
        {
            Debug.Log("�ҳడ ���� �þ� ������ ����ϴ�! ���� �߰��� ����ϴ�");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                //enemyBase.enemySight.GetComponent<SpriteRenderer>().enabled = true; // �� ��� ���� ǥ�� on
                enemyBase.isPatrol = true; // �� �÷��̾� �߰� �ߴ�
            }
        }
    }

    public IEnumerator FadeImage(Image image, float targetAlpha, float duration, bool isStart = false)
    {
        if (image == null) yield break;

        Color color = image.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            image.color = color;
            yield return null;
        }
        if(isStart == true )
        {
            StartCoroutine(FadeImage(gameoverBase, 1f, 2f));
            StartCoroutine(FadeText(gameoverText, 1f, 3f, true));
            StartCoroutine(FadeText(retryText, 1f, 3f));
            StartCoroutine(FadeText(homeText, 1f, 3f));
            StartCoroutine(FadeText(encourageText, 1f, 3f));
        }
        // ����: ��Ȯ�� ��ǥ������ ���߱�
        color.a = targetAlpha;
        image.color = color;
    }

    public IEnumerator FadeText(Text text, float targetAlpha, float duration, bool isStart = false)
    {
        if (text == null) yield break;

        Color color = text.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            text.color = color;
            yield return null;
        }

        if (isStart == true)
        {
            StartCoroutine(FadeText(retryText, 1f, 3f));
            StartCoroutine(FadeText(homeText, 1f, 3f));
            StartCoroutine(FadeText(encourageText, 1f, 3f));
        }

        // ����
        color.a = targetAlpha;
        text.color = color;
    }

    public void homeButton()
    {
        //StartCoroutine(PlayClickThenLoad(0));
        LoadingScene.LoadScene(0);
    }

    public void retryButton()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        //StartCoroutine(PlayClickThenLoad(currentScene));
        SceneManager.LoadScene(currentScene);
    }
    private IEnumerator PlayClickThenLoad(int sceneIndex)
    {
        UISoundManager.Instance.PlayClickSound();
        yield return new WaitForSeconds(UISoundManager.Instance.clickClip.length);
        LoadingScene.LoadScene(sceneIndex);
    }

    private IEnumerator PlayClickThenLoad(string sceneIndex)
    {
        UISoundManager.Instance.PlayClickSound();
        yield return new WaitForSeconds(UISoundManager.Instance.clickClip.length);
        SceneManager.LoadScene(sceneIndex);
    }
}