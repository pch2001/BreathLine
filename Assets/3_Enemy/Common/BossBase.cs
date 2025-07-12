using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBase : EnemyBase
{
    [SerializeField] protected GameObject pushAttackRange; // 밀격 공격 범위 오브젝트
    [SerializeField] protected GameObject storyObject; // 재생할 스토리 오브젝트
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
            if (!attackMode || isSpecialPhase) return;

            _isAttackRange = value; // 변경된 값 적용

            if (isDead || isStune || isSpecialPhase) return; // 죽음, 기절, 특수 패턴시 리턴

            if (isAttackRange) // 공격 모드 - 공격 실행 함수
            {
                if (!isAttacking) // 공격 중복 실행 방지
                {
                    targetPos = transform.position; // 공격시 목적지를 자신으로 설정(이동x)

                    if (nextAttackIndex == 0)
                    {
                        attackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // 0번째 공격(파란색)은 공격 취소 가능하도록 설정
                    }
                    else if (nextAttackIndex == 2)
                    {
                        rangeAttackCoroutine = StartCoroutine(attackPatterns[nextAttackIndex]()); // 2번째 공격(범위 공격)은 스크립트시 취소 가능하도록 설정(attackMode 변경 방지)
                    }
                    else
                        StartCoroutine(attackPatterns[nextAttackIndex]()); // 다른 공격(빨간색)은 공격 취소되지 않음
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


    protected IEnumerator PushAttack(float delay) // 밀격 함수
    {
        yield return new WaitForSeconds(delay); // 스턴 시간 이후

        Collider2D hit = Physics2D.OverlapCircle(transform.position, 3, 1 << LayerMask.NameToLayer("Player")); // 반지름 3, Player 레이어만 충돌하는 범위로 설정
        if (hit != null)
        {
            float dirX = (hit.transform.position.x - transform.position.x) >= 0 ? 1f : -1f;
            Vector2 dir = new Vector2(dirX, 0f);

            PlayerCtrlBase playerState = hit.GetComponent<PlayerCtrlBase>();
            playerState.isPushed = true;
            hit.GetComponent<Rigidbody2D>().AddForce(dir * 15f, ForceMode2D.Impulse);

            moveSpeed = 0;
            isStune = true;
            animator.SetTrigger("Push"); // 밀격 모션
            CancelAttack(); // 공격 중인 경우 취소
            pushAttackRange.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            pushAttackRange.SetActive(false);

            yield return new WaitForSeconds(1.5f);

            moveSpeed = defaultMoveSpeed; // 이동속도 복구
            isStune = false;
            playerState.isPushed = false;
            EvaluateCurrentState(); // 적 상태 적용 함수
        }
    }

    void OnDrawGizmos() // 밀격 범위 표시
    {
        Gizmos.color = Color.red; // 색상 지정
        Gizmos.DrawWireSphere(transform.position, 3); // 원형 범위
    }

    public override IEnumerator Damaged() // 피격시 반응 구현
    {
        if (currentHp >= maxHp) yield break; // 사망 이펙트와 충돌하여 hp 증가 방지

        if (GameManager.Instance.isReturned) // 회귀 후 플레이어 데미지 연결
            currentHp += player.GetComponent<PlayerSkill_R>().playerDamage;
        else // 회귀 전 플레이어 데미지 연결
            currentHp += player.GetComponent<PlayerSkill>().playerDamage;

        Debug.Log("현재 오염도 : " + currentHp);

        if (currentHp < maxHp) // 피격 반응
        {
            Debug.Log("적이 공격으로 인해 피해를 입습니다.");
            StartCoroutine(Stunned(0.3f)); // 0.3초 경직
            animator.SetTrigger("Damaged"); // 피격 애니메이션 실행
            hitEffect.SetActive(true); // 피격 이펙트 활성화

            yield return new WaitForSeconds(0.2f);

            hitEffect.SetActive(false); // 피격 이펙트 비활성화
        }
        else // 사망 반응 
        {
            Debug.Log("적이 고통스럽게 소멸합니다...");
            isDead = true;
            moveSpeed = 0f;
            animator.SetTrigger("Die");
            dieEffect.SetActive(true);
            GameManager.Instance.AddPolution(pollutionDegree);
            yield return new WaitForSeconds(1.5f);

            dieEffect.SetActive(false);
            storyObject.SetActive(true);
            gameObject.SetActive(false); // 적 비활성화
        }
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

