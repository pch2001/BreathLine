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
    public AudioSource videoAudioSource;
    private int playcount = 0; // ���� ��� Ƚ�� ī��Ʈ��
    bool isPlaying = false;

    public GameObject HPUI;
    public GameObject SkillUI;
    public bool isReturn = false;
    public bool boss4Stage = false; // ȸ�� �� ���� ���� ���� ��� �� ��ũ��Ʈ �����(true)

    void Start()
    {
        playcount = 0;
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

    //public void StartPlayingVideo()
    //{
    //    videoPlayer.Prepare(); // ���� �غ� ����
    //    videoPlayer.prepareCompleted += OnVideoPrepared;
    //    Invoke(nameof(SetIsPlayingTrue), 5f);
    //}

    public void PlayVideo() // ���� ���� �Լ�
    {
        if (playcount >= 1)
            return;

        GameObject playerCode = GameObject.FindWithTag("Player"); // Player �±� �ʿ�!
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
        HPUI.SetActive(false); // HP UI ��Ȱ��ȭ
        SkillUI.SetActive(false); // ��ų UI ��Ȱ��ȭ
        // ����� �ڵ� ����� ����
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudioSource);

        playcount++; // ���� ��� Ƚ�� ����
        videoPlayer.Prepare(); // ���� �غ� ����
        videoPlayer.prepareCompleted += OnVideoPrepared;
        Invoke(nameof(SetIsPlayingTrue), 5f);
    }

    public void nextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.K))
        {
            GameManager.Instance.Pollution = 90f; // ������ 90���� ����
            Debug.Log("������ 100���� ������");
        }

        if (Input.GetKeyDown(KeyCode.Space) && isPlaying)
        {
            videoPlayer.Stop(); 
            videoPlayer.prepareCompleted -= OnVideoPrepared; // �̺�Ʈ �����ؾ� �ٽ� ���� ���� ����
            
            videoAudioSource.Stop(); // ���� ����� ����
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // ����� ����

            rawImage.enabled = false;
            skip.text = "";
            GameObject playerCode = GameObject.FindWithTag("Player"); // Player �±� �ʿ�!
            if (GameManager.Instance.isReturned)
            {
                playerCtrl_R = playerCode.GetComponent<PlayerCtrl_R>();
                playerCtrl_R.OnEnable();
            }
            else
            {
                playerCtrl = playerCode.GetComponent<PlayerCtrl>();
                playerCtrl.OnEnable();

            }
            if (videoPlayer.targetTexture != null)
            {
                RenderTexture rt = videoPlayer.targetTexture;
                videoPlayer.targetTexture = null; // ���� ���� ����
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

                RenderTexture.active = rt;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
                rt.Release(); // ���� �ؽ�ó ����

                if (boss4Stage) // ȸ�� �� 4�������� ���� ����� ���, ���丮 ���� ��ũ��Ʈ ���� ����
                {
                    playerCtrl = playerCode.GetComponent<PlayerCtrl>();
                    StartCoroutine(playerCtrl.PlayWolfDie()); // ���� �� ��ũ��Ʈ ����
                    Debug.Log("���� 4 �������� ���� ����");
                }
                else
                {
                    Invoke(nameof(nextScene), 0.5f);
                }
            }
            Time.timeScale = 1f;
            HPUI.SetActive(true); // HP UI ��Ȱ��ȭ
            SkillUI.SetActive(true); // ��ų UI ��Ȱ��ȭ
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

