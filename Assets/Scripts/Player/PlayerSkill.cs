using System;
using System.Collections;
using System.Collections.Generic;
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

    private float piriStartTime; // �Ǹ����� ���� �ð�
    private bool isSoftPiriPlayed = false; // ��ȭ�� ���� ���ְ� �Ϸ�Ǿ�����
    private bool isSoftPiriStart = false; // ��ȭ�� ���� ���ְ� ���۵Ǿ�����
    [SerializeField] private float SoftPiriKeyDownTime; // ��ȭ�� ���� Ű�ٿ� �ð�
    private bool isRestoreSpeed = false; // RestoreSpeedAfterDelay �ڷ�ƾ�Լ� �ߺ����� ���� �÷���

    private SpriteRenderer wolfSpriteRenderer; // ���� ��������Ʈ ������
    public GameObject guardImg; // ���� ���� �̹���
    public Animator wolfEyesAnim; // ���� �� �ִϸ�����
    [SerializeField] private SpriteRenderer wolfEyes; // ���� �� ��������Ʈ

    [SerializeField] private GameObject wolfAttackArea; // �ҳ� �г��� ���� ���� ����
    [SerializeField] private GameObject AngerAttackArea; // �ҳ� ��ȭ�� ���� ���� ����
    [SerializeField] private GameObject PeaceAttackArea; // ���� ���� ����

    private bool wolfMoveReady = true; // ���� �̵� ��Ÿ��
    private bool wolfAttackReady = true; // ���� ���� ��Ÿ��
    private bool wolfGuardReady = true; // ���� ���� ��Ÿ��
    private float wolfPolution = 1f; // ���� ������ ���

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

        AngerAttackArea.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        AngerAttackArea.gameObject.SetActive(false);
    }

    private void PlaySoftPiriCanceled() // ��ȭ�� ���� ��� ��
    {
        Debug.Log("[��ȭ�� ����] ���� ����...");
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
        audioSource.time = 8.5f; // ��ȭ�� ���� ���κ�(8.5��)���� �̵� (������ ǥ��)
        RequestAnimSpeed?.Invoke(1f);
        RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Happy);
        RequestMoveSpeed?.Invoke(2.5f); // �̵��ӵ� 2.5�� ����
        isSoftPiriPlayed = true;

        PeaceAttackArea.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        PeaceAttackArea.gameObject.SetActive(false);

        StartCoroutine(RestoreSpeedAfterDelay(0.5f)); // 0.5�ʵ��� ��� �̵��ӵ� ����
    }

    public void OnUpdateStageData()  // ������ ���濡 ���� ������ ������Ʈ (ex. ����� ������ ��ųʸ��� �ʱ�ȭ)
    {
        angerMelody = GameManager.Instance.currentStageData.anger_audioClip; // ������ ���������� ���� �������� ��ü
        peaceMelody = GameManager.Instance.currentStageData.peace_audioClip;
        wolfPolution = GameManager.Instance.currentStageData.wolfCoefficient;

        AngerAttackArea.transform.localScale = Vector2.one * GameManager.Instance.currentStageData.anger_range;
        PeaceAttackArea.transform.localScale = Vector2.one * GameManager.Instance.currentStageData.peace_range;

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

    public IEnumerator WolfAppear() // ���� ���� ����
    {
        if(!wolfMoveReady) yield break; // �̵� ��Ÿ�ӽ� ���� �Ұ���(�ʹ� ����� �̵� ����)
        StartCoroutine(WolfMoveCool()); // �̵� ��Ÿ�� ����

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //���콺 ��ġ ����
        Vector2 isStartRight; // ���� ù����� ���� ���� ����

        if(mousePosition.x > playerCtrl.transform.position.x)
        {
            wolfSpriteRenderer.flipX = false;
            isStartRight = Vector2.left; 
        }
        else
        {
            wolfSpriteRenderer.flipX = true;
            isStartRight = Vector2.right;
        }

        // ������� ����
        RequestWolfAnimTrigger?.Invoke("Hide");
        yield return StartCoroutine(FadeCoroutine(0.0f, 0.3f)); // FadeOut

        playerCtrl.wolf.transform.position = mousePosition + isStartRight; // ���� �����
        RequestWolfAnimTrigger?.Invoke("Idle");

        while (Vector2.Distance(playerCtrl.wolf.transform.position, mousePosition) > 0.05f)
        {
            // ��ġ ����
            playerCtrl.wolf.transform.position
            = Vector2.MoveTowards(playerCtrl.wolf.transform.position, mousePosition, 10 * Time.deltaTime);

            yield return null;
        }

        wolfEyes.enabled = false; // �÷��̾� �� ���� �� ����
        yield return StartCoroutine(FadeCoroutine(1.0f, 1f)); // FadeIn
    }
    public IEnumerator WolfAttack() // ���� ���� ����
    {
        if(wolfAttackReady)
        {
            RequestWolfAnimTrigger?.Invoke("Attack");

            yield return new WaitForSeconds(0.4f);

            wolfAttackArea.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.1f); // ���� ��� ���

            wolfAttackArea.gameObject.SetActive(false);
            RequestWolfState(WolfState.Idle);
            StartCoroutine(WolfAttackCool()); // ��Ÿ�� �ڷ�ƾ ����
        }
    }
    public IEnumerator WolfHide(bool isGuarded) // ���� Hide ����, �Ű������� ���� ���带 ���� ȣ��� Hide���� ����
    {
        RequestWolfAnimTrigger?.Invoke("Hide");
        RequestWolfState(WolfState.Hide);

        if (isGuarded)
        {
            wolfSpriteRenderer.flipX = playerCtrl.spriteRenderer.flipX; // �ҳ�� ���� ������ �ٶ�
            playerCtrl.wolf.transform.position = playerCtrl.transform.position; // �ҳ� ��ġ�� �̵�
            yield return StartCoroutine(FadeCoroutine(1.0f, 0.4f)); // FadeIn
        }
        wolfEyesAnim.SetBool("isOpen", wolfGuardReady);
        wolfEyes.enabled = true; // ���� �� ��Ÿ�����
           
        yield return StartCoroutine(FadeCoroutine(0.0f, 0.3f)); // FadeOut
    }

    public void WolfGuard() // ���� ���� ����
    {
        if(wolfGuardReady)
        {
            StartCoroutine(WolfHide(true));
            StartCoroutine(WolfGuardEffect()); // ���� ����Ʈ �ڷ�ƾ ���� 
            StartCoroutine(WolfGuardCool()); // ���� ��Ÿ�� �ڷ�ƾ ����

            RequestWolfState(WolfState.Damaged); // ���� �λ�
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
        wolfMoveReady = false;
        yield return new WaitForSeconds(0.7f);
        wolfMoveReady = true;
    }

    private IEnumerator WolfAttackCool() // ���� ���� ��Ÿ�� �ڷ�ƾ
    {
        wolfAttackReady = false;
        RequestWolfStartAttack(2.5f); // PlayerCtrl���� ���� ���������� �˸� (UI ����ȭ)
        yield return new WaitForSeconds(2.5f);
        wolfAttackReady = true;
    }
    private IEnumerator WolfGuardCool() // ���� ���� ��Ÿ�� �ڷ�ƾ, ���� �� ��Ÿ�ӵ��� ���� ���� �Ұ�
    {
        wolfGuardReady = false;
        wolfEyesAnim.SetBool("isOpen", wolfGuardReady);
        Debug.Log("���� �λ�! ȸ����");

        yield return new WaitForSeconds(5.0f);

        RequestWolfState(WolfState.Hide);
        wolfGuardReady = true;
        wolfEyesAnim.SetBool("isOpen", wolfGuardReady);
        Debug.Log("���� ȸ��!");
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
