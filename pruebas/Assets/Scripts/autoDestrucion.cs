using UnityEngine;
using System.Collections;

public class autoDestrucion : MonoBehaviour {

	IEnumerator selfDestruct(){
		yield return new WaitForSeconds (0.8f);
		Destroy (gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
		StartCoroutine ("selfDestruct");
	}
}
