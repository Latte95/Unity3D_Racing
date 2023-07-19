using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public void ChangeScene(string scene)
    {
        if (PunManager.Instance != null)
        {
            Destroy(PunManager.Instance.gameObject);
        }
        SceneManager.LoadScene(scene);
    }
}
