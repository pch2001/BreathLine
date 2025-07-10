﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoreanTyper; //텍스트 출력 파일
using UnityEngine.InputSystem; //입력 막는거
public class Story_one : MonoBehaviour
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


    public GameObject telepoint;

    public GameObject player;

    void Start()
    {
        dialoguescript = new List<List<string>>
        {
            new List<string> {
                //"g:아, 빛이다… 근데 여긴… 어디지?",
                //"n:눈을 뜨니 알 수 없는 공간이다.\n새하얗게 질린 하늘과, 시야를 방해하는 안개.\n내가 누구인지, 왜 이곳에 있는 지 기억 나지 않는다.",
                //"n:(땅을 짚은 손에 차갑고 묵직한 감촉이 느껴진다.)",
                //"g:이건 뭐지?",
                //"n:(작고 오래된 피리 한 자루. 낯설지 않은 모습이다.)",
                //"g:여기 오기 전부터 가지고 있던 물건 같아.",
                //"n:(파스락-)",
                //"g:늑대…?",
                //"w:일어났구나.",
                //"w:안심해도 좋아. 나는 너를 해치지 않으니까.",
                //"g:…?",
                //"n:말을 하는 늑대는 처음 본다.\n하지만, 그보다도 내게 말을 걸어주는 존재가 있다는 게 더 신기하다.",
                //"w:뭐 그래 내 모습을 보고 놀란 것 같지는 않네.",
                //"w:그 피리…",
                //"w:좋아. 아직 피리가 남아 있다니 다행이야.",
                //"g:…?",
                //"w:그 피리는 너의 마음을 연주할 수 있는 악기거든.",
                //"w:피리의 힘은 네 안의 감정으로부터 비롯돼.",
                //"w:쥐고 있는 피리를 입에 한 번 대보겠니?",
                //"g:아, 네…",
                //"w:그래. 피리를 연주하는 방법은 두 가지가 있어. 한번씩 불어볼까?",
                "w:첫번째는 평화의 악장.\n상대를 수면 상태로 만들어줘.\n(W키를 길게 눌러주세요)",

                 "n:(희미한 바람 소리가 난다.)",
                "w:두번째는 분노의 악장.\n빠른 속도로 상대를 쓰러트릴 수 있지만,\n너 역시 피해를 입게 돼.\n(W키를 짧게 눌러주세요)",
                 "n:(힘없는 휘파람 소리가 들린다.)",

                "w:하하 괜찮아. \n너무 오랜만에 연주하는 거라 소리가 잘 나지 않는 게 당연해.",
                "w:소리가 어떻게 나는 지는 중요하지 않아.",
                "w:어떤 연주이든, 의도가 담긴다는 사실이 중요하지.",
                "w:나는 네가 어떤 연주를 하든 곁에 있을 거야.",
                "w:하지만 명심해. 그 연주가 결국,\n네 자신이 누구인지 보여줄 거라는 걸.",
                "g:…",
                "n:이 피리는 단순한 악기가 아닌 것 같다.\n잠든 기억을 깨울 수 있는 하나뿐인 열쇠가 아닐까?",
                "g:저는 이제 여기서 어떻게 하면 좋을까요?",
                "w:글쎄, 너는 이제 어떻게 하고 싶어?",
                "g:잘 모르겠지만… \n원래 있던 곳으로 돌아가야 하지 않을까요…?",
                "w:원래 있던 곳… 그래 좋아. \n나아가는 것도 용기인 법이지.",
                "w:그럼 우리 같이 모험을 한번 해볼까?",
                "g:무슨 말씀을 하시는 지 잘 모르겠지만… \n우선 앞으로 가면 될까요?",
                "w:…",
                "g:그…런거죠?"
            },

            new List<string>
            {
                "w:구울들이네.",
                "g:저것들은 어떻게 해치워야 하는 거죠?",
                "w:해치울 필요가 있을까?",
                "g:네?",
                "w:구울들은 네가 반응해주기 전까지 영역을 벗어나지 못해.",
                "w:관심을 원하는 가여운 망령에 불과하지.",
                "g:가여운 망령…"
            }

        };
    }
    void FixedUpdate()
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

            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.enabled = false;
            }

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

            if (line.Contains("n:(희미한 바람 소리가 난다.)") || line.Contains("n:(힘없는 휘파람 소리가 들린다.)"))
            {
                Debug.Log("테스트시작");
                sideImage.SetActive(false);
                textbehind.SetActive(false);
                playerCtrl.OnEnable();

                yield return StartCoroutine(WaitForPiriPerformance()); // 피리 연주 끝날 때까지 대기
                playerCtrl.OnDisable();
                textbehind.SetActive(true);

                sideImage.SetActive(true);


            }


            int length = dialogue.GetTypingLength();
            isTyping = true;
            isSkipping = false;

            for (int i = 0; i <= length; i++)
            {
                if(isSkipping)
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

        playerCtrl.OnEnable();


        yield return new WaitForSeconds(0.5f);
        if(index ==0)
            TeleportPlayer();
        Destroy(this.gameObject);
    }
    private IEnumerator WaitForPiriPerformance()
    {
        Debug.Log("피리 연주 시작!");
        bool played = false;
        float holdTime = 0f;

        while (!played)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                holdTime += Time.deltaTime;
            }
            else if (Keyboard.current.wKey.wasReleasedThisFrame)
            {
                if (holdTime > 1f)
                    Debug.Log("평화의 악장");
                else
                    Debug.Log("분노의 악장");

                played = true;
            }

            yield return null;
        }
    }

    void TeleportPlayer()
    {
        if (telepoint != null && player != null)
        {
            player.transform.position = telepoint.transform.position;
        }
    }
}