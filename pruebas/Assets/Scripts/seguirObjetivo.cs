using UnityEngine;


public abstract class segirObjetivo : MonoBehaviour {//clase que no se pude hacer una instatiation de esta clase solamente usando una subclase 
	[SerializeField] public Transform target;
	[SerializeField] private bool autoTargetPlayer=true;//guartda los datos uanque sean en otra subclase
	// Use this for initialization
	private GameObject targetObjet;
	virtual protected void Start(){
		if (autoTargetPlayer) {
			FindTargetPlayer();
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (autoTargetPlayer && (target == null) || !target.gameObject.activeSelf) {
			FindTargetPlayer();
		}//active self esque si esta  activo
		if (target != null && (target.GetComponent<Rigidbody>() != null && !target.GetComponent<Rigidbody>().isKinematic)) {
			Follow	(Time.deltaTime);	
		}
	}
	protected abstract void Follow (float deltaTime);

	public void FindTargetPlayer(){

		if (target == null) {
			 targetObjet = GameObject.FindGameObjectWithTag("Player");
		}if(targetObjet){
			setTarget(targetObjet.transform);
		}
	}

	virtual protected void setTarget(Transform newTransform){
		target = newTransform;
	}
	public Transform Target{get{return this.target;}}//????
}
