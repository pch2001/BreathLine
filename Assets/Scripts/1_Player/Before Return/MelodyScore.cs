using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MelodyScore", menuName = "BreathLine/MelodyScore")]
public class MelodyScore : ScriptableObject
{
    public string scoreName; // 스킬 이름
    public int scoreNum; // 스킬 넘버
    public bool isAnger; // 분노의 악장 or 평화의 악장 악보 여부
    public bool[] contents = new bool[5]; // 악보 내용 저장 배열

}
