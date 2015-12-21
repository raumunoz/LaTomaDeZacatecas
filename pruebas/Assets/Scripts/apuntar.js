#pragma strict
public var camara : Camera;
//public mira MiraActiva;
//public float miraMovedizaOfset;
//public FirstPersonController x; 
function Start () {
	Screen.lockCursor = true;
	Cursor.visible=false;

}

function Update () {
	if(Input.GetButtonDown("Fire2")){ 
		camara.fieldOfView = 45;
	}if(Input.GetButtonUp("Fire2")){
		camara.fieldOfView = 60;
	}
}