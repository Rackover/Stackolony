using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingWindow : MonoBehaviour {

    public bool shouldDisplayBackdrop = false;

    public Image backdropImage;

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
        if (SceneManager.GetActiveScene().name != GameManager.instance.menuSceneName) {
            shouldDisplayBackdrop = true;
        }

        backdropImage.enabled = shouldDisplayBackdrop;

        if (!GameManager.instance.IsLoading()) {
            Destroy();
        }
    }
}
