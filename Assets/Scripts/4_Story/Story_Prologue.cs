using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoreanTyper; //�ؽ�Ʈ ��� ����
using UnityEngine.InputSystem; //�Է� ���°�
public class Story_Prologue : MonoBehaviour
{
    private PlayerCtrl playerCtrl;

    //��� ���
    public GameObject girlImage;
    public GameObject wolfImage;
    public GameObject narrateImage;

    public GameObject sideImage;
    public GameObject textbehind;
    public Text printText1;
    public Text printText2;
    public Text printText3;


    List<List<string>> dialoguescript;//��� ��ũ��Ʈ �����


    void Start()
    {

        dialoguescript = new List<List<string>>
        {
        new List<string> {
    "g:��, ���̴١� �ٵ� ���䡦 �����?",
    "n:���� �ߴ� �� �� ���� �����̴�.\n���Ͼ�� ���� �ϴð�, �þ߸� �����ϴ� �Ȱ�.\n���� ��������, �� �̰��� �ִ� �� ��� ���� �ʴ´�.",
    "n:(���� ¤�� �տ� ������ ������ ������ ��������.)",
    "g:�̰� ����?",
    "n:(�۰� ������ �Ǹ� �� �ڷ�. ������ ���� ����̴�.)",
    "g:���� ���� ������ ������ �ִ� ���� ����.",
    "n:(�Ľ���-)",
    "g:���롦?",
    "w:�Ͼ����.",
    "w:�Ƚ��ص� ����. ���� �ʸ� ��ġ�� �����ϱ�.",
    "g:��?",
    "n:���� �ϴ� ����� ó�� ����.\n������, �׺��ٵ� ���� ���� �ɾ��ִ� ���簡 �ִٴ� �� �� �ű��ϴ�.",
    "w:�� �׷� �� ����� ���� ��� �� ������ �ʳ�.",
    "w:�� �Ǹ���",
    "w:����. ���� �Ǹ��� ���� �ִٴ� �����̾�.",
    "g:��?",
    "w:�� �Ǹ��� ���� ������ ������ �� �ִ� �Ǳ�ŵ�.",
    "w:�Ǹ��� ���� �� ���� �������κ��� ��Ե�.",
    "w:��� �ִ� �Ǹ��� �Կ� �� �� �뺸�ڴ�?",
    "g:��, �ס�",
    "n:(�Ǹ��� �Կ� ��� ���.)",
    "w:�׷�. �Ǹ��� �����ϴ� ����� �� ������ �־�. �ѹ��� �Ҿ��?",
    "w:ù��°�� ��ȭ�� ����. (����Ű ����) ��븦 ���� ���·� �������.",
    "n:(����� �ٶ� �Ҹ��� ����.)",
    "w:�ι�°�� �г��� ����. (����Ű ����) ���� �ӵ��� ��븦 ����Ʈ�� �� ������, �� ���� ���ظ� �԰� ��.",
    "n:(������ ���Ķ� �Ҹ��� �鸰��. �̳� ���� ��Ż�Ǵ� �Ҹ��� �鸰��.)",
    "w:���� ������. �ʹ� �������� �����ϴ� �Ŷ� �Ҹ��� �� ���� �ʴ� �� �翬��.",
    "w:�Ҹ��� ��� ���� ���� �߿����� �ʾ�.",
    "w:� �����̵�, �ǵ��� ���ٴ� ����� �߿�����.",
    "w:���� �װ� � ���ָ� �ϵ� �翡 ���� �ž�.",
    "w:������ �����. �� ���ְ� �ᱹ, �� �ڽ��� �������� ������ �Ŷ�� ��.",
    "g:��",
    "n:�� �Ǹ��� �ܼ��� �ǱⰡ �ƴ� �� ����.\n��� ����� ���� �� �ִ� �ϳ����� ���谡 �ƴұ�?",
    "g:���� ���� ���⼭ ��� �ϸ� �������?",
    "w:�۽�, �ʴ� ���� ��� �ϰ� �;�?",
    "g:�� �𸣰������� ���� �ִ� ������ ���ư��� ���� ������䡦?",
    "w:���� �ִ� ���� �׷� ����. ���ư��� �͵� ����� ������.",
    "w:�׷� �츮 ���� ������ �ѹ� �غ���?",
    "g:���� ������ �Ͻô� �� �� �𸣰������� �켱 ������ ���� �ɱ��?",
    "w:��",
    "g:�ס�������?" }
        };

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))  // Ư�� �±׷� Ȯ��
        {
            Debug.Log("��� ����");
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
        GameObject playerCode = GameObject.FindWithTag("Player"); // Player �±� �ʿ�!
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

            // ��ǳ�� �б�
            if (speaker == "g")
            {
                girlImage.SetActive(true);
                wolfImage.SetActive(false);
                narrateImage.SetActive(false);

                printText1.text = "";
            }
            else if (speaker == "w")
            {
                girlImage.SetActive(false);
                wolfImage.SetActive(true);
                narrateImage.SetActive(false);

                printText2.text = "";
            }
            else if (speaker == "n")
            {
                girlImage.SetActive(false);
                wolfImage.SetActive(false);
                narrateImage.SetActive(true);

                printText3.text = "";
            }
            int length = dialogue.GetTypingLength();
            for (int i = 0; i <= length; i++)
            {
                if (speaker == "g")
                    printText1.text = dialogue.Typing(i);
                else if (speaker == "w")
                    printText2.text = dialogue.Typing(i);
                else if (speaker == "n")
                    printText3.text = dialogue.Typing(i);

                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(2f);
        }
        // Wait 1 second at the end | �������� 2�� �߰� �����
        yield return new WaitForSeconds(1f);

        playerCtrl.OnEnable();
        yield return new WaitForSeconds(0.3f);

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



        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);
    }


}