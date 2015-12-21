using UnityEngine;
using System.Collections;

public class controladorAnimcaiones : MonoBehaviour {
	private Animator anim;
	void Start () {
		anim = GetComponent<Animator> ();
	}
	

	void Update () {
	
if (Input.GetButtonDown ("Fire1")) {
			anim.SetBool ("apunalar", true);
		} else {
			anim.SetBool("apunalar",false);
		}
	}
}


