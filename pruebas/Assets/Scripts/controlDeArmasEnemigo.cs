using UnityEngine;
using System.Collections;



public class controlDeArmasEnemigo : MonoBehaviour {
	public bool equip;
	public managerDeArmasEnemigo.WeaponType wepoType;
	public int MaxAmmo;
	public int MaxClipAmmo;
	public int curAmmo;
	public bool canBurts;
	public GameObject handPosition;
	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	GameObject bulletSpawnGD;
	ParticleSystem bulletPart;
	managerDeArmasEnemigo parentControl;
	bool fireBullet;
	AudioSource audioSource;
	Animator weponAnim;

	[Header("Posiciones")]//es un titulo nomas para el inspector
	public bool hasOwner;
	public Vector3 equipPosition;
	public Vector3 equipRotation;
	public Vector3 unEquipPosition;
	public Vector3 unEquipRotation;
	//Debuug scale

	Vector3 scale;

	public resPosition resPosicion;

	public enum resPosition
	{
		waist,RightHip

	}

	// Use this for initialization
	void Start () {

		curAmmo = MaxClipAmmo;
		bulletSpawnGD = Instantiate (bulletPrefab, transform.position, Quaternion.identity)as GameObject;
		bulletSpawnGD.AddComponent<direcionDeParticula> ();
		bulletSpawnGD.GetComponent<direcionDeParticula> ().wepon = bulletSpawn;
		bulletPart = bulletSpawnGD.GetComponent<ParticleSystem> ();
		audioSource = GetComponent<AudioSource> ();
		weponAnim = GetComponent<Animator> ();
		//weponAnim.SetInteger("cartuchio"
		scale = transform.localScale;
		/*if(resPosicion.waist){
			hasOwner=true
		}*/
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = scale;
		if(hasOwner){
			transform.parent = transform.GetComponentInParent<managerDeArmasEnemigo> ().transform.GetComponent<Animator> ().GetBoneTransform (HumanBodyBones.RightHand);
			transform.localPosition = equipPosition;
			transform.localRotation = Quaternion.Euler (equipRotation);
			if (fireBullet) {
			if (curAmmo > 0) {
				
					curAmmo--;
//					bulletPart.Emit (1);
					audioSource.Play ();
					//	weponAnim.SetTrigger("disparo");
					fireBullet = false;

				} else {
					if (MaxAmmo >= MaxClipAmmo) {

						curAmmo = MaxClipAmmo;
						MaxAmmo -= MaxClipAmmo;
					} else {
						curAmmo = MaxClipAmmo - (MaxClipAmmo - MaxAmmo);
					}

						
				}
			} else {
				if(equip){
					switch(resPosicion){
					case resPosition.waist:
						transform.parent = transform.GetComponentInParent<managerDeArmasEnemigo> ().transform.GetComponent<Animator> ().GetBoneTransform (HumanBodyBones.RightUpperLeg);
						break;
					case resPosition.RightHip:
						transform.parent = transform.GetComponentInParent<managerDeArmasEnemigo> ().transform.GetComponent<Animator> ().GetBoneTransform (HumanBodyBones.Spine);
						break;
					}
					transform.localPosition = unEquipPosition;
					transform.localRotation = Quaternion.Euler (unEquipRotation);
				}			
			}
		}
	}
	public void Fire(){
		fireBullet = true;
	} 
}
