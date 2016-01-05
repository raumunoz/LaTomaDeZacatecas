using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class managerDeArmasjugador : MonoBehaviour {
	
	public List<GameObject>listaDeArmas=new List<GameObject>();
	public controlDeArmasJugador armaActiva;
	Animator anim;

	//public Canvas texto;
	 //GUIText balas;

	public enum WeaponType
	{
		Pistol,Rifle
	}
	public WeaponType weaponTypes;
	// Use this for initialization
	void Start () {
		
		//balas.text="HOLAS";

		foreach (GameObject go in listaDeArmas) {
			go.GetComponent<controlDeArmasJugador> ().tineDuenio= true;
			//Debug.Log ("ARMA" + go.GetComponent<controlDeArmasJugador> ().wepoType);
			if(go.activeSelf){
				armaActiva = go.GetComponent<controlDeArmasJugador> ();
			}
		}

		//armaActiva = listaDeArmas [1].GetComponent<controlDeArmasJugador> ();
		weaponTypes = armaActiva.wepoType;
//		weaponType = armaActiva.wepoType;

	}
	
	// Update is called once per frame
	void Update () {
		
		if( Input.GetKeyDown( "1" ) )
		{
			//yield WaitForSeconds (.5f);
			selecionArma(0);


		}
		if( Input.GetKeyDown( "2" ) )
		{
			//yield WaitForSeconds (.55f);
			selecionArma(1);

		}  

	}
	void selecionArma(int indice){
		armaActiva.GetComponent<Animator> ().SetBool ("salir", true);
		armaActiva.equipado = false;
		foreach (GameObject go in listaDeArmas) {
			go.SetActive (false);
		}
		listaDeArmas [indice].SetActive (true);
		armaActiva = listaDeArmas [indice].GetComponent<controlDeArmasJugador> ();
		armaActiva.GetComponent<Animator> ().SetInteger ("cartuchos", armaActiva.GetComponent<controlDeArmasJugador> ().municionActual);
		armaActiva.GetComponent<Animator> ().SetInteger ("reservas", armaActiva.GetComponent<controlDeArmasJugador> ().municionReservas);
		//weaponType = armaActiva.wepoType;
		armaActiva.equipado = true;
		armaActiva.muestraMunicion ();
		weaponTypes = armaActiva.wepoType;
	}

}
