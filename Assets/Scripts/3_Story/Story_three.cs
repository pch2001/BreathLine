using KoreanTyper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Story_three : MonoBehaviour
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

    public GameObject Boss3;

    List<List<string>> dialoguescript;//��� ��ũ��Ʈ �����

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
            "n:(���� �Ȱ� ��, ������ �ǹ��� ���ص��� �׸���ó�� �ھ� �ִ�.)",
            "n:(��𼱰� ���� ���ſ� �︲�� õõ�� ������.)",
            "g:������ �־��. ���� ũ��, �ָ��� �츮�� �ٶ󺸴� �� ���ƿ�.",
            "g:(�Ǹ��� ��ݱ����ʹ� �޶��.\n�տ� ��� ������, �Ҹ��� �� ���� �� ���� ������ ����.)",
            "w:�̰��� �װ� ���� ���� ����ؼ� �������� ���̾�.",
            "w:��ﵵ, �������� �׸��� �� �ڽŵ�.",
            "g:������, ������ �ɱ��?",
            "w:�Ҿ������ �� �������Գ� ������ ���̾�.\n������ �װ� ������ �� ���� ��, ��μ� �������� ���ư� �� ����.",
            "n:(�ҳ�� ���� ��� ������ �ü��� ���Ѵ�.\n�ָ��� ���� ���̸� �����̴� ���𰡰� �����ȴ�.)",
            "g:���� ���𰡰� �־��. ������ �� �ƴϰ��ҡ�?",
            "w:���Ѻ���. ��� �� �ʸ� �������� �ʾ�.\n������ � ������ �ʸ� ������� ����.",
            "w:���� �翡 ������.\n�� ��ó�� ������, ������ �͵� �� �������� ���� �ž�.\n(���� ��ó������ ���� �о�� ���ݿ� �ǰݵ��� �ʽ��ϴ�.)",
            "g:���� ���������� ������ ������ ���� �� ������.",
            "w:�׷�. ���� �� �տ� �־�.\n�츮�� �� ����, �� ���� �� ������ �ȴ� �ͻ��̾�.",
            "n:(�ҳ�� ���� ���̷� ���ɽ��� ���� ����´�.\n���ظ� ��ġ�� �߳��� ���� ������ �Ͼ��.)"
        },
            new List<string>
        {
            "n:(������ ���ڶ�, ���Ϸ� �̾����� ���� ��� �տ� �����ߴ�.)",
            "n:(�� ���� ƴ�� ���� �޷��Դ� �߰�����, ������ ������ ���� ����.)",
            "g:���������. ���� ������� �Գ׿�.",
            "g:������ ���ص� �ſ�?",
            "g:�̹����¡� �г��� ������ ��������, ������� �� ���� �ſ���.",
            "g:�װ� ���߾��. ������, Ȯ���ϴϱ��.",
            "w:�׷�.",
            "w:�� ������ �ʸ� ��Ű�� ���� ���ܳ� �Ŵϱ�.",
            "w:�ٸ� ���� �ӹ��� ������ ��.",
            "w:�� ���� �ʸ� ������ ���ư��Ե� ������,\n�� �ڽ��� ��ó�� ���� �����ϱ�.",
            "g:�װ� ���� ���۰���?",
            "g:������ ���ο� �� �о�� �� �߸��ΰ���?",
            "w:�߸��� �ƴϾ�.",
            "w:����, �װ͸����δ� �� ������ �� ������ �� ���ٴ� ����.",
            "n:(�ҳ�� �Ǹ��� �ٶ󺻴�.\n�� ���� �����ϰ� ����� �ִ�.)",
            "g:�� �Ҹ��� ���� �ͼ�������.\n������ �������µ�, �������� �翬�ϰ� ��������.",
            "w:�װ� �� �ȿ� ���� �ӹ��� �ִ� �����̴ϱ�.",
            "n:(����� ������ �帰��. ���Ҹ��� ���� ���̰� �鸰��.)",
            "w:��������. ������.",
            "w:���� ��������,\n�װ� �� �ָ� ���� ���ؼ���顦 ����� �ߵ� �� �־�.",
            "w:������ ������ �ʾ�.\n��� �����İ�, �ᱹ �� ����� �ž�.",
            "n:(����� �ҳฦ �ٶ󺻴�.\n�� ������ �Ǵܵ�, �η��� ����.)",
            "w:���� �װ� � ����̵� �翡 ���� �ž�.",
            "w:�׸��� �� ������ ��鸱 ������,\n������ �ٽ� �Ͼ �� �ְԡ� �� ���� ������� ������.",
            "n:(�ҳ�� ������ ���� �����δ�.\n�� ������ �Ǹ��� ����, ��� �︰��.)"
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
        UI1.SetActive(false);
        UI2.SetActive(false);
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

                yield return new WaitForSeconds(0.03f);
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
