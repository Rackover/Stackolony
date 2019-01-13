using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingWindow : MonoBehaviour {
    
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void Update()
    {
        if (!GameManager.instance.IsLoading()) {
            Destroy();
        }
    }
}
