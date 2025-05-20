using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // 에디터상에서 수정 가능
public class PollutionStage
{
    [Header("피리 연주 - 분노의 악장")]
    public float anger_damage; // 분노의 악장 데미지
    public float anger_range; // 분노의 악장 스킬 범위
    public AudioClip anger_audioClip; // 분노의 악장 오디오 클립
    public GameObject anger_effect; // 분노의 악장 이펙트

    [Header("피리 연주 - 평화의 악장")]
    public float peace_cooldown; // 평화의 악장 사용 쿨타임
    public float peace_range; // 평화의 악장 스킬 범위
    public AudioClip peace_audioClip; // 평화의 악장 오디오 클립
    public GameObject peace_effect; // 평화의 악장 이펙트

    [Header("늑대 상태 변화 계수")]
    public float wolfCoefficient;
}