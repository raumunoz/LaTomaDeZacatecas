#pragma strict
var armas : GameObject[];
function Start () {
	selecionArma(0);
}

function Update () {
if( Input.GetKeyDown( "1" ) )
         {
       //yield WaitForSeconds (.5f);
             selecionArma(0);
             
             
         }
if( Input.GetKeyDown( "2" ) )
         {
         //yield WaitForSeconds (.55f);
             selecionArma(1);
         }  

                

}

function selecionArma(indice :int){
for(var obj: GameObject in armas)obj.SetActive(false);
armas[indice].SetActive(true);
}
