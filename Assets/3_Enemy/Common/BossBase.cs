using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBase : EnemyBase
{
    [SerializeField] protected GameObject pushAttackRange; // �а� ���� ���� ������Ʈ
    [SerializeField] protected GameObject storyObject; // ����� ���丮 ������Ʈ
    public GameObject attackTriggerRange; // �� ���� ���� ���� ����
    public bool isSpecialPhase = false; // Ư�� ���� �ߵ� ����

    public override bool isPatrol // ��Ʈ�� ��� ����
    {
        get => false; // ������ �׻� ���� x
        set { /* ���� */ }
    }

    public override bool isAttackRange
    {
        get => _isAttackRange;
        set
        {
            if (!attackMode || isSpecialPhase) return;

            _isAttackRange = value; // ����� �� ����

            if (isDead || isStune || isSpecialPhase) return; // ����, ����, Ư�� ���Ͻ� ����

            if (isAttackRange) // ���� ��� - ���� ���� �Լ�
            {
                if (!isAttacking) // ���� �ߺ� ���� ����
                {
                    targetPos = transform.position; // ���ݽ� �������� �ڽ����� ����(�̵�x)

                    if (nextAttackIndex == 0)
                    {
                        attackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // 0��° ����(�Ķ���)�� ���� ��� �����ϵ��� ����
                    }
                    else if (nextAttackIndex == 2)
                    {
                        rangeAttackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // 2��° ����(���� ����)�� ��ũ��Ʈ�� ��� �����ϵ��� ����(attackMode ���� ����)
                    }
                    else
                        StartCoroutine(attackPatterns[nextAttackIndex]()); // �ٸ� ����(������)�� ���� ��ҵ��� ����
                }
                else
                {
                    moveSpeed = 0f;
                }
            }
        }
    }

    private void Start()
    {
        isPatrol = false;
        isAttackRange = false;

        InitializeAttackPatterns(); // ���� ���� �Լ� ���� �ʱ�ȭ
        nextAttackIndex = Random.Range(0, attackPatterns.Length);
    }


    protected IEnumerator PushAttack(float delay) // �а� �Լ�
    {
        yield return new WaitForSeconds(delay); // ���� �ð� ����

        Collider2D hit = Physics2D.OverlapCircle(transform.position, 3, 1 << LayerMask.NameToLayer("Player")); // ������ 3, Player ���̾ �浹�ϴ� ������ ����
        if (hit != null)
        {
            float dirX = (hit.transform.position.x - transform.position.x) >= 0 ? 1f : -1f;
            Vector2 dir = new Vector2(dirX, 0f);

            PlayerCtrlBase playerState = hit.GetComponent<PlayerCtrlBase>();
            playerState.isPushed = true;
            hit.GetComponent<Rigidbody2D>().AddForce(dir * 15f, ForceMode2D.Impulse);

            moveSpeed = 0;
            isStune = true;
            animator.SetTrigger("Push"); // �а� ���
            CancelAttack(); // ���� ���� ��� ���
            pushAttackRange.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            pushAttackRange.SetActive(false);

            yield return new WaitForSeconds(1.5f);

            moveSpeed = defaultMoveSpeed; // �̵��ӵ� ����
            isStune = false;
            playerState.isPushed = false;
            EvaluateCurrentState(); // �� ���� ���� �Լ�
        }
    }

    void OnDrawGizmos() // �а� ���� ǥ��
    {
        Gizmos.color = Color.red; // ���� ����
        Gizmos.DrawWireSphere(transform.position, 3); // ���� ����
    }

    public override IEnumerator Damaged() // �ǰݽ� ���� ����
    {
        if (currentHp >= maxHp) yield break; // ��� ����Ʈ�� �浹�Ͽ� hp ���� ����

        if (GameManager.Instance.isReturned) // ȸ�� �� �÷��̾� ������ ����
            currentHp += player.GetComponent<PlayerSkill_R>().playerDamage;
        else // ȸ�� �� �÷��̾� ������ ����
            currentHp += player.GetComponent<PlayerSkill>().playerDamage;

        Debug.Log("���� ������ : " + currentHp);

        if (currentHp < maxHp) // �ǰ� ����
        {
            Debug.Log("���� �������� ���� ���ظ� �Խ��ϴ�.");
            StartCoroutine(Stunned(0.3f)); // 0.3�� ����
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
            GameManager.Instance.AddPolution(pollutionDegree);
            yield return new WaitForSeconds(1.5f);

            dieEffect.SetActive(false);
            storyObject.SetActive(true);
            gameObject.SetActive(false); // �� ��Ȱ��ȭ
        }
    }




    protected virtual void SetAttackTriggerRange(int index) // ���� ������ ���� ���� ���� ���� ���� 
    {

    }

    protected override void HandlerTriggerEnter(Collider2D collision) // �浹 ó�� ��� 
    {

    }

    protected override void HandlerTriggerStay(Collider2D collision) // �浹 ó�� ��� 
    {

    }

}

