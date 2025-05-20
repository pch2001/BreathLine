using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // �����ͻ󿡼� ���� ����
public class PollutionStage
{
    [Header("�Ǹ� ���� - �г��� ����")]
    public float anger_damage; // �г��� ���� ������
    public float anger_range; // �г��� ���� ��ų ����
    public AudioClip anger_audioClip; // �г��� ���� ����� Ŭ��
    public GameObject anger_effect; // �г��� ���� ����Ʈ

    [Header("�Ǹ� ���� - ��ȭ�� ����")]
    public float peace_cooldown; // ��ȭ�� ���� ��� ��Ÿ��
    public float peace_range; // ��ȭ�� ���� ��ų ����
    public AudioClip peace_audioClip; // ��ȭ�� ���� ����� Ŭ��
    public GameObject peace_effect; // ��ȭ�� ���� ����Ʈ

    [Header("���� ���� ��ȭ ���")]
    public float wolfCoefficient;
}