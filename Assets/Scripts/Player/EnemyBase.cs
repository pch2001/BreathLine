using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    protected Rigidbody2D rigidBody;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;
    protected GameObject player; // �÷��̾� ������Ʈ Ȯ�ο�

    private Color originalColor; // ���� SpriteRender ���� ����
    private Color currentColor;

    public GameObject hitEffect; // �ǰ� ����Ʈ
    public GameObject dieEffect; // Die ����Ʈ
    public GameObject enemyFadeEffect; // ����� �� ����Ʈ
    [SerializeField] private Transform enemyHpGauge; // �� Hp(������) UI

    public float maxHp; // �� �ִ� HP (�ִ� ������)
    [SerializeField] private float _currentHp;
    public float currentHp // ���� �� HP (���� ������)
    {
        get => _currentHp;
        set
        {
            _currentHp = Mathf.Clamp(value, 0f, maxHp);
            UpdateHpGauge(); // hp ������ ������Ʈ
        }
    }

    public float defaultMoveSpeed; // �� �⺻ �̵��ӵ�
    [SerializeField] private float _moveSpeed;
    public float moveSpeed // �� ���� �̵��ӵ�
    {
        get => _moveSpeed;
        set => _moveSpeed = Mathf.Clamp(value, 0f, defaultMoveSpeed);
    }

    public float decreaseHpSpeed; // �� HP(������) ���� �ӵ�
    public float damage; // �� ���ݷ�

    public bool attackMode = false; // �� ���� ���� ����(��� <-> �߰�)
    public bool isStune = false; // ���� ���� ����
    public bool isDead = false; // ���� ����
    
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");

        originalColor = spriteRenderer.color;
    }

    public virtual IEnumerator Stunned(float delay) // �� ���� ���� ����
    {
        moveSpeed = 0;
        isStune = true; // ��� ���� ����
        currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(currentColor.r * 0.5f, currentColor.g * 0.5f, currentColor.b * 0.5f, currentColor.a);
        animator.SetBool("isRun", false); // ��� Idle ���

        yield return new WaitForSeconds(delay);
        
        moveSpeed = defaultMoveSpeed; // �̵��ӵ� ����
        isStune = false; // ���� ���� ����
        spriteRenderer.color = originalColor; // ���� ����
    }

    public virtual IEnumerator EnemyFade(float duration) // ��ȭ�� �������� �� ����� �Լ�
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;

        isDead = true; // ���� ���·� ����
        enemyFadeEffect.SetActive(true);
        defaultMoveSpeed = 0f; // �̵� �Ұ���
        animator.SetBool("isRun", false);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        
        gameObject.SetActive(false); // �� ��Ȱ��ȭ
    }

    public virtual IEnumerator Damaged() // �ǰݽ� ���� ����
    {
        if (currentHp >= maxHp) yield break; // ��� ����Ʈ�� �浹�Ͽ� hp ���� ����

        currentHp += player.GetComponent<PlayerSkill>().playerDamage;
        Debug.Log("���� ������ : " + currentHp);

        if (currentHp < maxHp) // �ǰ� ����
        {
            Debug.Log("���� �������� ���� ���ظ� �Խ��ϴ�.");
            StartCoroutine(Stunned(0.7f)); // 0.7�� ����
            animator.SetTrigger("Damaged"); // �ǰ� �ִϸ��̼� ����
            hitEffect.SetActive(true); // �ǰ� ����Ʈ Ȱ��ȭ

            yield return new WaitForSeconds(0.2f);

            hitEffect.SetActive(false); // �ǰ� ����Ʈ ��Ȱ��ȭ
        }
        else // ��� ���� 
        {
            Debug.Log("���� ���뽺���� �Ҹ��մϴ�...");
            isDead = true;
            moveSpeed = 0f;
            animator.SetTrigger("Die");
            dieEffect.SetActive(true);
            yield return new WaitForSeconds(0.5f);

            dieEffect.SetActive(false);
            gameObject.SetActive(false); // �� ��Ȱ��ȭ
        }
    }

    public virtual void PushBack(float dir) // �а� ���� ����
    {
        if (dir > 0)
            rigidBody.AddForce(Vector2.right * 650);
        else
            rigidBody.AddForce(Vector2.left * 650); // �ڷ� ���� �Ÿ� �а�
        
        StartCoroutine(Stunned(3f)); // 3�ʰ� ����
    }

    private void UpdateHpGauge() // �� hp(������) ������ ������Ʈ
    {
        float hpRatio = currentHp / maxHp;
        enemyHpGauge.localScale = new Vector2(hpRatio, enemyHpGauge.localScale.y);
            
    }

    protected abstract void HandlerTriggerEnter(Collider2D collision); // �浹�� ���� �ֺ�(Enter) ��� �Լ�
    protected abstract void HandlerTriggerStay(Collider2D collision); // �浹�� ���� ��(Stay) ��� �Լ�

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(isDead) return; // ����� �浹üũ X

        HandlerTriggerEnter(collision); // ��ü���� �浹 ó�������� �ڽ� ��ũ��Ʈ���� �ñ�!
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead) return;

        HandlerTriggerStay(collision); // ������ ���������� �̶��� ���� ������Ʈ ������ ���� �̹� ���� ���ο� ���� ��� (���� ����)
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WolfAppear"))
        {
            Debug.Log("���� ������ ������ ����ϴ�");
            moveSpeed = defaultMoveSpeed; // ���� �ӵ�
        }
    }
}
