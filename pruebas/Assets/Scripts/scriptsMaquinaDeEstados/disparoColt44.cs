using UnityEngine;
using System.Collections;

public class disparoColt44 : StateMachineBehaviour {
	private int casquillos;
	private disparosDelJugador disparo;
	private GameObject canion;
	private Transform posicion;
	Animator anim;
	private Transform posicionHueso;
	private GameObject[] objetin;
	public GameObject esfera;
	private GameObject cartucho;
	private SkinnedMeshRenderer m;
	 controlDeArmasJugador controlArma;
//	private camaraMira funcionesDeCamara;
//	private AnotherScript anotherScript;
	//private GameObject otherScript;
	//OtherScript = GetComponent(OtherScript);
	//public GameObject objectToAccess;
	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//funcionesDeCamara = Camera.main.transform.root.GetComponent<camaraMira>();
		controlArma=GameObject.Find ("/JugadorFps/FirstPersonCharacter/armas/animacionColt44-40").GetComponent<controlDeArmasJugador> ();
		cartucho = GameObject.Find ("/JugadorFps/FirstPersonCharacter/armas/animacionColt44-40/cartucho_006");
		disparo = GameObject.Find ("armas").GetComponent<disparosDelJugador> ();
		anim = GameObject.Find ("animacionColt44-40").GetComponent<Animator> ();
		m =cartucho.GetComponent<SkinnedMeshRenderer>();
		objetin = GameObject.FindGameObjectsWithTag ("canionColt");
		m.enabled = false;
	
		posicionHueso = objetin [0].transform;
		//Instantiate(flash,);
		casquillos = animator.GetInteger ("casquillos");

		//GameObject Object1 = GameObject.Find ("armas");
		//Component anotherScript = Object1.GetComponent<disparosDelJugador> ();

		disparo.disparo (3);
		controlArma.disparar ();



		//Transform bone = GetComponent<Animator>().avatar.GetBone(BoneType.LeftShoulder);


//		Debug.Log("Posicion del hgueso -----------------"+posicionHueso.position+"----------------------");
		//funcionesDeCamara.miraMovedizaCamara ();
		Instantiate (esfera, posicionHueso.position,posicionHueso.rotation);

//		Script1.disparo ();
		/*abtenemos posicion del 
		posicion = GameObject.Find ("/FPSController/FirstPersonCharacter/armas/animacionColt44-40/marco").GetComponent<Transform>();
		Debug.Log ("+++++++++++ la poscion del flash es:   " + posicion.position + "+++++++++++++++++++");canion*/

		/*obtenemos un hueso*/


	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	//OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetInteger ("casquillos",casquillos+1);
		anim.SetInteger ("cartuchos",controlArma.municionActual);
		controlArma.muestraMunicion ();
		m.enabled = true;
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
