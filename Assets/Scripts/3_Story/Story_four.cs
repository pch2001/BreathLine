using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Story_four : MonoBehaviour
{
    private PlayerCtrl playerCtrl;

    //대사 출력
    public GameObject girlImage;
    public GameObject wolfImage;
    public GameObject sideImage;
    public GameObject textbehind;
    public Text printText1;
    public Text printText2;
    public Text printText3;

    public GameObject Boss4;
    public GameObject BossHp;
    public Story_note story4; 

    List<List<string>> dialoguescript;//대사 스크립트 저장소

    private bool isSkipping = false;
    private bool isTyping = false;
    public GameObject spawnThunder;

    public AudioSource bgm;
    public AudioClip audioClip; // 보스전 음악

    public GameObject telepoint;
    public GameObject player;

    public GameObject UI1;
    public GameObject UI2;
    void Start()
    {
        printText1.text = "";
        printText2.text = "";
        printText3.text = "";
        dialoguescript = new List<List<string>>
        {
        new List<string>
        {
            "n:(번쩍이는 붉은 빛이 주변을 가른다.)",
            "g:또 떨어졌어… 도대체 이건 뭐죠?",
            "w:그건 반복된 기억의 흔적이야.\n아직 끝나지 않은 상처들이 주변에서 터져나오는 거지.",
            "n:(주변의 적들이 전보다 더 날카로운 기세로 다가온다.)",
            "g:이 적들… 한 번 공격하고 끝이 아니에요.\n계속 다른 방식으로 덤벼요.",
            "w:같은 상처도, 반복되면 모습을 바꾸거든.",
            "w:어떤 건 말로,\n어떤 건 침묵으로,\n아니면 지금과 같은 형태로 나타나기도 해.",
            "n:(소녀가 무의식적으로 피리를 움켜쥔다.)",
            "g:그냥 저도 다 공격하고 싶어요.\n이렇게 계속 하다간, 아무것도 남지 않을 것 같아서…",
            "n:(늑대가 옆에서 조용히 숨을 고른다.\n이전보다 움직임이 느려져 있다.)",
            "g:…늑대, 괜찮아요?",
            "w:조금… 이런 공간은 나에게도 버겁긴 해.\n그래도 괜찮아. 아직은 견딜 수 있어.",
            "w:네가 여기까지 왔다는 것만으로도, 나는 충분해.",
            "w:지금 너는 반복의 한가운데에 있어.\n익숙한 아픔이 다가올 때마다, 이게 끝이라고 느낄 수 있어.",
            "w:하지만 네가 선택할 수 있어.\n반복 속에 머무를지, 그 밖으로 나아갈지.",
            "g:…"

        },new List<string>
        {
            "n:(폐허 끝자락, 부서진 바닥 틈새 아래로 어둠이 펼쳐져 있다.)",
            "g:여긴… 아래로 계속 이어지고 있어요.",
            "n:(희미한 숨결, 깊이를 가늠할 수 없는 침묵. 공기조차 무겁다.)",
            "w:그곳은 네 기억의 가장 밑바닥이야.",
            "w:지금까지 눌러온 감정들이, 그 아래에서 널 기다리고 있지.",
            "g:…마주하고 싶지 않았던 것들인가 봐요.",
            "w:하지만 결국, 꺼내야만 해.\n상처는 감춰진다고 사라지는 게 아니니까.",
            "n:(소녀는 조심스레 피리를 쥔다. 손끝이 조금 떨리고 있다.)",
            "g:이번에는… 주변에 상처주는 방식으로 피리를 불고 싶지 않아요.\n그런데 그게 더 쉬워 보일 때가 있어요.",
            "w:상처를 상처로 덮는 건,\n잠깐은 쉬울지 몰라도… 결국 너를 갉아먹게 될 거야.",
            "w:넌 다른 방법을 알고 있잖아.",
            "n:(늑대는 조용히 다가와 옆에 선다. 그 눈빛은 여전히 따뜻하다.)",
            "g:…늑대, 많이 힘들죠?",
            "w:이제 너는 정말 중요한 갈림길 앞에 있어.",
            "w:어둠이 깊어질수록, 네 빛은 더 선명해질 테니까.",
            "n:(소녀는 고개를 끄덕이며, 지하로 내려가는 발걸음을 내딛는다.)"
        },

        new List<string>
        {
            "n:(소녀의 몸이 붉은 기운에 휩싸여 어딘가로 떨어진다.)",
            "n:(착지한 곳은 축축하고 깊은 지하의 공간. 눈앞에 거대한 그림자가 모습을 드러낸다.)",
            "g:…여긴… 어디죠…?",
            "n:(앞에 있는 거대한 실루엣이 조금씩 움직인다. 수많은 다리, 떨리는 실, 붉게 빛나는 눈.)",
            "g:……당신은……",
            "n:(공기조차 무거워진다. 말하지 않아도 느껴지는 압박감. 익숙하지 않은데, 이상하리만치 낯설지도 않다.)",
            "g:(처음 보는 존재인데…… 왜 이렇게 가슴이 죄여 오는 거죠?)",
            "g:(이건…… 두려움인가요, 아니면……)",
            "n:(거미가 천천히 고개를 숙인다. 그 시선이, 마치 거울을 들이대듯 소녀를 꿰뚫는다.)",
            "w:……뒤로 물러나. 지금 너는, 이 존재를 감당하기엔 너무 가까이 있어.",
            "g:하지만…… 이건……",
            "n:(소녀는 피리를 꾹 쥐고, 어딘가 모르게 무너져 내리는 감정을 느낀다.)"
        },
        new List<string>
        {
            "g:늑대…?! 안 돼, 왜…!",
            "n:(거미의 붉은 기운이 늑대의 몸을 타고 퍼진다.\n늑대의 모습이 점점 희미해진다.)",

            "g:왜… 왜 나 대신…!\n저는 그저, 제 감정에만 휘둘렸는데…!",

            "w:너를 지키는 건,\n처음부터 내가 선택한 일이었어.",
            "w:너에겐 아직 너무 무거운 것 같아서,\n내가 조금 감당하고 있었지.",
            "n:(소녀의 손끝에 번지는 미약한 빛…\n그러나 붉은 기운은 이제 그녀의 몸으로 향하고 있다.)",
            "n:(소녀의 오염도가 체력으로 바뀝니다.)",

            "g:…그럼, 나 때문에 너는 점점 사라지고 있었던 거야…?",

            "w:그건 네 잘못이 아니야.\n네가 무너지지 않도록, 내가 곁에 있어주고 싶었을 뿐이야.",
            "w:그리고 지금, 네가 무너지기 직전이라면…\n내가 마지막까지 널 지켜주고 싶어.",

            "n:(늑대는 한 번 더 숨을 들이쉰다.\n그 눈동자엔 후회도, 원망도 없다.)",

            "w:이 반복의 끝이 네가 멈추는 곳이 아니라,\n다시 시작되는 곳이길 바랄게.",
            "w:지금 너라면… 분노가 아닌, 너만의 방식으로 싸울 수 있을 테니까.",

            "g:…늑대, 부탁해요. 이번엔 꼭, 끝까지 갈게요.",

            "w:좋아… 그 약속, 믿을게.",
            "w:자, 이제 돌아가.\n이번엔… 너의 의지로.",

            "n:(빛이 소녀를 감싸고, 시간의 흐름이 거꾸로 뒤집히기 시작한다.)"
        }
        };

    }
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                isSkipping = true;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))  // 특정 태그로 확인
        {
            sideImage.SetActive(true);
            UI1.SetActive(false);
            UI2.SetActive(false);
            string objectName = gameObject.name;
            if (int.TryParse(objectName, out int index))
            {
                if(index == 2)
                {
                    bgm.clip = audioClip; // 보스 배경음악 재생
                    TeleportPlayer();
                }
                StartCoroutine(TypingText(index));//이것만 실행하면 텍스트 나옴
            }
        }
    }

    public IEnumerator TypingText(int index)
    {
        GameObject playerCode = GameObject.FindWithTag("Player");

        playerCtrl = playerCode.GetComponent<PlayerCtrl>();
        playerCtrl.OnDisable();

        yield return new WaitForSeconds(1f);

        Camera cam = Camera.main;
        float startZoom = cam.orthographicSize;
        float targetZoom = 3f;
        float zoomDuration = 0.5f;
        float elapsed = 0f;

        Transform player = GameObject.FindWithTag("Player").transform;
        Vector3 camStartPos = cam.transform.position;
        Vector3 camTargetPos = new Vector3(player.position.x, player.position.y, cam.transform.position.z);

        //확대
        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, elapsed / zoomDuration);
            cam.transform.position = Vector3.Lerp(camStartPos, camTargetPos, elapsed / zoomDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = targetZoom;
        cam.transform.position = camTargetPos;
        textbehind.SetActive(true);
        //텍스트 출력
        // 대사 출력
        for (int t = 0; t < dialoguescript[index].Count; t++)
        {
            string line = dialoguescript[index][t];
            string[] parts = line.Split(':');

            if (parts.Length < 2) continue;

            string speaker = parts[0].Trim();
            string dialogue = parts[1].Trim();
            printText1.text = "";
            printText2.text = "";
            printText3.text = "";

            // 말풍선 분기
            if (speaker == "g")
            {
                girlImage.SetActive(true);
                wolfImage.SetActive(false);

            }
            else if (speaker == "w")
            {
                girlImage.SetActive(false);
                wolfImage.SetActive(true);
            }
            else if (speaker == "n")
            {
                girlImage.SetActive(false);
                wolfImage.SetActive(false);
            }

            int length = dialogue.GetTypingLength();
            isTyping = true;
            isSkipping = false;

            for (int i = 0; i <= length; i++)
            {
                if (isSkipping)
                {
                    if (speaker == "g")
                        printText1.text = dialogue;
                    else if (speaker == "w")
                        printText2.text = dialogue;
                    else if (speaker == "n")
                        printText3.text = dialogue;
                    break;
                }
                if (speaker == "g")
                    printText1.text = dialogue.Typing(i);
                else if (speaker == "w")
                    printText2.text = dialogue.Typing(i);
                else if (speaker == "n")
                    printText3.text = dialogue.Typing(i);

                yield return new WaitForSeconds(0.03f);
            }
            isTyping = false;
            isSkipping = false;
            while (!Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                yield return null;
            }
        }
        yield return new WaitForSeconds(1f);


        //확대된 카메라 되돌리기 
        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(targetZoom, startZoom, elapsed / zoomDuration);
            cam.transform.position = Vector3.Lerp(camTargetPos, camStartPos, elapsed / zoomDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = startZoom;
        cam.transform.position = camStartPos;

        textbehind.SetActive(false);
        sideImage.SetActive(false);
        girlImage.SetActive(false);
        wolfImage.SetActive(false);
        UI1.SetActive(true);
        UI2.SetActive(true);

        if(index == 1)
        {
            spawnThunder.SetActive(false);
        }

        if (index == 2)
        {
            bgm.clip = audioClip;
            bgm.volume = 0.6f;
            bgm.Play();

            Boss4.GetComponent<EnemyBase>().attackMode = true;
            BossHp.SetActive(true);
        }
        else if(index == 3)
        {
            GameManager.Instance.isReturned = true;
            GameManager.Instance.AddPolution(0f); // 초기화

            story4.nextScene(); // 늑대 희생 스크립트 재생 후, 다음 Scene으로 이동
        }
        yield return new WaitForSeconds(0.1f);

        playerCtrl.OnEnable();
        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }
    void TeleportPlayer()
    {
        if (telepoint != null && player != null)
        {
            player.transform.position = telepoint.transform.position;
        }
    }
}
