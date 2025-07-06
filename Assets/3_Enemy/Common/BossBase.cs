using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBase : EnemyBase
{
    [SerializeField] protected GameObject pushAttackRange; // �а� ���� ���� ������Ʈ
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
                    attackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // ���� ���� �� �������� ����
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

