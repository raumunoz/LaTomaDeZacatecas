using UnityEngine;
using System.Collections;

public class Rotator : SpritePackerAnimation {
	
	public Vector3 rotate;
	
	public override void Sample (float time, Vector3 camPos) {
		transform.rotation = Quaternion.identity;
		float dt = 0.01f;
		while(time > 0) {
			transform.Rotate(rotate *	dt);
			time -= dt;
		}
	}

	void Update () {
		transform.Rotate(rotate * Time.deltaTime);
	}
}
