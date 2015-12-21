using UnityEngine;
using System.Collections;

public class CoinParty : MonoBehaviour {

	public ParticleSystem coinSystem;
	public ParticleSystem glowSystem;
	
	public float emissionRate = 10;
	
	public int maxCoins = 1000;
	public float minLifetime = 10;
	public float maxLifetime = 10;
	public float minSpin = 100;
	public float maxSpin = 360;
	
	public float initialSpeed = 5f;
	public float gravity = 5;
	public float bounceFactor = 0.8f;
	
	public int frameCount = 16;
	
	public Color ambientColor;
	
	public float shinyness = 10f;
	public float specularBrightness = 5f;
	public float glowThreshold = 1.5f;
	public float glowCutoff = 1.5f;
	public float glowAmount = 0.5f;
	
	public Transform floor;
	
	public Light[] usedLights;
	
	public Transform player;
	
	public float collectDistance = 1f;
	
	Transform myTransform;
	float collectSqrDist;
	int maxGlows;
	float arbitraryLifetime = 1000;
	float longLifetime = 1000;
	
	Coin[] coins;
	ParticleSystem.Particle[] coinParticles;
	ParticleSystem.Particle[] glowParticles;
	Vector3[] frameNormals;
	
	float toEmit = 0;
	
	int coinID = 0;
	int glowID = 0;
	
	Vector3 parkPos;
	
	bool firstFrame = true;
	
	struct Coin {
		public float rotation;
		public float rotationSpeed;
		public float lifetime;
	}
	
	void Start () {
		coinSystem.transform.position = Vector3.zero;
		glowSystem.transform.position = Vector3.zero;
		coinSystem.transform.rotation = Quaternion.identity;
		glowSystem.transform.rotation = Quaternion.identity;
			
		myTransform = transform;
		
		frameNormals = new Vector3[frameCount];
		for(int i = 0; i < frameCount; i++) {
			int ui = i+1; if(ui < 0) ui += frameCount; if(ui >= frameCount) ui -= frameCount;
			float r = (float)i/frameCount * 360;
			if(r > 180) r -= 180;
			frameNormals[i] = Quaternion.AngleAxis(r, Vector3.up) * Vector3.forward;
			frameNormals[i] = new Vector3(-frameNormals[i].x, 0, frameNormals[i].z) * (frameNormals[i].z > 0 ? -1 : 1);
			
		}
		
		parkPos = new Vector3(-9999, -9999, -9999);
		collectSqrDist = collectDistance * collectDistance;
		maxGlows = maxCoins/8;
		
		coins = new Coin[maxCoins];
		coinParticles = new ParticleSystem.Particle[maxCoins];
		glowParticles = new ParticleSystem.Particle[maxGlows];
		
		coinSystem.emissionRate = 0;
		glowSystem.emissionRate = 0;
		coinSystem.startLifetime = longLifetime;
		glowSystem.startLifetime = longLifetime;
		
		coinSystem.Emit (maxCoins);
		glowSystem.Emit (maxGlows);
	}
	
