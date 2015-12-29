using UnityEngine;
using System.Collections;

public class introducirCartuchosWinchester : StateMachineBehaviour {
	private int cartuchos;
	private controlDeArmasJugador controlArma;
	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
		controlArma =GameObject.Find ("Animacionwinchester_94_30-30").GetComponent<controlDeArmasJugador> ();
		cartuchos = animator.GetInteger("cartuchos")+1;
		animator.SetInteger ("cartuchos", cartuchos);
		//controlArma.muestraMunicion ();
		controlArma.municionActual++;
		controlArma.dismiNuirParque ();
		//controlArma.muestraMunicion ();
		//controlArma.muestraMunicion ();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetInteger ("reservas", controlArma.municionReservas);
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
		controlArma.muestraMunicion();
	}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
