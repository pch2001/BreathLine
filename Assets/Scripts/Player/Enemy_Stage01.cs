using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class Enemy_Stage01 : EnemyBase
{
    public bool isLooking = false; // 플레이어가 적의 시야에 들어왔는지
    public int attackID = 0; // 적 중복 충돌 판정 방지

    private void Start()
    {
        maxHp = 50f; // 적 체력 설정
        currentHp = 0f; // 적 체력 초기화
        damage = 10f; // 적 공격력 설정 
        rigidBody.drag = 5f; // 기본 마찰력 설정
        moveSpeed = defaultMoveSpeed; 
        attackMode = false; // 기본 공격모드 false
    }

    private void Update()
    {
        if(moveSpeed == 2) // 늑대 등장시 오염도 감소
            currentHp -= 5f * Time.deltaTime; // 1초에 5 Hp씩 감소
        
        if (!isLooking || player == null || isStune || isDead ) return;
        // 적이 플레이어를 바라보고 있을 경우
        Vector2 direction = (player.transform.position - transform.position).normalized;
        if (direction.x > 0) 
            spriteRenderer.flipX = false;
        else 
            spriteRenderer.flipX = true;
        
        if (!attackMode) return; 
        // 공격 모드 or 스턴 상태가 아닐 경우
        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        rigidBody.velocity = velocity; // 적 이동

        animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // 속도에 따른 애니메이션 제어
    }

    protected override void HandlerTriggerEnter(Collider2D collision) // 충돌 처리 담당 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == attackID) return; // 적이 보고있지 않을 때 피격 + 이미 범위 충돌 완료시 리턴
            attackID = attackArea.attackGlobalID;

            Debug.Log("분노의 악장이 적을 공격합니다!!");
            
            if (!attackMode) // attackMode가 비활성화 되어있을 때 피격시
            {
                AttackMode(); // 공격모드 활성화
            }
            StartCoroutine(Damaged()); 
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (!attackMode) return; // 공격모드가 아닐 경우 평화의 악장 무시

            if(currentHp > 0) // 적 오염도가 존재할 경우, 오염도 초기화
            {
                currentHp = 0;
                DeActivateAttackMode(); // 적 초기 상태로 되돌림
            }
            else
            {
                Debug.Log("평화의 악장이 적을 안심시킵니다.");
                StartCoroutine(EnemyFade(3f)); // 적이 천천히 사라짐
            }

        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            currentHp -= 20f; // 적 오염도 즉시 20 감소 
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir);
        }
        else if (collision.gameObject.CompareTag("WolfAppear"))
        {
            if (!attackMode) return;

            Debug.Log("늑대가 적을 진정시킵니다");
            moveSpeed = 2f;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (!isLooking) 
            {
                isLooking = true; // 플레이어가 적의 시야에 들어왔을때 반응
                Debug.Log("적이 플레이어를 바라봅니다.");
            } 

            if (!attackMode || isStune) return; // 공격모드가 아닌 상황 or 스턴 상황에서 충돌시 무시

            Debug.Log("적 플레이어에게 피해를 입힙니다!");
            StartCoroutine(Die());
        }
    }

    protected override void HandlerTriggerStay(Collider2D collision) // 충돌 처리 담당 
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            var attackArea = collision.GetComponent<AttackArea>();
            if (!isLooking || attackArea == null || attackArea.attackGlobalID == attackID) return; // 이미 범위 충돌 완료시 리턴
            attackID = attackArea.attackGlobalID;

            Debug.Log("분노의 악장이 적을 공격합니다!!");

            if (!attackMode) // attackMode가 비활성화 되어있을 때 피격시
            {
                AttackMode(); // 공격모드 활성화
            }
            StartCoroutine(Damaged());
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            if (!attackMode) return; // 공격모드가 아닐 경우 평화의 악장 무시

            Debug.Log("평화의 악장이 적을 안심시킵니다.");
            
            isLooking = false;
            StartCoroutine(EnemyFade(3f)); // 적이 천천히 사라짐
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            if (!attackMode || isStune) return;

            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            PushBack(pushBackDir);
            StartCoroutine(Stunned(3f));
        }
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WolfAppear") && attackMode)
        {
            Debug.Log("적이 늑대의 범위를 벗어납니다");
            moveSpeed = defaultMoveSpeed; // 기존 속도
        }
        else if(collision.gameObject.CompareTag("Player") && isLooking)
        {
            AttackMode(); // 공격모드 활성화
        }
    }

    private IEnumerator Die() // 1스테이지 일반 몬스터의 특성(자폭)에 필요
    {
        Debug.Log("적이 고통스럽게 소멸합니다...");
        moveSpeed = 0f;
        animator.SetTrigger("Die");
        dieEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        dieEffect.SetActive(false);
        gameObject.SetActive(false); // 적 비활성화
    }

    private void AttackMode() // 공격 모드 활성화
    {
        Debug.Log("적이 플레이어에게 달려듭니다!");
        attackMode = true;
        boxCollider.size = new Vector2(0.03f, 0.23f); // 피격 범위 크기 변경 (플레이어 인식 범위 -> 적 충돌 범위)
        boxCollider.offset = new Vector2(0f, 0.125f); // 피격 범위 위치 변경
        moveSpeed = defaultMoveSpeed;
    }

    private void DeActivateAttackMode() // 공격 모드 해제 구현
    {
        Debug.Log("적이 진정됩니다...");

        isLooking = false;
        attackMode = false;
        animator.SetBool("isRun", false);
        boxCollider.size = new Vector2(1.5f, 1f); // 피격 범위 크기 변경 (적 충돌 범위 -> 플레이어 인식 범위)
        boxCollider.offset = new Vector2(0f, 0.51f); // 피격 범위 위치 변경

        StartCoroutine(Stunned(3f)); // 3초간 기절
    }
}
