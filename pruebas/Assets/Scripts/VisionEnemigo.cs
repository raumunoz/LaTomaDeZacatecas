using UnityEngine;
using System.Collections;

public class VisionEnemigo : MonoBehaviour {
	statsDePersonajes charStat;
	IAEnemigo enAi;

	// Use this for initialization
	void Start () {
		//charStat = GetComponentInParent<statsDePerosnaje> ();
		enAi = GetComponentInParent<IAEnemigo>();
		charStat=GetComponentInParent<statsDePersonajes>();
	}
	void OnTriggerEnter(Collider other){
		//Debug.Log("contactooooooooooooooooooooooo");
		if(other.GetComponent<statsDePersonajes>()){
			//Debug.Log("Personaje");
			if(other.GetComponent<statsDePersonajes>().Id != charStat.Id){
				
				if(!enAi.Enemies.Contains(other.gameObject)){
					//Debug.Log("Personaje");
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
