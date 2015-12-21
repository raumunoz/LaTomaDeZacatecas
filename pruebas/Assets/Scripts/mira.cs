using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class mira : MonoBehaviour {
	public string nombreDeLaMira;
	public float extensionPorDefecto=15;
	public float extensionMaxima=50;
	public float extensionMovediza=50;
	public float extensionMovedizaMaximaTimer=60;
	[HideInInspector]
	public float extensionActual=0;
	private float extensionObjetivo=0;
	private float extensionT=0;

	private Quaternion rotacionPorDefecto;
	private bool extencionEstaFuncionando=true;

	public float extensionVelocidad=0.2f;
	public float rotacionVelocidad=0.5f;
	public bool permitirRotacion=true;

	private float rotacionTImer=0;
	private bool rotacionEstaFuncionando=true;

	public bool extensionMientrasRota=true;
	public float extensionRotacion=0;
	public bool permitirExtencion=true;
	private bool movediza=false;
	public float movedizaTimer=0;
	public partesDeMira[] partes;
	// Use this for initialization
	[Serializable]//guarda lo que le pongamos en el inspector de otra fomra tenemos que ponerlos de nuevo
	public class partesDeMira{
		public Image imagen;
		public Vector2 arriba;
	}

	void Start () {
		extensionActual = extensionPorDefecto;
		rotacionPorDefecto = transform.rotation;
		cambiarExtencionDelCursor (extensionPorDefecto);
	}

	public void aplicarExtencion(){
		foreach (partesDeMira im in partes) {
			im.imagen.rectTransform.anchoredPosition= im.arriba * extensionActual;
		}
	}

	public void miraMovediza(){
		if(permitirExtencion){
			cambiarExtencionDelCursor(extensionMovediza);
			movediza=true;
		}
	}
	public void cambiarExtencionDelCursor(float valor){
		if(permitirExtencion){
			extencionEstaFuncionando=true;
			extensionObjetivo=valor;
			extensionT=0;
		}
	}

	public void rotacionDelCursor(float segundos){
		if(permitirRotacion){
			rotacionEstaFuncionando=true;
			rotacionTImer=segundos;
			if(extensionMientrasRota){
				cambiarExtencionDelCursor(rotacionVelocidad);
			}
		}
	}



	public static float AceleracionDeceleracionIterpolacion(float empezar,float fin,float t){
		float x = fin - empezar;
		float newT = (Mathf.Cos ((t + 1) * Mathf.PI) / 2) + .05f;
		x *= newT;
		float retVal = empezar + x;
		return retVal;
	}

	
	// Update is called once per frame
	void Update () {
		if(extencionEstaFuncionando){
			extensionT	+=	Time.deltaTime	/	extensionVelocidad;
			float valorDeExtencion	=	AceleracionDeceleracionIterpolacion(extensionActual,extensionObjetivo,extensionT);
			if(extensionT > 1){
				valorDeExtencion = extensionObjetivo;
				extensionT=0;
				if(movediza){
					if(movedizaTimer < extensionMovedizaMaximaTimer){
						movedizaTimer += Time.deltaTime;
					}else{
						movedizaTimer=0;
						movediza=false;
						extensionObjetivo= extensionPorDefecto;
					}
				}else{//(movediza)
					extencionEstaFuncionando=false;
				}
			}
			else{//(extensionT > 1)
				cambiarExtencionDelCursor(extensionPorDefecto);
			}
			extensionActual=valorDeExtencion;
			aplicarExtencion();
		}//if(extencionEstaFuncionando)
		if(rotacionEstaFuncionando){
			if(rotacionTImer>0){
				rotacionTImer -= Time.deltaTime;
				transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
				                                      transform.rotation.eulerAngles.y,
				                                      transform.rotation.eulerAngles.z + (360*Time.deltaTime * rotacionVelocidad)); 
			}else{
				rotacionEstaFuncionando=false;
				transform.rotation = rotacionPorDefecto; 
				if(extensionMientrasRota){
					cambiarExtencionDelCursor(extensionPorDefecto);
				}
			}	
		}
		if (Input.GetButtonDown ("Fire1")) {
			miraMovediza();
			
		}
	}

}
