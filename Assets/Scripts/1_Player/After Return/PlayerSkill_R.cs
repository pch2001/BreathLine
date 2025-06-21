using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerSkill_R : MonoBehaviour
{
    private PlayerCtrl_R playerCtrl;

    [SerializeField] private AudioSource audioSource;
    private Dictionary<string, AudioClip> piriClips; // ���� ���� Dictionary
    [SerializeField] private AudioClip angerMelody; // �г��� ���� ����
    [SerializeField] private AudioClip peaceMelody; // ��ȭ�� ���� ����
    [SerializeField] private AudioClip peaceCancelMelody; // ��ȭ�� ���� ���� ����

    [SerializeField] private GameObject AngerAttackArea; // �ҳ� �г��� ���� ���� ����
    [SerializeField] private GameObject PeaceAttackArea; //  �ҳ� ��ȭ�� ���� ���� ����
    [SerializeField] private GameObject EchoGuardAttackArea; //  �ҳ� ���ڰ��� ���� ����
    [SerializeField] private GameObject PeaceWaitingEffect; //  �ҳ� ��ȭ�� ���� �غ� ����Ʈ

    public float playerDamage; // �÷��̾��� ���ݷ�
    private float piriStartTime; // �Ǹ����� ���� �ð�
    private bool isSoftPiriPlayed = false; // ��ȭ�� ���� ���ְ� �Ϸ�Ǿ�����
    private bool isSoftPiriStart = false; // ��ȭ�� ���� ���ְ� ���۵Ǿ�����
    [SerializeField] private float SoftPiriKeyDownTime; // ��ȭ�� ���� Ű�ٿ� �ð�
    private bool isRestoreSpeed = false; // RestoreSpeedAfterDelay �ڷ�ƾ�Լ� �ߺ����� ���� �÷���
    private float playerPollution = 1f; // �ҳ� ������ ���
    private bool echoGuardReady = true; // ���ڰ��� ��Ÿ�� Ȯ��

    [SerializeField] private float purifyDuration = 2f; // ��ȭ�� ���� �ִ� ���ӽð�
    [SerializeField] private GameObject purifyRange; // ��ȭ�� ���� �ִ� ���ӽð�
    public bool purifyStepReady = true; // ��ȭ�� ���� ��Ÿ�� Ȯ��

    public event Action<float> RequestMoveSpeed; // playerCtrl�� moveSpeed ���� ���� �̺�Ʈ
    public event Action<string> RequestAnimTrigger; // playerCtrl�� �ִϸ��̼� Trigger ���� �̺�Ʈ
    public event Action<float> RequestAnimSpeed; // playerCtrl�� animator ����ӵ� ���� �̺�Ʈ
    public event Action<float> RequestEchoGuardStart; // playerCtrl���� ���ڰ��� ���� �˸� �̺�Ʈ
    public event Action<float> RequestPuriFyStepStart; // playerCtrl���� ��ȭ�� ���� ���� �˸� �̺�Ʈ
    public event Action<bool> RequestisPurifing; // playerCtrl�� isPurify ���� ����
    public event Action<float> RequestSetSpriteColor; // playerCtrl�� Sprite ������ ������ ���濡 ���� ���� �̺�Ʈ

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl_R>();
    }
    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RequestCurrentStage += OnUpdateStageData;
    }

    private IEnumerator PlayShortPiri() // �г��� ���� ����
    {
        Debug.Log("�Ǹ��� [�г��� ����]�� �����մϴ�!");
        PlayPiriSound("Anger");
        RequestMoveSpeed?.Invoke(0.5f); // �̵��ӵ� 0.5�� ����
        RestoreSpeedAfterDelay(0.5f);

        AngerAttackArea.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        AngerAttackArea.SetActive(false);
    }

    public void PlaySoftPiriCanceled() // ��ȭ�� ���� ��� ��
    {
        Debug.Log("[��ȭ�� ����] ���� ����...");
        PeaceWaitingEffect.SetActive(false); // ��ȭ�� ���� �غ� ����Ʈ ����
        audioSource.Stop();
        PlayPiriSound("PeaceFail");
    }

    public void StartPiri() // ���ֹ�ư �Է½� ��� ���� �� ������ ����
    {
        piriStartTime = Time.time;
        isSoftPiriStart = false;
        isSoftPiriPlayed = false;
        RequestMoveSpeed?.Invoke(0.5f); // �̵� ����
        RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Sad); // �Ǹ� �δ� ���� ����
        RequestAnimSpeed?.Invoke(0f); // �Ǹ� �δ� ������� �ִϸ��̼� ����
    }

    public void ReleasePiri() // ���ֹ�ư �Է� �ð��� ���� ���� �б� ���� (�г��� ���� + ��ȭ�� ���� ���н�)
    {
        float duration = Time.time - piriStartTime; // ���ֹ�ư ���� �ð�

        if (duration <= 0.3f)
        {
            StartCoroutine(PlayShortPiri());
        }
        else if (duration > 0.4f && duration < SoftPiriKeyDownTime)
        {
            PlaySoftPiriCanceled();
        }

        RequestAnimSpeed?.Invoke(1f);
        RequestMoveSpeed?.Invoke(2.5f); // �̵��ӵ� 2.5�� ����
        StartCoroutine(RestoreSpeedAfterDelay(0.5f)); // 0.5�ʵ��� ��� �̵��ӵ� ����
    }

    public void CheckSoftPiri() // ��ȭ�� ���� ��¡�ð� ���� Ȯ�� 
    {
        if (!isSoftPiriPlayed) // �Ǹ� ���ֽ� && ��ȭ�� ���� ���� �Ϸ� ����
        {
            float duration = Time.time - piriStartTime;
            if (duration > 0.4f && !isSoftPiriStart)
            {
                Debug.Log("[��ȭ�� ����] ���� ����");
                PeaceWaitingEffect.SetActive(true); // ��ȭ�� ���� �غ� ����Ʈ Ȱ��ȭ

                audioSource.clip = piriClips["Peace"];
                audioSource.time = 0f;
                RequestMoveSpeed?.Invoke(1.5f); // �̵� ����
                audioSource.Play(); // ��ȭ�� ���� ���� ����
                isSoftPiriStart = true;
            }
            if (duration > SoftPiriKeyDownTime)
            {
                StartCoroutine(PlaySoftPiri()); // ��ȭ�� ���� ���� ����
            }
        }
    }

    private IEnumerator PlaySoftPiri()
    {
        Debug.Log("�Ǹ��� [��ȭ�� ����]�� �����س��ϴ�.");
        PeaceWaitingEffect.SetActive(false); // ��ȭ�� ���� �غ� ����Ʈ ����

        audioSource.time = 9f; // ��ȭ�� ���� ���κ�(8.5��)���� �̵� (������ ǥ��)
        RequestAnimSpeed?.Invoke(1f);
        RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Happy);
        RequestMoveSpeed?.Invoke(2.5f); // �̵��ӵ� 2.5�� ����
        isSoftPiriPlayed = true;

        PeaceAttackArea.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        PeaceAttackArea.SetActive(false);

        StartCoroutine(RestoreSpeedAfterDelay(0.5f)); // 0.5�ʵ��� ��� �̵��ӵ� ����
    }

    public void OnUpdateStageData()  // ������ ���濡 ���� ������ ������Ʈ (ex. ����� ������ ��ųʸ��� �ʱ�ȭ)
    {
        angerMelody = GameManager.Instance.currentStageData.anger_audioClip; // ������ ���������� ���� �������� ��ü
        peaceMelody = GameManager.Instance.currentStageData.peace_audioClip;
        playerPollution = GameManager.Instance.currentStageData.pollution_Coefficient; // ���� ������ ���

        AngerAttackArea.transform.localScale = Vector3.one * GameManager.Instance.currentStageData.anger_range;
        PeaceAttackArea.transform.localScale = Vector3.one * GameManager.Instance.currentStageData.peace_range;
        playerDamage = GameManager.Instance.currentStageData.anger_damage;

        piriClips = new Dictionary<string, AudioClip>();
        piriClips.Add("Anger", angerMelody);
        piriClips.Add("Peace", peaceMelody);
        piriClips.Add("PeaceFail", peaceCancelMelody);

        float brightness = (255f - playerPollution * 30f) / 255f;
        brightness = Mathf.Clamp01(brightness); // 0~1 ���̷� ����

        RequestSetSpriteColor?.Invoke(brightness); // �������� �������� ���� �ҳ� ���� ��ȭ
    }

    private IEnumerator RestoreSpeedAfterDelay(float delay) // �����ð� �� �����ӵ��� ����
    {
        if (isRestoreSpeed) yield break; // �Լ� �ߺ� ���� ����
        isRestoreSpeed = true;

        yield return new WaitForSeconds(delay);
        RequestMoveSpeed?.Invoke(5f); // ���� �ӵ��� ����
        isRestoreSpeed = false;
    }

    private void PlayPiriSound(string type)
    {
        if (piriClips.ContainsKey(type))
        {
            audioSource.clip = piriClips[type];
            audioSource.Play();
        }
    }

    public IEnumerator EchoGuard()
    {
        if (echoGuardReady)
        {
            Debug.Log("���ڰ��� ����!");

            echoGuardReady = false;
            StartCoroutine(EchoGuardCoolTimer()); // ��Ÿ�� �ڷ�ƾ ����

            RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Sad);
            RequestMoveSpeed?.Invoke(0f); // �̵��ӵ� 0���� ����
            EchoGuardAttackArea.SetActive(true);
            yield return new WaitForSeconds(0.1f); // �����ð� ���ڰ��� Ȱ��ȭ ����

            EchoGuardAttackArea.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            RequestMoveSpeed?.Invoke(5f); // ���� �ӵ��� ����
        }
    }

    private IEnumerator EchoGuardCoolTimer() // ���ڰ��� ��Ÿ�� �ڷ�ƾ
    {
        RequestEchoGuardStart(1f); // PlayerCtrl_R���� ���ڰ��� ������ �˸� (UI ����ȭ)
        yield return new WaitForSeconds(1f);
        echoGuardReady = true;
    }

    public void PurifyStepStart() // ��ȭ�� ���� ���� �Լ�
    {
        if (purifyStepReady)
        {
            Debug.Log("�ҳడ ��ȭ�� ������ �����մϴ�."); // �ִϸ��̼� �߰��ؾ� ��! -> �ִϸ��̼� bool�� ���� ����, �ش絿�� loop�� ���� ���
            purifyStepReady = false;
            RequestisPurifing?.Invoke(true); // ��ȭ�� ���� ����
            RequestMoveSpeed?.Invoke(2.5f); // ��ȭ�� ���� �ӵ��� ����
            purifyRange.SetActive(true); // ��ȭ ���� Ȱ��ȭ

            StartCoroutine(PurifyDurationTimer()); // ��ȭ�� ���� ���� Ÿ�̸� ����
        }
    }

    public void PurifyStepStop() // ��ȭ�� ���� ���� �Լ�
    {
        Debug.Log("�ҳడ ��ȭ�� ������ ����ϴ�."); // �ִϸ��̼� bool�� false�� ���� -> �� isPurifying ���� �� Ȯ����!
        RequestMoveSpeed?.Invoke(5f); // �̵��ӵ� ���� �ӵ��� ����
        purifyRange.SetActive(false); // ��ȭ ���� ��Ȱ��ȭ
        RequestisPurifing?.Invoke(false); // ��ȭ�� ���� ����

        StartCoroutine(PurifyCoolTimer()); // ��ȭ�� ���� ��Ÿ�� Ÿ�̸� ����
    }

    IEnumerator PurifyDurationTimer() // ��ȭ�� ���� ��Ÿ�� �ڷ�ƾ
    {
        yield return new WaitForSeconds(purifyDuration);

        playerCtrl.PurifyStepStop(); // ��ȭ�� ���� ����
    }

    IEnumerator PurifyCoolTimer() // ��ȭ�� ���� ��Ÿ�� �ڷ�ƾ
    {
        RequestPuriFyStepStart(5f); // PlayerCtrl_R���� ��ȭ�� ���� ������ �˸� (UI ����ȭ)
        yield return new WaitForSeconds(5f); // ��Ÿ�� ���� 5��
        purifyStepReady = true; // ��ȭ�� ���� �غ� �Ϸ�
    }
}
