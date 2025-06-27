using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirlBoss : MonoBehaviour
{
    public GameObject[] linerange;
    public GameObject tracerange;

    public GameObject thunder;

    public GameObject player;

    public GameObject minibossSpawn;
        public GameObject pushObject;

    void Start()
    {
        // StartCoroutine(test());Pattern3
        pushObject.SetActive(false);

        StartCoroutine(Pattern4()); 
    }

    void Update()
    {
    }
    IEnumerator test()
    {
        yield return StartCoroutine(Pattern1());
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(Pattern2());
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(Pattern3());
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(Pattern4());
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(Pattern5());
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(Pattern6());
        yield return new WaitForSeconds(3f);

    }
    IEnumerator Pattern1()
    {
        for (int i = 0; i < 8; i++)
        {
            Quaternion dir = linerange[i].transform.rotation;
            StartCoroutine(ThunderWarning1(i, linerange[i].transform.position, dir));
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator Pattern2()
    {
        for (int i = 8; i < linerange.Length; i++)
        {
            Quaternion dir = linerange[i].transform.rotation;
            StartCoroutine(ThunderWarning1(i, linerange[i].transform.position, dir));
            yield return new WaitForSeconds(0.3f);
        }
    }
    IEnumerator Pattern3()
    {
        StartCoroutine(ThunderWarning3(player, Quaternion.identity));

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator Pattern4()
    {
        for(int i =0; i<10; i++)
        {
            int randomIndex = Random.Range(0, linerange.Length);
            Quaternion dir = linerange[randomIndex].transform.rotation;
            Vector3 pos = linerange[randomIndex].transform.position;

            StartCoroutine(ThunderWarning1(randomIndex, pos, dir));

            yield return new WaitForSeconds(0.3f);

        }
        yield return new WaitForSeconds(1f);
    }

    IEnumerator Pattern5()//°Å¹Ì ¼ÒÈ¯
    {
        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2 / 5;
            Vector3 spawnOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 2;
            Vector3 spawnPos = transform.position + spawnOffset;

            Instantiate(minibossSpawn, spawnPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(3f);

    }

    IEnumerator Pattern6()//¹ÐÄ¡±â
    {
        yield return new WaitForSeconds(0.3f);

        pushObject.SetActive(true);

        yield return new WaitForSeconds(0.3f);
        pushObject.SetActive(false);

    }


    IEnumerator ThunderWarning1(int index, Vector3 position, Quaternion rotation)
    {
        //GameObject warning = Instantiate(warningZonePrefab, position, rotation);  // ¹æÇâ ¹Ý¿µ
        linerange[index].SetActive(true);
        SpriteRenderer sr = linerange[index].GetComponent<SpriteRenderer>();

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.PingPong(Time.time * 3f, 1f);
            sr.color = new Color(1f, 0f, 0f, alpha); // »¡°£»ö ±ôºýÀÓ
            elapsed += Time.deltaTime;
            yield return null;
        }

        linerange[index].SetActive(false);

        GameObject thunders = Instantiate(thunder, position, rotation);  // ¹æÇâ ¹Ý¿µ

        yield return new WaitForSeconds(1f);
        Destroy(thunders);
    }
   

    IEnumerator ThunderWarning3(GameObject target, Quaternion rotation)
    {
        tracerange.SetActive(true);
        SpriteRenderer sr = tracerange.GetComponent<SpriteRenderer>();

        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 strikePos = target.transform.position;

        while (elapsed < duration)
        {
            // ´ë»ó À§Ä¡ ÃßÀû
            strikePos = target.transform.position; 
            tracerange.transform.position = strikePos;
            tracerange.transform.rotation = rotation;

            // »¡°£»ö ±ôºýÀÓ
            float alpha = Mathf.PingPong(Time.time * 3f, 1f);
            sr.color = new Color(1f, 0f, 0f, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.8f);

        tracerange.SetActive(false);

        // ¸¶Áö¸· À§Ä¡¿¡ ³«·Ú »ý¼º
        GameObject thunders = Instantiate(thunder, strikePos, rotation);

        yield return new WaitForSeconds(1f);
        Destroy(thunders);
    }
}
