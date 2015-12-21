using UnityEngine;
using System.Collections;

public class GaleriaPersonaje : MonoBehaviour {
	// Use this for initialization
	float a;
	public Texture[] tex =new Texture2D[10];
	Personaje[] personajes=new Personaje[10]; 
	void Start () {
		a=0;
		agregarPersonajes();
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	void OnGUI(){
		a=GUI.HorizontalSlider(new Rect(Screen.width/22*10,Screen.height/19*1,Screen.width/22*10,Screen.height/19),a,0,personajes.Length-1);
		int i=(int)a;
		a = (float)i;
		GUI.DrawTexture(new Rect(Screen.width/22,Screen.height/19,Screen.width/22*8,Screen.height/19*8), tex[i]);
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*2,Screen.width/22*10,Screen.height/19),personajes[i].getNombre());
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*4,Screen.width/22*10,Screen.height/19),"Nacimiento: "+personajes[i].getNac());
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*6,Screen.width/22*10,Screen.height/19),"Datos Interesantes: ");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*7,Screen.width/22*10,Screen.height/19*3),personajes[i].getDatInt());
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*11,Screen.width/22*10,Screen.height/19),"Muerte: "+personajes[i].getFechaMuerte());
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*13,Screen.width/22*10,Screen.height/19),"Causa de Muerte: ");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*14,Screen.width/22*5,Screen.height/19*4),personajes[i].getCausaMuerte());
		GUI.Box(new Rect(Screen.width/22,Screen.height/19*9,Screen.width/22*8,Screen.height/19),"Batallas importantes: ");
		GUI.Box(new Rect(Screen.width/22,Screen.height/19*11,Screen.width/22*8,Screen.height/19*6),personajes[i].getBatInt());
		if (GUI.Button(new Rect(Screen.width/22*16,Screen.height/19*16,Screen.width/22*4,Screen.height/19*2),"!Atras")) {
			Application.LoadLevel(2);
		}

	}
	void agregarPersonajes(){
		personajes[0]=new Personaje("Francisco Villa (Doroteo Arango Arámbula)","5 de junio de 1878","Por la pobreza de sus padres, Agustín Arango y Micaela \nArámbula, no tiene educación escolar. Trabaja de leñador \ny de labrador cuando fallece su padre. Se dedica al comercio, \ncon ayuda de Pablo Valenzuela, que le fía mercancía. ","Toma de Zacatecas\nToma de Torreon"," El viernes 20 de julio de 1923","Tras sufrir varios atentados, \nmuere emboscado en Hidalgo \ndel Parral, Chihuahua");
		personajes[1]=new Personaje("Felipe","1/6/1578","Se baño antes de la batalla","Toma de zacatecas","1/6/1987","YO LO MATE");
		personajes[2]=new Personaje("Carranza","1/4/2048 A.C.","se autonombro primer jefe","NINGUNA \nJAJAJAJAJA","1/6/3845 D.C.","YO LO MATE");
	}
}
