using UnityEngine;
using System.Collections;

public class GaleriaEdificio : MonoBehaviour {
	// Use this for initialization
	private float a;
	public Texture[] tex =new Texture2D[10];
	private Edificio[] edificios=new Edificio[10]; 
	void Start () {
		a=0;
		agregarEdificios();
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	void OnGUI(){
		a=GUI.HorizontalSlider(new Rect(Screen.width/22*10,Screen.height/19*1,Screen.width/22*10,Screen.height/19),a,0,edificios.Length-1);
		int i=(int)a;
		a = (float)i;
		GUI.DrawTexture(new Rect(Screen.width/22,Screen.height/19,Screen.width/22*8,Screen.height/19*8), tex[i]);
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*2,Screen.width/22*10,Screen.height/19),edificios[i].getNombre());
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*4,Screen.width/22*10,Screen.height/19),"Construccion: ");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*5,Screen.width/22*10,Screen.height/19),edificios[i].getEpocaCon());
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*7,Screen.width/22*10,Screen.height/19),"Constrores: ");
		GUI.Box(new Rect(Screen.width/22*10,Screen.height/19*8,Screen.width/22*10,Screen.height/19*3),edificios[i].getConst());
		GUI.Box(new Rect(Screen.width/22,Screen.height/19*12,Screen.width/22*8,Screen.height/19),"Datos Interesantes: ");
		GUI.Box(new Rect(Screen.width/22,Screen.height/19*13,Screen.width/22*14,Screen.height/19*5),edificios[i].getDatInt());
		if (GUI.Button(new Rect(Screen.width/22*16,Screen.height/19*16,Screen.width/22*4,Screen.height/19*2),"!Atras")) {
			Application.LoadLevel(2);
		}

	}
	private void agregarEdificios(){
		edificios[0]=new Edificio("catedral","1730 a 1760 aunque en el lugar han existido iglesias desde 1559","Anonimo","Torre Norte terminada en\n1904");
		edificios[1]=new Edificio("portales","1657-4524","Tu mama","mc donals");		edificios[2]=new Edificio("santo domingo","1427-1988","Tu","es una iglesia");
	}
}
