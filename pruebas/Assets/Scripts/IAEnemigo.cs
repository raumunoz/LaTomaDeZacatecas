using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class IAEnemigo : MonoBehaviour {
	NavMeshAgent agent;
	movimientoEnemigo charMove;
	//public Transform target;

	public GameObject wayPointHolder;
	public List<Transform> wayPoints = new List<Transform> ();
	float targetToleracian=1;

	Vector3 targetPos;
	Animator anim;

	float patrolTImer;
	public float waitingTime=4;
	public float atackRate=4;
	public float atackTimer=4;

	int wayPointIndex;
	public List<GameObject>Enemies=new List<GameObject>();
	public GameObject enemyToAtack;
	// Use this for initialization
	public enum AIstate {
		patrol,Atack
	}

	public AIstate aiState;

	void Start () {
		agent = GetComponentInChildren<NavMeshAgent> ();
		charMove = GetComponent<movimientoEnemigo> ();
		anim = GetComponent<Animator> ();
		Transform[] wayp = wayPointHolder.GetComponentsInChildren<Transform> ();
		foreach(Transform tr in wayp){
			if (tr != wayPointHolder.transform){
				wayPoints.Add(tr);
			}
		}
	}
	void DecideState(){
		if (Enemies.Count > 0) {
			if (!enemyToAtack) {
				foreach (GameObject enGo in Enemies) {
					Vector3 direction = enGo.transform.position - transform.position;
					float angle = Vector3.Angle (direction, transform.forward);
					if (angle < 110f * 0.5f) {
						RaycastHit hit;
						if (Physics.Raycast (transform.position + transform.up, direction.normalized, out hit, 15)) {//quinze es el radio de la mira
							if (hit.collider.GetComponent<statsDePersonajes> ()) {
								if (hit.collider.GetComponent<statsDePersonajes> ().Id != GetComponent<statsDePersonajes> ().Id) {
									Debug.Log ("!!!!!!!!!!!!!!!");
									enemyToAtack = hit.collider.gameObject;
								}
							}
						}
					}
				}
			}//if (!enemyToAtack)
			else{//si ya hay un enemigo
				aiState=AIstate.Atack;
			}
		}
	}

	void Attack(){
		agent.Stop ();
		anim.SetFloat ("girar", 0);
		anim.SetFloat ("avanzar", 0);

		charMove.Move(Vector3.zero,true,Vector3.zero);//deteien el movimiento del enemigo
		Vector3 direction = enemyToAtack.transform.position - transform.position;
		float angle = Vector3.Angle (direction, transform.forward);
		if (angle < 110f * 0.5f) {
			transform.LookAt(enemyToAtack.transform.position);


		}
		atackTimer += Time.deltaTime;
		if(atackTimer > atackRate){
			atackTimer=0;
			shootRay();
		}

	}

	void Patrolling(){
		agent.speed = 1;
		if (wayPoints.Count > 0) {

			if (agent.remainingDistance < agent.stoppingDistance) {
				patrolTImer += Time.deltaTime;
				if (patrolTImer >= waitingTime) {
					if (wayPointIndex == wayPoints.Count - 1) {
						wayPointIndex = 0;
					} else {
						wayPointIndex++;
					}
					patrolTImer = 0;
				}
			} else {//if(agent.remainingDistance < agent.stoppingDistance)
				patrolTImer = 0;
			}
			agent.transform.position = transform.position;
			agent.destination = wayPoints [wayPointIndex].position;

			Vector3 velocity = agent.desiredVelocity * 0.5f;

			charMove.Move (velocity,false,targetPos);

		}//if(wayPoints.Count	> 0)
		else {
			agent.transform.position = transform.position;
			Vector3 lookPos = (wayPoints.Count > 0)? wayPoints[wayPointIndex].position : Vector3.zero;
			charMove.Move(Vector3.zero,false,transform.position);//aqui tambie va si esta apuntado y hacia donde esta apuntado
		}
	}

	// Update is called once per framez
	void Update () {
		//codigo solo para un punto
		/*if (target != null) {
			if ((target.position - targetPos).magnitude > targetToleracian) {
				targetPos = target.position;
				agent.SetDestination (targetPos);
			}
			agent.transform.position = transform.position;
			charMove.Move (agent.desiredVelocity);
		} else {//si no hay objetivo
			charMove.Move(Vector3.zero);//le pasamos un valor nullo
		}*/
		DecideState ();
		switch (aiState) {
		case AIstate.Atack:
			Attack();
			break;
		case AIstate.patrol:
			Patrolling();
			break;
		}
	


	}
	void OnAnimatorIK(){
		if (enemyToAtack) {
			anim.SetLookAtWeight (1, 0.8f, 1, 1, 1);
			anim.SetLookAtPosition (enemyToAtack.transform.position);
		} else {
			anim.SetLookAtWeight(0);
		}
	}
	void shootRay(){
		Debug.Log("ENIMGO:Disparo");
		/*
		RaycastHit hit;
		GameObject go = Instantiate (weponManger.BulletPrefab, transform.position, Quaternion.identity)as GameObject;
		LineRenderer line = go.GetComponent<LineRenderer> ();
		Vector3 startPos = weponManger.Activewepon.bulletSpawn;
		Vector3 EndPos = Vector3.zero;
		int mask=~(1<<9);
		Vector3 directionToAttack = enemyToAtack.transform.position - transform.position;
		if(Physics.Raycast(startPos,directionToAttack,out hit,Mathf.Infinity,mask)){
			float distance=Vector3.Distance(weponManager.Active.bulletSpawn.transform.position,hit.point);
			RaycastHit[]hits=Physics.RaycastAll(startPos,hit.point-startPos,distance);

			foreach(RaycastHit hit2 in hits){
				if(hit2.transform.GetComponent<Rigidbody>()){
					Vector3 direction = hit2.transform.position-transform.position;
					direction = direction.normalized;
					hit2.transform.GetComponent<Rigidbody>().AddForce(direction*200);

				}
				else if(hit2.transform.transform.GetComponent<Destructible>()){
					hit2.transform.GetComponent<Destructible>().destruct=true;
				}
				else if(hit2.transform.transform.GetComponent<statsDePersonajes>()){

				}
			}
			EndPos=hit.point;
		}
		line.SetPosition (0, startPos);
		line.SetPosition (1, EndPos);
		*/
	}
}
