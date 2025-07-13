using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningSpawner : MonoBehaviour
{
    public GameObject warningPrefab;   // 경고 이펙트
    public GameObject lightningPrefab; // 실제 번개

    public Transform[] spawnPoints;

    public float spawnInterval = 5f;

    private void Start()
    {
        StartCoroutine(SpawnLightningRoutine());
    }

    private IEnumerator SpawnLightningRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(2f, 5f);
            yield return new WaitForSeconds(waitTime);

            if (spawnPoints.Length == 0 || lightningPrefab == null || warningPrefab == null)
                continue;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject warning = Instantiate(warningPrefab, spawnPoint.position, Quaternion.identity);
            Destroy(warning, 1f);

            yield return new WaitForSeconds(1f);

            GameObject lightning = Instantiate(lightningPrefab, spawnPoint.position, Quaternion.identity);

            AudioSource audio = lightning.GetComponent<AudioSource>();
            if (audio != null) audio.Play();

            StartCoroutine(DeactivateLightningVisual(lightning, 0.3f));
            Destroy(lightning, 1f);
        }
    }

    private IEnumerator DeactivateLightningVisual(GameObject lightning, float delay)
    {
        yield return new WaitForSeconds(delay);

        BoxCollider2D col = lightning.GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = false;

        SpriteRenderer sr = lightning.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
    }
}