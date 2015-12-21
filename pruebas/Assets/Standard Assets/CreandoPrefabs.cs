using UnityEngine;
using System.Collections;

public class CreandoPrefabs : MonoBehaviour {
	public GameObject zombie;
	float frec;
	void Start () {
		frec=Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if((Time.time-frec)>2.2){
			Vector3 a;
			a=transform.position;
			a.x=Random.Range(0,499);
			a.y=transform.position.y+110;
			a.z=Random.Range(0,499);
			Instantiate(zombie,a,transform.rotation);
			frec=Time.time;
		}
	}
}
