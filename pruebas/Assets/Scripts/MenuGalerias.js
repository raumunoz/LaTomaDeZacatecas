#pragma strict

function Start () {

}

function Update () {
   
}

function OnGUI(){
	var pe:boolean=GUI.Button(Rect(Screen.width/22*8,Screen.height/19*6,Screen.width/22*6,Screen.height/19*2),"Personajes");
	var ed:boolean=GUI.Button(Rect(Screen.width/22*8,Screen.height/19*10,Screen.width/22*6,Screen.height/19*2),"Edificios");
	var ar:boolean=GUI.Button(Rect(Screen.width/22*8,Screen.height/19*14,Screen.width/22*6,Screen.height/19*2),"Armas");
	var at:boolean=GUI.Button(Rect(Screen.width/22*16,Screen.height/19*16,Screen.width/22*4,Screen.height/19*2),"!Atras");
	if(pe)
		Application.LoadLevel(3);
	if(ed)
		Application.LoadLevel(4);
	if(ar)
		Application.LoadLevel(5);
	if(at)
		Application.LoadLevel(0);
}