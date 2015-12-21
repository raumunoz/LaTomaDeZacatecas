#pragma strict
var punto:Transform;
function Start () {

}

function Update () {
	transform.RotateAround(punto.position,Vector3.up,90*Time.deltaTime);
}