using UnityEngine;
using System.Collections;

public class flashDecanion : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine(esperaParaDestruir());
	}

	
	IEnumerator esperaParaDestruir() {
		yield return new WaitForSeconds(.2f);
		Destroy (gameObject);
	}
}
/* Comentario de Prueba con el Sourcetree */