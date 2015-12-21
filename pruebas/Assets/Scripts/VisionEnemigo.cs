using UnityEngine;
using System.Collections;

public class VisionEnemigo : MonoBehaviour {
	statsDePersonajes charStat;
	IAEnemigo enAi;

	// Use this for initialization
	void Start () {
		//charStat = GetComponentInParent<statsDePerosnaje> ();
		enAi = GetComponentInParent<IAEnemigo>();
	}
	void OnTriggerEnter(Collider other){
		Debug.Log("contactooooooooooooooooooooooo");
		if(other.GetComponent<statsDePersonajes>()){

			if(other.GetComponent<statsDePersonajes>().Id != charStat.Id){
				if(enAi.Enemies.Contains(other.gameObject)){
					enAi.Enemies.Add(other.gameObject);//agrega ala lista de enemigos
				}
			}
		}
	}

	void OnTriggerExit(Collider other){
		if(enAi.Enemies.Contains(other.gameObject)){
			enAi.Enemies.Remove(other.gameObject);
		}

	}

}
