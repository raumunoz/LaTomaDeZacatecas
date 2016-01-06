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
	 managerDeArmasEnemigo weponManger;
	Vector3 targetPos;
	Animator anim;
	bool InSIght=true;
	float patrolTImer;
	public float waitingTime=4;
	public float atackRate=4;
	public float atackTimer=4;
	public GameObject closestCover;
	int wayPointIndex;
	public List<GameObject>Enemies=new List<GameObject>();
	public GameObject enemyToAtack;
	public List<Transform>avaliableCover=new List<Transform>();
	bool takingCOver;
	bool attacking;
	public int timesFired;
	bool decision;
	Vector3 lastKnowPosition;
	objetodeCovertura objMc;
	// Update is called once per framez
	void Update () {

		if (enemyToAtack) {
			LookForEnemy ();
		}
			
		DecideState ();
	//	FindCoves();
		switch (aiState) {
		case AIstate.Atack:
		//	anim.SetLookAtPosition (enemyToAtack.transform.position);
			Attack();
			break;
		case AIstate.patrol:
			Patrolling();
			break;
		case AIstate.GoToCover:
			agent.speed = 6;
			takeCover ();
			break;
		case AIstate.InCover:
			inCOver ();
			break;
		}

	}

	// Use this for initialization
	public enum AIstate {
		patrol,Atack,GoToCover,InCover
	}

	public AIstate aiState;

	void Start () {
		decision = false;
		weponManger=GetComponent<managerDeArmasEnemigo> ();
		agent = GetComponentInChildren<NavMeshAgent> ();
		charMove = GetComponent<movimientoEnemigo> ();
		anim = GetComponent<Animator> ();
//		enemyToAtack=false;
		Transform[] wayp = wayPointHolder.GetComponentsInChildren<Transform> ();
		foreach(Transform tr in wayp){
			if (tr != wayPointHolder.transform){
				wayPoints.Add(tr);
			}
		}
	}

	void DecideState(){
		
		#region tenemos un enmegio ?
		if (Enemies.Count > 0) {
			if (!enemyToAtack) {
				//Debug.Log ("no hay enmigo ha Atacar");
				foreach (GameObject enGo in Enemies) {
					Vector3 direction = enGo.transform.position - transform.position;//direcion del personaje enemigo
					float angle = Vector3.Angle (direction, transform.forward);//encontrar el angula hacia el enemigo
					//Debug.Log("anguiloooooooooooooo"+ angle);
					if (angle < 110f * 0.5f ) {//angulo al que debe estar el enmigo
							Debug.Log("en Angulo de vision");

						RaycastHit hit;
						if (Physics.Raycast (transform.position + transform.up, direction.normalized, out hit, 20)) {//20 es el radio de la mira
							Debug.Log("dentro de la vision ");

							if (hit.collider.GetComponent<statsDePersonajes> ()) {//no funciona esta linea
								Debug.Log(" enemigo");
								if (hit.collider.GetComponent<statsDePersonajes> ().Id != GetComponent<statsDePersonajes> ().Id) {
									Debug.Log ("!!!!!!!!!!!!!!!");
									enemyToAtack = hit.collider.gameObject;
								}
							}

							/*if (hit.collider.GetComponentInParent<statsDePersonajes> ()) {//no funciona esta linea
								Debug.Log("tealvez enemigo juagador");
								Debug.Log("id de jugador  "+ hit.collider.GetComponentInParent<statsDePersonajes> ().Id );
								if (hit.collider.GetComponentInParent<statsDePersonajes> ().Id != GetComponent<statsDePersonajes> ().Id) {//no entra
									Debug.Log ("!!!!!!!!!!!!!!!");
									enemyToAtack = hit.collider.gameObject;
								}
							}*/
						
						}
					}
				}
			}//if (!enemyToAtack)
			else{//si ya hay un enemigo
				//no hemos decidido todavia
				if(!decision){
					Debug.Log("Hacer decicion");
					//compare las aramas
					//if(compararArma(weponManger.activeWeapon,enemyToAtack.GetComponentInChildren<managerDeArmasjugador>().armaActiva )){
					if(enemyToAtack.GetComponent<managerDeArmasEnemigo>()){
					if(compararArma(weponManger.activeWeapon,enemyToAtack.GetComponent<managerDeArmasEnemigo>().activeWeapon)){
						//el arma del enemico es superior
						//aiState=AIstate.Atack;
						aiState=AIstate.GoToCover;
					}else{
						//el arma enemiga es inferior tomames covertuira
						aiState=AIstate.GoToCover;		
					}
				
				
				decision=true;//solo una vez se toma esta decision
				}

					if(enemyToAtack.GetComponentInChildren<managerDeArmasjugador>()){
						
						if(compararArma(weponManger.activeWeapon,enemyToAtack.GetComponentInChildren<managerDeArmasjugador>().armaActiva)){
							//el arma del enemico es superior
							//aiState=AIstate.Atack;
							Debug.Log("Ve a cobertura");
							aiState=AIstate.GoToCover;
						}else{
							//el arma enemiga es inferior tomames covertuira
							aiState=AIstate.GoToCover;		
						}


						decision=true;//solo una vez se toma esta decision
					}
				
				}
				
			}
		}
		#endregion

		#region estamos en covertura?

		if (aiState == AIstate.GoToCover) {
			if(closestCover!=null){

				float distance=Vector3.Distance(transform.position,closestCover.transform.position);
				//Debug.Log("Distancia "+distance);
				if(distance <= 1.5f){
					//para animaciones de covertura

					aiState=AIstate.InCover;

				}
		}
		}
		#endregion
	}

	

	void Attack(){
		agent.Stop ();
		//anim.SetFloat ("girar", 0);
		//anim.SetFloat ("avanzar", 0);

		charMove.Move(Vector3.zero,true,Vector3.zero);//deteien el movimiento del enemigo
		//angulo del enemigo 
		Vector3 direction = lastKnowPosition - transform.position;
		float angle = Vector3.Angle (direction, transform.forward);

		if (angle < 110f * 0.5f) {

			//anim.SetBool ("apuntar", true);
			transform.LookAt(lastKnowPosition);


		}
		atackTimer += Time.deltaTime;

		if(atackTimer > atackRate){
			
			if (tenemosMUnicion (weponManger.activeWeapon)) {
				
				//anim.SetBool ("disparar", true);
				timesFired++;
				shootRay ();
			} else {
				timesFired++;
				shootRay ();
					
				/*if (regresaIntRandmo() >= 5) {
					Debug.Log ("Enemigo Decidio Recargar");
					RecargarArmaActiva ();
				} else {
					Debug.Log ("Enemigo Decidio cambiar arma");
					weponManger.changeWeapon (true);
				}*/
			}
			atackTimer = 0;
		}


	}

	void Patrolling(){
		agent.speed = 1f;
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
			moveTO (wayPoints[wayPointIndex].position,false,targetPos);

		}//if(wayPoints.Count	> 0)
		else {
			agent.transform.position = transform.position;
			Vector3 lookPos = (wayPoints.Count > 0)? wayPoints[wayPointIndex].position : Vector3.zero;
			charMove.Move(Vector3.zero,false,lookPos);//aqui tambie va si esta apuntado y hacia donde esta apuntado
		}
	}

	void moveTO(Vector3 destination,bool aim,Vector3 lookPos){
		agent.transform.position = transform.position;
		agent.destination = destination;

		Vector3 velocity = agent.desiredVelocity * 0.5f;

		charMove.Move (velocity,aim,lookPos);
		
	}

	void FindCoves(){
		//guarda todso los colaiders en ese radio en un arregro
		Collider[] hitColliders = Physics.OverlapSphere (transform.position, 20);//radio de la vista del enemigo

		if (hitColliders.Length > 0) {
//			Debug.Log ("TAMAÑpp"+hitColliders.Length);
			for (int i = 0; i < hitColliders.Length; i++) {
				if (hitColliders [i].gameObject.GetComponent<objetodeCovertura> ()) {

					//	Debug.DrawRay (hitColliders [i].transform.position + Vector3.up, direction + Vector3.up);

					if (!hitColliders [i].gameObject.GetComponent<objetodeCovertura> ().owner) {
						/*Vector3 direction = enemyToAtack.transform.position - hitColliders [i].transform.position;
						float dis = Vector3.Distance (hitColliders [i].transform.position, enemyToAtack.transform.position);//pared
					
						RaycastHit hit;
						Debug.DrawRay (hitColliders [i].transform.position + Vector3.up/2, direction + Vector3.up/2);
						if (Physics.Raycast (hitColliders [i].transform.position + Vector3.up/2, direction + Vector3.up/2,  out hit, dis)) { //up para que no este muy cerca del suelo
							if (hit.collider.gameObject.isStatic) {*/
//								Debug.Log ("Entro22222222222222");
						Vector3 direction =enemyToAtack.transform.position-hitColliders[i].transform.position;
						Vector3 forward = hitColliders [i].transform.forward;
						float angle = Vector3.Angle (direction, forward);
						if(angle<110f*.5f){
							
						
								if (!avaliableCover.Contains (hitColliders [i].transform)) {
									avaliableCover.Add (hitColliders [i].transform);
								}
							}
						}
					}
				}
			}
		else {//si no hay covertruras
			//attack
			//Debug.Log("no colliders");
		}

	}

	void takeCover(){
		if (closestCover == null) {
			/*objMc= closestCover.GetComponent<objetodeCovertura> ();
			if (objMc.owner != this.gameObject) {
			
			}*/
			if (avaliableCover.Count > 0) {
				Transform clCover = avaliableCover [0];

				float distance = Vector3.Distance (transform.position, avaliableCover [0].transform.position);
				for (int i = 0; i < avaliableCover.Count; i++) {
					//avaliableCover [i].GetComponent<objetodeCovertura> ().owner = this.gameObject;
					if (avaliableCover [i].GetComponent<objetodeCovertura> ().owner != this.gameObject) {
						float tempDistance = Vector3.Distance (transform.position, avaliableCover [i].transform.position);
						if (tempDistance < distance) {
							clCover = avaliableCover [i];//covertura mas cercana
							distance = tempDistance;

						}
						if (i == avaliableCover.Count - 1) {
							closestCover = clCover.gameObject;
							clCover.GetComponent<objetodeCovertura> ().owner = this.gameObject; 
						}

					}
				}
			} else {
				FindCoves ();
			}
		} else {
			moveTO (closestCover.transform.position, false, enemyToAtack.transform.position);
		}
	}

	void inCOver(){
		agent.Stop ();

		//anim.SetFloat ("girar", 0);
		anim.SetFloat ("avanzar", 0);
		transform.LookAt(lastKnowPosition);
		//Debug.Log("covertura  "+takingCOver);
		anim.SetBool ("covertura", takingCOver);
	//transform.rotation = closestCover.transform.rotation;
		//anim.SetLayerWeight (2, 0f);
		if (!attacking) {
			
			atackTimer += Time.deltaTime;
			if (atackTimer > atackRate) {
				takingCOver = false;
				attacking = true;
				atackTimer = 0;

			} else {
				//takingCOver = true;
			}
		} else {
			
			//takingCOver = true;
			anim.SetBool ("apuntar", true);
			atackTimer += Time.deltaTime;
			if(atackTimer>atackRate/2){
				if (InSIght) {
					Attack ();
				} else {
					Debug.Log ("ningun enemigo a la vista");
				}

			}
		}
		if(timesFired > 3){
			attacking = false;
			takingCOver = true;
			timesFired = 0;
			anim.SetBool ("apuntar", false);
		}
	}

	void OnAnimatorIK(){
		if (enemyToAtack) {
			//anim.SetLookAtWeight (1, 1, 1, 1, 1,0);
			//Debug.Log("enemigo a atatacar");
			anim.SetLookAtPosition (lastKnowPosition -(new Vector3 (0, 1, 0)));
		
		} else {
			anim.SetLayerWeight (0, 0);
		}
	}

	void shootRay(){
		//Debug.Log("ENIMGO:Disparo");

		RaycastHit hit;
		GameObject go = Instantiate (weponManger.BulletPrefab, Vector3.zero, Quaternion.identity)as GameObject;
		//Debug.Log ("instanciado en "+transform.position);
		LineRenderer line = go.GetComponent<LineRenderer> ();
		Vector3 startPos = weponManger.activeWeapon.bulletSpawn.TransformPoint(Vector3.zero);
		Vector3 EndPos = Vector3.zero;
		int mask=~(1<<10);//layers
		//Vector3 directionNueva = enemyToAtack.transform.position ;//para que apunte mas abajo 

		Vector3 directionToAttack = (lastKnowPosition- (new Vector3 (0, 1, 0)) - transform.position);
		if(Physics.Raycast(startPos,directionToAttack,out hit,Mathf.Infinity,mask)){
			float distance=Vector3.Distance(weponManger.activeWeapon.bulletSpawn.transform.position,hit.point);
			RaycastHit[]hits=Physics.RaycastAll(startPos,hit.point-startPos,distance);

			foreach(RaycastHit hit2 in hits){
				if(hit2.transform.GetComponent<Rigidbody>()){
					Vector3 direction = hit2.transform.position-transform.position;
					direction = direction.normalized;
					hit2.transform.GetComponent<Rigidbody>().AddForce(direction*200);

				}
				//si es destructible
				/*else if(hit2.transform.transform.GetComponent<Destructible>()){
					hit2.transform.GetComponent<Destructible>().destruct=true;
				}*/
				/*else if(hit2.transform.transform.GetComponent<statsDePersonajes>()){

				}*/
				if (hit2.transform.root.GetComponent<statsDePersonajes> ()) {
					statsDePersonajes sta = hit2.transform.root.GetComponent<statsDePersonajes> ();
					sta.health -= 50;
				}
			}
			EndPos=hit.point;
		}
		line.SetPosition (0, startPos);
		line.SetPosition (1, EndPos);
		weponManger.activeWeapon.curAmmo--;
	}

