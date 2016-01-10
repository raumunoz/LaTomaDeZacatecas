#pragma strict
var ray:Ray;
var selray:Ray;
var hit:RaycastHit;
var selhit:RaycastHit;

var salva:String = "";
var original:Vector3;
var regresa:GameObject;
var obtiene:GameObject;




function Start () {
	Screen.lockCursor=false;
}

function Update () {
efecto();
seleccion();
}



function efecto(){
ray = Camera.main.ScreenPointToRay(Input.mousePosition);
if(Physics.Raycast(ray,hit)){
	if(hit.collider.name == "juego"){
	salva = "juego";
	obtiene = GameObject.Find("juego");
	obtiene.transform.position.z = -4;
	}
	if(hit.collider.name == "continuar"){
	salva = "continuar";
	obtiene = GameObject.Find("continuar");
	obtiene.transform.position.z = -4;
	}
	if(hit.collider.name == "galerias"){
	salva = "galerias";
	obtiene = GameObject.Find("galerias");
	obtiene.transform.position.z = -4;
	}
	if(hit.collider.name == "opciones"){
	salva = "opciones";
	obtiene = GameObject.Find("opciones");
	obtiene.transform.position.z = -4;
	}
	if(hit.collider.name == "creditos"){
	salva = "creditos";
	obtiene = GameObject.Find("creditos");
	obtiene.transform.position.z = -4;
	}
	if(hit.collider.name == "salir"){
	salva = "salir";
	obtiene = GameObject.Find("salir");
	obtiene.transform.position.z = -4;
	}
	if(hit.collider.name == "Plane"){
	if(salva != "Plane"){
	regresa = GameObject.Find(salva);
	regresa.transform.position.z = -3.79;
	}else{

	}
	}
}
}

function seleccion(){
 if ( Input.GetMouseButtonDown (0)){ 
   
   selray = Camera.main.ScreenPointToRay(Input.mousePosition); 
   if (Physics.Raycast(selray,selhit)) {
     
     if(hit.collider.name == "juego"){
		Application.LoadLevel(1);
	}
	if(hit.collider.name == "continuar"){
		Application.LoadLevel(PlayerPrefs.GetInt("nivel"));
	}
	if(hit.collider.name == "galerias"){
		Application.LoadLevel(2);
	}
	if(hit.collider.name == "opciones"){
		Application.LoadLevel(6);
	}
	if(hit.collider.name == "creditos"){
		Application.LoadLevel(13);
	}
	if(hit.collider.name == "salir"){
		Application.Quit();
	}

	}
		
   }
 }