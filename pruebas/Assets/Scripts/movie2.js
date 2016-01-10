#pragma strict

var movTexture : MovieTexture;

function Start () {
	GetComponent.<Renderer>().material.mainTexture = movTexture;

	var cam:Camera  = Camera.main; 
    var pos:float = (cam.nearClipPlane + 0.01f);
    transform.position = cam.transform.position + cam.transform.forward * pos;
    transform.LookAt (cam.transform);
    transform.Rotate (90.0f, 0.0f, 0.0f);  
    var  h:float = (Mathf.Tan(cam.fieldOfView*Mathf.Deg2Rad*0.5f)*pos*2f) /10.0f; 
    transform.localScale = new Vector3(h*cam.aspect,1.0f, h);
    
    var aud: AudioSource = GetComponent.<AudioSource>();  
	aud.clip = movTexture.audioClip;
	
	movTexture.Play();
	
	aud.Play();
}

function Update(){
         if(movTexture.isPlaying != true){
         	Application.LoadLevel(13);
         }
}