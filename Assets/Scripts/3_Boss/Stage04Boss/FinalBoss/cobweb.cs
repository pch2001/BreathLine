﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cobweb : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("거미줄 닿음");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
