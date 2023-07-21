using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<GameObject> banana;
    public List<GameObject> greenShell;
    public List<GameObject> redShell;
    public List<GameObject> blueShell;

    #region ΩÃ±€≈Ê
    public static ItemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion ΩÃ±€≈Ê

    public GameObject MakeItem(EItem item)
    {
        GameObject clone;

        switch (item)
        {
            case EItem.Banana:
                foreach(GameObject gm in banana)
                {
                    if(!gm.activeSelf)
                    {
                        return gm;
                    }
                }
                clone = Instantiate(banana[0]);
                clone.name = banana[0].name;
                banana.Add(clone);
                clone.SetActive(false);
                return banana[banana.Count - 1];

            case EItem.GreenShell:
                foreach (GameObject gm in greenShell)
                {
                    if (!gm.activeSelf)
                    {
                        return gm;
                    }
                }
                clone = Instantiate(greenShell[0]);
                clone.name = greenShell[0].name;
                greenShell.Add(clone);
                clone.SetActive(false);
                return greenShell[greenShell.Count - 1];

            //case EItem.RedShell:
            //    foreach (GameObject gm in redShell)
            //    {
            //        if (!gm.activeSelf)
            //        {
            //            return gm;
            //        }
            //    }
            //    clone = Instantiate(redShell[0]);
            //    clone.name = redShell[0].name;
            //    redShell.Add(clone);
            //    clone.SetActive(false);
            //    return redShell[redShell.Count - 1];

            //case EItem.BlueShell:
            //    foreach (GameObject gm in blueShell)
            //    {
            //        if (!gm.activeSelf)
            //        {
            //            return gm;
            //        }
            //    }
            //    clone = Instantiate(blueShell[0]);
            //    clone.name = blueShell[0].name;
            //    blueShell.Add(clone);
            //    clone.SetActive(false);
            //    return blueShell[blueShell.Count - 1];
        }
        return null;
    }
}
