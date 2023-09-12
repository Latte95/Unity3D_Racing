using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        PhotonView photon = gameObject.GetPhotonView();
        photon.RPC("ActiveOn", RpcTarget.AllBuffered);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("AI"))
        {
            CharacterControl character = other.GetComponent<CharacterControl>();
            character.kart.anim.SetTrigger(character.kart.BananaHitHash);
            character.rigid.velocity *= 0.2f;
            gameObject.SetActive(false);
        }
    }
    [PunRPC]
    public void ActiveOn()
    {
        gameObject.SetActive(true);
    }
}
