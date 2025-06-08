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
    public float moveSpeed = 4f; // �̵� �ӵ�

    private Animator anim;
    private Transform player;
    public GameObject attackArea1; // ���� ���� ������Ʈ (BoxCollider2D ��)
    public GameObject attackArea2; // ���� ���� ������Ʈ (BoxCollider2D ��)
    public GameObject attackArea3; // ���� ���� ������Ʈ (BoxCollider2D ��)

    public GameObject[] boomArea;

    private bool isAttacking;
    private bool dontmove = true;

    public GameObject notePrefab;  // Inspector���� �Ҵ��� ��ǥ ������
    public Image fillImage; // Image ������Ʈ, Inspector���� �Ҵ�
    
    public GameObject[] attackAreas; // ���� ���� ������Ʈ �迭 

    void Start()
    {

        isAttacking = false;
        //attackArea1.SetActive(false); // ���� �� ���� ���� ���α�
        //attackArea2.SetActive(false); // ���� �� ���� ���� ���α�
        //attackArea3.SetActive(false); // ���� �� ���� ���� ���α�
        boomArea[0].SetActive(false);
        boomArea[1].SetActive(false);
        boomArea[2].SetActive(false);
        boomArea[3].SetActive(false);

        isdie = true; //���� �׾����� Ȯ��
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

        // x�� �Ÿ��� �� (y�� ����)
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        if (distanceX <= stopDistance && !isAttacking)
        {
            Attack();

            Debug.Log("����");
            return;
        }

        // ���� ��� (x�ุ)
        float directionX = Mathf.Sign(player.position.x - transform.position.x);
        Vector3 moveDir = new Vector3(directionX, 0f, 0f); // x�� ���⸸

        // �̵�
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // �¿� ����
        Vector3 scale = transform.localScale;
        scale.x = directionX < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    int num = 0;

    void Attack()
    {
        //StartCoroutine(Attack3());

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
                Debug.Log("���� HP: " + HP);
                HP--;
                Invoke(nameof(SetMove), 0.5f);
                fillImage.fillAmount = HP / maxHP;

            }

        }
    }
    private void HandleDeath()
    {
        // ��ǥ ����
        if (notePrefab != null)
        {
            Instantiate(notePrefab, transform.position, Quaternion.identity);
        }

        // ���� ����
        Destroy(gameObject);
    }
    private void SetMove()
    {
        dontmove = true;
    }

    public void SetAttack(int attacknum)
    {
       // Debug.Log("���� ��ȣ: " + attacknum);
        GameObject targetArea = attackAreas[attacknum];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = true; // ���� ���·� ����
        }
    }

    public void SetNoAttack(int attacknum)
    {
      //  Debug.Log("���� ��ȣ: " + attacknum);

        GameObject targetArea = attackAreas[attacknum];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = false; // ���� ���·� ����
        }
    }

    IEnumerator Attack1()
    {
        isAttacking = true;
        anim.SetTrigger("attack1");

        if (dontmove)
        {
            Collider2D col = attackArea1.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }
        yield return new WaitForSeconds(1.2f);
        isAttacking = false;
    }

    IEnumerator Attack2()
    {

        isAttacking = true;
        anim.SetTrigger("attack2");
        yield return new WaitForSeconds(0.7f);

        // ���� ���� �ð� ���
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

    public void ActiveBoom(int num)
    {
        boomArea[num].SetActive(true);

        GameObject targetArea = boomArea[num];
        attack attackScript = targetArea.GetComponent<attack>();

        if (attackScript != null)
        {
            attackScript.isAttacking = true; // ���� ���·� ����
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
            attackScript.isAttacking = false; // ���� ���·� ����
        }

        boomArea[num].SetActive(false);

    }


    IEnumerator cooltime()
    {
        isAttacking = true;

        yield return new WaitForSeconds(1.0f);
        isAttacking = false;

    }

}
