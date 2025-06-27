using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss3 : MonoBehaviour
{
    private Animator anim;

    public GameObject[] AttackRage;

    void Start()
    {
        anim = GetComponent<Animator>();

        foreach (GameObject obj in AttackRage)
        {
            obj.SetActive(false);
        }

       StartCoroutine(skillTest());
        //StartCoroutine(Attack5());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    IEnumerator skillTest()
    {
        yield return Attack1();
        yield return Attack2();
       // yield return Attack3();
        yield return Attack4();
        yield return Attack5();
        anim.SetTrigger("die");
        yield return Attack1();
        yield return Attack2();
        // yield return Attack3();
        yield return Attack4();
        yield return Attack5();
    }

    public void AttackStart(int count)
    {
        AttackRage[count].SetActive(true);
    }

    public void AttackEnd(int count)
    {
        AttackRage[count].SetActive(false);
    }   

    IEnumerator Attack1() { //boom 공격
        anim.SetTrigger("Attack1");
        yield return new WaitForSeconds(2f);
    }

    IEnumerator Attack2() {//그라운드 공격
        anim.SetTrigger("Attack2");
        yield return new WaitForSeconds(2f);
    }
   
    IEnumerator Attack4()//레이저 공격
    {
        anim.SetTrigger("Attack4");
        yield return new WaitForSeconds(2f);
    }
    IEnumerator Attack5() //크기 커지면서 회전 공격
    {
        anim.SetTrigger("Attack5");
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 2f;
        Vector3 originalPosition = transform.position;

        float t = 0f;
        yield return new WaitForSeconds(0.5f);

        while (t < 4)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / 4);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        Vector3 rightTarget = originalPosition + new Vector3(5f, 0f, 0f); // 오른쪽으로 5
        Vector3 leftTarget = originalPosition + new Vector3(-5f, 0f, 0f);  // 왼쪽으로 5



        float duration = 1f;

        yield return MoveBoss(originalPosition, rightTarget, duration);
        yield return MoveBoss(rightTarget, leftTarget, duration);
        yield return MoveBoss(leftTarget, rightTarget, duration);
        yield return MoveBoss(rightTarget, leftTarget, duration);
        yield return MoveBoss(leftTarget, rightTarget, duration);
        yield return MoveBoss(rightTarget, leftTarget, duration);
        yield return MoveBoss(leftTarget, rightTarget, duration);


        yield return new WaitForSeconds(0.5f);

        anim.SetTrigger("Attack5_end");

        yield return new WaitForSeconds(1f);


    }

    IEnumerator MoveBoss(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
    }

}
