using UnityEngine;
using System.Collections;

public class GaleriaArma : MonoBehaviour {
	// Use this for initialization
	private float a;
	public Texture[] tex =new Texture2D[10];
	private Arma[] armas=new Arma[10]; 
	void Start () {
		a=0;
		agregarArmas();
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	void OnGUI(){
		a=GUI.HorizontalSlider(new Rect(Screen.width/22*10,Screen.height/19*1,Screen.width/22*10,Screen.height/19),a,0,armas.Length-1);
		int i=(int)a;
		a = (float)i;
		GUI.DrawTexture(new Rect(Screen.width/22,Screen.height/19,Screen.width/22*8,Screen.height/19*8), tex[i]);
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*2,Screen.width/22*10,Screen.height/19),armas[i].getNombre());
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*4,Screen.width/22*10,Screen.height/19),"Largo: ");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*5,Screen.width/22*10,Screen.height/19),armas[i].getLargo()+"mm");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*7,Screen.width/22*10,Screen.height/19),"Largo del cañon: ");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*8,Screen.width/22*10,Screen.height/19),armas[i].getLargoCanon()+"mm");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*10,Screen.width/22*10,Screen.height/19),"Accion: ");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*11,Screen.width/22*10,Screen.height/19),armas[i].getAccion());
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*13,Screen.width/22*10,Screen.height/19),"Calibre: ");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*15,Screen.width/22*10,Screen.height/19),armas[i].getCalibre());
		GUI.Box(new Rect(Screen.width/22,Screen.height/19*10,Screen.width/22*8,Screen.height/19),"Otros datos: ");
		GUI.Box(new Rect(Screen.width/22,Screen.height/19*11,Screen.width/22*9,Screen.height/19*6),armas[i].getOtrosDatos());
		if (GUI.Button(new Rect(Screen.width/22*16,Screen.height/19*16,Screen.width/22*4,Screen.height/19*2),"!Atras")) {
			Application.LoadLevel(2);
		}

	}
	private void agregarArmas(){
		armas[0]=new Arma("Saint Chamond-Mondragón",960,960,"Palanca","75 mm L/28.5","Projectiles:HE 7.24kg , He 5.5kg \nAlcanze:6500mm \nCadencia de disparo: 15 - 18 disparos/min");
		armas[1]=new Arma("Colt Model 1883 Double Barrel Shotgun",4257,2452,"Sencilla","44 S&W5","");
		armas[2]=new Arma("Winchester Modelo 1894",1223,93260,"Palanca","30-30 Winchester","Capacidad: 7 cartuchos");
	}
}
