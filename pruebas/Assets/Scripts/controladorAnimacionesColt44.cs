using UnityEngine;
using System.Collections;

public class controladorAnimacionesColt44 : MonoBehaviour {
	private Animator anim;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		anim.SetBool("correr",false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton ("Fire1")) {
			anim.SetBool ("disparo", true);
		}
		if (Input.GetButtonUp ("Fire1")) {
			anim.SetBool ("disparo", false);
		} 

		if (Input.GetButtonDown ("Fire2")) {
			anim.SetBool ("apuntar", true);
		} 
		if (Input.GetButtonUp ("Fire2")) {
			anim.SetBool ("apuntar", false);
		} 

		if (Input.GetKeyDown (KeyCode.Space)) {
			anim.SetBool ("recargar", false);
		}
	}
}
