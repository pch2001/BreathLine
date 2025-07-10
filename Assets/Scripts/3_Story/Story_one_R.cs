using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Story_one_R : MonoBehaviour
{
    private PlayerCtrl_R playerCtrl;

    //대사 출력
    public GameObject girlImage;
    public GameObject wolfImage;
    public GameObject sideImage;
    public GameObject textbehind;
    public Text printText1;
    public Text printText2;
    public Text printText3;

    public GameObject Boss1;
    public GameObject BossHp;

    List<List<string>> dialoguescript;//대사 스크립트 저장소

    private bool isSkipping = false;
    private bool isTyping = false;

    void Start()
    {
        printText1.text = "";
        printText2.text = "";
        printText3.text = "";
        dialoguescript = new List<List<string>>
        {
        new List<string> { "g:무슨 일이 벌어진 걸까?\n내가 무슨 잘못을 한 거지?",
            "g:어디서부터 잘못된 걸까?\n처음부터 모든 게 문제였던 걸까…",
            "n:끝도 없는 질문과 정답 없는 생각이 나를 짓누른다.",
            "n:늑대는 없었다.",
            "n:따스한 시선도, 걱정 어린 이야기도 들을 수 없다.",
            "n:이제야 보인다.\n눈에 들어오지 않던 것들이",
            "n:오염도, 균열도, 내 안의 검은 웅덩이조차도.\n언제 이렇게 커져버린 걸까?",
            "n:분노의 악장은 나를 천천히 좀먹고 있었다.",
            "n:물론 아무래도 좋다고 생각했다.",
            "n:내 존재를 드러낼 수 있다면, 기꺼이 나를 불태우겠노라 생각했다.",
            "n:그저, 아무도 나를 내치지 않기를 바랐을 뿐이었는데.",
            "n:연주가 정말 나를 보여줄 수 있을까?",
            "n:이 혼란 속에서도… 내가 감히 평화를 바랄 수 있을까?",
            "g:평화의 악장이 단순히 평화를 상징하는 게 아닐지도 몰라.",
            "g:내가 마지막까지 붙잡고 싶었던, 단 하나의 소망이 아닐까…",
            "g:그런 거라면…",
            "n:(피리를 꼭 쥔 손이 미세하게 떨려 옵니다.)",
            "g:늑대의.. 따스했던 그 울음소리를 감히 흉내 낼 수 있을까?",
            "g:해보기 전까진 모르는 일이지. 좋아. 나아가자."
        },
        new List<string> { "n:전에 못 보던 것이 앞으로 막고 있다...",
                            "n:쉽게 보내줄 거 같지 않다."
        },
        new List<string> { "w:1", "g:2" }
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

        playerCtrl = playerCode.GetComponent<PlayerCtrl_R>();
        playerCtrl.OnDisable();

        yield return new WaitForSeconds(0.3f);

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
            }else if(speaker == "n")
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

        if(index == 1)
        {
            Boss1.GetComponent<EnemyBase>().attackMode = true;
            BossHp.SetActive(true);
        }

        playerCtrl.OnEnable();

        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }


}
