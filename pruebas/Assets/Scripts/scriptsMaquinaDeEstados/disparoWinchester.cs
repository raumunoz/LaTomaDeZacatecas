using UnityEngine;
using System.Collections;

public class disparoWinchester : StateMachineBehaviour {
	//private int casquillos;
	private disparosDelJugador disparo;
	controlDeArmasJugador controlArma;
	private Transform posicionHueso;
	private GameObject[] objetin;
	//public GameObject flash;
	Animator anim;
	public GameObject esfera;
	//	private AnotherScript anotherScript;
	//private GameObject otherScript;
	//OtherScript = GetComponent(OtherScript);
	//public GameObject objectToAccess;
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//Instantiate(flash,);
//		casquillos = animator.GetInteger ("casquillos");
		anim = GameObject.Find ("Animacionwinchester_94_30-30").GetComponent<Animator> ();
		controlArma=GameObject.Find ("Animacionwinchester_94_30-30").GetComponent<controlDeArmasJugador> ();
		disparo = GameObject.Find ("armas").GetComponent<disparosDelJugador> ();
		//GameObject Object1 = GameObject.Find ("armas");
		//Component anotherScript = Object1.GetComponent<disparosDelJugador> ();
		disparo.disparo(controlArma.danio);
		controlArma.disparar ();
		objetin = GameObject.FindGameObjectsWithTag ("canionWinchester");

		posicionHueso = objetin [0].transform;
		Instantiate (esfera, posicionHueso.position,posicionHueso.rotation);
		//		Script1.disparo ();
		
		
	}
	
	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
	
	//OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//		animator.SetInteger ("casquillos",casquillos+1);
		controlArma.muestraMunicion ();
		anim.SetInteger ("cartuchos", controlArma.municionActual);
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
