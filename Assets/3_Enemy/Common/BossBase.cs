using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBase : EnemyBase
{
    public GameObject attackTriggerRange; // 적 현재 공격 시작 범위
    public bool isSpecialPhase = false; // 특수 패턴 발동 여부

    public override bool isPatrol // 패트롤 기능 제외
    {
        get => false; // 보스는 항상 순찰 x
        set { /* 무시 */ }
    }

    public override bool isAttackRange
    {
        get => _isAttackRange;
        set
        {
            if (!attackMode) return;

            _isAttackRange = value; // 변경된 값 적용

            if (isDead || isStune || isSpecialPhase) return; // 죽음, 기절, 특수 패턴시 리턴

            if (isAttackRange) // 공격 모드 - 공격 실행 함수
            {
                if (!isAttacking) // 공격 중복 실행 방지
                {
                    targetPos = transform.position; // 공격시 목적지를 자신으로 설정(이동x)
                    attackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // 공격 패턴 중 랜덤으로 실행
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

        InitializeAttackPatterns(); // 보스 공격 함수 구성 초기화
        nextAttackIndex = Random.Range(0, attackPatterns.Length);
    }


    protected virtual void SetAttackTriggerRange(int index) // 공격 종류에 따른 공격 시작 범위 변경 
    {

    }

    protected override void HandlerTriggerEnter(Collider2D collision) // 충돌 처리 담당 
    {

    }

    protected override void HandlerTriggerStay(Collider2D collision) // 충돌 처리 담당 
    {

    }

}

