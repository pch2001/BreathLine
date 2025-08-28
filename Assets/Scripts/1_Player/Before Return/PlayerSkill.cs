using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerSkill : MonoBehaviour
{
    private PlayerCtrl playerCtrl;
    [SerializeField] private AudioSource girlAudioSource; // 소녀 소리재생
    [SerializeField] private AudioSource wolfAudioSource; // 늑대 소리 재생

    [SerializeField] private AudioClip[] angerNotes; // 분노의 음표 음원
    [SerializeField] private AudioClip[] peaceNotes; // 평화의 음표 음원
    [SerializeField] private AudioClip[] angerMelody; // 분노의 악장 음원
    [SerializeField] private AudioClip[] peaceMelody; // 평화의 악장 음원
    [SerializeField] private AudioClip cancelMelody; // 연주 실패 음원
    [SerializeField] private AudioClip wolfAttackAudio; // 늑대 공격 음원

    [SerializeField] private GameObject AngerAttackArea; // 소녀 분노의 악장 공격 범위
    [SerializeField] private GameObject AngerAttackEffect; // 소녀 분노의 악장 공격 이펙트
    [SerializeField] private GameObject PeaceAttackArea; //  소녀 평화의 악장 공격 범위
    [SerializeField] private GameObject PeaceWaitingEffect; //  소녀 평화의 악장 준비 이펙트
    [SerializeField] private GameObject wolfAppearArea; // 늑대 좌클릭 공격 범위
    [SerializeField] private GameObject wolfAttackArea; // 늑대 우클릭 공격 범위
    [SerializeField] private GameObject wolfPushArea; // 늑대 밀격 공격 범위

    // * 소녀 관련 변수

    [SerializeField] private Transform beatBox; // 콤보 표시 박스
    [SerializeField] private GameObject angerNote; // 분노의 악장 음표 프리팹
    [SerializeField] private GameObject peaceNote; // 평화의 악장 음표 프리팹
    [SerializeField] private List<MelodyScore> ownedScores; // 현재 보유한 악보 리스트
    private List<GameObject> spawnedNotes = new List<GameObject>(); // 생성된 음표 저장 리스트

    // 플레이어 능력치 변수
    public float playerDamage; // 플레이어의 공격력
    private float defaultSpeed = 5f; // 소녀 기본 이동 속도
    private float playingSpeed = 2.5f; // 소녀 연주시 이동 속도

    // 피리 시스템 변수
    private float piriStartTime; // 피리연주 시작 시간
    private bool isPlaying = false; // 연주 시작했는지 여부
    private bool isAngerMelody; // 현재 분노의 악장 연주중인지(false는 평화의 악장)
    private bool hasPlayedThisBeat = false; // 현재 박자에 연주가 완료되었는지 여부
    private bool isFinalMelody = false; // 마무리 연주 중인지 여부

    private int totalBeats = 5; // 전체 박자 개수
    public int currentBeat = 0; // 현재 박자 상태
    private float beatInterval; // 현재 박자 사이 간격
    private float angerBeatInterval = 0.3f; // 분노의 악장 박자 사이 간격
    private float peaceBeatInterval = 0.45f; // 평화의 악장 박자 사이 간격
    private float halfRange = 3f; // 콤보 박스 이동 거리(-3 ~ 3)
    public int maxPlayCnt = 4; // 한 사이클당 연주가능 횟수
    private int playCnt = 4;

    public bool[] inputNotes; // 입력된 연주 상태 여부 배열

    // * 늑대 관련 변수

    [SerializeField] private SpriteRenderer wolfEyes; // 늑대 눈 스프라이트
    private SpriteRenderer wolfSpriteRenderer; // 늑대 스프라이트 반전용
    public Animator wolfEyesAnim; // 늑대 눈 애니메이터
    public GameObject guardImg; // 늑대 가드 이미지
    public Coroutine hideCoroutine; // 늑대 Hide 코루틴

    private bool wolfMoveReady = true; // 늑대 이동 가능 여부
    private bool wolfAttackReady = true; // 늑대 공격 준비 여부  
    private bool wolfIsDamaged = false; // 늑대 부상 상태 확인

    private float wolfFadeinTime = 1f; // 좌클릭시 늑대가 나타나는 시간
    private float wolfFadeoutTime = 0.3f; // 좌클릭시 늑대가 사라지는 시간
    private float wolfPolution = 1f; // 늑대 오염도 계수
    private float wolfAttackCoolTime = 5f; // 늑대 공격 쿨타임

    // * 이벤트 함수

    // 소녀 관련 이벤트
    public event Action<float> RequestSetMoveSpeed; // playerCtrl의 moveSpeed 변수 변경 이벤트
    public event Action<bool> RequestPressingPiriState; // playerCtrl의 isPressingPiri 변경이벤트 
    public event Action<bool> RequestPeaceMelodyActived; // playerCtrl의 isPeaceMelody 변경이벤트 
    public event Action<bool> RequestReadyPiri; // playerCtrl의 isReadyPiri 변경이벤트 

    // 늑대 관련 이벤트
    public event Action<string> RequestWolfAnimTrigger; // 늑대의 애니메이션 Trigger 변경 이벤트
    public event Action<WolfState> RequestWolfState; // 늑대 상태 변경 이벤트
    public event Action<float> RequestWolfStartAttack; // 늑대 공격 알림 이벤트

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        wolfSpriteRenderer = playerCtrl.wolf.GetComponent<SpriteRenderer>();
        wolfEyesAnim = wolfEyes.GetComponent<Animator>();

        inputNotes = new bool[totalBeats];
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RequestCurrentStage += OnUpdateStageData;
    }

    private void OnDisable()      // 또는 OnDestroy
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RequestCurrentStage -= OnUpdateStageData;
    }

    // * 소녀 기능 구현 

    public void StartPiri() // 피리 시작 함수 (W / On)
    {
        piriStartTime = Time.time;
        RequestSetMoveSpeed(playingSpeed);
    }

    public void ReleasePiri() // 피리 종료 함수 (W / Off)
    {
        if (hasPlayedThisBeat || playCnt <= 0) return;

        float duration = Time.time - piriStartTime; // 연주버튼 누른 시간


        // 연주 시작시, 악장 모드 구분
        if (!isPlaying)
        {
            if (duration < 0.3f) // 분노의 악장 모드
            {
                isAngerMelody = true;
            }
            else // 평화의 악장 모드
            {
                isAngerMelody = false;
            }

            beatInterval = isAngerMelody ? angerBeatInterval : peaceBeatInterval; // 해당 모드 박자 간격으로 변경
            StartComboAt(Time.time); // 콤보 효과 시작
        }

        // 입력 내용 적용
        hasPlayedThisBeat = true;
        inputNotes[currentBeat] = true;

        // 모드에 따른 연주 실행
        if (isAngerMelody)
            StartCoroutine(PlayAngerNote());
        else
            StartCoroutine(PlayPeaceNote());

    }

    private void StartComboAt(float startTime) // 콤보 시작 함수
    {
        currentBeat = 0;
        isPlaying = true;
        hasPlayedThisBeat = false;

        StartCoroutine(BeatRoutine(startTime));
        StartCoroutine(BeatBoxRoutine(startTime));
    }

    private IEnumerator BeatRoutine(float startTime) // 공격시 박자 계산 코루틴
    {
        // 다음 박자 예정 시각
        hasPlayedThisBeat = false;
        float nextBeatAt = startTime + beatInterval;

        while (isPlaying)
        {
            // 다음 박자까지 대기(프레임마다 검사)
            while (Time.time < nextBeatAt)
                yield return null;

            currentBeat++;

            if (currentBeat >= totalBeats) // 0~5 완료
            {
                StopBeatRoutine();
                yield break;
            }

            // 다음 박자 시간 갱신
            hasPlayedThisBeat = false;
            nextBeatAt += beatInterval;
        }
    }

    private IEnumerator BeatBoxRoutine(float startTime) // 콤보박스 표시 코루틴
    {
        // 콤보 박스 등장
        beatBox.GetComponent<SpriteRenderer>().DOFade(1f, 0.3f);

        // 시작 위치 보정
        float duration = beatInterval * totalBeats;  // 총 5박 → 분노(0.3초 간격): 1.5초 / 평화(0.7초 간격): 3.5초
        float endAt = startTime + duration;
        beatBox.localPosition = new Vector3(-halfRange, 2.5f, 0f);

        // 콤보 박스 이동
        while (Time.time < endAt)
        {
            float t = Mathf.InverseLerp(startTime, endAt, Time.time);
            float x = Mathf.Lerp(-halfRange, halfRange, t);
            beatBox.localPosition = new Vector3(x, 2.5f, 0f);
            yield return null;
        }

        // 마지막 위치 보정
        beatBox.localPosition = new Vector3(halfRange, 2.5f, 0f);
    }

    private void StopBeatRoutine() // 공격 콤보 종료 함수
    {
        if (!isPlaying) return; // 콤보 진행중일 때만 확인

        Debug.LogWarning("콤보 종료!");

        currentBeat = 0;
        playCnt = maxPlayCnt;
        isPlaying = false;
        hasPlayedThisBeat = false;

        StopBeatBoxRoutine(); // 콤보 박스 종료
        ResetInputNote(); // 입력값 초기화

        if (!isFinalMelody) // 마무리 연주중이 아닐 경우, 피리연주 종료
        {
            RequestPressingPiriState(false);
            RequestSetMoveSpeed(defaultSpeed);
        }
    }

    private void StopBeatBoxRoutine() // 콤보 박스 정리 함수
    {
        // 콤보 상자 비활성화
        if (beatBox != null)
        {
            StartCoroutine(beatBox.GetComponent<SpriteRenderer>().FadeTo(0f, 0.5f));
        }

        // 생성된 음표 일괄 삭제 및 리스트 정리
        foreach (var note in spawnedNotes)
        {
            if (note != null)
            {
                NoteCtrl noteCtrl = note.GetComponent<NoteCtrl>();
                if (noteCtrl != null)
                {
                    StartCoroutine(FadeAndDestroy(noteCtrl));
                }
                else
                {
                    Destroy(note);
                }
            }
        }
        spawnedNotes.Clear();
    }

    private IEnumerator FadeAndDestroy(NoteCtrl noteCtrl) // Fade out 효과 후, Destroy 실행 코루틴
    {
        yield return StartCoroutine(noteCtrl.FadeCoroutine(0f, 0.2f));
        Destroy(noteCtrl.gameObject);
    }

    private void ResetInputNote() // 입력 노트 정리 함수
    {
        System.Array.Clear(inputNotes, 0, inputNotes.Length); // 전부 false로 초기화
    }

    private IEnumerator PlayAngerNote() // 분노의 음표 연주 함수
    {
        Debug.Log("[분노의 음표 연주]");

        // 분노의 음표 소리 및 이펙트 표시
        PlayPiriSound("Anger");
        AngerAttackEffect.transform.localScale = Vector3.one * 1 / playCnt * 15f;
        AngerAttackEffect.SetActive(true);
        playCnt--;

        // 분노의 음표 입력 표시
        GameObject note = Instantiate(angerNote, beatBox.position, Quaternion.identity);
        note.transform.SetParent(transform, true);
        spawnedNotes.Add(note);

        // 마지막 콤보시 입력과 악보 비교
        CheckMelodyScore();
        if (playCnt <= 0)
        {
            StopBeatBoxRoutine();
        }
        yield return new WaitForSeconds(0.2f);

        AngerAttackEffect.SetActive(false); // 이펙트 Off
    }

    private IEnumerator PlayPeaceNote() // 평화의 음표 연주 함수
    {
        Debug.Log("평화의 음표를 연주 중입니다.");

        // 평화의 음표 소리 및 이펙트 표시
        PlayPiriSound("Peace");
        PeaceAttackArea.transform.localScale = Vector3.one * 1 / playCnt * 15f;
        PeaceAttackArea.SetActive(true);
        playCnt--;

        // 평화의 음표 입력 표시
        GameObject note = Instantiate(peaceNote, beatBox.position, Quaternion.identity);
        note.transform.SetParent(transform, true);
        spawnedNotes.Add(note);

        // 마지막 콤보시 입력과 악보 비교
        CheckMelodyScore();
        if (playCnt <= 0)
        {
            StopBeatBoxRoutine();
        }
        yield return new WaitForSeconds(0.2f);

        PeaceAttackArea.SetActive(false); // 이펙트 Off
    }

    private IEnumerator TempFailMelody(float delay) // 피리 사용 봉인 코루틴 (연주 실패 / 적 봉인 공격)
    {
        Debug.Log("[연주 실패] : 피리사용 봉인");

        // 피리 사용 봉인 및 소리 재생
        RequestReadyPiri?.Invoke(false);
        PlayPiriSound("PeaceFail");
        yield return new WaitForSeconds(0.5f);

        // 콤보 종료 및 피리 상태 해제(대기)
        StopBeatRoutine();
        RequestPressingPiriState(false); // 피리연주 종료
        RequestSetMoveSpeed(defaultSpeed);
        yield return new WaitForSeconds(delay);

        // 피리 봉인 해제
        Debug.Log("피리 봉인 해제");
        RequestReadyPiri?.Invoke(true);
    }

    private void CheckMelodyScore() // 입력과 악보 비교 함수
    {
        // 보유한 악보와 입력 상태 비교
        foreach (var score in ownedScores)
        {
            if (score == null) continue;
            if (score.isAnger != isAngerMelody) continue;
            if (score.contents == null || score.contents.Length != totalBeats) continue;

            // 입력 상태 = 보유한 악보시, 마무리 연주 실행
            if (ScoreContentsEquals(inputNotes, score.contents))
            {
                Debug.LogWarning($"연주 실행: {score.name}");

                isFinalMelody = true; // 마무리 연주 On
                RequestReadyPiri?.Invoke(false);
                StopBeatBoxRoutine();

                switch (score.scoreNum)
                {
                    case 0:
                        StartCoroutine(PlayAngerMelody0()); break;
                    case 1:
                        StartCoroutine(PlayAngerMelody1()); break;
                    case 2:
                        StartCoroutine(PlayAngerMelody2()); break;
                    case 3:
                        StartCoroutine(PlayPeaceMelody0()); break;
                    case 4:
                        StartCoroutine(PlayPeaceMelody1()); break;
                    case 5:
                        StartCoroutine(PlayPeaceMelody2()); break;
                }
            }
        }
    }

    private bool ScoreContentsEquals(bool[] a, bool[] b) // 동일 여부 확인 후 bool값 리턴
    {
        for (int i = 0; i < totalBeats; i++)
            if (b[i] != a[i]) return false;

        return true;
    }


    private IEnumerator PlayAngerMelody0() // 분노의 악장 0 연주 함수
    {
        // 마지막 음표 입력과 간격을 둠
        yield return new WaitForSeconds(0.3f);

        // 공격 활성화 및 소리 재생
        AngerAttackArea.SetActive(true);
        girlAudioSource.clip = angerMelody[0];
        girlAudioSource.Play();
        yield return new WaitForSeconds(0.8f);

        // 연주 종료 및 피리 상태 해제 
        AngerAttackArea.SetActive(false);
        RequestSetMoveSpeed(defaultSpeed);
        isFinalMelody = false; // 마무리 연주 off
        RequestReadyPiri?.Invoke(true);
        RequestPressingPiriState(false);
    }

    private IEnumerator PlayAngerMelody1() // 분노의 악장 1 연주 함수
    {
        // 마지막 음표 입력과 간격을 둠
        yield return new WaitForSeconds(0.3f);

        // 공격 활성화 및 소리 재생
        AngerAttackArea.SetActive(true);
        girlAudioSource.clip = angerMelody[1];
        girlAudioSource.Play();
        yield return new WaitForSeconds(0.8f);

        // 연주 종료 및 피리 상태 해제 
        AngerAttackArea.SetActive(false);
        RequestSetMoveSpeed(defaultSpeed);
        isFinalMelody = false; // 마무리 연주 off
        RequestReadyPiri?.Invoke(true);
        RequestPressingPiriState(false);
    }

    private IEnumerator PlayAngerMelody2() // 분노의 악장 2 연주 함수
    {
        // 마지막 음표 입력과 간격을 둠
        yield return new WaitForSeconds(0.3f);

        // 공격 활성화 및 소리 재생
        AngerAttackArea.SetActive(true);
        girlAudioSource.clip = angerMelody[2];
        girlAudioSource.Play();
        yield return new WaitForSeconds(0.8f);

        // 연주 종료 및 피리 상태 해제 
        AngerAttackArea.SetActive(false);
        RequestSetMoveSpeed(defaultSpeed);
        isFinalMelody = false; // 마무리 연주 off
        RequestReadyPiri?.Invoke(true);
        RequestPressingPiriState(false);
    }

    private IEnumerator PlayPeaceMelody0() // 평화의 악장 0 연주 함수
    {
        // 마지막 음표 입력과 간격을 둠
        yield return new WaitForSeconds(0.3f);

        // 공격 활성화 및 소리 재생
        PeaceAttackArea.SetActive(true);
        girlAudioSource.clip = peaceMelody[0];
        girlAudioSource.Play();
        yield return new WaitForSeconds(2f);

        // 연주 종료 및 피리 상태 해제 
        PeaceAttackArea.SetActive(false);
        RequestSetMoveSpeed(defaultSpeed);
        isFinalMelody = false; // 마무리 연주 off
        RequestReadyPiri?.Invoke(true);
        RequestPressingPiriState(false);
    }
    private IEnumerator PlayPeaceMelody1() // 평화의 악장 1 연주 함수
    {
        // 마지막 음표 입력과 간격을 둠
        yield return new WaitForSeconds(0.3f);

        // 공격 활성화 및 소리 재생
        PeaceAttackArea.SetActive(true);
        girlAudioSource.clip = peaceMelody[1];
        girlAudioSource.Play();
        yield return new WaitForSeconds(2f);

        // 연주 종료 및 피리 상태 해제 
        PeaceAttackArea.SetActive(false);
        RequestSetMoveSpeed(defaultSpeed);
        isFinalMelody = false; // 마무리 연주 off
        RequestReadyPiri?.Invoke(true);
        RequestPressingPiriState(false);
    }
    private IEnumerator PlayPeaceMelody2() // 평화의 악장 2 연주 함수
    {
        // 마지막 음표 입력과 간격을 둠
        yield return new WaitForSeconds(0.3f);

        // 공격 활성화 및 소리 재생
        PeaceAttackArea.SetActive(true);
        girlAudioSource.clip = peaceMelody[2];
        girlAudioSource.Play();
        yield return new WaitForSeconds(2f);

        // 연주 종료 및 피리 상태 해제 
        PeaceAttackArea.SetActive(false);
        RequestSetMoveSpeed(defaultSpeed);
        isFinalMelody = false; // 마무리 연주 off
        RequestReadyPiri?.Invoke(true);
        RequestPressingPiriState(false);
    }

    public void PlaySoftPiriCanceled() // 평화의 악장 취소 함수
    {
        Debug.Log("[평화의 악장] 연주 실패...");
        PeaceWaitingEffect.SetActive(false); // 평화의 악장 준비 이펙트 종료
        girlAudioSource.Stop();
        PlayPiriSound("PeaceFail");
        RequestPeaceMelodyActived?.Invoke(false);
        RequestPressingPiriState(false); // 피리연주 종료
    }

    public void OnUpdateStageData() // 오염도에 따른 값 변경 함수
    {
        PollutionStage stageData = GameManager.Instance.currentStageData;

        // 기존 설정
        wolfPolution = stageData.pollution_Coefficient;
        //AngerAttackArea.transform.localScale = Vector3.one * stageData.anger_range;
        //PeaceAttackArea.transform.localScale = Vector3.one * stageData.peace_range;
        //playerDamage = stageData.anger_damage;

        // 밝기 조정
        float brightness = (255f - wolfPolution * 30f) / 255f;
        brightness = Mathf.Clamp01(brightness);
        wolfSpriteRenderer.color = new Color(brightness, brightness, brightness, wolfSpriteRenderer.color.a);

        // 늑대 관련 설정
        playerCtrl.defaultWolfExitTime = stageData.wolfAppearTime;
        wolfAttackCoolTime = stageData.wolfAttackCoolTime;
    }

    private void PlayPiriSound(string type) // 사운드 변경 및 재생 함수
    {
        if (type == "Anger" && angerNotes != null && angerNotes.Length > 0)
        {
            girlAudioSource.clip = angerNotes[currentBeat];
            girlAudioSource.Play();
        }
        else if (type == "Peace" && peaceNotes != null && peaceNotes.Length > 0)
        {
            girlAudioSource.clip = peaceNotes[currentBeat];
            girlAudioSource.Play();
        }
        else if (type == "PeaceFail" && cancelMelody != null)
        {
            girlAudioSource.clip = cancelMelody;
            girlAudioSource.Play();
        }
    }

    // 늑대 기능 구현 

    private float EaseOutExpo(float t)
    {
        return t == 0 ? 0 : Mathf.Pow(2, 15 * (t - 1));
    }

    public IEnumerator WolfAppear(bool isExist) // 늑대 등장 구현
    {
        if (!wolfMoveReady) yield break; // 이동 쿨타임시 조작 불가능(너무 빈번한 이동 방지)

        wolfMoveReady = false;
        StartCoroutine(WolfMoveCool()); // 이동 쿨타임 시작
        wolfEyes.enabled = false; // 플레이어 위 늑대 눈 숨김

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //마우스 위치 받음
        Vector2 isStartRight; // 늑대 첫등장시 더할 단위 벡터

        float timer = 0f;

        if (mousePosition.x > playerCtrl.transform.position.x)
        {
            wolfSpriteRenderer.flipX = false;
            isStartRight = Vector2.left * 4;
        }
        else
        {
            wolfSpriteRenderer.flipX = true;
            isStartRight = Vector2.right * 4;
        }

        if (isExist) // 늑대 이미 등장시
        {
            // 늑대 기존 위치에서 사라지는 연출
            Debug.Log("늑대가 사라집니다!");
            RequestWolfAnimTrigger?.Invoke("Hide");
            wolfAppearArea.SetActive(false); // 늑대 등장 효과 종료

            while (timer < wolfFadeoutTime)
            {
                timer += Time.deltaTime;
                float newAlpha = Mathf.Lerp(1f, 0f, timer / wolfFadeoutTime); // 선형적으로 fade out 변화
                wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, newAlpha);
                yield return null;
            }
            wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, 0f);
            timer = 0f; // 타이머 초기화
        }

        // 새로운 위치로 나타나는 연출
        Debug.Log("늑대가 나타납니다!");
        RequestWolfState(WolfState.Idle); // 늑대 Idle 상태로 변경
        playerCtrl.wolf.transform.position = mousePosition + isStartRight; // 늑대 출발점
        RequestWolfAnimTrigger?.Invoke("Move");
        while (timer < wolfFadeinTime)
        {
            timer += Time.deltaTime;
            float newAlpha = EaseOutExpo(timer); // 비선형적으로 뒤로 갈수록 빠르게 fade in 
            wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, newAlpha);
            playerCtrl.wolf.transform.position
            = Vector2.Lerp(playerCtrl.wolf.transform.position, mousePosition, EaseOutExpo(timer));
            yield return null;

            if (wolfIsDamaged)
            {
                Debug.Log("등장 중 늑대가 소녀 곁으로 이동합니다!");
                playerCtrl.wolf.transform.position = playerCtrl.transform.position; // 소녀 위치로 이동
                yield break; // 늑대 부상시 함수 실행 중지
            }
        }
        wolfAppearArea.SetActive(true); // 늑대 등장 효과 시작 (적을 둔화시킴)
        wolfSpriteRenderer.color = new Color(wolfSpriteRenderer.color.r, wolfSpriteRenderer.color.g, wolfSpriteRenderer.color.b, 1f);
        playerCtrl.wolf.transform.position = mousePosition;
    }
    public IEnumerator WolfAttack() // 늑대 공격 구현
    {
        if (wolfAttackReady)
        {
            wolfAttackReady = false;
            StartCoroutine(WolfAttackCool()); // 쿨타임 코루틴 실행

            if (wolfAttackAudio != null)
            {
                wolfAudioSource.clip = wolfAttackAudio;
                wolfAudioSource.Play();
            }

            RequestWolfAnimTrigger?.Invoke("Attack");
            yield return new WaitForSeconds(0.4f);

            wolfAppearArea.SetActive(false); // 늑대 상시 이펙트 중지
            wolfAttackArea.SetActive(true);
            yield return new WaitForSeconds(0.4f); // 공격 모션 대기

            wolfAttackArea.SetActive(false);
            wolfAppearArea.SetActive(true); // 늑대 상시 이펙트 실행
            RequestWolfState(WolfState.Idle);
        }
    }
    public IEnumerator WolfHide(bool isGuarded) // 늑대 Hide 구현, 매개변수는 늑대 가드를 통해 호출된 Hide인지 구별
    {
        RequestWolfState(WolfState.Hide);
        wolfAppearArea.SetActive(false); // 늑대 등장 효과 종료

        if (isGuarded)
        {
            RequestWolfAnimTrigger?.Invoke("Damaged");
            wolfSpriteRenderer.flipX = playerCtrl.spriteRenderer.flipX; // 소녀와 같은 방향을 바라봄
            playerCtrl.wolf.transform.position = playerCtrl.transform.position + Vector3.up * 0.5f; // 소녀 위치로 이동
            RequestWolfState(WolfState.Damaged); // 늑대 상태 변화(Damaged)
            yield return StartCoroutine(FadeCoroutine(1.0f, 0.4f)); // FadeIn
        }
        else
        {
            RequestWolfAnimTrigger?.Invoke("Hide");
        }
        wolfEyesAnim.SetBool("wolfDamaged", wolfIsDamaged);
        wolfEyesAnim.Play("open", -1, 0f); // 애니메이션 처음부터 실행
        wolfEyes.enabled = true; // 늑대 눈 나타내기기

        yield return StartCoroutine(FadeCoroutine(0.0f, 0.3f)); // FadeOut
    }

    public void WolfGuard() // 늑대 가드 구현
    {
        if (!wolfIsDamaged)
        {
            wolfIsDamaged = true; // 늑대부상 변수 (등장중일 경우, 코루틴 탈출)

            if (wolfAttackAudio != null)
            {
                wolfAudioSource.Stop();
            }

            hideCoroutine = StartCoroutine(WolfHide(true));
            StartCoroutine(WolfGuardEffect()); // 가드 이펙트 코루틴 실행 
            StartCoroutine(WolfGuardCool()); // 가드 쿨타임 코루틴 실행

        }
    }

    private IEnumerator WolfGuardEffect() // 늑대 가드 이펙트트 코루틴
    {
        guardImg.transform.position = this.transform.position;
        guardImg.SetActive(true);
        wolfPushArea.SetActive(true);
        yield return new WaitForSeconds(0.2f); // 0.2초 후 사라짐
        guardImg.SetActive(false);
        wolfPushArea.SetActive(false);
    }

    private IEnumerator WolfMoveCool() // 늑대 이동 쿨타임 코루틴
    {
        yield return new WaitForSeconds(1f);
        wolfMoveReady = true;
    }

    private IEnumerator WolfAttackCool() // 늑대 공격 쿨타임 코루틴
    {
        RequestWolfStartAttack(2.5f); // PlayerCtrl에게 늑대 공격했음을 알림 (UI 동기화)
        yield return new WaitForSeconds(wolfAttackCoolTime);
        wolfAttackReady = true;
    }
    private IEnumerator WolfGuardCool() // 늑대 가드 쿨타임 코루틴, 성공 후 쿨타임동안 늑대 제어 불가
    {
        Debug.Log("늑대 부상! 회복중");
        wolfEyesAnim.SetBool("wolfDamaged", wolfIsDamaged);

        yield return new WaitForSeconds(5.0f);

        Debug.Log("늑대 회복!");
        RequestWolfState(WolfState.Hide);
        wolfIsDamaged = false; // 늑대 부상 회복
        wolfEyesAnim.SetBool("wolfDamaged", wolfIsDamaged);
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

    public void SetWolfEyesVisible(bool visible) // 늑대 눈 UI 표시 변경 함수
    {
        if (wolfEyes != null)
        {
            wolfEyes.enabled = visible;
        }
    }
}
