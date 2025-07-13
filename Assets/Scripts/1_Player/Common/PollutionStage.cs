using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PollutionStage
{
    [Header("피리 연주 - 분노의 악장")]
    public float anger_damage;     // 분노의 악장 데미지
    public float anger_range;      // 분노의 악장 스킬 범위

    [Header("피리 연주 - 평화의 악장")]
    public float peace_cooldown;   // 평화의 악장 사용 쿨타임
    public float peace_range;      // 평화의 악장 스킬 범위

    [Header("오염도 상태 변화 계수")]
    public float pollution_Coefficient;

    [Header("오염도에 따른 늑대 설정")]
    public float wolfAppearTime;     // 늑대 연출 시간
    public float wolfAttackCoolTime;  // 늑대 공격 쿨타임
}