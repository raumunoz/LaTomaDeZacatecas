#pragma strict
public var projectile:Rigidbody;
public var speed:float;
var desviacion:float;
var tiempo:float;
var tiempoEspera:float;
function Start () {
	tiempoEspera=Random.Range(6,10);
	tiempo=Time.time;
	tiempoEspera=Random.Range(6,30);
	desviacion=0;
}

function Update () {
	//Debug.Log(Time.time-tiempo+"    "+tiempoEspera);
	speed=Random.Range(100,200);
	if(Time.time-tiempo>tiempoEspera){
		var instantiatedProjectile : Rigidbody = Instantiate(projectile, transform.position, transform.rotation );
		instantiatedProjectile.velocity = transform.TransformDirection( Vector3(0, desviacion, speed) );
		DontDestroyOnLoad(instantiatedProjectile);
		//Physics.IgnoreCollision( instantiatedProjectile. GetComponent.<Collider>(), transform.root.GetComponent.<Collider>() );
		tiempo=Time.time;
		tiempoEspera=Random.Range(6,10);
		desviacion=Random.Range(10,50);
	}
}