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
    public SpriteRenderer spriteRenderer; // Sprite ������
    public PlayerInputAction_R playerinputAction; // ��ȭ�� Input ��� ���

    private Coroutine echoGuardCoolRoutine; // ���ڰ��� ��Ÿ�� �ڷ�ƾ
    public Image echoGuardCool; // ���ڰ��� ��Ÿ�� UI 

    private Coroutine purifySteoCoolRoutine; // ��ȭ�� ���� ��Ÿ�� �ڷ�ƾ
    public Image purifyStepCool; // ��ȭ�� ���� ��Ÿ�� UI 

    private Coroutine SealAttackRoutine; // ���� ���� ���� �ڷ�ƾ
    public GameObject SealUI; // �ҳ� ��ų ���� ǥ�� UI

    public Vector3 savePoint; // ���� ������������ ����� �ӽ� ���̺� ����Ʈ
    private Color originColor; // �ҳ� ��������Ʈ ����
    private Color damagedColor = new Color(0.2f, 0.2f, 0.2f); // �ҳ� �ǰݽ� ������ �� ����

    float h; // �÷��̾� �¿� �̵���
    public float moveSpeed = 5f; // �̵��ӵ�
    public float jumpForce = 12f; // ������
    private bool isGrounded = true; // ���� ����
    public bool isPressingPiri { get; private set; } = false; // �Ǹ� ���� ����
    private bool isReadyPiri = true; // �Ǹ� ���� ���� ���� 
    public bool isPurifying = false; // ��ȭ�� ���� ����
    public bool isDamaged = false; // ���� �ǰݻ��� ����
    public float damagedTime = 1f; // �ǰ� ���� ���� �ð�
    private float blinkInterval = 0.15f; // �ǰݽ� �ѹ� �����ϴ� �ð�

    private bool isPushed; // �а� ��Ȳ���� �ӵ� ������Ʈ ����
    public bool isLocked; // ��ȣ�ۿ�� �ൿ ����
    public bool isPase4 = false;

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
        isLocked = true; // ������ ���� ����

        // inputAction Ȱ��ȭ
        playerinputAction.Enable();

        // PlayerCtrl ���� ���� �̺�Ʈ
        playerSkill.RequestMoveSpeed += OnSetMoveSpeed;
        playerSkill.RequestAnimTrigger += OnSetAnimTrigger;
        playerSkill.RequestAnimSpeed += OnSetAnimSpeed;
        playerSkill.RequestEchoGuardStart += SetEchoGuardCoolTime;
        playerSkill.RequestPuriFyStepStart += SetPurifyStepCoolTime;
        playerSkill.RequestisPurifing += SetisPurifing;
        playerSkill.RequestSetSpriteColor += OnSetSpriteColor;

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
        isLocked = false;// ��ȭ�� ������ �� �����̴� ����

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
        isLocked = false;

        playerSkill.OnUpdateStageData(); // ����� ���� ��ųʸ��� �ʱ�ȭ
        GameManager.Instance.isReturned = true; // ȸ�� �ķ� ���� ����
    }

    void Update()
    {
        if (isLocked || isPushed) return; // �ൿ ���� ���� Ȱ��ȭ�� ����

        OnPlaySoftPiri(); // ��ȭ�� ���� ���� ��¡ Ȯ��
        UpdatePurifyStep(); // ��ȭ�� ������ ���� ����
        OnPlayerMove(); // �̵� ����
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

    private void OnSetSpriteColor(float brightness) // ������ ����� ���� ���� ���� �� sprite ����
    {
        spriteRenderer.color = new Color(brightness, brightness, brightness, spriteRenderer.color.a);
        originColor = spriteRenderer.color; // ���� ������ ����
    }

    private void OnStartPiri(InputAction.CallbackContext context) // ���ֹ�ư�� ������ �� ����
    {
        if (isGrounded && isReadyPiri) // ����, �Ǹ� �غ�ÿ��� ����
        {
            StartCoroutine(StartPiriCool()); // �Ǹ� ���� ��Ÿ�� ����(0.2f);
            playerSkill.StartPiri();
            isPressingPiri = true; // �Ǹ� ���� ����
        }
    }

    private IEnumerator StartPiriCool()
    {
        isReadyPiri = false;

        yield return new WaitForSeconds(0.8f);
        isReadyPiri = true;
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

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Clamp01(elapsed / duration);
            purifyStepCool.fillAmount = fill;
            yield return null;
        }
        purifyStepCool.fillAmount = 1f;
    }

    private IEnumerator OnEnemySealAttack(float enemyDamage, float enemyPosX)
    {
        Debug.Log("�ҳ��� �ɷ��� ���ε˴ϴ�.");
        SealUI.SetActive(true); // ��ų ���� UI ǥ��

        if (!isPurifying) // ��ȭ�� ������ �ǰ� ���� ����
        {
            isPushed = true; // �аݻ��� ����
            animator.SetTrigger(PlayerAnimTrigger.Hit);
            rb.AddForce(Vector2.right * ((enemyPosX - transform.position.x > 0) ? -1 : 1) * 30, ForceMode2D.Impulse); // �ǰݽ� �ݴ�������� ��¦ �аݵ�

        }
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


    private IEnumerator OnDamagedStart(float enemyDamage, float enemyPosX) // �ҳ� �ǰ� ���� �Լ�
    {
        Debug.Log("�ҳ� �ǰ�! �ҳ��� �������� �����մϴ�!");

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

        // �ǰݽ� ����, ���ڰ���, ��ȭ�� ���� ����
        playerinputAction.Player.PlayPiri.Enable();
        playerinputAction.Player.EchoGuard.Enable();
        playerinputAction.Player.PurifyingStep.Enable();

        isDamaged = false; // �ǰݻ��� ����
    }

    private IEnumerator GameOver() // GameOver�� �Լ�
    {
        Debug.LogWarning("�ҳడ �������ϴ�.. GameOver");
        Debug.LogWarning("GameOverǥ�� + ���ӿ��� UI ǥ�� �߰�");

        Time.timeScale = 0.5f; // �ð� �������� ����
        yield return new WaitForSeconds(1f);

        Time.timeScale = 1f;
        GameManager.Instance.Pollution = 0f;
        GameManager.Instance.AddPolution(0f); // ������ UI �ʱ�ȭ
    }

    private void OnCollisionEnter2D(Collision2D collision) // ���� �浹�� ����
    {
        if (collision.gameObject.CompareTag("Ground")) // �ٴڰ� �浹�� �� �ʱ�ȭ
        {
            animator.speed = 1f; // �ٽ� �ִϸ��̼� ����
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Fall")) // �߶� ������
        {
            Debug.Log("�ֱ� ���̺� ����Ʈ�� �̵�");
            transform.position = savePoint;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // �ֵ� �浹�� Trigger���� ���
    {
        if (collision.gameObject.CompareTag("Enemy")) // ���� �浹�� ������ or ����
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>(); // Enemy �⺻ Ŭ���� ������
            if (enemy != null)
            {
                if (!enemy.attackMode || enemy.isStune || enemy.isDead || isDamaged) return; // ���� ���� ��尡 false or ����, ��� ������ ��� or �ҳ� �ǰ� ���½� �浹 X

                if (isPressingPiri)
                {
                    playerSkill.PlaySoftPiriCanceled(); // ��ȭ�� �������̾��� ��� ĵ��
                }
                StartCoroutine(OnDamagedStart(enemy.damage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // �ǰ� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
            }
            else
            {
                Debug.Log("�ش� ���� EnemyBase Ŭ������ ������� �ʾҽ��ϴ�! ��������");
            }
        }
        else if (collision.gameObject.CompareTag("EnemyAttack")) // ���ڰ��� ������ �׷α� �������� ���� �� �ִ� Attack
        {
            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();

            if (enemyAttack != null)
            {
                if (isDamaged) return; // �ҳ� �ǰ� ���½� �浹 X

                if (playerSkill.isEchoGuarding) // ���ڰ��� ���½� �� �׷α� ������ ����
                {
                    Debug.Log("[EnemyAttack] ������ ����� ���� �׷α� �������� ���Դϴ�!");
                    StartCoroutine(enemyAttack.enemyOrigin.GetComponent<EnemyBase>().EchoGuardSuccess(collision));
                    return;
                }

                if (isPressingPiri) // ��ȭ�� �������̾��� ��� ĵ��
                {
                    playerSkill.PlaySoftPiriCanceled(); 
                }
                StartCoroutine(OnDamagedStart(enemyAttack.attackDamage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // �ǰ� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
            }
            else
            {
                Debug.Log("�ش� ���� EnemyBase Ŭ������ ������� �ʾҽ��ϴ�! ��������");
            }
        }

        else if (collision.gameObject.CompareTag("EnemyAttack_NoGroggy")) // ���ڰ��� �����ÿ��� �׷α� �������� ���� �� ���� Attack
        {
            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();

            if (enemyAttack != null)
            {
                if (isDamaged) return; // �ҳ� �ǰ� ���½� �浹 X

                if (playerSkill.isEchoGuarding) // ���ڰ��� ���½� �� �׷α� ������ ����
                {
                    Debug.Log("[EnemyAttack_NoGroggy] ������ ����� ���� ��� �����մϴ�!");
                    enemyAttack.enemyOrigin.GetComponent<EnemyBase>().EchoGuardSuccess_NoGloogy(collision);
                    return;
                }

                if (isPressingPiri) // ��ȭ�� �������̾��� ��� ĵ��
                {
                    playerSkill.PlaySoftPiriCanceled();
                }
                StartCoroutine(OnDamagedStart(enemyAttack.attackDamage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // �ǰ� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
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

                if (playerSkill.isEchoGuarding) // ���ڰ��� ���½� �� �׷α� ������ ����
                {
                    Debug.Log("[EnemyAttack] ������ ����� ���� �׷α� �������� ���Դϴ�!");
                    StartCoroutine(enemyAttack.enemyOrigin.GetComponent<EnemyBase>().EchoGuardSuccess(collision));
                    return;
                }

                if (isPressingPiri) // ��ȭ�� �������̾��� ��� ĵ��
                {
                    playerSkill.PlaySoftPiriCanceled();
                }
                if(SealAttackRoutine != null)
                {
                    StopCoroutine(SealAttackRoutine); // ���� �������� ���� ���� ����
                }
                SealAttackRoutine = StartCoroutine(OnEnemySealAttack(enemyAttack.attackDamage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // ���ΰ��� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)
            }
            else
            {
                Debug.Log("�ش� ���� EnemyBase Ŭ������ ������� �ʾҽ��ϴ�! ��������");
            }
        }

        else if (collision.gameObject.CompareTag("EnemyProjectile")) // ���� ���� ������ ���⸸ �ϴ� Attack
        {
            Debug.Log("�ҳడ ���� ����ü�� �������� �Խ��ϴ�.");
            var enemyAttack = collision.gameObject.GetComponent<EnemyAttackBase>();
            StartCoroutine(OnDamagedStart(enemyAttack.attackDamage * (isPurifying ? 0.2f : 1), collision.transform.position.x)); // �ǰ� ���� ���� (��ȭ�� ������ ������ 20%�� �ݰ�)

            Destroy(collision.gameObject); // ����ü ����
        }
        else if (collision.gameObject.CompareTag("EnemySight"))
        {
            Debug.Log("���� �ҳฦ �߰��߽��ϴ�!!");
            var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.enemySight.GetComponent<SpriteRenderer>().enabled = false; // �� ��� ���� ǥ�� off
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
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
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
                enemyBase.enemySight.GetComponent<SpriteRenderer>().enabled = true; // �� ��� ���� ǥ�� on
                enemyBase.isPatrol = true; // �� �÷��̾� �߰� �ߴ�
            }
        }
    }
}