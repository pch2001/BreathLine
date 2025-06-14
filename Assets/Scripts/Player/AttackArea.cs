using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public int attackGlobalID = 0; // 중복 충돌판정 방지 ID(해당 ID값을 가진 적만 충돌반응)

    private void OnEnable() 
    {
        attackGlobalID++; // 활성화될 때 초기화
    }
}
