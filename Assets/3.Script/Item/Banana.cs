using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("AI"))
        {
            CharacterControl character = other.GetComponent<CharacterControl>();
            character.kart.anim.SetTrigger(character.kart.BananaHitHash);
            character.rigid.velocity /= 5;
            gameObject.SetActive(false);
        }
    }
}
