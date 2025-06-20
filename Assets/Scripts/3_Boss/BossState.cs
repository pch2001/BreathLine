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
    //public GameObject attackArea1; // 공격 판정 오브젝트
    public GameObject attackArea2;
    public GameObject attackArea3;
   
    public GameObject[] boomArea; //3번째패턴 공격 범위

    private bool isAttacking;
    private bool dontmove = true;

    public GameObject notePrefab;  // Inspector에서 할당할 음표 프리팹
    public Image fillImage; // Image 컴포넌트, Inspector에서 할당
    
    public GameObject[] attackAreas; // 공격 범위 오브젝트 배열 

    void Start()
    {

        isAttacking = false;
        //attackArea1.SetActive(false); // 시작 시 공격 범위 꺼두기
        //attackArea2.SetActive(false); // 시작 시 공격 범위 꺼두기
        //attackArea3.SetActive(false); // 시작 시 공격 범위 꺼두기
        boomArea[0].SetActive(false);
        boomArea[1].SetActive(false);
        boomArea[2].SetActive(false);
        boomArea[3].SetActive(false);

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

        float stopDistance = 1f;

        // x축 거리만 비교 (y축 무시)
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        if (distanceX <= stopDistance && !isAttacking)
        {
            StartCoroutine("HealBoss");
            //Attack();
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

    public void SetAttack(int attacknum)
    {
        Collider2D col = attackAreas[attacknum].GetComponent<Collider2D>();
        col.enabled = false;
        col.enabled = true;

        Debug.Log("공격 번호: " + attacknum);

        GameObject targetArea = attackAreas[attacknum];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = true; // 공격 상태로 설정
        }


    }

    public void SetNoAttack(int attacknum)
    {
        //Debug.Log("공격 번호: " + attacknum);

        GameObject targetArea = attackAreas[attacknum];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = false; // 공격 상태로 설정
        }
    }

    IEnumerator Attack1()
    {
        isAttacking = true;
        anim.SetTrigger("attack1");

        if (dontmove)
        {
         
        }
        yield return new WaitForSeconds(1.2f);
        isAttacking = false;
    }

    IEnumerator Attack2()
    {

        isAttacking = true;
        anim.SetTrigger("attack2");
        yield return new WaitForSeconds(0.7f);

        // 공격 지속 시간 대기
        if (dontmove)
        {
            //attackArea2.SetActive(true);
            Collider2D col = attackArea2.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    IEnumerator Attack3()
    {
        isAttacking = true;
        anim.SetTrigger("attack3");

        yield return new WaitForSeconds(2.5f);


        isAttacking = false;
    }

    IEnumerator HealBoss()
    {
        isAttacking = true;

        anim.SetTrigger("heal");
        yield return new WaitForSeconds(3f);
        isAttacking = false;

    }

    public void ActiveBoom(int num)
    {
        boomArea[num].SetActive(true);

        GameObject targetArea = boomArea[num];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = true; // 공격 상태로 설정
        }
        if (dontmove)
        {
            //attackArea2.SetActive(true);
            Collider2D col = attackArea2.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }
    }
    public void PassiveBoom(int num)
    {

        GameObject targetArea = boomArea[num];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = false; // 공격 상태로 설정
        }

        boomArea[num].SetActive(false);

    }

    //public void is
    IEnumerator cooltime()
    {
        isAttacking = true;

        yield return new WaitForSeconds(1.0f);
        isAttacking = false;

    }

}
