using UnityEngine;
using System.Collections;

public class introducirCartuchos : StateMachineBehaviour {
	private int cartuchos;
	private GameObject cartucho;
	private SkinnedMeshRenderer m;
	private GameObject cartucho0;
	SkinnedMeshRenderer m0;
	private controlDeArmasJugador controlArma;
	//private GameObject cartucho1;
	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//cartuchos = animator.GetInteger("cartuchos")+1;
		controlArma =GameObject.Find ("/JugadorFps/FirstPersonCharacter/armas/animacionColt44-40").GetComponent<controlDeArmasJugador> ();
		cartucho = GameObject.Find ("/JugadorFps/FirstPersonCharacter/armas/animacionColt44-40/cartucho_006");
		m =cartucho.GetComponent<SkinnedMeshRenderer>();

		cartucho0 = GameObject.Find ("/JugadorFps/FirstPersonCharacter/armas/animacionColt44-40/cartucho_000");
		m0 =cartucho0.GetComponent<SkinnedMeshRenderer>();
		controlArma.municionActual++;
		controlArma.dismiNuirParque ();
		m0.enabled = false;
		m.enabled = true;

		//cartucho.SetActive(false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetInteger ("cartuchos", controlArma.municionActual);
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
