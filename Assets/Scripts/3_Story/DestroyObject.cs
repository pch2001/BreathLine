using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float destorytime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destorytime); // 1초 후에 오브젝트를 파괴합니다.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
