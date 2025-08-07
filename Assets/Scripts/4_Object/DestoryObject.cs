using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, 1f); // Destroys the game object after 2 seconds
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
