using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float pollution; // 오염도 변수
    public float Pollution
    {
        get => pollution;
        set
        {
            pollution = Mathf.Clamp(value, 0f, 100f); // 오염도 0~100으로 제한
            UpdateStageData(pollution);
        }
    }

    public List<PollutionStage> pollutionStages; // 오염도 단계별 데이터 저장 리스트
    public PollutionStage currentStageData; // 현재 오염도 단계 데이터
    public int currentStageIndex; // 현재 오염도 단계
    [SerializeField] RectTransform pollutionGauge; // 오염도 UI 

    public Vector3 savePoint; // 세이브 포인트 위치

    public event Action RequestCurrentStage; // 새로운 PollutionStage 값으로 변경 이벤트
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // GameManager 파괴 방지
        }
        else
        {
            Destroy(gameObject); // 이미 있으면 GameManager 파괴
        }
    }

    private void Start()
    {
        Pollution = 0; // 오염도값 초기화
        currentStageIndex = 0;
        AddPolution(0); // 오염도 UI 초기화
    }

    private void UpdateStageData(float pollution) // 오염도 단계 업데이트
    {
        int index = Mathf.Clamp((int)(pollution / 20), 0, 4); // 오염도 0~100 값을 0~4단계(5단계)로 나눔 (변경o)
        currentStageData = pollutionStages[index];
        if (currentStageIndex == index) return; // 오염도 단계가 변경될 때 내용 초기화

        currentStageIndex = index;
        RequestCurrentStage.Invoke(); // PlayerSkill의 OnUpdateStageData() 함수 실행
    }

    public void AddPolution(float data)
    {
        Pollution += data;
        pollutionGauge.sizeDelta 
            = new Vector2(pollutionGauge.sizeDelta.x, Mathf.Clamp01(pollution / 100f) * 700f);
        Debug.Log(currentStageIndex);
    }
}
