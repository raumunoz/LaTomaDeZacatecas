using UnityEngine;
using System.Collections;



public class camaraMira : MonoBehaviour {
	public mira miraActivas;
	public float miraMovedizaOffset;

	// Use this for initialization
	void Start () {
		cambiaMIra ();
	}


	public void cambiaMIra(){
//		miraActivas= GameObject.FindGameObjectWithTag("managerMIra").GetComponent<mangaerMira>().miraActiva;
	}
	public void miraMovedizaCamara(){
		miraActivas.miraMovediza ();
	}

}
