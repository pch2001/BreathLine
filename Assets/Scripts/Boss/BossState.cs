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
    // Start is called before the first frame update
    void Start()
    {

        isAttacking = false;
        attackArea1.SetActive(false); // ���� �� ���� ���� ���α�
        attackArea2.SetActive(false); // ���� �� ���� ���� ���α�
        attackArea3.SetActive(false); // ���� �� ���� ���� ���α�
        boomArea[0].SetActive(false);
        boomArea[1].SetActive(false);
        boomArea[2].SetActive(false);

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

        float stopDistance = 1.5f;

        // x�� �Ÿ��� �� (y�� ����)
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        if (distanceX <= stopDistance && !isAttacking)
        {
            //Attack();

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

    IEnumerator Attack1()
    {
        isAttacking = true;
        anim.SetTrigger("attack1");


        // ���� ���� �ð� ���
        yield return new WaitForSeconds(0.8f);
        if (dontmove)
        {
            attackArea1.SetActive(true);
            Collider2D col = attackArea1.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }

        yield return new WaitForSeconds(0.2f);

        // ���� ���� ��Ȱ��ȭ
        attackArea1.SetActive(false);

        // ��Ÿ��
        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
    }

    IEnumerator Attack2()
    {

        isAttacking = true;
        anim.SetTrigger("attack2");

        // ���� ���� �ð� ���
        yield return new WaitForSeconds(0.7f);
        if (dontmove)
        {
            attackArea2.SetActive(true);
            Collider2D col = attackArea2.GetComponent<Collider2D>();
            col.enabled = false;
            col.enabled = true;
        }
        yield return new WaitForSeconds(0.2f);

        // ���� ���� ��Ȱ��ȭ
        attackArea2.SetActive(false);

        // ��Ÿ��
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    IEnumerator Attack3()
    {
        isAttacking = true;
        anim.SetTrigger("attack3");

        // ���� ���� �ð� ���
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


        // ���� ���� ��Ȱ��ȭ
        attackArea3.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        boomArea[2].SetActive(false);

        // ��Ÿ��
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
