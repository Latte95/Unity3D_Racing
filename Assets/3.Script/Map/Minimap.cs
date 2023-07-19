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

        icon.transform.SetParent(minimap);
    }
    private void Update()
    {
        float proportionX = -transform.position.x / normalDistance;
        float proportionY = -transform.position.z / normalDistance;

        Vector3 minimapPosition = new Vector3(
            proportionX * minimapDistance,
            proportionY * minimapDistance, 0);

        icon.rectTransform.anchoredPosition = minimapPosition;
    }
}
