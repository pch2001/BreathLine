using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_Stage03_ChasingBoss : MonoBehaviour
{
    Queue<Vector3> playerPositions = new Queue<Vector3>();
    [SerializeField] private float updateRate = 0.1f;  // 0.1�ʸ��� ��ġ ����
    [SerializeField] private Color targetColor;  // ��ǥ ���� ����
    [SerializeField] private GameObject hitEffect; // ���� ������Ʈ
    [SerializeField] private GameObject attackRange; // ���� ���� ������Ʈ
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackRadius = 10f;

    private SpriteRenderer spriteRenderer;
    private Camera mainCam;
    private Vector3 originalCamPos;
    private float attackTimer; // ���� �ð� ��� Ÿ�̸�
    private GameObject player; // �÷��̾� ������Ʈ

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;

        if (mainCam != null)
        {
            originalCamPos = mainCam.transform.localPosition;
        }

        StartCoroutine(TrackPlayerPosition());
    }

    IEnumerator TrackPlayerPosition() // �÷��̾� ���� ��ġ ����
    {
        while (true)
        {
            playerPositions.Enqueue(player.transform.position);
            if (playerPositions.Count > 5) playerPositions.Dequeue(); // ������ ��ġ ����
            yield return new WaitForSeconds(updateRate);
        }
    }

    private void FixedUpdate()
    {
        if (playerPositions.Count > 0)
        {
            Vector3 targetPos = playerPositions.Peek(); // �Էµ� ť�� ���� ������ �κ� ��ǥ��(5������)
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.05f);
        }

        attackTimer += Time.deltaTime;

        // ���� ������ ����
        float colorRatio = Mathf.Clamp01(attackTimer / 10f); // 9�ʵ��� ���� ����
        spriteRenderer.color = Color.Lerp(Color.white, targetColor, colorRatio);

        // ��� ����Ʈ ǥ��
        if (attackTimer >= 9f && !hitEffect.activeSelf)
            hitEffect.SetActive(true);

        if (attackTimer >= 10f)
        {
            attackTimer = 0f;
            spriteRenderer.color = Color.white;
            hitEffect.SetActive(false);
            StartCoroutine(Attack0()); // ���� ����
        }
    }

    IEnumerator Attack0()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRadius, playerLayer);
        StartCoroutine(ShakeCamera(1f, 0.5f)); // ī�޶� ���� ȿ��

        if (hit != null)
        {
            if (!hit.GetComponent<PlayerCtrl>().isCovered) // �������� �ƴ� ���
            {
                attackRange.SetActive(true);
                yield return new WaitForSeconds(0.3f);
                attackRange.SetActive(false);
            }
        }
        yield return null;
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetx = Random.Range(-3f, 3f) * magnitude;
            float offsety = Random.Range(-3f, 3f) * magnitude;

            mainCam.transform.localPosition = originalCamPos + new Vector3(offsetx, offsety, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.localPosition = originalCamPos;
    }
}
