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
    public Text skip;
    private PlayerCtrl playerCtrl;
    private PlayerCtrl_R playerCtrl_R;

    bool isPlaying = false;

    public bool isReturn = false;
    public bool boss4Stage = false; // 회귀 전 보스 때는 영상 재생 후 스크립트 연계됨(true)

    void Start()
    {
        skip.text = "";
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
        if (collision.CompareTag("Player"))
        {
            if(isReturn)
                Invoke(nameof(nextScene), 0.5f);
            else
            {
                PlayVideo();

            }
        }
    }

    public void StartPlayingVideo()
    {
        videoPlayer.Prepare(); // 비디오 준비 시작
        isPlaying = true;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        Invoke(nameof(SetIsPlayingTrue), 20f);
    }

    public void PlayVideo() // 영상 실행 함수
    {
        GameObject playerCode = GameObject.FindWithTag("Player"); // Player 태그 필요!
        if(GameManager.Instance.isReturned)
            playerCtrl_R = playerCode.GetComponent<PlayerCtrl_R>();
        else
            playerCtrl = playerCode.GetComponent<PlayerCtrl>();
        
        playerCtrl.OnDisable();
        videoPlayer.Prepare(); // 비디오 준비 시작
        isPlaying = true;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        Invoke(nameof(SetIsPlayingTrue), 20f);
    }

    public void nextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    videoPlayer.Prepare(); // 비디오 준비 시작
        //    videoPlayer.prepareCompleted += OnVideoPrepared;
        //    isPlaying = true;
        //    Invoke(nameof(SetIsPlayingTrue), 8f);

        //}
        if (Input.GetKeyDown(KeyCode.Space) && isPlaying)
        {
            videoPlayer.Stop();
            rawImage.enabled = false;
            skip.text = "";
            if (videoPlayer.targetTexture != null)
            {
                RenderTexture rt = videoPlayer.targetTexture;
                videoPlayer.targetTexture = null; // 연결 먼저 끊기
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
                rt.Release(); // 렌더 텍스처 해제

                if (boss4Stage) // 회귀 전 4스테이지 보스 장면의 경우, 스토리 이후 스크립트 전개 시작
                {
                    GameObject playerCode = GameObject.FindWithTag("Player"); // Player 태그 필요!
                    playerCtrl = playerCode.GetComponent<PlayerCtrl>();
                    StartCoroutine(playerCtrl.PlayWolfDie()); // 연출 및 스크립트 시작
                }
                else
                {
                    Invoke(nameof(nextScene), 0.5f);
                }
            }
            Time.timeScale = 1f;
            
        }
    }

    void SetIsPlayingTrue()
    {
        isPlaying = true;
        skip.text = "스페이스바를 누르면 영상이 종료됩니다.";
    }
    void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.enabled = true; // 영상 출력
        vp.Play();
    }

}

