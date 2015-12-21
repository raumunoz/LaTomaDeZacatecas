using UnityEngine;
using System.Collections;

public class conroladorArmasCuerpoAcuerpo : MonoBehaviour {
	private Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Fire1")){
			anim.SetBool ("ataque", true);
		}else{
			anim.SetBool ("ataque", false);
		}
		if((Input.GetKey(KeyCode.LeftShift))){
			anim.SetBool ("correr", true);
			//Debug.Log("Apuntarrrrrrrrrrrrrrrrrr");
		}else{
			anim.SetBool ("correr", false);
		}
		if((Input.GetKeyDown("1")||Input.GetKeyDown("2")||Input.GetKeyDown("3"))){
			anim.SetBool ("salir", true);
			//Debug.Log("Apuntarrrrrrrrrrrrrrrrrr");
		}else{
			anim.SetBool ("salir", false);
		}

	}
}
