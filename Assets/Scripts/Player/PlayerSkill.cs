using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerSkill : MonoBehaviour
{
    private PlayerCtrl playerCtrl;

    [SerializeField] private AudioSource audioSource;
    private Dictionary<string, AudioClip> piriClips; // 음원 저장 Dictionary
    [SerializeField] private AudioClip angerMelody; // 분노의 악장 음원
    [SerializeField] private AudioClip peaceMelody; // 평화의 악장 음원
    [SerializeField] private AudioClip peaceCancelMelody; // 평화의 악장 실패 음원

    private float piriStartTime; // 피리연주 시작 시간
    private bool isSoftPiriPlayed = false; // 평화의 악장 연주가 완료되었는지
    private bool isSoftPiriStart = false; // 평화의 악장 연주가 시작되었는지
    [SerializeField] private float SoftPiriKeyDownTime; // 평화의 악장 키다운 시간
    private bool isRestoreSpeed = false; // RestoreSpeedAfterDelay 코루틴함수 중복실행 방지 플래그

    private SpriteRenderer wolfSpriteRenderer; // 늑대 스프라이트 반전용
    public GameObject guardImg; // 늑대 가드 이미지
    public Animator wolfEyesAnim; // 늑대 눈 애니메이터
    [SerializeField] private SpriteRenderer wolfEyes; // 늑대 눈 스프라이트

    [SerializeField] private GameObject wolfAttackArea; // 소녀 분노의 악장 공격 범위
    [SerializeField] private GameObject AngerAttackArea; // 소녀 평화의 악장 공격 범위
    [SerializeField] private GameObject PeaceAttackArea; // 늑대 공격 범위

    private bool wolfMoveReady = true; // 늑대 이동 쿨타임
    private bool wolfAttackReady = true; // 늑대 공격 쿨타임
    private bool wolfGuardReady = true; // 늑대 가드 쿨타임
    private float wolfPolution = 1f; // 늑대 오염도 계수

    public event Action<float> RequestMoveSpeed; // playerCtrl의 moveSpeed 변수 변경 이벤트
    public event Action<string> RequestAnimTrigger; // playerCtrl의 애니메이션 Trigger 변경 이벤트
    public event Action<float> RequestAnimSpeed; // playerCtrl의 animator 재생속도 변경 이벤트

    public event Action<string> RequestWolfAnimTrigger; // 늑대의 애니메이션 Trigger 변경 이벤트
    public event Action<WolfState> RequestWolfState; // 늑대 상태 변경 이벤트

    public event Action<float> RequestWolfStartAttack; // 늑대 공격 알림 이벤트

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        wolfSpriteRenderer = playerCtrl.wolf.GetComponent<SpriteRenderer>();
        wolfEyesAnim = wolfEyes.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameManager.Instance.RequestCurrentStage += OnUpdateStageData; // 변화된 오염도 상태로 초기화
    }

    private IEnumerator PlayShortPiri() // 분노의 악장 구현
    {
        Debug.Log("피리로 [분노의 악장]을 연주합니다!");
        PlayPiriSound("Anger");
        RequestMoveSpeed?.Invoke(0.5f); // 이동속도 0.5로 변경
        RestoreSpeedAfterDelay(0.5f);

        AngerAttackArea.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        AngerAttackArea.gameObject.SetActive(false);
    }

    private void PlaySoftPiriCanceled() // 평화의 악장 취소 시
    {
        Debug.Log("[평화의 악장] 연주 실패...");
        audioSource.Stop();
        PlayPiriSound("PeaceFail");
    }

    public void StartPiri() // 연주버튼 입력시 모션 실행 및 변수값 저장
    {
        piriStartTime = Time.time;
        isSoftPiriStart = false;
        isSoftPiriPlayed = false;
        RequestMoveSpeed?.Invoke(0f); // 이동 중지
        RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Sad); // 피리 부는 듯한 연출
        RequestAnimSpeed?.Invoke(0f); // 피리 부는 모습으로 애니메이션 멈춤
    }

    public void ReleasePiri() // 연주버튼 입력 시간에 따른 연주 분기 조건 (분노의 악장 + 평화의 악장 실패시)
    {
            float duration = Time.time - piriStartTime; // 연주버튼 누른 시간

            if (duration <= 0.3f)
            {
                StartCoroutine(PlayShortPiri());
            }
            else if (duration > 0.4f && duration < SoftPiriKeyDownTime)
            {
                PlaySoftPiriCanceled();
            }

            RequestAnimSpeed?.Invoke(1f);
            RequestMoveSpeed?.Invoke(2.5f); // 이동속도 2.5로 변경
            StartCoroutine(RestoreSpeedAfterDelay(0.5f)); // 0.5초동안 잠시 이동속도 감소
    }

    public void CheckSoftPiri() // 평화의 악장 차징시간 도달 확인 
    {
        if (!isSoftPiriPlayed) // 피리 연주시 && 평화의 악장 연주 완료 여부
        {
            float duration = Time.time - piriStartTime;
            if (duration > 0.4f && !isSoftPiriStart)
            {
                Debug.Log("[평화의 악장] 연주 시작");
                audioSource.clip = piriClips["Peace"];
                audioSource.time = 0f;
                RequestMoveSpeed?.Invoke(0f); // 이동 중지
                audioSource.Play(); // 평화의 악장 연주 시작
                isSoftPiriStart = true;
            }
            if (duration > SoftPiriKeyDownTime)
            {
                StartCoroutine(PlaySoftPiri()); // 평화의 악장 연주 성공
            }
        }
    }

    private IEnumerator PlaySoftPiri()
    {
        Debug.Log("피리로 [평화의 악장]을 연주해냅니다.");
        audioSource.time = 8.5f; // 평화의 악장 끝부분(8.5초)으로 이동 (잔음도 표현)
        RequestAnimSpeed?.Invoke(1f);
        RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Happy);
        RequestMoveSpeed?.Invoke(2.5f); // 이동속도 2.5로 변경
        isSoftPiriPlayed = true;

        PeaceAttackArea.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        PeaceAttackArea.gameObject.SetActive(false);

        StartCoroutine(RestoreSpeedAfterDelay(0.5f)); // 0.5초동안 잠시 이동속도 감소
    }

    public void OnUpdateStageData()  // 오염도 변경에 따른 데이터 업데이트 (ex. 연결된 음원들 딕셔너리에 초기화)
    {
        angerMelody = GameManager.Instance.currentStageData.anger_audioClip; // 오염도 스테이지에 따른 음원으로 교체
        peaceMelody = GameManager.Instance.currentStageData.peace_audioClip;
        wolfPolution = GameManager.Instance.currentStageData.wolfCoefficient;

        AngerAttackArea.transform.localScale = Vector2.one * GameManager.Instance.currentStageData.anger_range;
        PeaceAttackArea.transform.localScale = Vector2.one * GameManager.Instance.currentStageData.peace_range;

        piriClips = new Dictionary<string, AudioClip>();
        piriClips.Add("Anger", angerMelody);
        piriClips.Add("Peace", peaceMelody);
        piriClips.Add("PeaceFail", peaceCancelMelody);

        float brightness = (255f - wolfPolution * 30f) / 255f;
        brightness = Mathf.Clamp01(brightness); // 0~1 사이로 보정
        wolfSpriteRenderer.color = new Color(brightness, brightness, brightness, wolfSpriteRenderer.color.a);
    }

    private IEnumerator RestoreSpeedAfterDelay(float delay) // 일정시간 후 원래속도로 복귀
    {
        if (isRestoreSpeed) yield break; // 함수 중복 실행 방지
        isRestoreSpeed = true;

        yield return new WaitForSeconds(delay);
        RequestMoveSpeed?.Invoke(5f); // 기존 속도로 변경
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

//여기서부터 늑대 구현부

    public IEnumerator WolfAppear() // 늑대 등장 구현
    {
        if(!wolfMoveReady) yield break; // 이동 쿨타임시 조작 불가능(너무 빈번한 이동 방지)
        StartCoroutine(WolfMoveCool()); // 이동 쿨타임 시작

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //마우스 위치 받음
        Vector2 isStartRight; // 늑대 첫등장시 더할 단위 벡터

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

        // 사라지는 연출
        RequestWolfAnimTrigger?.Invoke("Hide");
        yield return StartCoroutine(FadeCoroutine(0.0f, 0.3f)); // FadeOut

        playerCtrl.wolf.transform.position = mousePosition + isStartRight; // 늑대 출발점
        RequestWolfAnimTrigger?.Invoke("Idle");

        while (Vector2.Distance(playerCtrl.wolf.transform.position, mousePosition) > 0.05f)
        {
            // 위치 갱신
            playerCtrl.wolf.transform.position
            = Vector2.MoveTowards(playerCtrl.wolf.transform.position, mousePosition, 10 * Time.deltaTime);

            yield return null;
        }

        wolfEyes.enabled = false; // 플레이어 위 늑대 눈 숨김
        yield return StartCoroutine(FadeCoroutine(1.0f, 1f)); // FadeIn
    }
    public IEnumerator WolfAttack() // 늑대 공격 구현
    {
        if(wolfAttackReady)
        {
            RequestWolfAnimTrigger?.Invoke("Attack");

            yield return new WaitForSeconds(0.4f);

            wolfAttackArea.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.1f); // 공격 모션 대기

            wolfAttackArea.gameObject.SetActive(false);
            RequestWolfState(WolfState.Idle);
            StartCoroutine(WolfAttackCool()); // 쿨타임 코루틴 실행
        }
    }
    public IEnumerator WolfHide(bool isGuarded) // 늑대 Hide 구현, 매개변수는 늑대 가드를 통해 호출된 Hide인지 구별
    {
        RequestWolfAnimTrigger?.Invoke("Hide");
        RequestWolfState(WolfState.Hide);

        if (isGuarded)
        {
            wolfSpriteRenderer.flipX = playerCtrl.spriteRenderer.flipX; // 소녀와 같은 방향을 바라봄
            playerCtrl.wolf.transform.position = playerCtrl.transform.position; // 소녀 위치로 이동
            yield return StartCoroutine(FadeCoroutine(1.0f, 0.4f)); // FadeIn
        }
        wolfEyesAnim.SetBool("isOpen", wolfGuardReady);
        wolfEyes.enabled = true; // 늑대 눈 나타내기기
           
        yield return StartCoroutine(FadeCoroutine(0.0f, 0.3f)); // FadeOut
    }

    public void WolfGuard() // 늑대 가드 구현
    {
        if(wolfGuardReady)
        {
            StartCoroutine(WolfHide(true));
            StartCoroutine(WolfGuardEffect()); // 가드 이펙트 코루틴 실행 
            StartCoroutine(WolfGuardCool()); // 가드 쿨타임 코루틴 실행

            RequestWolfState(WolfState.Damaged); // 늑대 부상
        }
    }

    private IEnumerator WolfGuardEffect() // 늑대 가드 이펙트트 코루틴
    {
        guardImg.transform.position = this.transform.position;
        guardImg.SetActive(true);
        yield return new WaitForSeconds(0.4f); // 0.4초 후 사라짐
        guardImg.SetActive(false);
    }
    
    private IEnumerator WolfMoveCool() // 늑대 이동 쿨타임 코루틴
    {
        wolfMoveReady = false;
        yield return new WaitForSeconds(0.7f);
        wolfMoveReady = true;
    }

    private IEnumerator WolfAttackCool() // 늑대 공격 쿨타임 코루틴
    {
        wolfAttackReady = false;
        RequestWolfStartAttack(2.5f); // PlayerCtrl에게 늑대 공격했음을 알림 (UI 동기화)
        yield return new WaitForSeconds(2.5f);
        wolfAttackReady = true;
    }
    private IEnumerator WolfGuardCool() // 늑대 가드 쿨타임 코루틴, 성공 후 쿨타임동안 늑대 제어 불가
    {
        wolfGuardReady = false;
        wolfEyesAnim.SetBool("isOpen", wolfGuardReady);
        Debug.Log("늑대 부상! 회복중");

        yield return new WaitForSeconds(5.0f);

        RequestWolfState(WolfState.Hide);
        wolfGuardReady = true;
        wolfEyesAnim.SetBool("isOpen", wolfGuardReady);
        Debug.Log("늑대 회복!");
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration) // 늑대의 fade in/out을 위한 함수, targetAlpha는 투명도, duration은 실행시간
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
