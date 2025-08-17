using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MelodyScore", menuName = "BreathLine/MelodyScore")]
public class MelodyScore : ScriptableObject
{
    public string scoreName; // ��ų �̸�
    public int scoreNum; // ��ų �ѹ�
    public bool isAnger; // �г��� ���� or ��ȭ�� ���� �Ǻ� ����
    public bool[] contents = new bool[5]; // �Ǻ� ���� ���� �迭

}
