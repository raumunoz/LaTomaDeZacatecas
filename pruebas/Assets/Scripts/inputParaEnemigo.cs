using UnityEngine;
using System.Collections;

public class inputParaEnemigo : MonoBehaviour {

	public bool walkByDefault = false;


	private movimientoEnemigo character;
	private Transform cam;
	private Vector3 camForward;
	private Vector3 move;
	public bool aim;
	public float aiminWaight;

	Animator anim;
	//float autoTUrnTreshold=10;
	//float autoTurnSpeed=20;
	//bool aim;
	public bool lookIncameraDirection;
	Vector3 lookPos;
	//Vector3 currentLookPosition;
	//IK sTUff 
	public Transform spine;
	public float aimingZ = 213.46f;
	public float aimingX = -65.93f;
	public float aimingY = 20.1f;
	public float point = 30;

	public ParticleSystem particula;
	void Start()
	{
		anim = GetComponent<Animator> ();
		if (Camera.main != null)
		{
			cam = Camera.main.transform;
		}
		
		character = GetComponent<movimientoEnemigo>();
	}
	void Update(){
		aim = !Input.GetMouseButton(1);
		if(aim && Input.GetMouseButton(0)){
			anim.SetTrigger("disparar");
			particula.Emit(1);
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

	void LateUpdate(){
		//solo auqi podemos modificar la animacion
		aiminWaight = Mathf.MoveTowards (aiminWaight, (aim) ? 1.0f : 0.0f, Time.deltaTime * 5);
		Vector3 normalState = new Vector3 (0, 0, -2);
		Vector3 aimingState = new Vector3 (0, 0, -1f);
		Vector3 pos = Vector3.Lerp (normalState, aimingState, aiminWaight);
		cam.transform.localPosition=pos;
		if (aim) {
			Vector3 eulerAngleOffset=Vector3.zero;
			eulerAngleOffset=new Vector3(aimingX,aimingY,aimingZ);//
			Ray ray = new Ray(cam.position,cam.forward);
			Vector3 lookPosition=ray.GetPoint(point);//mira 30 puntos adelante de la camara
			spine.LookAt(lookPosition);
			spine.Rotate(eulerAngleOffset,Space.Self);
		}
	}
	void onAwake(){
		


	}
}
