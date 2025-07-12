using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Story_note : MonoBehaviour
{

    public RawImage rawImage; // UI RawImage
    public VideoPlayer videoPlayer;
    public string videoFileName = "test.mp4";
    public GameObject skip;
    private PlayerCtrl playerCtrl;
    private PlayerCtrl_R playerCtrl_R;
    public AudioSource videoAudioSource;
    private int playcount = 0; // 영상 재생 횟수 카운트용
    bool isPlaying = false;

    public GameObject HPUI;
    public GameObject SkillUI;
    public GameObject SkillUI2;
    public bool isReturn = false;
    public bool boss4Stage = false; // 회귀 전 보스 때는 영상 재생 후 스크립트 연계됨(true)
    public bool bossLastStage = false; // 회귀 후 마지막 보스 때는 영상 재생 후 시작 화면으로 돌아감.

    public float floatAmplitude = 0.5f; // 위아래로 움직이는 폭 (얼마나 높이 움직이는지)
    public float floatSpeed = 2f;       // 움직이는 속도
    private Vector3 startPos;           // 오브젝트의 초기 위치

    void Start()
    {
        startPos = transform.position; // 시작 시 오브젝트의 현재 위치를 저장
        playcount = 0;
        skip.SetActive(false);
        isPlaying = false;
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        videoPlayer.Stop();
        rawImage.enabled = false;

        if (videoPlayer.targetTexture != null)
        {
            RenderTexture rt = videoPlayer.targetTexture;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("영상재생");

        if (collision.CompareTag("Player"))
        {
            if(isReturn)
                Invoke(nameof(nextScene), 0.5f);
            else
            {
                Debug.Log("영상재생");

                PlayVideo();
            }
        }
    }

    //public void StartPlayingVideo()
    //{
    //    videoPlayer.Prepare(); // 비디오 준비 시작
    //    videoPlayer.prepareCompleted += OnVideoPrepared;
    //    Invoke(nameof(SetIsPlayingTrue), 5f);
    //}

    public void PlayVideo() // 영상 실행 함수
    {
        Debug.Log("영상재생");
        if (playcount >= 1)
            return;

        GameObject playerCode = GameObject.FindWithTag("Player"); // Player 태그 필요!
        if (GameManager.Instance.isReturned)
        {
            playerCtrl_R = playerCode.GetComponent<PlayerCtrl_R>();
            playerCtrl_R.OnDisable();
        }
        else
        {
            playerCtrl = playerCode.GetComponent<PlayerCtrl>();
            playerCtrl.OnDisable();
        }

        HPUI.SetActive(false); // HP UI 비활성화
        SkillUI.SetActive(false); // 스킬 UI 비활성화

        if (SceneManager.GetActiveScene().buildIndex == 8)
        {
            SkillUI2.SetActive(false); // 정화의 걸음 UI까지 비활성화
        }

        // 오디오 자동 재생을 방지
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudioSource);

        playcount++; // 영상 재생 횟수 증가
        videoPlayer.Prepare(); // 비디오 준비 시작
        videoPlayer.prepareCompleted += OnVideoPrepared;
        Invoke(nameof(SetIsPlayingTrue), 5f);
    }

    public void nextScene()
    {
        //SceneManager.LoadScene();
        
        LoadingScene.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private void Update()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // 초기 Y 위치에 계산된 newY 값을 더하여 왕복 운동을 만듭니다.
        transform.position = new Vector3(startPos.x, startPos.y + newY, startPos.z);
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameManager.Instance.Pollution = 90f; // 오염도 90으로 설정
            Debug.Log("오염도 90으로 설정됨");
        }

        if (Input.GetKeyDown(KeyCode.Space) && isPlaying)
        {
            isPlaying = false;   
            videoPlayer.Stop(); 
            videoPlayer.prepareCompleted -= OnVideoPrepared; // 이벤트 제거해야 다시 수행 방지 가능
            
            videoAudioSource.Stop(); // 영상 오디오 정지
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // 오디오 차단

            rawImage.enabled = false;
            GameObject playerCode = GameObject.FindWithTag("Player"); // Player 태그 필요!
            
            if (videoPlayer.targetTexture != null)
            {
                RenderTexture rt = videoPlayer.targetTexture;
                videoPlayer.targetTexture = null; // 연결 먼저 끊기
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

                RenderTexture.active = rt;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
                rt.Release(); // 렌더 텍스처 해제

                if (boss4Stage) // 회귀 전 4스테이지 보스 장면의 경우, 스토리 이후 스크립트 전개 시작
                {
                    playerCtrl = playerCode.GetComponent<PlayerCtrl>();
                    StartCoroutine(playerCtrl.PlayWolfDie()); // 연출 및 스크립트 시작
                    Debug.Log("보스 4 스테이지 연출 시작");
                }
                else if (SceneManager.GetActiveScene().buildIndex == 8)
                {
                    SceneManager.LoadScene(0);
                }
                else
                {
                    Invoke(nameof(nextScene), 0.5f);
                }
            }
            Time.timeScale = 1f;
            skip.SetActive(false);
            HPUI.SetActive(true); // HP UI 활성화
            SkillUI.SetActive(true); // 스킬 UI 활성화
        }
    }

    void SetIsPlayingTrue()
    {
        isPlaying = true;
        skip.SetActive(true);
    }
    void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.enabled = true; // 영상 출력
        vp.Play(); 
    }

}

