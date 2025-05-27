using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class Enemy_Stage01 : EnemyBase
{
    private Rigidbody2D rigidBody;
    private Animator animator;
    private GameObject player;
    private SpriteRenderer spriteRenderer;

    public GameObject hitEffect; // 피격 이펙트
    public GameObject dieEffect; // Die 이펙트


    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");

        hp = 30f; // 적 체력 설정
        attackPoint = 10f; // 적 공격력 설정 
        rigidBody.drag = 5f; // 기본 마찰력 설정
        moveSpeed = 2f;
    }

    private void Update()
    {
        if (!attackMode || player == null) return; // 공격모드 아닐 경우 이동x

        Vector2 direction = (player.transform.position - transform.position).normalized;
        Vector2 velocity = new Vector2(direction.x * moveSpeed, rigidBody.velocity.y);
        rigidBody.velocity = velocity; // 적 이동

        if (direction.x > 0) 
            spriteRenderer.flipX = true;
        else 
            spriteRenderer.flipX = false;

        animator.SetBool("isRun", Mathf.Abs(velocity.x) > 0.1f); // 속도에 따른 애니메이션 제어

    }

    private IEnumerator Die() // 사망 반응 구현
    {
        animator.SetTrigger("Die");
        dieEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        dieEffect.SetActive(false);
        gameObject.SetActive(false); // 적 비활성화
    }

    IEnumerator PushBack(float dir) // 밀격 반응 구현
    {
        float originalDrag = rigidBody.drag;
        rigidBody.drag = 1f; // 밀격시 잠시 마찰력 감소

        if (dir > 0)
            rigidBody.AddForce(Vector2.right * 650);
        else
            rigidBody.AddForce(Vector2.left * 650); // 뒤로 일정 거리 밀격

        yield return new WaitForSeconds(0.2f);
        rigidBody.drag = originalDrag;
    }

    IEnumerator Damaged() // 피격 반응 구현
    {
        hp -= 10;
        attackMode = true;

        StartCoroutine(Stunned(0.5f)); // 0.5초 경직
        animator.SetTrigger("Damaged"); // 피격 애니메이션 실행
        hitEffect.SetActive(true); // 피격 이펙트 활성화

        yield return new WaitForSeconds(0.2f);

        hitEffect.SetActive(false); // 피격 이펙트 비활성화
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("AngerMelody"))
        {
            Debug.Log("분노의 악장이 적을 공격합니다!!");
            if(hp <= 0)
            {
                Debug.Log("적이 고통스럽게 소멸합니다...");
                StartCoroutine(Die());
            }
            else
            {
                Debug.Log("적이 공격으로 인해 피해를 입습니다.");
                StartCoroutine(Damaged());
            }
        }
        else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            Debug.Log("평화의 악장이 적을 안심시킵니다.");
            attackMode = false; 
            animator.SetBool("isRun", false);
            StartCoroutine(EnemyFade(4f)); // 적이 천천히 사라짐
        }
        else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
            float pushBackDir = transform.position.x - collision.transform.position.x; // 적이 밀격될 방향 계산
            StartCoroutine(PushBack(pushBackDir));
            StartCoroutine(Stunned(3f));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!attackMode) return; // 공격모드가 아닌 상황에서 충돌시 무시

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("적 플레이어에게 피해를 입힙니다!");
            StartCoroutine(Die());
        }
    }
}
