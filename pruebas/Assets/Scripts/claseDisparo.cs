using UnityEngine;
using System.Collections;

public class claseDisparo  : MonoBehaviour{
	
		//float cadenciaDeDisparo = .5f;

		float danio= 25f;
		// Use this for initialization

		
		// Update is called once per frame

		
		public void disparo(){

			Debug.Log("Disparo¡¡¡¡");
			//	Vector3 puntoDeImpacto;
			//espera = cadeDisparo;
			Ray ray = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
			//Transform impactoTransformMasCercano(ray);
			Transform impactoTransform;
			Vector3 puntoDeImpacto;
			
			impactoTransform=impactoTransformMasCercano(ray, out puntoDeImpacto);
			//RaycastHit impactoInfo;
			//RaycastHit impactoMasCercano;
			
			/*if(Physics.Raycast(ray,out impactoInfo)){
			Debug.Log("Impacto en: "+impactoInfo.collider.name);	

		}*/
			
			//Physics.Raycast ();
			
			if (impactoTransform != null) {
				Debug.Log("Impactamos : "+ impactoTransform.transform.name);
				//aqui podemos agregar efectos en el punto de esta locacion 
				//efecto(ImpactoTransform)
				
				Vida v= impactoTransform.transform.GetComponent<Vida>();
				
				while (v!=null && impactoTransform.parent){
					impactoTransform=impactoTransform.parent;
					v=impactoTransform.GetComponent<Vida>();
				}
				//aqui el punto de impacto pude que no sea con el que empesamos
				if(v != null){
					v.danio(danio);
				}
				//espera=cadenciaDeDisparo;
			}
		}
		
		Transform impactoTransformMasCercano(Ray ray, out Vector3 puntoimpacto){
			RaycastHit[] impactos = Physics.RaycastAll (ray);
			puntoimpacto = Vector3.zero;
			Transform impactoMasCercano=null;
			float distancia = 0;
			foreach (RaycastHit impacto in impactos) {
				if(impacto.transform != this.transform && (impactoMasCercano==null || impacto.distance < distancia)){
					//impactamos algo que:
					//a)no somos nosostros
					//b)la primera cosa que impactamos no somos nosotoros
					//c) si, no es b, es la cosa mas cercana que la cosa mas cercana
					impactoMasCercano= impacto.transform;
					distancia= impacto.distance;
					puntoimpacto=impacto.point;
					
				}
				
			}
			//impacto mas cercano no puede ser todavia nullo (ie nulo ) o contiene la cosa valida mas cercana para impactar
			return impactoMasCercano;
		}
		
	}