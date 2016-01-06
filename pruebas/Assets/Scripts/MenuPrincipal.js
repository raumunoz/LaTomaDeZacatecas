#pragma strict
var estilo:GUIStyle;
function Start () {
	Screen.lockCursor=false;
}

function Update () {
	//Debug.Log();
}

function OnGUI(){
	//GUI.Label(Rect(Screen.width/3,0,Screen.width/3*2,Screen.height/6),"La Toma de Zacatecas",estilo);
	var jn:boolean=GUI.Button(Rect(Screen.width/22*8,Screen.height/19*4,Screen.width/22*6,Screen.height/19*2),"Juego Nuevo");
	var co:boolean=GUI.Button(Rect(Screen.width/22*8,Screen.height/19*7,Screen.width/22*6,Screen.height/19*2),"Continuar");
	var ga:boolean=GUI.Button(Rect(Screen.width/22*8,Screen.height/19*10,Screen.width/22*6,Screen.height/19*2),"Galerias");
	var cr:boolean=GUI.Button(Rect(Screen.width/22*8,Screen.height/19*13,Screen.width/22*6,Screen.height/19*2),"Creditos");
	var op:boolean=GUI.Button(Rect(Screen.width/22*8,Screen.height/19*16,Screen.width/22*6,Screen.height/19*2),"Opciones");
	var s:boolean=GUI.Button(Rect(Screen.width/22*16,Screen.height/19*16,Screen.width/22*4,Screen.height/19*2),"!Salir");
	if(jn){
		Application.LoadLevel(1);
		Debug.Log("Juego Nuevo");
	}
	/*if(p2){
		Application.LoadLevel(4);
	}*/
	if(co)
		Application.LoadLevel(PlayerPrefs.GetInt("nivel"));
	if(ga)
		Application.LoadLevel(2);
	if(op)
		Application.LoadLevel(6);
	if(s)//salida
		Application.Quit();
}