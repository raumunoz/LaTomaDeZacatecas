using UnityEngine;
using System.Collections;

public class Edificio: MonoBehaviour {
	//////////////////////////////////////
	private string nombre;
	private string epcaCon;
	private string constructor;
	private string datInt;
	//////////////////////////////////////
	public Edificio(string n,string ec,string co,string di){
		nombre=n;
		epcaCon=ec;
		constructor=co;
		datInt=di;
	}
	public string getNombre(){
		return nombre;
	}
	public string getEpocaCon(){
		return epcaCon;
	}
	public string getConst(){
		return constructor;//datos interesantes
	}
	public string getDatInt(){
		return datInt;//batallas interesantes
	}
	
}
