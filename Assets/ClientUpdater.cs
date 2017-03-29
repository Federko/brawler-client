using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientUpdater : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        ClientManager.Init();
	}
	
	// Update is called once per frame
	void LateUpdate () {

        ClientManager.Update();
	}
}
