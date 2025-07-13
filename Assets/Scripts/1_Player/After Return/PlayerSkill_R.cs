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

    [SerializeField] private AudioClip[] angerMelodies; // �г��� ���� ������
    [SerializeField] private AudioClip[] peaceMelodies; // ��ȭ�� ���� ������
    [SerializeField] private AudioClip[] purifyingMelodies; // ��ȭ�� ���� ������
    [SerializeField] private AudioClip peaceCancelMelody; // ��ȭ�� ���� ���� ����
    [SerializeField] private AudioClip echoGuardMelody; // ���� ���� ����

    [SerializeField] private GameObject AngerAttackArea; // �ҳ� �г��� ���� ���� ����
    [SerializeField] private GameObject AngerAttackEffect; // �ҳ� �г��� ���� ���� ����Ʈ
    [SerializeField] private GameObject PeaceAttackArea; //  �ҳ� ��ȭ�� ���� ���� ����
    [SerializeField] private GameObject PeaceWaitingEffect; //  �ҳ� ��ȭ�� ���� �غ� ����Ʈ
    [SerializeField] private GameObject EchoGuardAttackArea; //  �ҳ� ���ڰ��� ���� ����

    public float playerDamage; // �÷��̾��� ���ݷ�
    private float piriStartTime; // �Ǹ����� ���� �ð�
    private bool sharpPiriStart = false; // �г��� ���� ���ְ� ���۵Ǿ�����
    private bool isSoftPiriStart = false; // ��ȭ�� ���� ���ְ� ���۵Ǿ�����
    private bool isSoftPiriPlayed = false; // ��ȭ�� ���� ���ְ� �Ϸ�Ǿ�����
    public float SoftPiriKeyDownTime; // ��ȭ�� ���� Ű�ٿ� �ð�
    private float playerPollution = 1f; // �ҳ� ������ ���

    public bool isEchoGuarding = false; // ���ڰ��� ����
    private bool echoGuardReady = true; // ���ڰ��� ��Ÿ�� Ȯ��

    [SerializeField] private float purifyDuration = 2f; // ��ȭ�� ���� �ִ� ���ӽð�
    [SerializeField] private GameObject purifyRange; // ��ȭ�� ���� ���� ������Ʈ
    public bool purifyStepReady = true; // ��ȭ�� ���� ��Ÿ�� Ȯ��

    public event Action<float, float> RequestSetMoveSpeedAndTime; // playerCtrl�� ���� �ð����� moveSpeed ���� ���� �̺�Ʈ
    public event Action<float> RequestSetMoveSpeed; // playerCtrl�� moveSpeed ���� ���� �̺�Ʈ
    public event Action<string> RequestAnimTrigger; // playerCtrl�� �ִϸ��̼� Trigger ���� �̺�Ʈ
    public event Action<float> RequestSetSpriteColor; // playerCtrl�� Sprite ������ ������ ���濡 ���� ���� �̺�Ʈ
    public event Action<bool> RequestisPurifing; // playerCtrl�� isPurify ���� ����
    public event Action<bool> RequestPressingPiriState; // playerCtrl�� isPressingPiri ���� �̺�Ʈ 
    public event Action<bool> RequestPeaceMelodyActived; // playerCtrl�� isPeaceMelody ���� �̺�Ʈ 
    
    public event Action<float> RequestEchoGuardStart; // playerCtrl���� ���ڰ��� ���� �˸� �̺�Ʈ
    public event Action<float> RequestPuriFyStepStart; // playerCtrl���� ��ȭ�� ���� ���� �˸� �̺�Ʈ
    public event Action<bool> RequestEchoGuardingState; // playerCtrl���� ���ڰ��� ���� �˸� �̺�Ʈ
    
    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl_R>();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RequestCurrentStage += OnUpdateStageData;
    }

    private void OnDisable()      // �Ǵ� OnDestroy
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RequestCurrentStage -= OnUpdateStageData;
    }

    // �ҳ� �⺻ ��� ����

    public void StartPiri() // ���ֹ�ư �Է½� ��� ���� �� ������ ����
    {
        piriStartTime = Time.time;
        isSoftPiriStart = false;
        isSoftPiriPlayed = false;
        RequestSetMoveSpeed?.Invoke(2.5f); // �̵� ����
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

        RequestSetMoveSpeedAndTime?.Invoke(4f, 0.5f); // 2.5f�� 0.5�ʵ��� �ӵ� ����
    }

    private IEnumerator PlayShortPiri() // �г��� ���� ����
    {
        Debug.Log("�Ǹ��� [�г��� ����]�� �����մϴ�!");
        PlayPiriSound("Anger");
        sharpPiriStart = true;

        // �÷��̾ �ٶ󺸴� �������� ����
        float direction = playerCtrl.spriteRenderer.flipX ? -1f : 1f;
        Vector3 attackPosition = AngerAttackArea.transform.localPosition;
        attackPosition.x = Mathf.Abs(attackPosition.x) * direction;
        AngerAttackArea.transform.localPosition = attackPosition;
        
        AngerAttackEffect.SetActive(true);
        AngerAttackArea.SetActive(true);
        yield return new WaitForSeconds(1f);

        sharpPiriStart = false;
        AngerAttackEffect.SetActive(false);
        AngerAttackArea.SetActive(false);
        RequestPressingPiriState(false); // �Ǹ����� ����
    }

    public void PlaySoftPiriCanceled() // ��ȭ�� ���� ��� ��
    {
        Debug.Log("[��ȭ�� ����] ���� ����...");
        PeaceWaitingEffect.SetActive(false); // ��ȭ�� ���� �غ� ����Ʈ ����
        audioSource.Stop();
        PlayPiriSound("PeaceFail");
        RequestPeaceMelodyActived?.Invoke(false);
        RequestPressingPiriState(false); // �Ǹ����� ����
    }

    public void CheckSoftPiri() // ��ȭ�� ���� ��¡�ð� ���� Ȯ�� 
    {
        if (!isSoftPiriPlayed) // �г��� ���� ����x / ��ȭ�� ���� ���� �Ϸ� X ��Ȳ Ȯ��
        {
            float duration = Time.time - piriStartTime;
            if (duration > 0.8f && !isSoftPiriStart && !sharpPiriStart)
            {
                Debug.Log("[��ȭ�� ����] ���� ����");
                RequestPeaceMelodyActived?.Invoke(true); // ��ȭ�� ���� �غ� ���� ���� �˸�
                PeaceWaitingEffect.SetActive(true); // ��ȭ�� ���� �غ� ����Ʈ Ȱ��ȭ

                if (peaceMelodies != null && peaceMelodies.Length > 0) // ���� ���
                {
                    int randomIndex = UnityEngine.Random.Range(0, peaceMelodies.Length);
                    audioSource.clip = peaceMelodies[randomIndex];
                    audioSource.time = 0f;
                    audioSource.Play();
                }

                RequestSetMoveSpeed?.Invoke(2.5f); // �̵��ӵ� 1.5f�� ����
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

        RequestPeaceMelodyActived?.Invoke(false);
        PeaceWaitingEffect.SetActive(false); // ��ȭ�� ���� �غ� ����Ʈ ����

        RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Happy);
        RequestSetMoveSpeedAndTime?.Invoke(4f, 0.5f); // �̵��ӵ� 2.5�� 0.5�� ���� ����
        isSoftPiriPlayed = true;
        PeaceAttackArea.SetActive(true);

        yield return new WaitForSeconds(0.4f);

        PeaceAttackArea.SetActive(false);
        RequestPressingPiriState?.Invoke(false); // �Ǹ� ���� ����
    }

    public void OnUpdateStageData()  // ������ ���濡 ���� ������ ������Ʈ (ex. ����� ������ ��ųʸ��� �ʱ�ȭ)
    {
        playerPollution = GameManager.Instance.currentStageData.pollution_Coefficient; // ���� ������ ���

        AngerAttackArea.transform.localScale = Vector3.one * GameManager.Instance.currentStageData.anger_range;
        PeaceAttackArea.transform.localScale = Vector3.one * GameManager.Instance.currentStageData.peace_range;
        playerDamage = GameManager.Instance.currentStageData.anger_damage;

        float brightness = (255f - playerPollution * 30f) / 255f;
        brightness = Mathf.Clamp01(brightness); // 0~1 ���̷� ����

        RequestSetSpriteColor?.Invoke(brightness); // �������� �������� ���� �ҳ� ���� ��ȭ
    }

    private void PlayPiriSound(string type)
    {
        if (type == "Anger" && angerMelodies != null && angerMelodies.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, angerMelodies.Length);
            audioSource.clip = angerMelodies[randomIndex];
            audioSource.Play();
        }
        else if (type == "Peace" && peaceMelodies != null && peaceMelodies.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, peaceMelodies.Length);
            audioSource.clip = peaceMelodies[randomIndex];
            audioSource.Play();
        }
        else if (type == "PeaceFail" && peaceCancelMelody != null)
        {
            audioSource.clip = peaceCancelMelody;
            audioSource.Play();
        }
    }

    // ȸ���� �߰� ��� ����

    public IEnumerator EchoGuard()
    {
        if (echoGuardReady)
        {
            Debug.Log("���ڰ��� ����!");

            audioSource.clip = echoGuardMelody;
            audioSource.Play();

            RequestEchoGuardingState(true);
            isEchoGuarding = true; // ���ڰ��� ������
            echoGuardReady = false;
            StartCoroutine(EchoGuardCoolTimer()); // ��Ÿ�� �ڷ�ƾ ����

            RequestSetMoveSpeed?.Invoke(2f); // �̵��ӵ� 0���� ����
            
            EchoGuardAttackArea.SetActive(true);
            yield return new WaitForSeconds(0.5f); // �����ð� ���ڰ��� Ȱ��ȭ ����

            RequestEchoGuardingState(false);
            RequestSetMoveSpeed?.Invoke(5f); // �̵��ӵ� 5�� ����
            EchoGuardAttackArea.SetActive(false);
            isEchoGuarding = false; // ���ڰ��� ���� ����
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


            if (purifyingMelodies != null && purifyingMelodies.Length > 0) // ���� ���
            {
                int randomIndex = UnityEngine.Random.Range(0, purifyingMelodies.Length);
                audioSource.clip = purifyingMelodies[randomIndex];
                audioSource.time = 0f;
                audioSource.Play();
            }

            purifyStepReady = false;
            RequestisPurifing?.Invoke(true); // ��ȭ�� ���� ����
            RequestSetMoveSpeed?.Invoke(2.5f); // ��ȭ�� ���� �ӵ��� ����
            purifyRange.SetActive(true); // ��ȭ ���� Ȱ��ȭ

            StartCoroutine(PurifyDurationTimer()); // ��ȭ�� ���� ���� Ÿ�̸� ����
        }
    }

    public void PurifyStepStop() // ��ȭ�� ���� ���� �Լ�
    {
        Debug.Log("�ҳడ ��ȭ�� ������ ����ϴ�."); // �ִϸ��̼� bool�� false�� ���� -> �� isPurifying ���� �� Ȯ����!
        audioSource.Stop(); // ���� ����

        RequestSetMoveSpeed?.Invoke(5f); // �̵��ӵ� ���� �ӵ��� ����
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
