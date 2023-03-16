using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
    //private bool isFirstUpdate = true;
    void Update()
    {
        Loader.LoaderCallback();
        gameObject.SetActive(false);
    }
}
