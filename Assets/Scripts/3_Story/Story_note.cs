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
    public bool boss4Stage = false; // ȸ�� �� ���� ���� ���� ��� �� ��ũ��Ʈ �����(true)

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
        videoPlayer.Prepare(); // ���� �غ� ����
        isPlaying = true;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        Invoke(nameof(SetIsPlayingTrue), 20f);
    }

    public void PlayVideo() // ���� ���� �Լ�
    {
        GameObject playerCode = GameObject.FindWithTag("Player"); // Player �±� �ʿ�!
        if(GameManager.Instance.isReturned)
            playerCtrl_R = playerCode.GetComponent<PlayerCtrl_R>();
        else
            playerCtrl = playerCode.GetComponent<PlayerCtrl>();
        
        playerCtrl.OnDisable();
        videoPlayer.Prepare(); // ���� �غ� ����
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
        //    videoPlayer.Prepare(); // ���� �غ� ����
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
                videoPlayer.targetTexture = null; // ���� ���� ����
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
                rt.Release(); // ���� �ؽ�ó ����

                if (boss4Stage) // ȸ�� �� 4�������� ���� ����� ���, ���丮 ���� ��ũ��Ʈ ���� ����
                {
                    GameObject playerCode = GameObject.FindWithTag("Player"); // Player �±� �ʿ�!
                    playerCtrl = playerCode.GetComponent<PlayerCtrl>();
                    StartCoroutine(playerCtrl.PlayWolfDie()); // ���� �� ��ũ��Ʈ ����
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
        skip.text = "�����̽��ٸ� ������ ������ ����˴ϴ�.";
    }
    void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.enabled = true; // ���� ���
        vp.Play();
    }

}

