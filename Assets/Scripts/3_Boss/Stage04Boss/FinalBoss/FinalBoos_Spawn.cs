using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoos_Spawn : MonoBehaviour
{

    public float moveSpeed = 2f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        anim.SetTrigger("Run");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        // 실제로 이동
        transform.position += (Vector3)(direction * moveSpeed * Time.fixedDeltaTime);

        // 좌우 반전
        Vector3 scale = transform.localScale;
        scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("자폭 공격");
            Destroy(this.gameObject, 0.3f);
        }
        if (collision.gameObject.CompareTag("AngerMelody") || collision.gameObject.CompareTag("PeaceMelody"))
        {
            Destroy(this.gameObject);
        }
    }
   
  

}