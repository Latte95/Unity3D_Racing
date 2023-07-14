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

    private void Awake()
    {
        normalDistance = Vector3.Distance(normalEnd.position, normalStart.position);
        minimapDistance = Vector3.Distance(minimapEnd.position, minimapStart.position);
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
