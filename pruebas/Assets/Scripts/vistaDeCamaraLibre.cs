using UnityEngine;
using UnityEditor;

public class vistaDeCamaraLibre : Pivot {
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float TurnSpeed = 5f;
	[SerializeField] private float turnSmoothing = 5f;
	[SerializeField] private float tiltMax = 75f;
	[SerializeField] private float tiltMin = 45f;//rotacion del pivote
	[SerializeField] private bool lockCurosr = false;

	private float lookAngle;
	private float tiltAngle;
	private const float lookDIstance = 100f; 

	private float smoothx=0;
	private float smoothy=0;
	private float smoothyVeolcity=0;
	private float smoothxVelocity=0;


	// Use this for initialization
	protected override void Awake(){
		base.Awake ();
		Screen.lockCursor = lockCurosr;
		cam = GetComponentInChildren<Camera> ().transform;
		pivot = cam.parent;
		Cursor.visible = true;
	}
	
	// Update is called once per frame
	protected override void Update () {
		HandleRotationMOvement ();
		base.Update ();
		if(lockCurosr && Input.GetMouseButtonUp(0)){
			Screen.lockCursor=lockCurosr;
		}
	}
	protected override void Follow (float deltaTime){//cada subclase debe tener este metodo obligatoraimente
		transform.position = Vector3.Lerp (transform.position, target.position, deltaTime * moveSpeed);
	}
	void HandleRotationMOvement(){
		float x= Input.GetAxis("Mouse X");
		float y= Input.GetAxis("Mouse Y");

		if (turnSmoothing > 0) {
			smoothx = Mathf.SmoothDamp (smoothx, x, ref smoothxVelocity, turnSmoothing);
			smoothy = Mathf.SmoothDamp (smoothy, y, ref smoothyVeolcity, turnSmoothing);
		} else {
			smoothx = x;
			smoothy = y;
		}
		lookAngle += smoothx * TurnSpeed;
		transform.rotation = Quaternion.Euler (0f, lookAngle, 0);
		tiltAngle -= smoothy * TurnSpeed;
		tiltAngle = Mathf.Clamp (tiltAngle, -  tiltMin, tiltMax);
		pivot.localRotation = Quaternion.Euler (tiltAngle, 0, 0);

	}
	void OnDisable(){
		Screen.lockCursor = false;
	}

}
