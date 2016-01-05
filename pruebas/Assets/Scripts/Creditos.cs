using UnityEngine;
using System.Collections;

public class Creditos {
	private string nombre, institucion, aportacion;
	public Creditos(string n, string i, string a){
		nombre = n;
		institucion = i;
		aportacion=a;
	}
	public string getNombre(){
		return nombre;
	}
	public string getInstitucion(){
		return institucion;
	}
	public string getAportacion(){
		return aportacion;
	}
}
