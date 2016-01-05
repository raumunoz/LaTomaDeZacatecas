using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]public class IKtargetPos{
	[Header("objetivos")]
	public Transform handPlacement;
	public Transform elbowPlacement;
	[Header("psoicion de codos")]
	public Vector3 elbowPistolpos = new Vector3 (-2.30f, 0.9f, 2.78f);
	public Vector3 elbowRiflepos = new Vector3 (-2.30f, 0.9f, 2.78f);

	public bool DebugIk;
}

public class managerDeArmasEnemigo : MonoBehaviour {
	public GameObject BulletPrefab;
	public List<GameObject>WeaponList=new List<GameObject>();
	public controlDeArmasEnemigo activeWeapon;
	int weaponNumber;

	public enum WeaponType
	{
		Pistol,Rifle
	}
	public WeaponType weaponType;
	//customIK customiK;
	Animator anim;
	//IKtargetPos iKtargetPos:
	public IKtargetPos IKtargetPo;

	// Use this for initialization
	void Start () {
		weaponNumber=0;
		foreach (GameObject go in WeaponList) {
			go.GetComponent<controlDeArmasEnemigo> ().hasOwner = true;

			Debug.Log ("ARMA" + go.GetComponent<controlDeArmasEnemigo> ().wepoType);
		}
		//customiK = GetComponentInChildren < customIK> ();
		activeWeapon = WeaponList [weaponNumber].GetComponent<controlDeArmasEnemigo> ();
		IKtargetPo.handPlacement = activeWeapon.handPosition.transform;
		IKtargetPo.elbowPlacement = new GameObject ().transform;
		activeWeapon.equip = true;

		//customiK.target = IKtargetPo.handPlacement;
		//customiK.elbowTarget=IKtargetPo.elbowPlacement;

		IKtargetPo.elbowPlacement.parent = transform;
		anim = GetComponent<Animator> ();


	}
	
	// Update is called once per frame
	void Update () {
	//	activeWeapon = WeaponList [weaponNumber].GetComponent<controlDeArmasEnemigo> ();
		activeWeapon = WeaponList [weaponNumber].GetComponent<controlDeArmasEnemigo> ();
		IKtargetPo.handPlacement = activeWeapon.handPosition.transform;
		activeWeapon.equip = false;
		//customiK.target = IKtargetPo.handPlacement;

		weaponType = activeWeapon.wepoType;
		if(!IKtargetPo.DebugIk){
			switch (weaponType) {
			case WeaponType.Pistol:
				anim.SetInteger ("arma", 0);
				IKtargetPo.elbowPlacement.localPosition = IKtargetPo.elbowPistolpos;
				break;
			case WeaponType.Rifle:
				anim.SetInteger ("arma", 1);
				IKtargetPo.elbowPlacement.localPosition = IKtargetPo.elbowRiflepos;
				break;
			}
		}
	}

	public void FireActiveWrapon(){
		if (activeWeapon != null) {
			activeWeapon.Fire ();

		}
	}
	public void changeWeapon (bool ascendig)
	{
		/*weaponNumber = 1;
		if (WeaponList.Count > 1) {
			activeWeapon.equip = false;
			if (ascendig) {
				if (weaponNumber < WeaponList.Count - 1) {
					weaponNumber++;
				} else {
					weaponNumber = 0;
				}
			} else {
				if (weaponNumber > 0) {
					weaponNumber = WeaponList.Count - 1;
				}
			}
		}*/
		activeWeapon.equip = true;
		if (ascendig) {
			weaponNumber = 0;
		
		} if(!ascendig) {
			weaponNumber = 1;

		}
	}

			


}