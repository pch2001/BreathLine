using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CureBreath : MonoBehaviour
{
    public float floatAmplitude = 0.3f; // 위아래로 움직이는 폭 (얼마나 높이 움직이는지)
    public float floatSpeed = 1f;       // 움직이는 속도

    private Vector3 startPos;           // 오브젝트의 초기 위치

    void Start()
    {
        startPos = transform.position; // 시작 시 오브젝트의 현재 위치를 저장
    }

    void Update()
    {
        // Mathf.Sin은 -1에서 1 사이의 값을 반환합니다.
        // Time.time은 게임 시작 후 경과된 시간입니다.
        // floatSpeed를 곱하여 사이클 속도를 조절합니다.
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // 초기 Y 위치에 계산된 newY 값을 더하여 왕복 운동을 만듭니다.
        transform.position = new Vector3(startPos.x, startPos.y + newY, startPos.z);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //오염도 내리기
            Destroy(this.gameObject);
        }
    }
}
