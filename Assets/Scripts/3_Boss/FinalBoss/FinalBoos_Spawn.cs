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

        // ½ÇÁ¦·Î ÀÌµ¿
        transform.position += (Vector3)(direction * moveSpeed * Time.fixedDeltaTime);

        // ÁÂ¿ì ¹ÝÀü
        Vector3 scale = transform.localScale;
        scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("ºÎ‹HÈû");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("ÀÚÆø °ø°Ý");
            Destroy(this.gameObject);
        }
    }
  

}