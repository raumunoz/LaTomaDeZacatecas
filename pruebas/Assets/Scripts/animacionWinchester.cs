using UnityEngine;
using System.Collections;

public class animacionWinchester : MonoBehaviour {
	private Animator anim;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetButtonUp ("Fire1")) {
			anim.SetBool ("apunta", true);
		} else {
			anim.SetBool ("apunta", false);
		}
		

		
		if (Input.GetKeyUp (KeyCode.Space)) {
			anim.SetBool ("espacio", false);
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			anim.SetBool ("espacio", true);
		}
	}
}
