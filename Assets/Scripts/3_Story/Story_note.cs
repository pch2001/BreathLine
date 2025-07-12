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
    private int playcount = 0; // ���� ��� Ƚ�� ī��Ʈ��
    bool isPlaying = false;

    public GameObject HPUI;
    public GameObject SkillUI;
    public GameObject SkillUI2;
    public bool isReturn = false;
    public bool boss4Stage = false; // ȸ�� �� ���� ���� ���� ��� �� ��ũ��Ʈ �����(true)
    public bool bossLastStage = false; // ȸ�� �� ������ ���� ���� ���� ��� �� ���� ȭ������ ���ư�.

    public float floatAmplitude = 0.5f; // ���Ʒ��� �����̴� �� (�󸶳� ���� �����̴���)
    public float floatSpeed = 2f;       // �����̴� �ӵ�
    private Vector3 startPos;           // ������Ʈ�� �ʱ� ��ġ

    void Start()
    {
        startPos = transform.position; // ���� �� ������Ʈ�� ���� ��ġ�� ����
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
        Debug.Log("�������");

        if (collision.CompareTag("Player"))
        {
            if(isReturn)
                Invoke(nameof(nextScene), 0.5f);
            else
            {
                Debug.Log("�������");

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
        Debug.Log("�������");
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

        if (SceneManager.GetActiveScene().buildIndex == 8)
        {
            SkillUI2.SetActive(false); // ��ȭ�� ���� UI���� ��Ȱ��ȭ
        }

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
        //SceneManager.LoadScene();
        
        LoadingScene.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private void Update()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // �ʱ� Y ��ġ�� ���� newY ���� ���Ͽ� �պ� ��� ����ϴ�.
        transform.position = new Vector3(startPos.x, startPos.y + newY, startPos.z);
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameManager.Instance.Pollution = 90f; // ������ 90���� ����
            Debug.Log("������ 90���� ������");
        }

        if (Input.GetKeyDown(KeyCode.Space) && isPlaying)
        {
            isPlaying = false;   
            videoPlayer.Stop(); 
            videoPlayer.prepareCompleted -= OnVideoPrepared; // �̺�Ʈ �����ؾ� �ٽ� ���� ���� ����
            
            videoAudioSource.Stop(); // ���� ����� ����
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // ����� ����

            rawImage.enabled = false;
            GameObject playerCode = GameObject.FindWithTag("Player"); // Player �±� �ʿ�!
            
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
            HPUI.SetActive(true); // HP UI Ȱ��ȭ
            SkillUI.SetActive(true); // ��ų UI Ȱ��ȭ
        }
    }

    void SetIsPlayingTrue()
    {
        isPlaying = true;
        skip.SetActive(true);
    }
    void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.enabled = true; // ���� ���
        vp.Play(); 
    }

}

