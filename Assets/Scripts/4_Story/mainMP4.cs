using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class mainMP4 : MonoBehaviour
{
    public RawImage rawImage;             // 영상이 출력될 UI RawImage
    public VideoPlayer videoPlayer;       // VideoPlayer 컴포넌트
    public string videoFileName = "main.mp4"; // StreamingAssets 경로에 위치한 영상 파일명

    void Start()
    {
        // 영상 경로 설정
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);

        // VideoPlayer 설정
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.isLooping = true; // 무한 반복
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // 소리 제거 (필요 시 해제)

        // 시작 전 화면 초기화
        rawImage.enabled = false;
        if (videoPlayer.targetTexture != null)
        {
            RenderTexture rt = videoPlayer.targetTexture;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }

        // 재생 준비
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.enabled = true; // UI 출력 활성화
        vp.Play();
    }
}
