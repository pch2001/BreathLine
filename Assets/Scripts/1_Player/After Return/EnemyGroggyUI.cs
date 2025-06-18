using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyGroggyUI : MonoBehaviour
{
    [SerializeField] private GameObject gaugeSlotPrefab; // ���� ������
    [SerializeField] private Transform slotParent; // �θ� ��ü (Groggy UI)
    private List<SpriteRenderer> slotList = new List<SpriteRenderer>();

    private Color activatedColor = new Color(0.5f, 0.86f, 0.58f);
    private int maxGauge = 3;
    private int currentGauge = 0;

    public void SetupGroggySpriteGauge(int maxGroggy) // ���� �ʱ�ȭ �� ���� �Լ�
    {
        float slotSpacing = 0.2f; // ���� ����
        Vector3 startPos = new Vector3(-slotSpacing * (maxGroggy - 1) / 2f, 1f, 0f);

        for (int i = 0; i < maxGroggy; i++)
        {
            GameObject slot = Instantiate(gaugeSlotPrefab, slotParent); // enemy �ڽ�����
            slot.transform.localPosition = startPos + new Vector3(i * slotSpacing, 0f, 0f);
            slotList.Add(slot.GetComponent<SpriteRenderer>());
        }
    }

    public void AddGroggyState()
    {
        if (currentGauge >= maxGauge) return; // �ִ� ������������ ����

        slotList[currentGauge].color = activatedColor; // Ȱ��ȭ ����
        currentGauge++;

        if (currentGauge >= maxGauge)
        {
            Debug.Log("UI�� �׷α� ���¸� ǥ���մϴ�.");
        }
    }

    public void ResetGroggyState() // �׷α� ���� ���� �ʱ�ȭ
    {
        Debug.Log("�ʱ�ȭ �߾��");
        currentGauge = 0;
        foreach (var img in slotList)
            img.color = Color.gray;
    }
}
