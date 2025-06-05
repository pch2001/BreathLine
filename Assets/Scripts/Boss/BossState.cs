using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class BossState : MonoBehaviour
{

    public float HP = 10;
    public float maxHP = 10;
    public float moveSpeed = 4f; // 이동 속도

    private Animator anim;
    private Transform player;
    public GameObject attackArea1; // 공격 판정 오브젝트 (BoxCollider2D 등)
    public GameObject attackArea2; // 공격 판정 오브젝트 (BoxCollider2D 등)
    public GameObject attackArea3; // 공격 판정 오브젝트 (BoxCollider2D 등)

    public GameObject[] boomArea;

    private bool isAttacking;
    private bool dontmove = true;

    public GameObject notePrefab;  // Inspector에서 할당할 음표 프리팹
    public Image fillImage; // Image 컴포넌트, Inspector에서 할당
    // Start is called before the first frame update
    void Start()
    {

        isAttacking = false;
        attackArea1.SetActive(false); // 시작 시 공격 범위 꺼두기
        attackArea2.SetActive(false); // 시작 시 공격 범위 꺼두기
        attackArea3.SetActive(false); // 시작 시 공격 범위 꺼두기
        boomArea[0].SetActive(false);
        boomArea[1].SetActive(false);
        boomArea[2].SetActive(false);

        isdie = true; //보스 죽었는지 확인
        anim = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

    }

    private void FixedUpdate()
    {
        if (dontmove && !isAttacking && isdie)
            MoveToPlayer();

    }
    private void MoveToPlayer()
    {
        if (player == null) return;

        float stopDistance = 1.5f;

        // x축 거리만 비교 (y축 무시)
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        if (distanceX <= stopDistance && !isAttacking)
        {
            //Attack();

            Debug.Log("공격");
            return;
        }

        // 방향 계산 (x축만)
        float directionX = Mathf.Sign(player.position.x - transform.position.x);
        Vector3 moveDir = new Vector3(directionX, 0f, 0f); // x축 방향만

        // 이동
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // 좌우 반전
        Vector3 scale = transform.localScale;
        scale.x = directionX < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    int num = 0;

    void Attack()
    {

        if (num < 3)
        {
            StartCoroutine(Attack1());
            num++;
        }
        else if (num < 5)
        {
            StartCoroutine(Attack2());
            num++;
        }
        else if (num == 5)
        {
            StartCoroutine(Attack3());
            num = 0;
        }
        //StartCoroutine(cooltime());
    }
    bool isdie;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("AngerMelody") && !isAttacking)
        {
            if (HP <= 0 && isdie)
            {
                isdie = false;
                dontmove = false;
                anim.SetTrigger("die");

                Invoke(nameof(HandleDeath), 1.5f);
            }
            else
            {
                dontmove = false;
                anim.SetTrigger("isHit");
                Debug.Log("보스 HP: " + HP);
                HP--;
                Invoke(nameof(SetMove), 0.5f);
                fillImage.fillAmount = HP / maxHP;

            }

        }
    }
    private void HandleDeath()
    {
        // 음표 생성
        if (notePrefab != null)
        {
            Instantiate(notePrefab, transform.position, Quaternion.identity);
        }

        // 보스 제거
        Destroy(gameObject);
    }
    private void SetMove()
    {
        dontmove = true;
    }

    IEnumerator Attack1()
    {
        isAttacking = true;
        anim.SetTrigger("attack1");


        // 공격 지속 시간 대기
        yield return new WaitForSeconds(0.8f);
        if (dontmove)
        {
            attackArea1.SetActive(true);
            Collider2D col = attackArea1.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }

        yield return new WaitForSeconds(0.2f);

        // 공격 판정 비활성화
        attackArea1.SetActive(false);

        // 쿨타임
        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
    }

    IEnumerator Attack2()
    {

        isAttacking = true;
        anim.SetTrigger("attack2");

        // 공격 지속 시간 대기
        yield return new WaitForSeconds(0.7f);
        if (dontmove)
        {
            attackArea2.SetActive(true);
            Collider2D col = attackArea2.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }
        yield return new WaitForSeconds(0.2f);

        // 공격 판정 비활성화
        attackArea2.SetActive(false);

        // 쿨타임
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    IEnumerator Attack3()
    {
        isAttacking = true;
        anim.SetTrigger("attack3");

        // 공격 지속 시간 대기
        yield return new WaitForSeconds(1.5f);
        boomArea[0].SetActive(true);
        yield return new WaitForSeconds(0.2f);
        boomArea[1].SetActive(true);
        yield return new WaitForSeconds(0.2f);
        boomArea[0].SetActive(false);
        boomArea[2].SetActive(true);
        yield return new WaitForSeconds(0.2f);
        boomArea[1].SetActive(false);

        attackArea3.SetActive(true);


        // 공격 판정 비활성화
        attackArea3.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        boomArea[2].SetActive(false);

        // 쿨타임
        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }
    IEnumerator cooltime()
    {
        isAttacking = true;

        yield return new WaitForSeconds(1.0f);
        isAttacking = false;

    }

}
