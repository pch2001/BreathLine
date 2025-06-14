using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class possionarrow : MonoBehaviour
{
    public float moveSpeed = 15f;
    public GameObject spawnEffectPrefab; // 생성될 오브젝트 (예: 독 효과)

    private Vector2 targetPosition;
    private bool hasLanded = false;

    void Start()
    {
        Transform player = GameObject.FindWithTag("Player").transform;
        targetPosition = new Vector2(player.position.x, player.position.y - 0.5f);

    }

    void Update()
    {
        if (hasLanded) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            Land();
        }
    }

    void Land()
    {
        hasLanded = true;

        if (spawnEffectPrefab != null)
        {
            Instantiate(spawnEffectPrefab, targetPosition, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
