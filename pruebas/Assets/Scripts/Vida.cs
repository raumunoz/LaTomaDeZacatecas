using UnityEngine;
using System.Collections;

public class Vida : MonoBehaviour {
	float puntosActuales;
	private Animator anim;
	float puntosDeVida=1f;
	private Collider colaider;
	//public AudioClip myClip;
	void Start () {
		puntosActuales = puntosDeVida;
		anim = GetComponent<Animator> ();
		colaider = GetComponent<Collider> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void danio(float amt){
		puntosActuales -= amt;
		if (puntosActuales<=0){
			morir();
		}
	}

	void morir(){
		//Debug.Log("MORIIIIIIIIIIIIIIII");
		//Destroy (gameObject);
		if(gameObject.tag=="jarron"){
			anim.SetBool ("explosion", true);
			Destroy (colaider);
		}
		if(gameObject.tag=="enemigo"){
			anim.SetBool ("muerte", true);
			Debug.Log("MUERTEEEEEEEEEEEEEEEEEEE");
			//Destroy (colaider);

		}



		//AudioSource.PlayClipAtPoint(myClip, transform.position);
	}
}


