using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Story_note : MonoBehaviour
{

    public RawImage rawImage; // UI RawImage
    public VideoPlayer videoPlayer;
    public string videoFileName = "test.mp4";
    public Text skip;
    private PlayerCtrl playerCtrl;

    bool isPlaying = false;
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
        // videoPlayer.Prepare();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameObject playerCode = GameObject.FindWithTag("Player"); // Player 태그 필요!
            playerCtrl = playerCode.GetComponent<PlayerCtrl>();
            playerCtrl.OnDisable();

            videoPlayer.Prepare(); // 비디오 준비 시작
            videoPlayer.prepareCompleted += OnVideoPrepared;
            isPlaying = true;
            Invoke(nameof(SetIsPlayingTrue), 8f);
        }



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
        if (Input.GetKeyDown(KeyCode.Space) && !isPlaying)
        {
            GameObject playerCode = GameObject.FindWithTag("Player"); // Player 태그 필요!
            playerCtrl = playerCode.GetComponent<PlayerCtrl>();
            playerCtrl.OnEnable();
            videoPlayer.Stop();
            rawImage.enabled = false;
            skip.text = "";
            if (videoPlayer.targetTexture != null)
            {
                RenderTexture rt = videoPlayer.targetTexture;
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
            }
        }
    }
    void SetIsPlayingTrue()
    {
        isPlaying = false;
        skip.text = "스페이스바를 누르면 영상이 종료됩니다.";

    }
    void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.enabled = true; // 영상 출력
        vp.Play();

    }
}
