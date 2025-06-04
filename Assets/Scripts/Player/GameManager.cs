using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float pollution; // ������ ����
    public float Pollution
    {
        get => pollution;
        set
        {
            pollution = Mathf.Clamp(value, 0f, 100f); // ������ 0~100���� ����
            UpdateStageData(pollution);
        }
    }

    public List<PollutionStage> pollutionStages; // ������ �ܰ躰 ������ ���� ����Ʈ
    public PollutionStage currentStageData; // ���� ������ �ܰ� ������
    public int currentStageIndex; // ���� ������ �ܰ�
    [SerializeField] RectTransform pollutionGauge; // ������ UI 

    public Vector3 savePoint; // ���̺� ����Ʈ ��ġ

    public event Action RequestCurrentStage; // ���ο� PollutionStage ������ ���� �̺�Ʈ
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // GameManager �ı� ����
        }
        else
        {
            Destroy(gameObject); // �̹� ������ GameManager �ı�
        }
    }

    private void Start()
    {
        Pollution = 0; // �������� �ʱ�ȭ
        currentStageIndex = 0;
        AddPolution(0); // ������ UI �ʱ�ȭ
    }

    private void UpdateStageData(float pollution) // ������ �ܰ� ������Ʈ
    {
        int index = Mathf.Clamp((int)(pollution / 20), 0, 4); // ������ 0~100 ���� 0~4�ܰ�(5�ܰ�)�� ���� (����o)
        currentStageData = pollutionStages[index];
        if (currentStageIndex == index) return; // ������ �ܰ谡 ����� �� ���� �ʱ�ȭ

        currentStageIndex = index;
        RequestCurrentStage.Invoke(); // PlayerSkill�� OnUpdateStageData() �Լ� ����
    }

    public void AddPolution(float data)
    {
        Pollution += data;
        pollutionGauge.sizeDelta 
            = new Vector2(pollutionGauge.sizeDelta.x, Mathf.Clamp01(pollution / 100f) * 700f);
        Debug.Log(currentStageIndex);
    }
}
