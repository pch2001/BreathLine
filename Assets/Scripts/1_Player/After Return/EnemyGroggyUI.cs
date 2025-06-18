using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyGroggyUI : MonoBehaviour
{
    [SerializeField] private GameObject gaugeSlotPrefab; // 슬롯 프리팹
    [SerializeField] private Transform slotParent; // 부모 객체 (Groggy UI)
    private List<SpriteRenderer> slotList = new List<SpriteRenderer>();

    private Color activatedColor = new Color(0.5f, 0.86f, 0.58f);
    private int maxGauge = 3;
    private int currentGauge = 0;

    public void SetupGroggySpriteGauge(int maxGroggy) // 슬롯 초기화 및 정렬 함수
    {
        float slotSpacing = 0.2f; // 슬롯 간격
        Vector3 startPos = new Vector3(-slotSpacing * (maxGroggy - 1) / 2f, 1f, 0f);

        for (int i = 0; i < maxGroggy; i++)
        {
            GameObject slot = Instantiate(gaugeSlotPrefab, slotParent); // enemy 자식으로
            slot.transform.localPosition = startPos + new Vector3(i * slotSpacing, 0f, 0f);
            slotList.Add(slot.GetComponent<SpriteRenderer>());
        }
    }

    public void AddGroggyState()
    {
        if (currentGauge >= maxGauge) return; // 최대 게이지까지만 증가

        slotList[currentGauge].color = activatedColor; // 활성화 색상
        currentGauge++;

        if (currentGauge >= maxGauge)
        {
            Debug.Log("UI가 그로기 상태를 표시합니다.");
        }
    }

    public void ResetGroggyState() // 그로기 슬롯 상태 초기화
    {
        Debug.Log("초기화 했어요");
        currentGauge = 0;
        foreach (var img in slotList)
            img.color = Color.gray;
    }
}
