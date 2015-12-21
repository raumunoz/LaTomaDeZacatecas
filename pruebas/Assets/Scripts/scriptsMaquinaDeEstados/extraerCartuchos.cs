using UnityEngine;
using System.Collections;

public class extraerCartuchos : StateMachineBehaviour {
	private int casquillos;
	private GameObject cartucho6;
	SkinnedMeshRenderer m;
	private GameObject cartucho0;
	SkinnedMeshRenderer m0;
	 //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		casquillos = animator.GetInteger("casquillos")-1;
		animator.SetInteger ("casquillos", casquillos);
		cartucho6 = GameObject.Find ("/JugadorFps/FirstPersonCharacter/armas/animacionColt44-40/cartucho_006");
		m =cartucho6.GetComponent<SkinnedMeshRenderer>();
		m.enabled = false;
		//cartucho6 = GameObject.Find ("/FPSController/FirstPersonCharacter/armas/animacionColt44-40/cartucho_006");
		//cartucho6.GetComponent<Renderer>().enabled = true;
		cartucho0 = GameObject.Find ("/JugadorFps/FirstPersonCharacter/armas/animacionColt44-40/cartucho_000");
		m0 =cartucho0.GetComponent<SkinnedMeshRenderer>();
		m0.enabled = true;

	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
		//cartucho6.GetComponent<Renderer>().enabled = false;
		//cartucho6.SetActive(false);
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
