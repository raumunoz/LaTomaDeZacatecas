using UnityEngine;
using System.Collections;

public class FechaObjetivo : MonoBehaviour {
	public GameObject sierpe, grillo, bufa, palacio, explocion;
	private bool objetivo1, objetivo2;
	private float ex;
	private bool ex1;
	// Use this for initialization
	void Start () {
		ex1 = true;
		objetivo1 = false;
		objetivo2 = false;
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log (Application.loadedLevel);
		if ((!objetivo1) && (Mathf.Sqrt((transform.position.x-sierpe.transform.position.x)*(transform.position.x-sierpe.transform.position.x)+(transform.position.z-sierpe.transform.position.z)*(transform.position.z-sierpe.transform.position.z))<160)) {
			objetivo1=true;
			Application.LoadLevel(9);
		}
		if ((!objetivo2) && (Mathf.Sqrt((transform.position.x-grillo.transform.position.x)*(transform.position.x-grillo.transform.position.x)+(transform.position.z-grillo.transform.position.z)*(transform.position.z-grillo.transform.position.z))<90)) {
			objetivo2=true; 
			Application.LoadLevel(10);
			ex=Time.time;
		}
		/*if(objetivo2){
			Destroy(this.gameObject);
		}*/
		if (!objetivo1) {
			//Debug.Log("sierpe: "+Mathf.Sqrt((transform.position.x-sierpe.transform.position.x)*(transform.position.x-sierpe.transform.position.x)+(transform.position.y-sierpe.transform.position.y)*(transform.position.y-sierpe.transform.position.y))); 
			this.gameObject.transform.LookAt (sierpe.transform);

		} else {
			if(!objetivo2){
			//	Debug.Log("grillo: "+Mathf.Sqrt((transform.position.x-grillo.transform.position.x)*(transform.position.x-grillo.transform.position.x)+(transform.position.y-grillo.transform.position.y)*(transform.position.y-grillo.transform.position.y)));
				this.gameObject.transform.LookAt (grillo.transform);
			}else{
				this.gameObject.transform.LookAt (bufa.transform);
				Debug.Log(Time.time-ex);
				if((Time.time-ex>30)&&ex1){
					Debug.Log("ex1");
					Destroy(palacio);
					Instantiate(explocion,palacio.transform.position,palacio.transform.rotation);
					ex1=false;
				}
				if(Time.time-ex>40){
					//carga video final
					Application.LoadLevel(12);
				}
			}
		}
	}
}
