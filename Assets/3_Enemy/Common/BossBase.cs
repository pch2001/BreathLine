using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBase : EnemyBase
{
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
            if (!attackMode) return;

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

