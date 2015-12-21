using UnityEngine;
using System.Collections;

public class controladorAnimcionesArmas : MonoBehaviour {
	private Animator anim;
	private int cartuchos; 
	// Use this for initialization
	void Start () {
		anim = GetComponentInChildren<Animator> ();
		anim.SetInteger ("cartuchos", 6);
		escojerAnimatorActivo();

	}
	void escojerAnimatorActivo(){

		
		/*foreach (var childAnimator in GetComponentsInChildren<Animator>())
		{
			if (childAnimator)
			{
				anim=childAnimator;
				break; //si se encontro el primer animator deja de buscar
			}
		}*/
	}
	
	// Update is called once per frame
	void Update () {

		/*if (Input.GetKeyUp (KeyCode.Shift)) {
			anim.SetBool ("espacio", false);
		}*/
		if(Input.GetButtonDown("Fire1")){
			anim.SetBool ("disparo", true);
		}else{
			anim.SetBool ("disparo", false);
		}
		if((Input.GetMouseButton(1))){
			anim.SetBool ("apuntar", true);

		}else{
			anim.SetBool ("apuntar", false);
		}

		
			
		if((Input.GetKey(KeyCode.LeftShift))&&(Input.GetKey(KeyCode.W))){
			anim.SetBool ("correr", true);

		}else{
			anim.SetBool ("correr", false);
		}
		if((Input.GetKey(KeyCode.R))){
			anim.SetBool ("recargar", true);
			
		}else{
			anim.SetBool ("recargar", false);
		}
		if((Input.GetKey(KeyCode.Alpha1))||(Input.GetKey(KeyCode.Alpha2))){
			//anim.SetBool ("recargar", true);
			Debug.Log("cambiar------------------");
			
		}else{
			//anim.SetBool ("recargar", false);
		}

	}
}

