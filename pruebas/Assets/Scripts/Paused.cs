using UnityEngine;
using System.Collections;

public class Paused : MonoBehaviour {
	string[] asd=new string[4];
	string[] eligeGrafico=new string[6];
	int grafico;
	int a;
	float b;
	string asdd;
	public bool paused;
	void Start () {
		//QualitySettings.currentLevel = QualityLevel.Fastest;
		paused = false;
		Debug.Log(Time.timeScale);
		PlayerPrefs.SetInt ("nivel", Application.loadedLevel);
		a = 3;
		b = 1;
		Debug.Log(QualitySettings.GetQualityLevel ());
		grafico=QualitySettings.GetQualityLevel ();
		eligeGrafico [0]="muy malos";
		eligeGrafico [1]="malos";
		eligeGrafico [2]="regulares";
		eligeGrafico [3]="buenos";
		eligeGrafico [4]="muy buenos";
		eligeGrafico [5]="los mejores";
		
		asd [0] = "800x600";
		asd [1] = "1024x768";
		asd [2] = "1080x900";
		asd [3] = "1366x768";
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp("p")||Input.GetKeyUp(KeyCode.Escape)) {
			paused=true;
		}
		if (paused) {
			Screen.lockCursor=false;
			Time.timeScale=0;
		} else {
			Screen.lockCursor=true;
			Time.timeScale=1;
		}switch (a) {
			case 0:{
				Screen.SetResolution(800,600,false);
				asdd=asd[0];
			}break;
			case 1:{
				Screen.SetResolution(1024,768,true);
				asdd=asd[1];
			}break;
			case 2:{
				Screen.SetResolution(1080,900,true);
				asdd=asd[2];
			}break;
			case 3:{
				Screen.SetResolution(1366,768,true);
				asdd=asd[3];
			}break;
		}
		
		switch (grafico) {
		case 0:{
			QualitySettings.currentLevel = QualityLevel.Fastest;
		}break;
		case 1:{
			QualitySettings.currentLevel = QualityLevel.Fast;
		}break;
		case 2:{
			QualitySettings.currentLevel = QualityLevel.Simple;
		}break;
		case 3:{
			QualitySettings.currentLevel = QualityLevel.Good;
		}break;
		case 4:{
			QualitySettings.currentLevel = QualityLevel.Beautiful;
		}break;
		case 5:{
			QualitySettings.currentLevel = QualityLevel.Fantastic;
		}break;
		}
	}
	void OnGUI(){
		if (paused) {
			if (GUI.Button(new Rect(Screen.width/22*16,Screen.height/19*16,Screen.width/22*4,Screen.height/19*2),"!Salir")) {
				Application.LoadLevel (0);
			}
			if (GUI.Button(new Rect(Screen.width/22*1,Screen.height/19*16,Screen.width/22*4,Screen.height/19*2),"Continuar")) {
				paused=false;
			}
			GUI.Label(new Rect(Screen.width/22*10,Screen.height/19*2,Screen.width/22*10,Screen.height/19),"Resolucion: "+asdd);
			a=GUI.Toolbar(new Rect(Screen.width/22*10,Screen.height/19*4,Screen.width/22*10,Screen.height/19),a,asd);
			GUI.Label(new Rect(Screen.width/22*10,Screen.height/19*12,Screen.width/22*10,Screen.height/19),"Graficos: ");
			grafico=GUI.Toolbar(new Rect(Screen.width/22*10,Screen.height/19*13,Screen.width/22*10,Screen.height/19),grafico,eligeGrafico);
			b=GUI.HorizontalSlider(new Rect(Screen.width/22*10,Screen.height/19*10,Screen.width/22*10,Screen.height/19),b,0,1);
			GUI.Label(new Rect(Screen.width/22*10,Screen.height/19*9,Screen.width/22*10,Screen.height/19),"Volumen: "+decimal.Round((decimal)b*100,2)+"%");
			AudioListener.volume = b;
		}
	}
	public void cargarPartida(){
		
	}
	public void guardarPartida(){
		
	}
}
