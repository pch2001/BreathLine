using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float destorytime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destorytime); // 1�� �Ŀ� ������Ʈ�� �ı��մϴ�.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
