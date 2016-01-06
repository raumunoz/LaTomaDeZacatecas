using UnityEngine;
using System.Collections;

public class inputParaEnemigo : MonoBehaviour {

	public bool walkByDefault = false;

	public Transform espina;
	private movimientoEnemigo character;
	private Transform cam;
	private Vector3 camForward;
	private Vector3 move;
	public bool aim;
	public float aiminWaight;
	managerDeArmasEnemigo weaponManager;
	//customIK cusIK;
	public GameObject bulletPrefab;
	Animator anim;
	//float autoTUrnTreshold=10;
	//float autoTurnSpeed=20;
	//bool aim;
	public bool lookIncameraDirection;
	Vector3 lookPos;
	//Vector3 currentLookPosition;
	//IK sTUff 
	[SerializeField]public IK ik;
	[System.Serializable] public class IK{
	public Transform spine;
	public float aimingZ = 213.46f;
	public float aimingX = -65.93f;
	public float aimingY = 20.1f;
	public float point = 30;
		public	bool debugAim;
	}
	//public ParticleSystem particula;
	void Start()
	{
		anim = GetComponent<Animator> ();
		if (Camera.main != null)
		{
			cam = Camera.main.transform;
		}
		
		character = GetComponent<movimientoEnemigo>();
		//cusIK = GetComponentInChildren<customIK>();
		weaponManager = GetComponent<managerDeArmasEnemigo> ();
	}

	void Update(){
		if(!ik.debugAim)
		aim = Input.GetMouseButton (1);
		
		//aim = Input.GetMouseButton(1);
		if(aim){
			anim.SetTrigger ("apuntar");
			//ShotRay ();
		}
		if(aim && Input.GetMouseButtonDown(0)){
			if(weaponManager.activeWeapon.canBurts){//problema
			anim.SetTrigger("disparar");
			ShotRay ();
			weaponManager.FireActiveWrapon ();
			//particula.Emit(1);	
			}else{
				if (Input.GetMouseButtonDown (0)) {
					anim.SetTrigger("disparar");
					weaponManager.FireActiveWrapon ();

				}
			}
		}
		//if(Input.GetAxis("Mouse ScrollWheel")!=0){
		if(Input.GetKey(KeyCode.Q)){
				weaponManager.changeWeapon (false);
			}
		if(Input.GetKey(KeyCode.E)){
				weaponManager.changeWeapon (true);
			}


	}

	void FixedUpdate()
	{

		float horizontal = Input.GetAxis ("Horizontal");
		float vertical = Input.GetAxis ("Vertical");
		if (!aim) {
			if (cam != null) {
				camForward = Vector3.Scale (cam.forward, new Vector3 (1, 0, 1)).normalized;
				move = vertical * camForward + horizontal * cam.right;
			} else {
				move = vertical * Vector3.forward + horizontal * Vector3.right;
			}
		
		} else {//si esta esta apuntando
			move=Vector3.zero;
			Vector3 dir=lookPos-transform.position;
			dir.y=0;

			transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(dir),10*Time.deltaTime);//mirar dpdme lacamarea este

			anim.SetFloat ("avanzar",vertical);
			anim.SetFloat ("girar",horizontal);
		}

		if (move.magnitude > 1)
			move.Normalize();
		
		bool walkToggle = Input.GetKey(KeyCode.LeftShift)|| aim;
		
		float walkMultiplier = 1;
		
		if (walkByDefault)
		{
			if (walkToggle)
			{
				walkMultiplier = 1;
			}
			else
			{
				walkMultiplier = 0.5f;
			}
		}
		else
		{
			if (walkToggle)
			{
				walkMultiplier = 0.5f;
			}
			else
			{
				walkMultiplier = 1;
			}
		}
	lookPos = (lookIncameraDirection && cam != null) ? transform.position + cam.forward * 100 : transform.position + transform.forward * 100;
	move *= walkMultiplier;
	character.Move(move,aim,lookPos);
	}
		
	void ShotRay(){
		float x = Screen.width / 2;
		float y = Screen.height / 2;
		Ray ray = Camera.main.ScreenPointToRay (new Vector3 (x, y, 0));
		RaycastHit hit;
		GameObject go = Instantiate (bulletPrefab, Vector3.zero, Quaternion.identity)as GameObject;
		LineRenderer line = go.GetComponent<LineRenderer> ();
		Vector3 starPos = weaponManager.activeWeapon.bulletSpawn.TransformPoint (Vector3.zero);
		Vector2 endPos = Vector3.zero;
		if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
			float distance = Vector3.Distance (weaponManager.activeWeapon.bulletSpawn.transform.position, hit.point);
			RaycastHit[] hits = Physics.RaycastAll (starPos, hit.point - starPos, distance);
			foreach (RaycastHit hit2 in hits) {
				if (hit2.transform.GetComponent<Rigidbody> ()) {
					Vector3 direction = hit2.transform.position - transform.position;
					direction = direction.normalized;
					hit2.transform.GetComponent<Rigidbody> ().AddForce (direction * 200);
				} /*else {
					
				}*/


			}
			endPos = hit.point;

		} else {
			endPos = ray.GetPoint (100);
		}
		line.SetPosition (0, starPos);
		line.SetPosition (1, endPos);
	}

	void LateUpdate(){
		//solo auqi podemos modificar la animacion
		aiminWaight = Mathf.MoveTowards (aiminWaight, (aim) ? 1.0f : 0.0f, Time.deltaTime * 5);
		Vector3 normalState = new Vector3 (0, 0, -2);
		Vector3 aimingState = new Vector3 (0, 0, -1f);
		Vector3 pos = Vector3.Lerp (normalState, aimingState, aiminWaight);
		cam.transform.localPosition=pos;
		if (aim) {
			Vector3 eulerAngleOffset=Vector3.zero;
			eulerAngleOffset=new Vector3(ik.aimingX,ik.aimingY,ik.aimingZ);//
			Ray ray = new Ray(cam.position,cam.forward);
			Vector3 lookPosition=ray.GetPoint(ik.point);//mira 30 puntos adelante de la camara
			ik.spine.LookAt(lookPosition);
			ik.spine.Rotate(eulerAngleOffset,Space.Self);
		}
	}

}
