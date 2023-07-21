using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public RectTransform minimapStart;
    public RectTransform minimapEnd;
    public Transform normalStart;
    public Transform normalEnd;

    public Transform minimap;
    public Image icon;

    private float normalDistance;
    private float minimapDistance;
    private Vector2 defaultResolution = new Vector2(1920, 1080);

    private void Awake()
    {
        minimapStart = GameObject.FindGameObjectWithTag("MiniStart").GetComponent<RectTransform>();
        minimapEnd = GameObject.FindGameObjectWithTag("MiniEnd").GetComponent<RectTransform>();
        normalStart = GameObject.FindGameObjectWithTag("MapStart").GetComponent<Transform>();
        normalEnd = GameObject.FindGameObjectWithTag("MapEnd").GetComponent<Transform>();
        minimap = GameObject.FindGameObjectWithTag("MiniMap").GetComponent<Transform>();

        normalDistance = Vector3.Distance(normalEnd.position, normalStart.position);
        minimapDistance = Vector3.Distance(minimapEnd.position, minimapStart.position);
        // 해상도에 따른 minimapDistance 보정
        float currentResolutionRatio = Mathf.Min(Screen.width / defaultResolution.x, Screen.height / defaultResolution.y);
        minimapDistance /= currentResolutionRatio;

        StartCoroutine(SetIcon_co());
    }
    private void Update()
    {
        float proportionX = -transform.position.x / normalDistance;
        float proportionY = -transform.position.z / normalDistance;

        Vector3 minimapPosition = new Vector3(
            proportionX * minimapDistance,
            proportionY * minimapDistance, 0);

        if (icon != null)
        {
            icon.rectTransform.anchoredPosition = minimapPosition;
        }
    }
    private IEnumerator SetIcon_co()
    {
        // 캐릭터 모델 생성 기다림
        yield return new WaitForSeconds(1);
        // 캐릭터 모델에 있던 icon 오브젝트를 minimap UI로 이동
        icon.transform.SetParent(minimap);
        // 해상도에 따른 아이콘 크기 조절
        float currentResolutionRatio = Mathf.Min(Screen.width / defaultResolution.x, Screen.height / defaultResolution.y);
        icon.transform.localScale *= currentResolutionRatio;
    }
}
