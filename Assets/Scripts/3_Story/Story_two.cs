using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Story_two : MonoBehaviour
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


    List<List<string>> dialoguescript;//대사 스크립트 저장소

    private bool isSkipping = false;
    private bool isTyping = false;
    private bool ising = false;

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
                "g:…조금 전, 그건… 제 기억이었겠죠.",
                "n:(소녀는 조용히 숨을 들이쉰다.\n풀벌레 울음소리조차 마음을 어지럽힌다.)",
                "g:그런데 여긴 왜 이렇게 답답하죠…?\n숲인데, 숨이 잘 안 쉬어져요.",
                "w:마음을 흔드는 걸 본 뒤엔,\n세상이 조금 다르게 보이기도 해.",
                "w:이 숲은 고요해 보여도,\n오래된 상처와 폭력을 품고 있거든.",
                "g:폭력… 그런 게 이곳에 있다고요?",
                "w:소리를 지르지 않아도,\n손을 들지 않아도,\n상처는 남을 수 있어.",
                "w:이 숲의 아이들은\n그런 걸 반복하며 살아왔어.",
                "g:그럼… 저 아이들을 멈춰야 하는 거예요?",
                "w:그건 네가 선택할 수 있어.",
                "w:분노의 악장을 연주하면,\n그들이 던진 상처를 그대로 돌려줄 수 있지.",
                "n:(W키를 짧게 눌러 분노의 악장을 연주하면,\n적의 탄환을 반사할 수 있습니다.)",
                "w:하지만 같은 방식으로 상처를 되갚는 건,\n너 스스로를 더 다치게 할지도 몰라.",
                "g:……",
                "w:필요하다면, 나를 불러줘도 돼.",
                "w:내가 곁에 있는 동안,\n아이들은 느려지고 조용해질 거야.",
                "n:(좌클릭 시 늑대가 등장합니다.\n늑대는 주변 적을 둔화시키고, 탄환을 사라지게 하며,\n오염도를 지속적으로 낮춥니다.)",
                "g:늑대는… 아무것도 하지 않아도 괜찮은 거예요?",
                "w:상처를 되돌리는 대신,\n넌 너만의 방식으로… 저 아이들을 대할 수 있을 거야."
            },
            new List<string>
            {
                "g:…그 애가 먼저 공격했어요.",
                "g:그래서… 저도 그냥, 되돌려준 것뿐인데.",
                "n:(소녀는 고개를 숙인 채, 손에 쥔 피리를 꼭 쥐고 있다.)",
                "w:상처를 받은 사람은,\n그 상처를 어떻게 다뤄야 할지 모를 때가 많아.",
                "w:그래서 종종,\n받은 그대로 되돌려버리게 되지.",
                "g:제가 잘못한 건가요…?",
                "w:아니. 너는 지금,\n그저 네가 아팠다는 걸 표현한 거야.",
                "w:다만 기억해 줘.",
                "w:그렇게 되돌려진 상처는,\n또다시 누군가의 안에 남게 된다는 걸.",
                "w:그게 너였던 때처럼 말이야.",
                "w:힘들 땐,\n피리 대신 내 소리를 빌려도 좋아.",
                "w:그 소리는,\n상처받은 마음을 잠시 멈추게 해줄 수 있어.",
                "n:(우클릭 시 늑대가 울음소리를 냅니다.\n울음소리는 주변 적의 오염도를 낮추고,\n적을 일정 시간 기절시켜 플레이어와 충돌하지 않게 만듭니다.)",
                "w:모든 걸 혼자 버티지 않아도 돼.\n기댈 수 있는 누군가가 곁에 있다면, 기대도 좋아."
            },
            new List<string>
            {
                "g:여긴… 유난히 조용하네요.",
                "n:(붉은 음표 하나가 공중에 떠 있다.\n바람도 멈춘 듯한 침묵 속에서, 음표는 묵묵히 소녀를 바라본다.)",
                "g:그 아이를… 도와주고 싶었어요.\n근데, 먼저 공격해 버렸어요.",
                "g:그땐, 그게 더 쉬웠거든요.",
                "w:쉬운 길이 늘 옳은 건 아니야.\n하지만 그걸 알기 위해선, 그 길을 먼저 지나쳐야 할 때도 있지.",
                "g:분노의 악장이 점점 더 강해지고 있어요.\n더 멀리, 더 강하게 퍼져요.",
                "g:이 방법이 계속 쉬워진다면…\n나중엔 되돌아갈 수 없을까 봐, 무서워요.",
                "n:(소녀는 피리를 꼭 쥐고 있다.\n손끝이 하얗게 질릴 만큼 힘이 들어가 있다.)",
                "w:너는 이미 그 두려움을 느끼고 있잖아.\n그것만으로도 잘하고 있는 거야.",
                "g:하지만… 상처를 되돌리는 대신,\n제 방식으로 저 아이들을 대할 수 있을까요?",
                "w:물론이지.\n네가 걷고 있는 길이, 그 증거니까.",
                "g:…마주해볼게요. 지금은, 괜찮을지도 몰라요.",
                "n:(소녀는 조심스레 손을 뻗는다.\n붉은 음표는 미세하게 떨리며, 천천히 공기를 울린다.)"
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
            if (ising) return;

            ising = true;
            sideImage.SetActive(true);
            UI1.SetActive(false);
            UI2.SetActive(false);
            string objectName = gameObject.name;
            if (int.TryParse(objectName, out int index))
            {
                StartCoroutine(TypingText(index));
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

                yield return new WaitForSeconds(0.02f);
            }
            isTyping = false;
            isSkipping = false;
            while (!Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                yield return null;
            }
        }
        // Wait 1 second at the end | 마지막에 2초 추가 대기함
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
        yield return new WaitForSeconds(0.1f);

        playerCtrl.OnEnable();


        Destroy(this.gameObject);
    }
}
