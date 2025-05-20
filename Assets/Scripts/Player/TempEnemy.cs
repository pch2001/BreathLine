using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEnemy : MonoBehaviour
{
    private bool isStunned = false;
    private Color currentColor;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        currentColor = spriteRenderer.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("AngerMelody"))
        {
            StartCoroutine(Stunned());
            Debug.Log("분노의 악장이 적을 공격합니다!!");
        }else if (collision.gameObject.CompareTag("PeaceMelody"))
        {
            StartCoroutine(Stunned());
            Debug.Log("평화의 악장이 적을 안심시킵니다.");
        }else if (collision.gameObject.CompareTag("WolfAttack"))
        {
            StartCoroutine(Stunned());
            Debug.Log("늑대의 공격이 적을 기절시킵니다!");
        }
    }

    private IEnumerator Stunned()
    {
        if(!isStunned)
        {
            isStunned = true;
            currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r * 0.5f, currentColor.g * 0.5f, currentColor.b * 0.5f, currentColor.a);
        
            yield return new WaitForSeconds(3.0f);

            isStunned = false;
            spriteRenderer.color = originalColor;
        }
    }
}
