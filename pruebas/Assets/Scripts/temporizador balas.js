var nTime : float = 10;
public var Aceleracion : float = 1;
var DisparoChoque : GameObject;

function Start () {
	//Destroy(this.gameObject, nTime);
}

function OnCollisionEnter(){
	Instantiate(DisparoChoque, transform.position, transform.rotation);
	Destroy(gameObject);
}