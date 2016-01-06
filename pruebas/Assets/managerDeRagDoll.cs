using UnityEngine;
using System.Collections;

public class managerDeRagDoll : MonoBehaviour {
	public Collider[] cols;
	public Rigidbody[] rigids;
	Animator anim;
	bool goRagDoll;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		rigids = GetComponentsInChildren<Rigidbody> ();
		cols = GetComponentsInChildren<Collider> ();
		foreach(Rigidbody rig in rigids){
			if(rig.gameObject.layer==10){
				rig.isKinematic = true;
			}
		}
		foreach(Collider col in cols){
			if(col.gameObject.layer==10){
				col.isTrigger= true;
			}
		}
	}
	
	public void RagdollCharacter(){
		if (!goRagDoll) {
			anim.enabled = false;
			goRagDoll = true;


			foreach(Rigidbody rig in rigids){
				if(rig.gameObject.layer==10){
					rig.isKinematic = false;
				}
			}
			foreach(Collider col in cols){
				if(col.gameObject.layer==10){
					col.isTrigger= false;
				}
			}
		}
	}
	//parate a la que inmpactamos

}
