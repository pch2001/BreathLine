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


    public GameObject spawnBoss;

    public float HP = 10;
    public float maxHP = 10;


    private Transform player;
    private Animator anim;
    private bool isAttacking = false;
    private bool canTeleport = true;

    public Transform possionPoint;

    public GameObject baseattack;

    public GameObject[] attackPoints;

    public GameObject thunderObj; // 번개 공격 프리팹
    public GameObject warningZonePrefab; // 경고 구역 프리팹

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
        StartCoroutine(Attack3());

    }
    int count = 0;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (isAttacking || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 scale = transform.localScale;
        scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        float dist = Vector2.Distance(transform.position, player.position); // 거리

        //StartCoroutine(TeleportAndAOE());
        //StartCoroutine(Attack1());
        //StartCoroutine(Attack2());

        //if (canTeleport && count == 5)
        //{
        //    StartCoroutine(TeleportAndAOE());
        //}
        //if (dist < meleeRange)
        //{
        //    count++;
        //    StartCoroutine(Attack1());
        //}
        //else if (dist < rangeAttackRange)
        //{
        //    count++;
        //    StartCoroutine(Attack2());
        //}
        //else
        //{
        //    FollowPlayer();
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

    }

    public void Attack1Start()
    {
        baseattack.SetActive(true);
        attack attackScript = baseattack.GetComponent<attack>();
        if (attackScript != null)
        {
            attackScript.isAttacking = true; // 공격 상태로 설정
        }
    }

    public void Attack1End()
    {
        baseattack.SetActive(false);
        attack attackScript = baseattack.GetComponent<attack>();
        if (attackScript != null)
        {
            attackScript.isAttacking = false; // 공격 상태로 설정
        }
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
        yield return new WaitForSeconds(0.1f);

        Instantiate(arrow, possionPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    IEnumerator Attack3()
    {
        isAttacking = true;

        Debug.Log("공격3 시작");
        anim.SetTrigger("Die");

        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2 / 5;
            Vector3 spawnOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 2;
            Vector3 spawnPos = transform.position + spawnOffset;

            Instantiate(spawnBoss, spawnPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(3f);

        anim.SetTrigger("Idle");

        yield return new WaitForSeconds(1f);

        isAttacking = false;

    }

    IEnumerator TeleportAndAOE()
    {
        canTeleport = false;
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);

        anim.SetTrigger("attack3");


        yield return new WaitForSeconds(0.8f);

        // 순간이동 위치 중 랜덤 선택
        Transform targetPos = teleportPositions[Random.Range(0, teleportPositions.Length)];
        yield return new WaitForSeconds(0.5f);

        transform.position = targetPos.position;

        // 다시 나타나기
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;

        anim.SetTrigger("attack3_1");
        yield return new WaitForSeconds(1.5f);

        //============================================
        for (int i = 0; i < attackPoints.Length; i++)
        {
            if (i % 2 == 1)
            {
                StartCoroutine(ThunderWarning(attackPoints[i].transform.position));
            }
        }
        yield return new WaitForSeconds(3f);

        // 짝수 공격
        for (int i = 0; i < attackPoints.Length; i++)
        {
            if (i % 2 == 0)
            {
                StartCoroutine(ThunderWarning(attackPoints[i].transform.position));
            }
        }

        //============================================

        yield return new WaitForSeconds(3f);
        for (int i = 0; i < attackPoints.Length; i++)
        {
            attackPoints[i].SetActive(false);
        }
        anim.SetTrigger("attack3_2");

        

        yield return new WaitForSeconds(1f);

        anim.SetTrigger("attack3_3");
        isAttacking = false;
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

    IEnumerator ThunderWarning(Vector3 position)
    {
        GameObject warning = Instantiate(warningZonePrefab, position, Quaternion.identity);
        SpriteRenderer sr = warning.GetComponent<SpriteRenderer>();

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.PingPong(Time.time * 3f, 1f);
            sr.color = new Color(1f, 0f, 0f, alpha); // 빨간색 깜빡임
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(warning);

        GameObject thunders = Instantiate(thunderObj, position, Quaternion.identity);

        yield return new WaitForSeconds(1f);
        Destroy(thunders);
    }
}
