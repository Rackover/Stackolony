using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfNotDebugging : MonoBehaviour {
	void Start () {
		if (!GameManager.instance.DEBUG_MODE){
            this.gameObject.SetActive(false);
        }
	}
}
