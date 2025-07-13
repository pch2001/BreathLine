using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Story_two : MonoBehaviour
{
    private PlayerCtrl playerCtrl;

    //��� ���
    public GameObject girlImage;
    public GameObject wolfImage;
    public GameObject sideImage;
    public GameObject textbehind;
    public Text printText1;
    public Text printText2;
    public Text printText3;


    List<List<string>> dialoguescript;//��� ��ũ��Ʈ �����

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
                "g:������ ��, �װǡ� �� ����̾�����.",
                "n:(�ҳ�� ������ ���� ���̽���.\nǮ���� �����Ҹ����� ������ ����������.)",
                "g:�׷��� ���� �� �̷��� ������ҡ�?\n���ε�, ���� �� �� ��������.",
                "w:������ ���� �� �� �ڿ�,\n������ ���� �ٸ��� ���̱⵵ ��.",
                "w:�� ���� ����� ������,\n������ ��ó�� ������ ǰ�� �ְŵ�.",
                "g:���¡� �׷� �� �̰��� �ִٰ��?",
                "w:�Ҹ��� ������ �ʾƵ�,\n���� ���� �ʾƵ�,\n��ó�� ���� �� �־�.",
                "w:�� ���� ���̵���\n�׷� �� �ݺ��ϸ� ��ƿԾ�.",
                "g:�׷��� �� ���̵��� ����� �ϴ� �ſ���?",
                "w:�װ� �װ� ������ �� �־�.",
                "w:�г��� ������ �����ϸ�,\n�׵��� ���� ��ó�� �״�� ������ �� ����.",
                "n:(WŰ�� ª�� ���� �г��� ������ �����ϸ�,\n���� źȯ�� �ݻ��� �� �ֽ��ϴ�.)",
                "w:������ ���� ������� ��ó�� �ǰ��� ��,\n�� �����θ� �� ��ġ�� ������ ����.",
                "g:����",
                "w:�ʿ��ϴٸ�, ���� �ҷ��൵ ��.",
                "w:���� �翡 �ִ� ����,\n���̵��� �������� �������� �ž�.",
                "n:(��Ŭ�� �� ���밡 �����մϴ�.\n����� �ֺ� ���� ��ȭ��Ű��, źȯ�� ������� �ϸ�,\n�������� ���������� ����ϴ�.)",
                "g:����¡� �ƹ��͵� ���� �ʾƵ� ������ �ſ���?",
                "w:��ó�� �ǵ����� ���,\n�� �ʸ��� ������Ρ� �� ���̵��� ���� �� ���� �ž�."
            },
            new List<string>
            {
                "g:���� �ְ� ���� �����߾��.",
                "g:�׷����� ���� �׳�, �ǵ����� �ͻ��ε�.",
                "n:(�ҳ�� ���� ���� ä, �տ� �� �Ǹ��� �� ��� �ִ�.)",
                "w:��ó�� ���� �����,\n�� ��ó�� ��� �ٷ�� ���� �� ���� ����.",
                "w:�׷��� ����,\n���� �״�� �ǵ��������� ����.",
                "g:���� �߸��� �ǰ��䡦?",
                "w:�ƴ�. �ʴ� ����,\n���� �װ� ���ʹٴ� �� ǥ���� �ž�.",
                "w:�ٸ� ����� ��.",
                "w:�׷��� �ǵ����� ��ó��,\n�Ǵٽ� �������� �ȿ� ���� �ȴٴ� ��.",
                "w:�װ� �ʿ��� ��ó�� ���̾�.",
                "w:���� ��,\n�Ǹ� ��� �� �Ҹ��� ������ ����.",
                "w:�� �Ҹ���,\n��ó���� ������ ��� ���߰� ���� �� �־�.",
                "n:(��Ŭ�� �� ���밡 �����Ҹ��� ���ϴ�.\n�����Ҹ��� �ֺ� ���� �������� ���߰�,\n���� ���� �ð� �������� �÷��̾�� �浹���� �ʰ� ����ϴ�.)",
                "w:��� �� ȥ�� ��Ƽ�� �ʾƵ� ��.\n��� �� �ִ� �������� �翡 �ִٸ�, ��뵵 ����."
            },
            new List<string>
            {
                "g:���䡦 ������ �����ϳ׿�.",
                "n:(���� ��ǥ �ϳ��� ���߿� �� �ִ�.\n�ٶ��� ���� ���� ħ�� �ӿ���, ��ǥ�� ������ �ҳฦ �ٶ󺻴�.)",
                "g:�� ���̸��� �����ְ� �;����.\n�ٵ�, ���� ������ ���Ⱦ��.",
                "g:�׶�, �װ� �� �����ŵ��.",
                "w:���� ���� �� ���� �� �ƴϾ�.\n������ �װ� �˱� ���ؼ�, �� ���� ���� �����ľ� �� ���� ����.",
                "g:�г��� ������ ���� �� �������� �־��.\n�� �ָ�, �� ���ϰ� ������.",
                "g:�� ����� ��� �������ٸ顦\n���߿� �ǵ��ư� �� ������ ��, ��������.",
                "n:(�ҳ�� �Ǹ��� �� ��� �ִ�.\n�ճ��� �Ͼ�� ���� ��ŭ ���� �� �ִ�.)",
                "w:�ʴ� �̹� �� �η����� ������ ���ݾ�.\n�װ͸����ε� ���ϰ� �ִ� �ž�.",
                "g:�������� ��ó�� �ǵ����� ���,\n�� ������� �� ���̵��� ���� �� �������?",
                "w:��������.\n�װ� �Ȱ� �ִ� ����, �� ���Ŵϱ�.",
                "g:�������غ��Կ�. ������, ���������� �����.",
                "n:(�ҳ�� ���ɽ��� ���� ���´�.\n���� ��ǥ�� �̼��ϰ� ������, õõ�� ���⸦ �︰��.)"
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
        if (collider.gameObject.CompareTag("Player"))  // Ư�� �±׷� Ȯ��
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

        //Ȯ��
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
        //�ؽ�Ʈ ���
        // ��� ���
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
            // ��ǳ�� �б�
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
        // Wait 1 second at the end | �������� 2�� �߰� �����
        yield return new WaitForSeconds(1f);


        //Ȯ��� ī�޶� �ǵ����� 
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
