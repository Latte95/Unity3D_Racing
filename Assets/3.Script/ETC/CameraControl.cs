using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float speed;

    public Transform player;
    private Rigidbody rigid;
    public Vector3 Offset;

    void Start()
    {
        player.TryGetComponent(out rigid);
    }

    void FixedUpdate()
    {
        Vector3 playerForward = (rigid.velocity + player.transform.forward).normalized;
        transform.position = Vector3.Lerp(transform.position,
            player.position + player.transform.TransformVector(Offset)
            + playerForward * (-5f),
            speed * Time.deltaTime);
        transform.LookAt(player);
    }
}
