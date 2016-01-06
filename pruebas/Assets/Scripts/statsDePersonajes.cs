using UnityEngine;
using System.Collections;

public class statsDePersonajes : MonoBehaviour {

	public string Id;
	public float health = 100;
	managerDeRagDoll rM;
	bool muerto;

	Animator anim;

	void Start(){
		
		muerto = false;
		if (GetComponent<managerDeRagDoll> ()) {
			rM = GetComponent<managerDeRagDoll> ();
			anim = GetComponent<Animator> ();
		}
	}

	void Update(){
		if (health <= 0) {
			if (!muerto) { 
				if (rM != null) {
					rM.RagdollCharacter ();
					closeAllComponents ();

				}
				muerto = true;
			}
		}
	}
	public void closeAllComponents(){
		if (GetComponent<movimientoEnemigo> ()) {
			movimientoEnemigo charMOve = GetComponent<movimientoEnemigo> ();
			charMOve.enabled = false;
		}
		if (GetComponent<IAEnemigo> ()) {
			IAEnemigo charIA = GetComponent<IAEnemigo> ();
			charIA.enabled = false;
		}

		if (GetComponent<statsDePersonajes> ()) {
			statsDePersonajes charStats = GetComponent<statsDePersonajes> ();
			charStats.enabled = false;
		}

		if (GetComponent<managerDeArmasEnemigo> ()) {
			managerDeArmasEnemigo charManagerArmas = GetComponent<managerDeArmasEnemigo> ();
			charManagerArmas.enabled = false;
		}

		if (GetComponentInChildren<VisionEnemigo> ()) {
			VisionEnemigo charVision = GetComponentInChildren<VisionEnemigo> ();
			charVision .enabled = false;
		}

		if (GetComponentInChildren<NavMeshAgent> ()) {
			NavMeshAgent charNAv= GetComponentInChildren<NavMeshAgent> ();
			charNAv.enabled = false;
		}
		if (GetComponent<NavMeshAgent> ()) {
			NavMeshAgent charNAv= GetComponent<NavMeshAgent> ();
			charNAv.enabled = false;
		}
		Collider col = GetComponent<Collider> ();
		col.enabled = false;

		Rigidbody rig = GetComponent<Rigidbody> ();
		rig.isKinematic = true;
	}

	public void HitDetectionPArt(Transform hitPart){
		Debug.Log ("Transform de impacto  " + hitPart);
		//Debug.Log ("Transform de cabeza  " + anim.GetBoneTransform(HumanBodyBones.Neck));

		if(hitPart.name == "Head"){
			Debug.Log("Impactamos cabeza!!!");
			health -= 100;
		}
		if(hitPart == anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg)){
			Debug.Log("pierna izquierad!!!");
			health -= 25;
		}
		if(hitPart == anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg)){
			Debug.Log("Impactamos pantorrilla izquierda!!!");
			health -= 25;
		}
		if(hitPart == anim.GetBoneTransform(HumanBodyBones.RightUpperLeg)){
			Debug.Log("pierna Derecha!!!");
			health -= 25;
		}
		if(hitPart == anim.GetBoneTransform(HumanBodyBones.RightLowerLeg)){
			Debug.Log("Impactamos pantorrilla Derecha!!!");
			health -= 25;
		}

		if(hitPart == anim.GetBoneTransform(HumanBodyBones.LeftUpperArm)){
			Debug.Log("brazo izquierd");
			health -= 25;
		}
		if(hitPart == anim.GetBoneTransform(HumanBodyBones.LeftLowerArm)){
			Debug.Log("Impactamos antebrazo Izquierdio!!!");
			health -= 25;
		}
		if(hitPart == anim.GetBoneTransform(HumanBodyBones.RightUpperArm)){
			Debug.Log("brazo derecho");
			health -= 25;
		}
		if(hitPart == anim.GetBoneTransform(HumanBodyBones.RightLowerArm)){
			Debug.Log("Impactamos antebrazo derecho!!!");
			health -= 25;
		}

		if(hitPart == anim.GetBoneTransform(HumanBodyBones.Hips)){
			Debug.Log("Caderas");
			health -= 50;
		}
		if(hitPart.name == "Spine1"){
			Debug.Log("Inmpactamos pecho Pecho");
			health -= 75;
		}
	}
}
