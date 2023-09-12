using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenShell : MonoBehaviour
{
    private Rigidbody rigid;
    private Vector3 velocity;

    private void Awake()
    {
        TryGetComponent(out rigid);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PhotonView photon = gameObject.GetPhotonView();
        photon.RPC("ActiveOn", RpcTarget.AllBuffered);
        velocity = 70 * transform.forward;
        velocity.y = -9.8f;
    }

    private void FixedUpdate()
    {
        rigid.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("AI"))
        {
            CharacterControl character = other.GetComponent<CharacterControl>();
            character.kart.anim.SetTrigger(character.kart.ShellHitHash);
            character.rigid.velocity = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("fence"))
        {
            velocity = Vector3.Reflect(velocity, collision.contacts[0].normal);
            velocity.y = -9.8f;
        }
    }
    [PunRPC]
    public void ActiveOn()
    {
        gameObject.SetActive(true);
    }
}
