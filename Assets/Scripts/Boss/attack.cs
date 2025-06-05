using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attack : MonoBehaviour
{
    public float knockbackForce = 3f;         // 밀려나는 힘
    public float knockbackDuration = 0.2f;    // 밀리는 시간


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("대미지 입음!");

            Animator playerAnimator = collision.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("isHit");
            }

            // 밀어낼 방향: 플레이어 → 반대 방향
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;

            // 넉백 처리 시작
            StartCoroutine(ApplyKnockbackToTarget(collision.transform, hitDirection));

        }
    }

    private IEnumerator ApplyKnockbackToTarget(Transform target, Vector2 direction)
    {
        // Y 방향 제거: X축 넉백만
        direction = new Vector2(direction.x, 0).normalized;

        Vector3 start = target.position;
        Vector3 targetPos = start + (Vector3)(direction * knockbackForce);

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            target.position = Vector3.Lerp(start, targetPos, elapsed / knockbackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.position = targetPos;
    }



}


