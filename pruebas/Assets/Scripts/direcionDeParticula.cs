using UnityEngine;
using System.Collections;

public class direcionDeParticula : MonoBehaviour {
	public Transform wepon;


	
	// Update is called once per frame
	void Update () {
	//transform.rotation.y = wepon.rotation.y;
	transform.rotation = wepon.transform.localRotation;
	transform.position = wepon.TransformPoint(Vector3.zero);//poscion de la arma
	transform.position = wepon.transform.position;//
		transform.forward = wepon.TransformDirection (Vector3.forward);
	}
	void OnParticleCollision (GameObject other){
		if(other.gameObject.GetComponent<Rigidbody>()){
			Vector3 direction = other.transform.position - transform.position;
			direction = direction.normalized;
			other.GetComponent<Rigidbody>().AddForce(direction*50);
			//Destroy(other);
		}
	}
}
