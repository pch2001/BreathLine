using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class mainMP4 : MonoBehaviour
{
    public RawImage rawImage;             // ������ ��µ� UI RawImage
    public VideoPlayer videoPlayer;       // VideoPlayer ������Ʈ
    public string videoFileName = "main.mp4"; // StreamingAssets ��ο� ��ġ�� ���� ���ϸ�

    void Start()
    {
        // ���� ��� ����
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);

        // VideoPlayer ����
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.isLooping = true; // ���� �ݺ�
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // �Ҹ� ���� (�ʿ� �� ����)

        // ���� �� ȭ�� �ʱ�ȭ
        rawImage.enabled = false;
        if (videoPlayer.targetTexture != null)
        {
            RenderTexture rt = videoPlayer.targetTexture;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }

        // ��� �غ�
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.enabled = true; // UI ��� Ȱ��ȭ
        vp.Play();
    }
}
