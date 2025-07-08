using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_Stage03_ChasingBoss : MonoBehaviour
{
    Queue<Vector3> playerPositions = new Queue<Vector3>();
    [SerializeField] private float updateRate = 0.1f;  // 0.1초마다 위치 저장
    [SerializeField] private Color targetColor;  // 목표 색상 설정
    [SerializeField] private GameObject hitEffect; // 경고등 오브젝트
    [SerializeField] private GameObject attackRange; // 공격 범위 오브젝트
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackRadius = 10f;

    private SpriteRenderer spriteRenderer;
    private Camera mainCam;
    private Vector3 originalCamPos;
    private float attackTimer; // 공격 시간 계산 타이머
    private GameObject player; // 플레이어 오브젝트

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

    IEnumerator TrackPlayerPosition() // 플레이어 지연 위치 추적
    {
        while (true)
        {
            playerPositions.Enqueue(player.transform.position);
            if (playerPositions.Count > 5) playerPositions.Dequeue(); // 오래된 위치 제거
            yield return new WaitForSeconds(updateRate);
        }
    }

    private void FixedUpdate()
    {
        if (playerPositions.Count > 0)
        {
            Vector3 targetPos = playerPositions.Peek(); // 입력된 큐의 가장 마지막 부분 목표로(5프레임)
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.05f);
        }

        attackTimer += Time.deltaTime;

        // 색상 점진적 변경
        float colorRatio = Mathf.Clamp01(attackTimer / 10f); // 9초동안 색상 변경
        spriteRenderer.color = Color.Lerp(Color.white, targetColor, colorRatio);

        // 경고 이펙트 표시
        if (attackTimer >= 9f && !hitEffect.activeSelf)
            hitEffect.SetActive(true);

        if (attackTimer >= 10f)
        {
            attackTimer = 0f;
            spriteRenderer.color = Color.white;
            hitEffect.SetActive(false);
            StartCoroutine(Attack0()); // 공격 실행
        }
    }

    IEnumerator Attack0()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRadius, playerLayer);
        StartCoroutine(ShakeCamera(1f, 0.5f)); // 카메라 흔드는 효과

        if (hit != null)
        {
            if (!hit.GetComponent<PlayerCtrl>().isCovered) // 엄폐중이 아닐 경우
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
