using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public int attackGlobalID = 0; // �ߺ� �浹���� ���� ID(�ش� ID���� ���� ���� �浹����)

    private void OnEnable() 
    {
        attackGlobalID++; // Ȱ��ȭ�� �� �ʱ�ȭ
    }
}
