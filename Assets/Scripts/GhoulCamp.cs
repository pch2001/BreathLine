using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhoulCamp : MonoBehaviour
{
    public Animator[] ghoulAnim;
    public SpriteRenderer[] ghoulSprite;
    public GameObject effect;
    public GameObject ignoreCollider;
    public GameObject seed;
    private BoxCollider2D cureCollider;
    
    // Start is called before the first frame update
    void Awake()
    {
        cureCollider = gameObject.GetComponent<BoxCollider2D>();
    }

    public IEnumerator Cure(bool isCured)
    {
        if (isCured == true)
        {
            ignoreCollider.SetActive(false);
            for (int i = 0; i < ghoulAnim.Length; i++)
            {
                ghoulAnim[i].SetTrigger("Cured");
                StartCoroutine(FadeCoroutine(0f, 0.5f, ghoulSprite[i]));
            }
            cureCollider.enabled = false;
            yield return new WaitForSeconds(0.5f);
            seed.SetActive(true);
        }
        else
        {
            ignoreCollider.SetActive(false);
            cureCollider.enabled = false;
            for (int i = 0; i < ghoulAnim.Length; i++)
            {
                ghoulAnim[i].SetTrigger("Ignored");
                StartCoroutine(FadeCoroutine(0f, 0.5f, ghoulSprite[i]));
            }
            yield return new WaitForSeconds(0.2f);
            effect.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            effect.SetActive(false);
            
            GameManager.Instance.AddPolution(10f);
            
        }
    }
    private IEnumerator FadeCoroutine(float targetAlpha, float duration, SpriteRenderer sprite) // 늑대의 fade in/out을 위한 함수, targetAlpha는 투명도, duration은 실행시간
    {
        float startAlpha = sprite.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, newAlpha);
            yield return null;
        }

        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, targetAlpha);
    }
}