static bool compararArma(controlDeArmasEnemigo armaE,controlDeArmasEnemigo armaJ ){
		bool retVal = false;
		Debug.Log ("Arma del JUGADOR" + armaJ.wepoType.ToString ());
		Debug.Log ("Arma del Enemigo" + armaE.wepoType.ToString());

	if(armaE.wepoType.ToString() == armaJ.wepoType.ToString()){
		//decider
		//posibilidad de atacar del 50 %
		int random=Random.Range(0,11);
		Debug.Log ("tiene la misma Arma");
		if(random<= 5){
			Debug.Log ("tiene la misma Arma y ataco con valor "+random);
				retVal = true;
		}
	}else{
		if (armaE.wepoType == managerDeArmasEnemigo.WeaponType.Rifle) {
			Debug.Log ("rifle en el no jugador");	
			retVal = true;
		}if(armaJ.wepoType.ToString()== managerDeArmasEnemigo.WeaponType.Pistol.ToString()){
			Debug.Log ("el jugador sua una pistola");	
					retVal = true;
			
		}
	}
		return retVal;
	}

static bool compararArma(controlDeArmasEnemigo armaE,controlDeArmasJugador armaJ ){
	bool retVal = false;
	Debug.Log ("Arma del JUGADOR" + armaJ.wepoType.ToString ());
	Debug.Log ("Arma del Enemigo" + armaE.wepoType.ToString());

	if(armaE.wepoType.ToString() == armaJ.wepoType.ToString()){
		//decider
		//posibilidad de atacar del 50 %
		int random=Random.Range(0,11);
		Debug.Log ("tiene la misma Arma");
		if(random<= 5){
			Debug.Log ("tiene la misma Arma y ataco con valor "+random);
			retVal = true;
		}
	}else{
		if (armaE.wepoType == managerDeArmasEnemigo.WeaponType.Rifle) {
			Debug.Log ("rifle en el no jugador");	
			retVal = true;
		}if(armaJ.wepoType.ToString()== managerDeArmasEnemigo.WeaponType.Pistol.ToString()){
			Debug.Log ("el jugador sua una pistola");	
			retVal = true;

		}
	}
	return retVal;
}

