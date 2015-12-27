using UnityEngine;
using System.Collections;

public class movimientoEnemigo : MonoBehaviour {

	float moveSpeedMultiplier = 1;
	float stationaryTurnSpeed = 20; 
	float movingTurnSpeed = 360;
	
	public bool onGround; 
	
	Animator animator;

	public Transform leftHand;
	Vector3 moveInput; 
	float turnAmount; 
	float forwardAmount; 
	Vector3 velocity;
	float jumpPower = 10;
	
	IComparer rayHitComparer;
	Rigidbody rigy;

	float autoTurnThreshold=10;
	float autoTurnSpeed=20;
	bool aim;
	Vector3 currentLookPosition;
	
	void Start()
	{
		animator = GetComponentInChildren<Animator>();
		leftHand = animator.GetBoneTransform (HumanBodyBones.LeftHand);
		SetUpAnimator();
		
		  
	}
	
	void SetUpAnimator()
	{
		animator = GetComponent<Animator>();
		
		foreach (var childAnimator in GetComponentsInChildren<Animator>())
		{
			if (childAnimator != animator)
			{
				animator.avatar = childAnimator.avatar;
				Destroy(childAnimator);
				break; //si se encontro el primer animator deja de buscar
			}
		}

	}
	
	void OnAnimatorMove()
	{
		if (onGround && Time.deltaTime > 0)
		{
			Vector3 v = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime; //calcula la velocidad
			v.y = GetComponent<Rigidbody>().velocity.y;
			GetComponent<Rigidbody>().velocity = v;
		}
	}
	
	public void Move(Vector3 move,bool aim,Vector3 lookPos)
	{    
		if (move.magnitude > 1)
			move.Normalize();
		
		this.moveInput = move; 
		this.aim = aim;
		this.currentLookPosition = lookPos;

		velocity = GetComponent<Rigidbody>().velocity; 
		
		ConvertMoveInput();
		if (!aim) {
			ApplyExtraTurnRotation();
			turnTowarsdCamaraFoward ();
		}



		GroundCheck ();
		UpdateAnimator();
		
	}
	
	void ConvertMoveInput()
	{
		Vector3 localMove = transform.InverseTransformDirection(moveInput);
		
		turnAmount = Mathf.Atan2(localMove.x, localMove.z);
		
		forwardAmount = localMove.z;
	}
	
	void ApplyExtraTurnRotation()
	{
		float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
		transform.Rotate(0, (turnAmount * turnSpeed * Time.deltaTime), 0);
	}
	
	void UpdateAnimator()
	{ 
		if(!aim){
		animator.applyRootMotion = true;
		animator.SetFloat("avanzar", forwardAmount, 0.1f, Time.deltaTime);
		animator.SetFloat("girar", turnAmount, 0.1f, Time.deltaTime);
		animator.SetBool ("apuntar", aim);
		}
	}
	
	void GroundCheck()
	{
		Ray ray = new Ray(transform.position + Vector3.up * .1f, -Vector3.up);
		
		RaycastHit[] hits = Physics.RaycastAll(ray, .1f);
		rayHitComparer = new RayHitComparer();
		System.Array.Sort(hits, rayHitComparer);
		
		if (velocity.y < jumpPower * .5f)//asume que el personaje esta en el aire
		{ 	
			onGround = false;
			GetComponent<Rigidbody>().useGravity = true;
			
			foreach (var hit in hits)
			{ 
				if (!hit.collider.isTrigger)
				{
					if (velocity.y <= 0)
					{
						GetComponent<Rigidbody>().position = Vector3.MoveTowards(GetComponent<Rigidbody>().position, hit.point, Time.deltaTime * 100);
					}
					
					onGround = true; 
					GetComponent<Rigidbody>().useGravity = false; 
				}
			}
		}
	}
	void turnTowarsdCamaraFoward(){
		if (Mathf.Abs (forwardAmount) < .01f) {
			Vector3 lookDelta=transform.InverseTransformDirection(currentLookPosition-transform.position);
			float lookAngle=Mathf.Atan2(lookDelta.x,lookDelta.z)*Mathf.Rad2Deg;
			if(Mathf.Abs(lookAngle)>autoTurnThreshold){
				turnAmount+=lookAngle*autoTurnSpeed*.001f;
			}
		}
	}
	
	class RayHitComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
			/*esto regresa si <0 si x <y
			 > 0 si x=y*/
		}
	}

}



