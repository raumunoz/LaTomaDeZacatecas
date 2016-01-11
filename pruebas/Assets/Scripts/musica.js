#pragma strict
var uno:AudioClip;
var dos:AudioClip;
var tres:AudioClip;
var cuatro:AudioClip;
var cinco:AudioClip;
var seis:AudioClip;
var numero:int;



function Start () {
	musica();
}

function Update () {

}

function Awake(){
DontDestroyOnLoad(transform.gameObject);
if(FindObjectsOfType(GetType()).Length>1){
Destroy(gameObject);
}
}


function musica (){
var audiocancion:AudioSource = GetComponent.<AudioSource>();
	numero = 0;
	numero += Random.Range(0,5);
	Debug.Log(numero);
	switch(numero){
	case 0: audiocancion.clip= uno;
        	audiocancion.Play();
        	break;
    case 1: audiocancion.clip= dos;
        	audiocancion.Play();
        	break;
    case 2: audiocancion.clip= tres;
        	audiocancion.Play();
        	break;
    case 3: audiocancion.clip= cuatro;
        	audiocancion.Play();
        	break;
    case 4: audiocancion.clip= cinco;
        	audiocancion.Play();
        	break;
    default: audiocancion.clip= seis;
        	 audiocancion.Play();
        break;

}
}