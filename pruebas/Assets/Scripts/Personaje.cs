using UnityEngine;
using System.Collections;

public class Personaje : MonoBehaviour {
	//////////////////////////////////////
	private string nombre;
	private string fechaNac;
	private string datInt;//datos interesantes
	private string batInt;//batallas interesantes
	private string fechaMuerte;
	private string causaMuerte;
	//////////////////////////////////////
	public Personaje(string n,string fn,string dt,string bt,string fm,string cm){
		nombre=n;
		fechaNac=fn;
		datInt=dt;
		batInt=bt;
		fechaMuerte = fm;
		causaMuerte = cm;
	}
	public string getNombre(){
		return nombre;
	}
	public string getNac(){
		return fechaNac;
	}
	public string getDatInt(){
		return datInt;//datos interesantes
	}
	public string getBatInt(){
		return batInt;//batallas interesantes
	}
	public string getFechaMuerte(){
		return fechaMuerte;
	}
	public string getCausaMuerte(){
		return causaMuerte;
	}
}
