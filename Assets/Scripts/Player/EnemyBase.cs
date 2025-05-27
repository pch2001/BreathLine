using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color currentColor;

    public float hp; // �� HP
    public float attackPoint; // �� ���ݷ�
    public float moveSpeed; // �� �̵��ӵ�
    public bool attackMode = false; // �浹�� ���� �÷��̾ �������� ����

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public IEnumerator Stunned(float delay)
    {
        float currentMoveSpeed = moveSpeed; // ���� �ӵ� ����
        
        moveSpeed = 0;
        currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(currentColor.r * 0.5f, currentColor.g * 0.5f, currentColor.b * 0.5f, currentColor.a);

        yield return new WaitForSeconds(delay);

        moveSpeed = currentMoveSpeed; // �̵��ӵ� ����
        spriteRenderer.color = originalColor; // ���� ����
    }

    public IEnumerator EnemyFade(float duration) // ��ȭ�� �������� �� ����� �Լ�
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
        
        gameObject.SetActive(false); // �� ��Ȱ��ȭ
    }
}
