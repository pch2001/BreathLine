using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Story_four_R : MonoBehaviour
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


    List<List<string>> dialoguescript;//대사 스크립트 저장소

    private bool isSkipping = false;
    private bool isTyping = false;

    public GameObject telepoint;

    public GameObject player;

    void Start()
    {
        printText1.text = "";
        printText2.text = "";
        printText3.text = "";
        dialoguescript = new List<List<string>>
        {
            new List<string>
            {
                "g:어...?",
                "w:따뜻한 손길 하나 제대로 받아본 적 없는 주제에",
                "w:고작 환상 같은 빛 한줄기 봤다고 너무 들뜬 거 아니야?",
                "w:나아가기 위해 너는 어떻게 행동했지?",
                "w:나는 너야. 누구보다 널 가장 잘 알지. 분노도, 슬픔도, 증오도…",
                "w:너는 너를 무너트리면서까지 공격을 멈추지 않았잖아?",
                "w:그런 얄팍한 위선으로 벗어날 수 있으리라 생각하다니..",
                "w:멍청하지…",
                "w:널 증오해.",
                "w:너는 고통이 무엇인 지 알면서도 외면했지.",
                "w:넌 정말 나쁜 아이야. 그리고, 어른이 되어도…",
                "w:다르지 않을 거야."
             },

            new List<string>
            {
                "w:자기 하나 제대로 간수 못하는 주제에.",
                "w:착한 어른이 될 수 있을 것 같았어?",
                "w:네게 남은 건 분노와 증오밖에 없어.",
                "g:그래. 나는… 그 어두운 장롱 안을, 방을 여전히 벗어나지 못했어.",
                "g:고작 일지도 모르는 작은 틈새 속 빛 줄기가 내 전부였지.",
                "g:변하겠다는 말 하나로 모든 걸 되돌릴 수 없다는 걸 알아.",
                "g:하지만, 여기서 멈출 수는 없어.",
                "g:위선자라 해도 좋아."
            },

            new List<string> {
                "n:나는 내가 가장 미워했던 이의 모습을 닮아 있었다.",
                "n:잘못되었음을 앎에도 내가 본 건, 겪은 건 그게 전부였기에.",
                "n:그런 변명 뿐인 말을 늘어 놓기 바빴다.",
                "n:늑대 같은 어른이 되고 싶었다.",
                "n:의지할 수 있고, 안아줄 수 있는 따듯한 어른이 되고 싶었다.",
                "n:나의 아이에게는 달콤한 말을, 사랑스러운 말을 전하리라",
                "n:언젠가 그런 다짐도 했던 것 같다.",
                "n:다짐이 무너지는 건 순식간이었다.",
                "n:하지만,",
                "g:이 고통을, 아픔을 더는 되풀이할 수는 없어.",
                "g:되풀이하고 싶지 않아.",
                "n:(부드러운 피리 소리가 허공을 채웁니다.)",
                "n:(당신의 마음이 악보가 되어 새겨집니다.)",
                "n:(당신은 지금 어떤 “나”를 마주하고 있나요?)"

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
                if(index == 1)
                {
                    TeleportPlayer();
                }
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
