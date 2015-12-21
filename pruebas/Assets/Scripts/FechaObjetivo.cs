using UnityEngine;
using System.Collections;

public class FechaObjetivo : MonoBehaviour {
	public GameObject sierpe, grillo, bufa, palacio, explocion;
	private bool objetivo1, objetivo2;
	// Use this for initialization
	void Start () {
		objetivo1 = false;
		objetivo2 = false;
	}
	
	// Update is called once per frame
	void Update () {
		if ((!objetivo1) && (Mathf.Sqrt((transform.position.x-sierpe.transform.position.x)*(transform.position.x-sierpe.transform.position.x)+(transform.position.y-sierpe.transform.position.y)*(transform.position.y-sierpe.transform.position.y))<100)) {
			objetivo1=true; 
		}
		if ((!objetivo2) && (Mathf.Sqrt((transform.position.x-grillo.transform.position.x)*(transform.position.x-grillo.transform.position.x)+(transform.position.y-grillo.transform.position.y)*(transform.position.y-grillo.transform.position.y))<3)) {
			objetivo2=true; 
		}
		if(objetivo2){
			Destroy(this.gameObject);
		}
		if (!objetivo1) {
			Debug.Log("sierpe: "+Mathf.Sqrt((transform.position.x-sierpe.transform.position.x)*(transform.position.x-sierpe.transform.position.x)+(transform.position.y-sierpe.transform.position.y)*(transform.position.y-sierpe.transform.position.y))); 
			this.gameObject.transform.LookAt (sierpe.transform);
		} else {
			if(!objetivo2){
				Debug.Log("grillo: "+Mathf.Sqrt((transform.position.x-grillo.transform.position.x)*(transform.position.x-grillo.transform.position.x)+(transform.position.y-grillo.transform.position.y)*(transform.position.y-grillo.transform.position.y)));
				this.gameObject.transform.LookAt (grillo.transform);
			}else{
				this.gameObject.transform.LookAt (bufa.transform);
				Instantiate(explocion,palacio.transform.position,palacio.transform.rotation);
				Destroy(palacio);
			}
		}
	}
}
