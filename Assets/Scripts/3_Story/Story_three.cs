using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Story_three : MonoBehaviour
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

    public GameObject Boss3;

    List<List<string>> dialoguescript;//대사 스크립트 저장소

    private bool isSkipping = false;
    private bool isTyping = false;

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
            "n:(깊은 안개 속, 무너진 건물의 잔해들이 그림자처럼 솟아 있다.)",
            "n:(어디선가 낮고 무거운 울림이 천천히 퍼진다.)",
            "g:…뭔가 있어요. 아주 크고, 멀리서 우리를 바라보는 것 같아요.",
            "g:(피리… 방금까지와는 달라요.\n손에 쥐고 있지만, 소리가 안 나올 것 같은 느낌이 들어요.)",
            "w:이곳은 네가 가진 것을 계속해서 가져가는 곳이야.",
            "w:기억도, 감정도… 그리고 너 자신도.",
            "g:……저, 괜찮은 걸까요?",
            "w:잃어버리는 건 누구에게나 무서운 일이야.\n하지만 그걸 마주할 수 있을 때, 비로소 다음으로 나아갈 수 있지.",
            "n:(소녀는 고개를 들어 앞으로 시선을 향한다.\n멀리서 잔해 사이를 움직이는 무언가가 감지된다.)",
            "g:저기 무언가가 있어요. 위험한 건 아니겠죠…?",
            "w:지켜보자. 모든 게 너를 위협하진 않아.\n하지만 어떤 것은… 너를 흔들지도 모르지.",
            "w:내가 곁에 있을게.\n내 근처에 있으면, 무엇이 와도 넌 쓰러지지 않을 거야.\n(늑대 근처에서는 적의 밀어내기 공격에 피격되지 않습니다.)",
            "g:조금 무섭지만… 지금은 앞으로 가야 할 때예요.",
            "w:그래. 길은 늘 앞에 있어.\n우리가 할 일은, 그 위를 한 걸음씩 걷는 것뿐이야.",
            "n:(소녀는 폐허 사이로 조심스레 발을 내딛는다.\n잔해를 스치는 발끝에 작은 먼지가 일어난다.)"
        },
            new List<string>
        {
            "n:(폐허의 끝자락, 지하로 이어지는 낡은 통로 앞에 도착했다.)",
            "n:(숨 돌릴 틈도 없이 달려왔던 발걸음이, 그제야 조용히 멈춰 선다.)",
            "g:…끝났어요. 드디어 여기까지 왔네요.",
            "g:솔직히 말해도 돼요?",
            "g:이번에는… 분노의 악장이 없었으면, 여기까지 못 왔을 거예요.",
            "g:그게 편했어요. 빠르고, 확실하니까요.",
            "w:그래.",
            "w:그 감정은 너를 지키기 위해 생겨난 거니까.",
            "w:다만 오래 머물게 두지는 마.",
            "w:그 힘은 너를 앞으로 나아가게도 하지만,\n너 자신을 상처낼 수도 있으니까.",
            "g:그게 뭐가 나쁜가요?",
            "g:무섭고 괴로운 걸 밀어내는 게 잘못인가요?",
            "w:잘못은 아니야.",
            "w:그저, 그것만으로는 네 마음을 다 설명할 수 없다는 거지.",
            "n:(소녀는 피리를 바라본다.\n그 끝은 묵직하게 물들어 있다.)",
            "g:이 소리… 점점 익숙해져요.\n예전엔 무서웠는데, 지금은… 당연하게 느껴져요.",
            "w:그건 네 안에 오래 머물러 있던 감정이니까.",
            "n:(늑대는 말끝을 흐린다. 숨소리가 조금 무겁게 들린다.)",
            "w:…괜찮아. 아직은.",
            "w:조금 무겁지만,\n네가 더 멀리 가기 위해서라면… 충분히 견딜 수 있어.",
            "w:감정은 나쁘지 않아.\n어떻게 쓰느냐가, 결국 널 만드는 거야.",
            "n:(늑대는 소녀를 바라본다.\n그 눈빛엔 판단도, 두려움도 없다.)",
            "w:나는 네가 어떤 모습이든 곁에 있을 거야.",
            "w:그리고 그 마음이 흔들릴 때마다,\n언제든 다시 일어설 수 있게… 네 곁을 비워두지 않을게.",
            "n:(소녀는 조용히 고개를 끄덕인다.\n그 끝에서 피리가 낮고, 길게 울린다.)"
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

        if(index == 1)
        {
            Boss3.SetActive(false);
        }

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
        UI1.SetActive(false);
        UI2.SetActive(false);
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
        if (index == 0)
        {
            Boss3.SetActive(true);
        }
        yield return new WaitForSeconds(0.1f);

        playerCtrl.OnEnable();
        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }
}
