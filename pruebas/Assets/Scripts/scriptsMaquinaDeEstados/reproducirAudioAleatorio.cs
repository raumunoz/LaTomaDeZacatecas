using UnityEngine;
using System.Collections;

public class reproducirAudioAleatorio : StateMachineBehaviour {
	public AudioClip sonido;
	public AudioClip sonido1;
	public AudioClip sonido2;
	public AudioClip sonido3;
	public AudioClip sonido4;
	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		int numero = Random.Range(1,5); 

		switch (numero)
		{
		case 5:
			AudioSource.PlayClipAtPoint (sonido, animator.transform.position,0.5f);
			break;
		case 4:
			AudioSource.PlayClipAtPoint (sonido1, animator.transform.position,0.5f);
			break;
		case 3:
			AudioSource.PlayClipAtPoint (sonido2, animator.transform.position,0.5f);
			break;
		case 2:
			AudioSource.PlayClipAtPoint (sonido3, animator.transform.position,0.5f);
			break;
		case 1:
			AudioSource.PlayClipAtPoint (sonido4, animator.transform.position,0.5f);
			break;
		default:
			AudioSource.PlayClipAtPoint (sonido4, animator.transform.position,0.5f);
			break;
		}





	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
