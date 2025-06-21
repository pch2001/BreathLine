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
    private Dictionary<string, AudioClip> piriClips; // 음원 저장 Dictionary
    [SerializeField] private AudioClip angerMelody; // 분노의 악장 음원
    [SerializeField] private AudioClip peaceMelody; // 평화의 악장 음원
    [SerializeField] private AudioClip peaceCancelMelody; // 평화의 악장 실패 음원

    [SerializeField] private GameObject AngerAttackArea; // 소녀 분노의 악장 공격 범위
    [SerializeField] private GameObject PeaceAttackArea; //  소녀 평화의 악장 공격 범위
    [SerializeField] private GameObject EchoGuardAttackArea; //  소녀 에코가드 공격 범위
    [SerializeField] private GameObject PeaceWaitingEffect; //  소녀 평화의 악장 준비 이펙트

    public float playerDamage; // 플레이어의 공격력
    private float piriStartTime; // 피리연주 시작 시간
    private bool isSoftPiriPlayed = false; // 평화의 악장 연주가 완료되었는지
    private bool isSoftPiriStart = false; // 평화의 악장 연주가 시작되었는지
    [SerializeField] private float SoftPiriKeyDownTime; // 평화의 악장 키다운 시간
    private bool isRestoreSpeed = false; // RestoreSpeedAfterDelay 코루틴함수 중복실행 방지 플래그
    private float playerPollution = 1f; // 소녀 오염도 계수
    private bool echoGuardReady = true; // 에코가드 쿨타임 확인

    [SerializeField] private float purifyDuration = 2f; // 정화의 걸음 최대 지속시간
    [SerializeField] private GameObject purifyRange; // 정화의 걸음 최대 지속시간
    public bool purifyStepReady = true; // 정화의 걸음 쿨타임 확인

    public event Action<float> RequestMoveSpeed; // playerCtrl의 moveSpeed 변수 변경 이벤트
    public event Action<string> RequestAnimTrigger; // playerCtrl의 애니메이션 Trigger 변경 이벤트
    public event Action<float> RequestAnimSpeed; // playerCtrl의 animator 재생속도 변경 이벤트
    public event Action<float> RequestEchoGuardStart; // playerCtrl에게 에코가드 실행 알림 이벤트
    public event Action<float> RequestPuriFyStepStart; // playerCtrl에게 정화의 걸음 실행 알림 이벤트
    public event Action<bool> RequestisPurifing; // playerCtrl의 isPurify 변수 변경
    public event Action<float> RequestSetSpriteColor; // playerCtrl의 Sprite 색상을 오염도 변경에 따른 설정 이벤트

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl_R>();
    }
    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RequestCurrentStage += OnUpdateStageData;
    }

    private IEnumerator PlayShortPiri() // 분노의 악장 구현
    {
        Debug.Log("피리로 [분노의 악장]을 연주합니다!");
        PlayPiriSound("Anger");
        RequestMoveSpeed?.Invoke(0.5f); // 이동속도 0.5로 변경
        RestoreSpeedAfterDelay(0.5f);

        AngerAttackArea.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        AngerAttackArea.SetActive(false);
    }

    public void PlaySoftPiriCanceled() // 평화의 악장 취소 시
    {
        Debug.Log("[평화의 악장] 연주 실패...");
        PeaceWaitingEffect.SetActive(false); // 평화의 악장 준비 이펙트 종료
        audioSource.Stop();
        PlayPiriSound("PeaceFail");
    }

    public void StartPiri() // 연주버튼 입력시 모션 실행 및 변수값 저장
    {
        piriStartTime = Time.time;
        isSoftPiriStart = false;
        isSoftPiriPlayed = false;
        RequestMoveSpeed?.Invoke(0.5f); // 이동 중지
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
                PeaceWaitingEffect.SetActive(true); // 평화의 악장 준비 이펙트 활성화

                audioSource.clip = piriClips["Peace"];
                audioSource.time = 0f;
                RequestMoveSpeed?.Invoke(1.5f); // 이동 중지
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
        PeaceWaitingEffect.SetActive(false); // 평화의 악장 준비 이펙트 종료

        audioSource.time = 9f; // 평화의 악장 끝부분(8.5초)으로 이동 (잔음도 표현)
        RequestAnimSpeed?.Invoke(1f);
        RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Happy);
        RequestMoveSpeed?.Invoke(2.5f); // 이동속도 2.5로 변경
        isSoftPiriPlayed = true;

        PeaceAttackArea.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        PeaceAttackArea.SetActive(false);

        StartCoroutine(RestoreSpeedAfterDelay(0.5f)); // 0.5초동안 잠시 이동속도 감소
    }

    public void OnUpdateStageData()  // 오염도 변경에 따른 데이터 업데이트 (ex. 연결된 음원들 딕셔너리에 초기화)
    {
        angerMelody = GameManager.Instance.currentStageData.anger_audioClip; // 오염도 스테이지에 따른 음원으로 교체
        peaceMelody = GameManager.Instance.currentStageData.peace_audioClip;
        playerPollution = GameManager.Instance.currentStageData.pollution_Coefficient; // 현재 오염도 계수

        AngerAttackArea.transform.localScale = Vector3.one * GameManager.Instance.currentStageData.anger_range;
        PeaceAttackArea.transform.localScale = Vector3.one * GameManager.Instance.currentStageData.peace_range;
        playerDamage = GameManager.Instance.currentStageData.anger_damage;

        piriClips = new Dictionary<string, AudioClip>();
        piriClips.Add("Anger", angerMelody);
        piriClips.Add("Peace", peaceMelody);
        piriClips.Add("PeaceFail", peaceCancelMelody);

        float brightness = (255f - playerPollution * 30f) / 255f;
        brightness = Mathf.Clamp01(brightness); // 0~1 사이로 보정

        RequestSetSpriteColor?.Invoke(brightness); // 이제부터 오염도에 따라 소녀 색상 변화
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

    public IEnumerator EchoGuard()
    {
        if (echoGuardReady)
        {
            Debug.Log("에코가드 실행!");

            echoGuardReady = false;
            StartCoroutine(EchoGuardCoolTimer()); // 쿨타임 코루틴 실행

            RequestAnimTrigger?.Invoke(PlayerAnimTrigger.Sad);
            RequestMoveSpeed?.Invoke(0f); // 이동속도 0으로 변경
            EchoGuardAttackArea.SetActive(true);
            yield return new WaitForSeconds(0.1f); // 일정시간 에코가드 활성화 유지

            EchoGuardAttackArea.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            RequestMoveSpeed?.Invoke(5f); // 기존 속도로 변경
        }
    }

    private IEnumerator EchoGuardCoolTimer() // 에코가드 쿨타임 코루틴
    {
        RequestEchoGuardStart(1f); // PlayerCtrl_R에게 에코가드 실행을 알림 (UI 동기화)
        yield return new WaitForSeconds(1f);
        echoGuardReady = true;
    }

    public void PurifyStepStart() // 정화의 걸음 시작 함수
    {
        if (purifyStepReady)
        {
            Debug.Log("소녀가 정화의 걸음을 시작합니다."); // 애니메이션 추가해야 해! -> 애니메이션 bool값 만들어서 실행, 해당동작 loop로 만들어서 사용
            purifyStepReady = false;
            RequestisPurifing?.Invoke(true); // 정화의 걸음 시작
            RequestMoveSpeed?.Invoke(2.5f); // 정화의 걸음 속도로 변경
            purifyRange.SetActive(true); // 정화 범위 활성화

            StartCoroutine(PurifyDurationTimer()); // 정화의 걸음 지속 타이머 시작
        }
    }

    public void PurifyStepStop() // 정화의 걸음 종료 함수
    {
        Debug.Log("소녀가 정화의 걸음을 멈춥니다."); // 애니메이션 bool값 false로 변경 -> 꼭 isPurifying 순서 잘 확인해!
        RequestMoveSpeed?.Invoke(5f); // 이동속도 기존 속도로 변경
        purifyRange.SetActive(false); // 정화 범위 비활성화
        RequestisPurifing?.Invoke(false); // 정화의 걸음 종료

        StartCoroutine(PurifyCoolTimer()); // 정화의 걸음 쿨타임 타이머 시작
    }

    IEnumerator PurifyDurationTimer() // 정화의 걸음 쿨타임 코루틴
    {
        yield return new WaitForSeconds(purifyDuration);

        playerCtrl.PurifyStepStop(); // 정화의 걸음 종료
    }

    IEnumerator PurifyCoolTimer() // 정화의 걸음 쿨타임 코루틴
    {
        RequestPuriFyStepStart(5f); // PlayerCtrl_R에게 정화의 걸음 실행을 알림 (UI 동기화)
        yield return new WaitForSeconds(5f); // 쿨타임 설정 5초
        purifyStepReady = true; // 정화의 걸음 준비 완료
    }
}
