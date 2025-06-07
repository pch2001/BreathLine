using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerSkill : MonoBehaviour
{
    private PlayerCtrl playerCtrl;

    [SerializeField] private AudioSource audioSource;
    private Dictionary<string, AudioClip> piriClips; // ���� ���� Dictionary
    [SerializeField] private AudioClip angerMelody; // �г��� ���� ����
    [SerializeField] private AudioClip peaceMelody; // ��ȭ�� ���� ����
    [SerializeField] private AudioClip peaceCancelMelody; // ��ȭ�� ���� ���� ����

    public float playerDamage; // �÷��̾��� ���ݷ�
    private float piriStartTime; // �Ǹ����� ���� �ð�
    private bool isSoftPiriPlayed = false; // ��ȭ�� ���� ���ְ� �Ϸ�Ǿ�����
    private bool isSoftPiriStart = false; // ��ȭ�� ���� ���ְ� ���۵Ǿ�����
    [SerializeField] private float SoftPiriKeyDownTime; // ��ȭ�� ���� Ű�ٿ� �ð�
    private bool isRestoreSpeed = false; // RestoreSpeedAfterDelay �ڷ�ƾ�Լ� �ߺ����� ���� �÷���

    private SpriteRenderer wolfSpriteRenderer; // ���� ��������Ʈ ������
    public GameObject guardImg; // ���� ���� �̹���
    public Animator wolfEyesAnim; // ���� �� �ִϸ�����
    [SerializeField] private SpriteRenderer wolfEyes; // ���� �� ��������Ʈ

    [SerializeField] private GameObject AngerAttackArea; // �ҳ� �г��� ���� ���� ����
    [SerializeField] private GameObject PeaceAttackArea; //  �ҳ� ��ȭ�� ���� ���� ����
    [SerializeField] private GameObject PeaceWaitingEffect; //  �ҳ� ��ȭ�� ���� �غ� ����Ʈ
    [SerializeField] private GameObject wolfAppearArea; // ���� ��Ŭ�� ���� ����
    [SerializeField] private GameObject wolfAttackArea; // ���� ��Ŭ�� ���� ����

    private bool wolfMoveReady = true; // ���� �̵� ��Ÿ��
    private bool wolfAttackReady = true; // ���� ���� ��Ÿ��
    private bool wolfIsDamaged = false; // ���� �λ� ���� Ȯ��

    private float wolfPolution = 1f; // ���� ������ ���
    private float wolfFadeoutTime = 0.3f; // ��Ŭ���� ���밡 ������� �ð�
    private float wolfFadeinTime = 1f; // ��Ŭ���� ���밡 ��Ÿ���� �ð�

    public event Action<float> RequestMoveSpeed; // playerCtrl�� moveSpeed ���� ���� �̺�Ʈ
    public event Action<string> RequestAnimTrigger; // playerCtrl�� �ִϸ��̼� Trigger ���� �̺�Ʈ
    public event Action<float> RequestAnimSpeed; // playerCtrl�� animator ����ӵ� ���� �̺�Ʈ

    public event Action<string> RequestWolfAnimTrigger; // ������ �ִϸ��̼� Trigger ���� �̺�Ʈ
    public event Action<WolfState> RequestWolfState; // ���� ���� ���� �̺�Ʈ
    public event Action<float> RequestWolfStartAttack; // ���� ���� �˸� �̺�Ʈ

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        wolfSpriteRenderer = playerCtrl.wolf.GetComponent<SpriteRenderer>();
        wolfEyesAnim = wolfEyes.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameManager.Instance.RequestCurrentStage += OnUpdateStageData; // ��ȭ�� ������ ���·� �ʱ�ȭ
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

    private void PlaySoftPiriCanceled() // ��ȭ�� ���� ��� ��
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
        RequestMoveSpeed?.Invoke(0f); // �̵� ����
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
                RequestMoveSpeed?.Invoke(0f); // �̵� ����
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
        wolfPolution = GameManager.Instance.currentStageData.wolfCoefficient;

        AngerAttackArea.transform.localScale = Vector3.one * GameManager.Instance.currentStageData.anger_range;
        PeaceAttackArea.transform.localScale = Vector3.one * GameManager.Instance.currentStageData.peace_range;
        playerDamage = GameManager.Instance.currentStageData.anger_damage;

        piriClips = new Dictionary<string, AudioClip>();
        piriClips.Add("Anger", angerMelody);
        piriClips.Add("Peace", peaceMelody);
        piriClips.Add("PeaceFail", peaceCancelMelody);

        float brightness = (255f - wolfPolution * 30f) / 255f;
        brightness = Mathf.Clamp01(brightness); // 0~1 ���̷� ����

        wolfSpriteRenderer.color = new Color(brightness, brightness, brightness, wolfSpriteRenderer.color.a);
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

    //���⼭���� ���� ������

    private float EaseOutExpo(float t)
    {
        return t == 0 ? 0 : Mathf.Pow(2, 15 * (t - 1));
    }

    public IEnumerator WolfAppear(bool isExist) // ���� ���� ����
    {
        if(!wolfMoveReady) yield break; // �̵� ��Ÿ�ӽ� ���� �Ұ���(�ʹ� ����� �̵� ����)

        wolfMoveReady = false;
        StartCoroutine(WolfMoveCool()); // �̵� ��Ÿ�� ����
        wolfEyes.enabled = false; // �÷��̾� �� ���� �� ����

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //���콺 ��ġ ����
        Vector2 isStartRight; // ���� ù����� ���� ���� ����

        float timer = 0f;

        if(mousePosition.x > playerCtrl.transform.position.x)
        {
            wolfSpriteRenderer.flipX = false;
            isStartRight = Vector2.left * 4; 
        }
        else
        {
            wolfSpriteRenderer.flipX = true;
            isStartRight = Vector2.right * 4;
        }

        if (isExist) // ���� �̹� �����
        {
            // ���� ���� ��ġ���� ������� ����
            Debug.Log("���밡 ������ϴ�!");
            RequestWolfAnimTrigger?.Invoke("Hide");
            wolfAppearArea.SetActive(false); // ���� ���� ȿ�� ����

            while (timer < wolfFadeoutTime)
            {
                timer += Time.deltaTime;
                float newAlpha = Mathf.Lerp(1f, 0f, timer / wolfFadeoutTime); // ���������� fade out ��ȭ
                wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, newAlpha);
                yield return null;
            }
            wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, 0f);
            timer = 0f; // Ÿ�̸� �ʱ�ȭ
        }

        // ���ο� ��ġ�� ��Ÿ���� ����
        Debug.Log("���밡 ��Ÿ���ϴ�!");
        RequestWolfState(WolfState.Idle); // ���� Idle ���·� ����
        playerCtrl.wolf.transform.position = mousePosition + isStartRight; // ���� �����
        RequestWolfAnimTrigger?.Invoke("Move");
        while (timer < wolfFadeinTime)
        {
            timer += Time.deltaTime;
            float newAlpha = EaseOutExpo(timer); // ���������� �ڷ� ������ ������ fade in 
            wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, newAlpha);
            playerCtrl.wolf.transform.position
            = Vector2.Lerp(playerCtrl.wolf.transform.position, mousePosition, EaseOutExpo(timer));
            yield return null;

            if (wolfIsDamaged)
            {
                Debug.Log("���� �� ���밡 �ҳ� ������ �̵��մϴ�!");
                playerCtrl.wolf.transform.position = playerCtrl.transform.position; // �ҳ� ��ġ�� �̵�
                yield break; // ���� �λ�� �Լ� ���� ����
            }
        }
        wolfAppearArea.SetActive(true); // ���� ���� ȿ�� ���� (���� ��ȭ��Ŵ)
        wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, 1f);
        playerCtrl.wolf.transform.position = mousePosition;
    }
    public IEnumerator WolfAttack() // ���� ���� ����
    {
        if(wolfAttackReady)
        {
            wolfAttackReady = false;
            StartCoroutine(WolfAttackCool()); // ��Ÿ�� �ڷ�ƾ ����

            RequestWolfAnimTrigger?.Invoke("Attack");
            yield return new WaitForSeconds(0.4f);

            wolfAppearArea.SetActive(false); // ���� ��� ����Ʈ ����
            wolfAttackArea.SetActive(true);
            yield return new WaitForSeconds(0.4f); // ���� ��� ���

            wolfAttackArea.SetActive(false);
            wolfAppearArea.SetActive(true); // ���� ��� ����Ʈ ����
            RequestWolfState(WolfState.Idle);
        }
    }
    public IEnumerator WolfHide(bool isGuarded) // ���� Hide ����, �Ű������� ���� ���带 ���� ȣ��� Hide���� ����
    {
        RequestWolfAnimTrigger?.Invoke("Hide");
        RequestWolfState(WolfState.Hide);
        wolfAppearArea.SetActive(false); // ���� ���� ȿ�� ����

        if (isGuarded)
        {
            wolfSpriteRenderer.flipX = playerCtrl.spriteRenderer.flipX; // �ҳ�� ���� ������ �ٶ�
            playerCtrl.wolf.transform.position = playerCtrl.transform.position; // �ҳ� ��ġ�� �̵�
            RequestWolfState(WolfState.Damaged); // ���� ���� ��ȭ(Damaged)
            yield return StartCoroutine(FadeCoroutine(1.0f, 0.4f)); // FadeIn
        }
        wolfEyesAnim.SetBool("wolfDamaged", wolfIsDamaged);
        wolfEyes.enabled = true; // ���� �� ��Ÿ�����
           
        yield return StartCoroutine(FadeCoroutine(0.0f, 0.3f)); // FadeOut
    }

    public void WolfGuard() // ���� ���� ����
    {
        if(!wolfIsDamaged)
        {
            wolfIsDamaged = true; // ����λ� ���� (�������� ���, �ڷ�ƾ Ż��)
            
            StartCoroutine(WolfHide(true));
            StartCoroutine(WolfGuardEffect()); // ���� ����Ʈ �ڷ�ƾ ���� 
            StartCoroutine(WolfGuardCool()); // ���� ��Ÿ�� �ڷ�ƾ ����

        }
    }

    private IEnumerator WolfGuardEffect() // ���� ���� ����ƮƮ �ڷ�ƾ
    {
        guardImg.transform.position = this.transform.position;
        guardImg.SetActive(true);
        yield return new WaitForSeconds(0.4f); // 0.4�� �� �����
        guardImg.SetActive(false);
    }
    
    private IEnumerator WolfMoveCool() // ���� �̵� ��Ÿ�� �ڷ�ƾ
    {
        yield return new WaitForSeconds(1f);
        wolfMoveReady = true;
    }

    private IEnumerator WolfAttackCool() // ���� ���� ��Ÿ�� �ڷ�ƾ
    {
        RequestWolfStartAttack(2.5f); // PlayerCtrl���� ���� ���������� �˸� (UI ����ȭ)
        yield return new WaitForSeconds(2.5f);
        wolfAttackReady = true;
    }
    private IEnumerator WolfGuardCool() // ���� ���� ��Ÿ�� �ڷ�ƾ, ���� �� ��Ÿ�ӵ��� ���� ���� �Ұ�
    {
        Debug.Log("���� �λ�! ȸ����");
        wolfEyesAnim.SetBool("wolfDamaged", wolfIsDamaged);

        yield return new WaitForSeconds(5.0f);

        Debug.Log("���� ȸ��!");
        RequestWolfState(WolfState.Hide);
        wolfIsDamaged = false; // ���� �λ� ȸ��
        wolfEyesAnim.SetBool("wolfDamaged", wolfIsDamaged);
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration) // ������ fade in/out�� ���� �Լ�, targetAlpha�� ����, duration�� ����ð�
    {
        float startAlpha = wolfSpriteRenderer.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, newAlpha);
            yield return null;
        }

        wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, targetAlpha);
    }
}
