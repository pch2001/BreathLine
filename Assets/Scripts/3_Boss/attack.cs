using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attack : MonoBehaviour
{
    public float knockbackForce = 3f;         // 밀려나는 힘
    public float knockbackDuration = 0.2f;    // 밀리는 시간

    public GameObject fireEffectPrefab;

    public bool isAttacking = false; // 공격 중인지 여부


    void Start()
    {
        //isAttacking = false;
    }


       
    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("충돌중");

        if (collision.CompareTag("Player") && isAttacking)
        {
            isAttacking = false;
            Debug.Log("대미지 입음!");
            GameObject fire = Instantiate(fireEffectPrefab, collision.transform.position, Quaternion.identity);
            Destroy(fire, 1f);

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