static bool tenemosMUnicion(controlDeArmasEnemigo aW){
		if (aW.curAmmo > 0) {
			return true;
		} else {
			return false;
	}
}

static int regresaIntRandmo(){
		int retVal = Random.Range (0, 11);
		return retVal;
}

void RecargarArmaActiva(){
		int cur = weponManger.activeWeapon.curAmmo-weponManger.activeWeapon.MaxAmmo;
		if (cur > 0) {
		if (cur > weponManger.activeWeapon.MaxClipAmmo) {
			weponManger.activeWeapon.curAmmo = weponManger.activeWeapon.MaxClipAmmo;
			weponManger.activeWeapon.MaxAmmo -= weponManger.activeWeapon.MaxClipAmmo;
			}
		} else {
		weponManger.activeWeapon.curAmmo = cur;
		weponManger.activeWeapon.MaxAmmo -= cur;
			anim.SetTrigger ("recargar");
	}
}

void LookForEnemy(){


	
		RaycastHit hit;
		Vector3 directionNueva = enemyToAtack.transform.position - (new Vector3 (0, 1, 0));//para que apunte mas abajo 
	Vector3 direction = directionNueva- transform.position;
	Debug.DrawRay (weponManger.activeWeapon.bulletSpawn.TransformPoint(Vector3.zero),direction, Color.red, 0.2f);
	if (Physics.Raycast (weponManger.activeWeapon.bulletSpawn.TransformPoint(Vector3.zero), direction, out hit)) {
			if (hit.transform.root.GetComponent<statsDePersonajes> ()) {
				if (hit.transform.root.GetComponent<statsDePersonajes> ().Id != GetComponent<statsDePersonajes> ().Id) {
					Debug.Log ("en vision");
					lastKnowPosition = enemyToAtack.transform.position;
					InSIght = true;
				} else {
					
				}
			} else {
				InSIght = false;
			}
		} else {
		InSIght = false;
	}
}
}
