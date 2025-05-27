using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color currentColor;

    public float hp; // 적 HP
    public float attackPoint; // 적 공격력
    public float moveSpeed; // 적 이동속도
    public bool attackMode = false; // 충돌시 적이 플레이어를 공격할지 여부

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public IEnumerator Stunned(float delay)
    {
        float currentMoveSpeed = moveSpeed; // 현재 속도 저장
        
        moveSpeed = 0;
        currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(currentColor.r * 0.5f, currentColor.g * 0.5f, currentColor.b * 0.5f, currentColor.a);

        yield return new WaitForSeconds(delay);

        moveSpeed = currentMoveSpeed; // 이동속도 복구
        spriteRenderer.color = originalColor; // 색상 복구
    }

    public IEnumerator EnemyFade(float duration) // 평화의 악장으로 적 사라짐 함수
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        
        gameObject.SetActive(false); // 적 비활성화
    }
}
