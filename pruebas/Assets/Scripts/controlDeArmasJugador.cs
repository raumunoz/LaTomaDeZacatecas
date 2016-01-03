using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class controlDeArmasJugador : MonoBehaviour {
	public int municionReservas;
	public int capacidadCargador;
	public int municionActual;
	public bool equipado;
	public bool tineDuenio;
	public bool disparo;
	public float danio;
	public managerDeArmasjugador.WeaponType wepoType;
	private disparosDelJugador dispParoDelJugador;
	public Text municion;
	Animator anim;

	// Use this for initialization
	void Start(){
		anim = GetComponent<Animator> ();
		dispParoDelJugador = GetComponentInParent<disparosDelJugador> ();
		anim.SetInteger ("cartuchos", municionActual);
		anim.SetInteger ("reservas", municionReservas);

		muestraMunicion ();
	}
	
	// Update is called once per frame
	void Update (){
		if (equipado) {
			if (disparo) {
				if(municionActual>0){
					disparo = false;
					municionActual--;
				}

			}
		}
	}

	public void disparar(){
		disparo = true;

		//Debug.Log ("DISPARARASDASDASFASFasf");
	}

	public void dismiNuirParque(){
		municionReservas--;
	}

	public void muestraMunicion(){
		municion.text =(municionActual + "/" + municionReservas);
	}
}
