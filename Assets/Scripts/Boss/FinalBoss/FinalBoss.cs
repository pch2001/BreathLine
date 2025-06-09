using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float meleeRange = 5f;   // 근거리 공격 범위
    public float rangeAttackRange = 15f; //원거리 공격 범위
    public float teleportCooldown = 10f;

    public GameObject arrow; // 원거리 공격용 화살 프리팹
    public Transform[] teleportPositions; // 순간이동 위치

    //public GameObject

    public float HP = 10;
    public float maxHP = 10;


    private Transform player;
    private Animator anim;
    private bool isAttacking = false;
    private bool canTeleport = true;

    public Transform possionPoint;

    public GameObject[] attackPoints;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
    }   

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isAttacking || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 scale = transform.localScale;
        scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        float dist = Vector2.Distance(transform.position, player.position); // 거리

        if (dist < meleeRange)
        {
            StartCoroutine(Attack1());
        }
        else if (dist < rangeAttackRange)
        {
            StartCoroutine(Attack2());
        }
        else
        {
            FollowPlayer();
        }

        //if (canTeleport)
        //{
        //    StartCoroutine(TeleportAndAOE());
        //}
    }


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("AngerMelody") && !isAttacking)
        {
            if (HP <= 0)
            {
                anim.SetTrigger("Die");

            }
            else
            {
              
                anim.SetTrigger("Hit");
                Debug.Log("보스 HP: " + HP);
                HP--;
                //fillImage.fillAmount = HP / maxHP;

            }

        }
    }

    void FollowPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

        // 좌우 반전
        
    }

    IEnumerator Attack1()
    {
        isAttacking = true;
        anim.SetTrigger("attack1");
        yield return new WaitForSeconds(0.5f); // 공격 시간

        isAttacking = false;
    }

    IEnumerator Attack2()
    {
        isAttacking = true;
        anim.SetTrigger("attack2");
        yield return new WaitForSeconds(0.5f);

        Instantiate(arrow, possionPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    IEnumerator TeleportAndAOE()
    {
        canTeleport = false;
        isAttacking = true;
        yield return new WaitForSeconds(1f);

        anim.SetTrigger("attack3");

        yield return new WaitForSeconds(0.5f);


        // 순간이동 위치 중 랜덤 선택
        Transform targetPos = teleportPositions[Random.Range(0, teleportPositions.Length)];
        yield return new WaitForSeconds(1f);

        transform.position = targetPos.position;

        // 다시 나타나기
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;

        anim.SetTrigger("attack3_1");
        yield return new WaitForSeconds(0.5f);

        //============================================
        for (int i = 0; i < attackPoints.Length; i++)
        {
            if (i % 2 == 1)
            {
                attackPoints[i].SetActive(true);
            }
            if (i % 2 == 0)
            {
                attackPoints[i].SetActive(false);
            }
        }
        yield return new WaitForSeconds(3f); // 잠깐 대기

        // 짝수 인덱스 (0, 2, 4...) 공격
        for (int i = 0; i < attackPoints.Length; i++)
        {
            if (i % 2 == 1)
            {
                attackPoints[i].SetActive(false);
            }
            if (i % 2 == 0) 
            {
                attackPoints[i].SetActive(true);
            }
        }

        //============================================

        yield return new WaitForSeconds(3f);

        anim.SetTrigger("attack3_2");

        isAttacking = false;
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;

        yield return new WaitForSeconds(3f);

        anim.SetTrigger("attack3_3");
    }
}
