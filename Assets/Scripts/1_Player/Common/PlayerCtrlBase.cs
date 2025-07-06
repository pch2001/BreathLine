using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerCtrlBase : MonoBehaviour
{
    public virtual bool isPressingPiri 
    {
        get;
        set;
    }

    public bool isPushed = false;
}
