using UnityEngine;
using System.Collections;

public class customIK : MonoBehaviour {
	//huesos
	public Transform upperArm;
	public Transform forearm;
	 Transform hand;
	//objetivos
	public Transform target;
	public Transform elbowTarget;
	// Use this for initialization
	public bool isEneable;
	public float weight=1;
	//variablesInternas
	Quaternion upperArmStartRotation;
	Quaternion foreArmStartRotation;
	Quaternion handStartRottation;

	Vector3 targetRelativeStartPosition;
	Vector3 elbowRelativeStartPosition;
	//objetos auxiliares
	GameObject upperArmAxisCorrection;
	GameObject foreArmAxisCorrection;
	GameObject handAxisCorrection;
	//para guardar la ultima posicion de un objeto dado
	Vector3 lastUpperArmPosition;
	Vector3 lastTargetPosition;
	Vector3 lastElbowArmPosition;

	Animator anim;

	void Start () {
		anim = GetComponentInParent<Animator> ();
		hand = GetComponentInParent<movimientoEnemigo> ().leftHand;
		forearm = hand.parent;
		upperArm = forearm.parent;


		upperArmStartRotation = upperArm.rotation;
		foreArmStartRotation = forearm.rotation;
		handStartRottation = hand.rotation;

		elbowRelativeStartPosition = elbowTarget.position - upperArm.position;

		upperArmAxisCorrection = new GameObject ("upperArmAxisCorrection");
		foreArmAxisCorrection = new GameObject ("foreArmAxisCorrection");
		handAxisCorrection= new GameObject ("handAxisCorrection");

		upperArmAxisCorrection.transform.parent = transform;
		foreArmAxisCorrection.transform.parent = upperArmAxisCorrection.transform;
		handAxisCorrection.transform.parent = foreArmAxisCorrection.transform;

	}
	void Update(){
		if (anim.GetCurrentAnimatorStateInfo (1).IsTag ("apuntar")) {
			isEneable = true;
		} else {
			isEneable = false;
		}
	}
	
	// Update is called once per frame
	//para que la animacion se reproduca y al final del cuadro realizar los cambios
	void LateUpdate () {
		if(isEneable){
			calculateIK();
		}
	}

	void calculateIK(){
		if(target==null){
			targetRelativeStartPosition = Vector3.zero;
			return;//sale de la funcion
		}
		if(targetRelativeStartPosition ==Vector3.zero && target !=null){
			targetRelativeStartPosition = target.position - upperArm.position;
		}
		lastElbowArmPosition = elbowTarget.position;
		lastTargetPosition = target.position;
		lastUpperArmPosition=upperArm.position;

		float UpperArmLength = Vector3.Distance (upperArm.position, forearm.position);
		float foreArmLenght = Vector3.Distance (forearm.position, hand.position);

		float armLenght = UpperArmLength + foreArmLenght;
		float hypotenuse = UpperArmLength;
		float targetDistance = Vector3.Distance (upperArm.position, target.position);
		//para no dislocar
		targetDistance = Mathf.Min(targetDistance, armLenght - 0.0001f);
		//adjacencia
		float adjacent = (hypotenuse * hypotenuse - foreArmLenght + targetDistance * targetDistance) / (2 * targetDistance);
		float IkAngle = Mathf.Acos (adjacent / hypotenuse) * Mathf.Rad2Deg;

		Vector3 targetPosition = target.position;
		Vector3 elbowTargetPosition = elbowTarget.position;

		Transform upperArmParent = upperArm.parent;
		Transform foreArmParent = forearm.parent;
		Transform handParent = hand.parent;

		Vector3 upperArmScale= upperArm.localScale;
		Vector3 foremArmScale = forearm.localScale;
		Vector3 handScale = hand.localScale;



		Vector3 upperArmLocalPosition = upperArm.localPosition;
		Vector3 foremArmLocalPosition = forearm.localPosition;
		Vector3 handLocalPosition = hand.localPosition;
		//rotaciones
		Quaternion upperArmRotation=upperArm.rotation;
		Quaternion foreArmRotation=forearm.rotation;
		Quaternion handRootation=hand.rotation;
		Quaternion handLocalRotation = hand.localRotation;

		target.position = targetRelativeStartPosition + upperArm.position;
		elbowTarget.position = elbowRelativeStartPosition + upperArm.position;
		upperArm.rotation = upperArmStartRotation;
		forearm.rotation = foreArmStartRotation;
		hand.rotation = handStartRottation;
		//correcta posicion
		transform.position = upperArm.position;

		transform.LookAt (targetPosition, elbowTargetPosition - transform.position);
		//coorrecion del antebrazo
		upperArmAxisCorrection.transform.position = upperArm.position;
		upperArmAxisCorrection.transform.LookAt (forearm.position, upperArm.up);
		upperArm.parent = upperArmAxisCorrection.transform;

		foreArmAxisCorrection.transform.position = forearm.position;
		foreArmAxisCorrection.transform.LookAt (hand.position, forearm.up);
		forearm.parent = foreArmAxisCorrection.transform;

		handAxisCorrection.transform.position = hand.position;
		hand.parent = handAxisCorrection.transform;

		target.position = targetPosition;
		elbowTarget.position = elbowTargetPosition;

		upperArmAxisCorrection.transform.LookAt (target, elbowTarget.position - upperArmAxisCorrection.transform.position);
		upperArmAxisCorrection.transform.localRotation = Quaternion.Euler (upperArmAxisCorrection.transform.localRotation.eulerAngles - new Vector3 (IkAngle, 0, 0));
		foreArmAxisCorrection.transform.LookAt (target, elbowTarget.position - upperArmAxisCorrection.transform.position);
		handAxisCorrection.transform.rotation = target.rotation;

		upperArm.parent = upperArmParent;
		forearm.parent = foreArmParent;
		hand.parent = handParent;

		upperArm.localScale = upperArmScale;
		forearm.localScale = foremArmScale;
		hand.localScale = handScale;

		hand.localScale = handScale;
		upperArm.localPosition = upperArmLocalPosition;
		forearm.localPosition = foremArmLocalPosition;
		hand.localPosition = handLocalPosition;

		weight = Mathf.Clamp01 (weight);
		upperArm.rotation = Quaternion.Slerp (upperArmRotation, upperArm.rotation, weight);
		forearm.rotation = Quaternion.Slerp (foreArmRotation, forearm.rotation, weight);
		hand.rotation = target.rotation;
	}
}
