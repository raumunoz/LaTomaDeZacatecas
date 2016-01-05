using UnityEngine;
using System.Collections;

public class Permanencia : MonoBehaviour {

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
		if ((Application.loadedLevel == 0)||(Application.loadedLevel == 12)||(Application.loadedLevel == 11)) {
			Destroy(this.gameObject);
		}
	}
}