	void Update () {
		if(firstFrame) {
			coinSystem.GetParticles(coinParticles);
			for(int i = 0; i < maxCoins; i++) {
				coins[i].lifetime = longLifetime;
				coinParticles[i].position = parkPos;
			}
			coinSystem.SetParticles(coinParticles, maxCoins);
			
			glowSystem.GetParticles(glowParticles);
			for(int i = 0; i < maxGlows; i++) {
				glowParticles[i].position = parkPos;
			}
			glowSystem.SetParticles(glowParticles, maxCoins);
			firstFrame = false;
			return;
		}
		
		coinSystem.GetParticles(coinParticles);
		glowSystem.GetParticles(glowParticles);
		
		toEmit += emissionRate * Time.deltaTime;
		while(toEmit > 0) {
			Emit();
			toEmit --;	
		}
		
		if(!firstFrame) {
			float recip360 = 1f/360f;
			float deg2Frame = (1f/360f)*frameCount;
			float recipFrameCount = 1f/frameCount;
			float recipLightCount = 1f/usedLights.Length;
			Vector3 cameraPosition = Camera.main.transform.position;
			float floorHeight = floor.position.y+coinParticles[0].size*0.5f;
			Vector3[] lightPositions = new Vector3 [usedLights.Length];
			Vector3 cX = Camera.main.transform.right;
			Vector3 cY = Camera.main.transform.up;
			Vector3 cZ = Camera.main.transform.forward;
			Vector4[] lightColors = new Vector4 [usedLights.Length];
			for(int i = 0; i < usedLights.Length; i++) { 
				lightPositions[i] = usedLights[i].transform.position;
				Vector3 c = new Vector3(usedLights[i].color.r, usedLights[i].color.g, usedLights[i].color.b) * usedLights[i].intensity;
				lightColors[i] = usedLights[i].enabled ? new Vector4(c.x, c.y, c.z, 1f/usedLights[i].range) : Vector4.zero;
			}
			// reset glows
			for(int i = 0; i < glowID; i++) {
				glowParticles[i].position = parkPos;
				glowParticles[i].lifetime = longLifetime;
				glowParticles[i].startLifetime = longLifetime;
			}
			glowID = 0;
			for(int i = 0; i < maxCoins; i++) {
				if(coins[i].lifetime < longLifetime-1) {
					// coin death
					bool collected = (coinParticles[i].position - player.position).sqrMagnitude < collectSqrDist;
					if(coins[i].lifetime < 0 || collected) {
						if(collected) {
							// do something	
						}
						coinParticles[i].position = parkPos;
						coins[i].lifetime = longLifetime;
						coinParticles[i].lifetime = longLifetime;
						coinParticles[i].startLifetime = longLifetime;
					}
					
					// rotate coin
					float delta = coins[i].rotationSpeed * Time.deltaTime;
					if(delta > -360 || delta < 360)  coins[i].rotation += delta;
					if(coins[i].rotation < 0) coins[i].rotation += 360;
					if(coins[i].rotation > 360) coins[i].rotation -= 360;
					
					coins[i].lifetime -= Time.deltaTime;
					
					// render coin color
					int usedFrame = (int)(coins[i].rotation * deg2Frame);
					if(usedFrame >= frameNormals.Length) usedFrame = frameNormals.Length - 1;
					if(usedFrame < 0) usedFrame = 0;
					Vector3 normal = frameNormals[usedFrame].x*cX + frameNormals[usedFrame].y*cY + frameNormals[usedFrame].z*cZ;
					Vector3 viewDir = (cameraPosition - coinParticles[i].position).normalized;
					Vector3 lightAccum = new Vector3(ambientColor.r, ambientColor.g, ambientColor.b);
					float avgLightHeight = 0f;
					for(int ii = 0; ii < usedLights.Length; ii++) {
						Vector3 lightDir = lightPositions[ii]-coinParticles[i].position;
						
						float dist = lightDir.magnitude;
						if(dist != 0) lightDir *= (1f/dist);
						float atten = dist*lightColors[ii].w; 
						if(atten > 1) atten = 1;
						atten = 1f-atten;
						
						avgLightHeight += lightDir.y * atten * lightColors[ii].w * 5;
						
						float diffuse = Vector3.Dot(normal, lightDir); if(diffuse < 0) diffuse = 0;
						float spec = Vector3.Dot(normal, (lightDir+viewDir).normalized); if(spec < 0) spec = 0;
						
						spec = Mathf.Pow (spec, shinyness) * specularBrightness;
						
						//Debug.DrawRay(coinParticles[i].position, lightDir, Color.red);
						//Debug.DrawRay(coinParticles[i].position, (lightDir+viewDir).normalized, Color.white);
					
						lightAccum += new Vector3(lightColors[ii].x, lightColors[ii].y, lightColors[ii].z) * (diffuse + spec) * atten;
					}
					
					//Debug.DrawRay(coinParticles[i].position, normal, Color.green);
					//Debug.DrawRay(coinParticles[i].position, viewDir, Color.blue);
					//Debug.Log ((0.5f+avgLightHeight*recipFrameCount));
					//Debug.DrawRay(coinParticles[i].position, (Vector3.Cross(normal, Vector3.up)+(avgLightHeight*recipLightCount)*Vector3.up*5).normalized, Color.white);
					
					coinParticles[i].color = new Color(lightAccum.x*0.25f, lightAccum.y*0.25f, lightAccum.z*0.25f, 0.5f+avgLightHeight*recipLightCount);
					// apply coin rotation
					coinParticles[i].lifetime = arbitraryLifetime + arbitraryLifetime*coins[i].rotation*recip360;
					coinParticles[i].startLifetime = arbitraryLifetime*2; 
					
					coinParticles[i].velocity += Vector3.up*-gravity*Time.deltaTime;
					
					if(coinParticles[i].position.y < floorHeight) {
						if(coinParticles[i].velocity.y < 0) {
							coinParticles[i].velocity = new Vector3(coinParticles[i].velocity.x, coinParticles[i].velocity.y*-bounceFactor, coinParticles[i].velocity.z);
						} else {
							coinParticles[i].position = new Vector3(coinParticles[i].position.x, floorHeight, coinParticles[i].position.z);
						}	
					}
					
					// if coin color is bright, add glow!
					float max = (lightAccum.x + lightAccum.y + lightAccum.z) * 0.5f;
					if(lightAccum.x > max) max = lightAccum.x; if(lightAccum.y > max) max = lightAccum.y; if(lightAccum.y > max) max = lightAccum.y;
					if(max > glowThreshold) {
						float g = (max-glowThreshold); if(g > glowCutoff) g = glowCutoff;
						Vector3 glowCol = lightAccum.normalized * g * glowAmount;
						glowParticles[glowID].position = coinParticles[i].position;
						glowParticles[glowID].color = new Color(glowCol.x, glowCol.y, glowCol.z, 1);
						glowParticles[glowID].lifetime = arbitraryLifetime + arbitraryLifetime*coins[i].rotation*recip360;
						glowParticles[glowID].startLifetime = arbitraryLifetime*2; 
						glowID++;
						if(glowID >= maxGlows) glowID = 0;
					}
				}
			}
		}

		
		glowSystem.SetParticles(glowParticles, maxGlows);
		coinSystem.SetParticles(coinParticles, maxCoins);	
	}
	
	void Emit () {
		coins[coinID].lifetime = Random.Range(minLifetime, maxLifetime);
		coins[coinID].rotation = Random.value * 360;
		coins[coinID].rotationSpeed = Random.Range(minSpin, maxSpin) * (Random.value > 0.5 ? 1 : -1);
		Vector3 vel = (Random.onUnitSphere * initialSpeed) + (Vector3.up * initialSpeed);
		vel = new Vector3(vel.x, vel.y, 0);
		coinParticles[coinID].velocity = vel;
		coinParticles[coinID].position = myTransform.position;
		coinParticles[coinID].lifetime = minLifetime*2;
		coinParticles[coinID].startLifetime = minLifetime*2;
		
		coinID ++;
		if(coinID >= maxCoins) coinID = 0;
	}
}

