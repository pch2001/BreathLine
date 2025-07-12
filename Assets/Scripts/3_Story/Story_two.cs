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
        new List<string> { "g:이건… 뭐지? \r\n내가 잊고 있던 기억인가…", "g:정말 그렇다면…나는 존재해선 안되는 아이였던 걸까?", "g:눈엣가시처럼, 아무 쓸모도 없는…\n보이지 말아야 할 투명한 존재…\r\n…",
                            "g :싫어.. \r\n그렇게 살고 싶지 않아.\r\n이대로 가만히 있을 순 없어.\r\n어서 내 쓸모를 증명해야 해. ", "n: -피리의 힘이 강화됩니다. \r\n-분노의 악장 사용 시 공격력이 강화됩니다.\r\n-평화의 악장 지속 시간이 길어집니다. "
        },
        new List<string> { "w:괜찮니 아이야?", "g:네 아무것도 아니에요. 괜찮아요." ,
                            "w:....",
                            "w:그거 아니? 인간의 마음에는 비 온 후 길가의 웅덩이처럼, 고이는 물이 있어.",
                            "w:시간이 흐르면, 그 작은 웅덩이는 스스로도 닿을 수 없는 어둠이 되지.",
                            "w:무언가를 표현하고, 털어 놓는 게 처음은 힘들 지 몰라도.\n너라면 분명 해낼 수 있을 거라 믿어.",
                            "g:..."
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
