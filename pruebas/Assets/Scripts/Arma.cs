using UnityEngine;
using System.Collections;

public class Arma: MonoBehaviour {
	//////////////////////////////////////
	private string nombre;
	private int largo;
	private int largoCanon;
	private string accion;
	private string calibre;
	private string otrosDatos;
	//////////////////////////////////////
	public Arma(string n,int l,int lc,string ac,string ca,string od){
		nombre = n;
		largo = l;
		largoCanon = lc;
		accion = ac;
		calibre = ca;
		otrosDatos = od;
	}
	public string getNombre(){
		return nombre;
	}
	public int getLargo(){
		return largo;
	}
	public int getLargoCanon(){
		return largoCanon;
	}
	public string getAccion(){
		return accion;
	}
	public string getCalibre(){
		return calibre;
	}
	public string getOtrosDatos(){
		return otrosDatos;
	}
}
