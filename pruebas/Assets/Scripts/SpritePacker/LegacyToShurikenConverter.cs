using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class LegacyToShurikenConverter {
	
	//ShapeModuleUI.cs line 29
	enum ShapeTypes { Sphere, SphereShell, HemiSphere, HemiSphereShell, Cone, Box, Mesh };
	
	[MenuItem ("CONTEXT/ParticleEmitter/Convert To Shuriken")]
	static void ConvertMenuItem () {
		if(Selection.activeTransform != null && GetAllParticleEmitters(Selection.activeTransform).Length != 0) {
			Convert (Selection.activeTransform);
		}
	}
	
	public static Transform Convert (Transform legacyParent) {
		Transform systemParent = Object.Instantiate(legacyParent) as Transform;
		
		systemParent.gameObject.name = legacyParent.gameObject.name;
		
		legacyParent.gameObject.name += "_legacy";
		
		ParticleEmitter[] particleEmitters = GetAllParticleEmitters(systemParent);
		
		Selection.activeTransform = systemParent;
		
		foreach(ParticleEmitter pe in particleEmitters) {
			GameObject oldGO = pe.gameObject;
			ParticleRenderer pr = oldGO.GetComponent<ParticleRenderer>();
			ParticleAnimator pa = oldGO.GetComponent<ParticleAnimator>();
			
			ParticleSystem newPS = oldGO.AddComponent<ParticleSystem>();
			ParticleSystemRenderer newPSR = oldGO.GetComponent<ParticleSystemRenderer>();
			SerializedObject psSerial = new SerializedObject (newPS);
			SerializedObject psrSerial = new SerializedObject (newPSR);
			SerializedObject peSerial = new SerializedObject (pe);
			SerializedObject paSerial = new SerializedObject (pa);
			SerializedObject prSerial = new SerializedObject (pr);
			
			psSerial.Update ();
			psrSerial.Update ();
			peSerial.Update ();
			paSerial.Update ();
			prSerial.Update ();
			
			psSerial.FindProperty("moveWithTransform").boolValue = !pe.useWorldSpace;
			
			if(peSerial.FindProperty("m_OneShot").boolValue) {
				psSerial.FindProperty("EmissionModule.m_BurstCount").intValue = 1;
				psSerial.FindProperty("EmissionModule.time0").floatValue = 0;
				psSerial.FindProperty("EmissionModule.cnt0").intValue = (int)((pe.minEmission + pe.maxEmission) * 0.5f);
				SetShurikenMinMaxCurve(psSerial, "EmissionModule.rate", 0, 0, true);
			} else {
				SetShurikenMinMaxCurve(psSerial, "EmissionModule.rate", pe.minEmission, pe.maxEmission, true);	
			}
			
			SetShurikenMinMaxCurve(psSerial, "InitialModule.startLifetime", pe.minEnergy, pe.maxEnergy, true);
			SetShurikenMinMaxCurve(psSerial, "InitialModule.startSize", pe.minSize, pe.maxSize, true);
			SetShurikenMinMaxCurve(psSerial, "InitialModule.startRotation", 0.0f, pe.rndRotation ? 360.0f : 0.0f, true);
			
			bool rotModule = pe.angularVelocity > 0 || pe.rndAngularVelocity > 0;
			psSerial.FindProperty("RotationModule.enabled").boolValue = rotModule;
			SetShurikenMinMaxCurve(psSerial, "RotationModule.curve", pe.angularVelocity - pe.rndAngularVelocity, pe.angularVelocity + pe.rndAngularVelocity, true);
			
			Vector3 primaryDirection = pe.localVelocity.sqrMagnitude > pe.worldVelocity.sqrMagnitude ? pe.transform.TransformDirection(pe.localVelocity) : pe.worldVelocity;
			
			float primaryAmount = primaryDirection.magnitude;
			float randomAmount = pe.rndVelocity.magnitude;
			primaryAmount += peSerial.FindProperty("tangentVelocity").vector3Value.magnitude;
			
			float primaryRatio = primaryAmount / randomAmount;
			
			SerializedProperty ellipsoid = peSerial.FindProperty("m_Ellipsoid");
			if(ellipsoid == null) {
				psSerial.FindProperty("ShapeModule.type").intValue = (int)(ShapeTypes.Mesh);
				//string[] types = new string [] {"Vertex", "Edge", "Triangle"};
				psSerial.FindProperty("ShapeModule.placementMode").intValue = (peSerial.FindProperty("m_InterpolateTriangles").boolValue ? 2 : 0);
				psSerial.FindProperty("ShapeModule.m_Mesh").objectReferenceValue = peSerial.FindProperty("m_Mesh").objectReferenceValue;
				primaryAmount += peSerial.FindProperty("m_MinNormalVelocity").floatValue;
				randomAmount += peSerial.FindProperty("m_MaxNormalVelocity").floatValue;
				primaryRatio = primaryAmount / randomAmount;
			} else {
				float minEmitterRange = peSerial.FindProperty("m_MinEmitterRange").floatValue;
				if(primaryRatio < 0.75f) {
					psSerial.FindProperty("ShapeModule.type").intValue = (int)(minEmitterRange > 0.75f ? ShapeTypes.SphereShell : ShapeTypes.Sphere);
				} else if(primaryRatio < 2.0f) {
					psSerial.FindProperty("ShapeModule.type").intValue = (int)(minEmitterRange > 0.75f ? ShapeTypes.HemiSphereShell : ShapeTypes.HemiSphere);
				} else {
					psSerial.FindProperty("ShapeModule.type").intValue = (int)(ShapeTypes.Cone);
					psSerial.FindProperty("ShapeModule.angle").floatValue = 90.0f / Mathf.Sqrt(primaryRatio);
				}
				psSerial.FindProperty("ShapeModule.radius").floatValue = ellipsoid.vector3Value.magnitude*0.5f;
			}
			psSerial.FindProperty("ShapeModule.randomDirection").boolValue = (primaryRatio < 0.8f);
			
			SetShurikenMinMaxCurve(psSerial, "InitialModule.startSpeed", primaryAmount, primaryAmount + randomAmount, true);
			
			psSerial.FindProperty("playOnAwake").boolValue = pe.emit;
			
			psSerial.FindProperty("InitialModule.inheritVelocity").floatValue = pe.emitterVelocityScale; 
			
			psSerial.FindProperty("InitialModule.gravityModifier").floatValue = -pa.force.y;
			
			bool sizeGrow = (pa.sizeGrow != 0);
			psSerial.FindProperty("SizeModule.enabled").boolValue = sizeGrow;
			if(sizeGrow) {
				SetShurikenMinMaxCurve(psSerial, "SizeModule.curve", 1.0f, 1.0f + (pa.sizeGrow * (pe.minEnergy + pe.maxEnergy) * 0.5f / ((pe.minSize + pe.maxSize) * 0.5f)), false);
			}
			
			bool dampen = (pa.damping != 1);
			psSerial.FindProperty("ClampVelocityModule.enabled").boolValue = dampen;
			if(dampen) {
				SetShurikenMinMaxCurve(psSerial, "ClampVelocityModule.magnitude", 0.0001f, 0.0001f, true);
				float d = 1.0f-pa.damping;
				psSerial.FindProperty("ClampVelocityModule.dampen").floatValue = d*d*d;
			}
			 
			psSerial.FindProperty("ColorModule.enabled").boolValue = pa.doesAnimateColor;
			if(pa.doesAnimateColor) {
				psSerial.FindProperty("ColorModule.gradient.minMaxState").intValue = 1;
		
				psSerial.FindProperty("ColorModule.gradient.maxGradient.key0").colorValue = pa.colorAnimation[0];
				psSerial.FindProperty("ColorModule.gradient.maxGradient.key1").colorValue = pa.colorAnimation[1];
				psSerial.FindProperty("ColorModule.gradient.maxGradient.key2").colorValue = pa.colorAnimation[2];
				psSerial.FindProperty("ColorModule.gradient.maxGradient.key3").colorValue = pa.colorAnimation[3];
				psSerial.FindProperty("ColorModule.gradient.maxGradient.key4").colorValue = pa.colorAnimation[4];
				psSerial.FindProperty("ColorModule.gradient.maxGradient.ctime0").intValue = 0;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.ctime1").intValue = 16383;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.ctime2").intValue = 32767;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.ctime3").intValue = 49151;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.ctime4").intValue = 65535;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.atime0").intValue = 0;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.atime1").intValue = 16383;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.atime2").intValue = 32767;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.atime3").intValue = 49151;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.atime4").intValue = 65535;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.m_NumColorKeys").intValue = 5;
				psSerial.FindProperty("ColorModule.gradient.maxGradient.m_NumAlphaKeys").intValue = 5;
			}
			
			bool hasTextureAnimation = pr.uvAnimationXTile > 1 || pr.uvAnimationYTile > 1;
			psSerial.FindProperty("UVModule.enabled").boolValue = hasTextureAnimation;
			psSerial.FindProperty("UVModule.tilesX").intValue = pr.uvAnimationXTile;
			psSerial.FindProperty("UVModule.tilesY").intValue = pr.uvAnimationYTile;
			psSerial.FindProperty("UVModule.cycles").floatValue = (int)pr.uvAnimationCycles;
			
			if(pr.particleRenderMode == ParticleRenderMode.SortedBillboard) psrSerial.FindProperty("m_SortMode").intValue = 1;

			psSerial.ApplyModifiedProperties(); 
			psrSerial.ApplyModifiedProperties();
			
			if(pr.particleRenderMode == ParticleRenderMode.Billboard || pr.particleRenderMode == ParticleRenderMode.SortedBillboard) newPSR.renderMode = ParticleSystemRenderMode.Billboard;
			if(pr.particleRenderMode == ParticleRenderMode.HorizontalBillboard) newPSR.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
			if(pr.particleRenderMode == ParticleRenderMode.VerticalBillboard) newPSR.renderMode = ParticleSystemRenderMode.VerticalBillboard;
			if(pr.particleRenderMode == ParticleRenderMode.Stretch) {
				newPSR.renderMode = ParticleSystemRenderMode.Stretch;
				newPSR.lengthScale = pr.lengthScale;
				newPSR.velocityScale = pr.velocityScale;
				newPSR.cameraVelocityScale = pr.cameraVelocityScale;
			}
			
			newPSR.maxParticleSize = pr.maxParticleSize;
			newPSR.sharedMaterials = pr.sharedMaterials;
			
			if(primaryDirection != Vector3.zero) oldGO.transform.rotation = Quaternion.LookRotation(primaryDirection);
			
			if(pe) Object.DestroyImmediate(pe);
			if(pr) Object.DestroyImmediate(pr);
			if(pa) Object.DestroyImmediate(pa);
		}
		
		float dist = GetBounds(systemParent);
		legacyParent.position -= Vector3.right * dist;	
		
		return systemParent;
	}
	
	
	public static void SetShurikenMinMaxCurve (SerializedObject psSerial, string moduleAndCurveName, float minValue, float maxValue, bool random) {
			int minMax = (Mathf.Approximately(minValue, maxValue) ? 0 : (random ? 3 : 1));
			psSerial.FindProperty(moduleAndCurveName + ".minMaxState").intValue = minMax;
			SerializedProperty scalarCurve = psSerial.FindProperty(moduleAndCurveName + ".scalar");
		
			float absMin = Mathf.Abs(minValue);
			float absMax = Mathf.Abs(maxValue);
		
			float newScalar = absMax > absMin ? absMax : absMin;
			scalarCurve.floatValue = newScalar;
			newScalar =  Mathf.Max(newScalar, 0.0001f);
			
			if(minMax == 1) {
				SerializedProperty maxCurve = psSerial.FindProperty(moduleAndCurveName + ".maxCurve");
	
				Keyframe[] keys = new Keyframe[2];
				keys[0] = new Keyframe(0f, minValue / newScalar);
				keys[0] = new Keyframe(1f, maxValue / newScalar);
				maxCurve.animationCurveValue = new AnimationCurve (keys);
			} else {
				SerializedProperty maxCurve = psSerial.FindProperty(moduleAndCurveName + ".maxCurve");
				SerializedProperty minCurve = psSerial.FindProperty(moduleAndCurveName + ".minCurve");
	
				Keyframe[] keys = new Keyframe[1];
				keys[0] = new Keyframe(0f, minValue / newScalar);
				minCurve.animationCurveValue = new AnimationCurve (keys);
				keys = new Keyframe[1];
				keys[0] = new Keyframe(0f, maxValue / newScalar);
				maxCurve.animationCurveValue = new AnimationCurve (keys);
			}
	}
	
	static ParticleEmitter[] GetAllParticleEmitters (Transform p) {
		List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>(); 
		FindAllLegacyParticles(particleEmitters, p);
		ParticleEmitter[] res = new ParticleEmitter[particleEmitters.Count];
		int i = 0;
		foreach(ParticleEmitter pe in particleEmitters) {
			res[i] = pe;
			i++;
		}
		return res;
	}
	
	static void FindAllLegacyParticles(List<ParticleEmitter> listP, Transform p) {
		foreach(Transform c in p) {
			FindAllLegacyParticles(listP, c);
		}
		ParticleEmitter pe = p.GetComponent<ParticleEmitter>();
		if(pe) listP.Add(pe);
	}
	
	static float GetBounds (Transform systemParent) {
		float increment = 0.3f;
		ParticleSystem[] particleSystems = GetAllParticleSystems(systemParent);
		ParticleSystem.Particle[] ps = new ParticleSystem.Particle[10000];
		Bounds systemBounds = new Bounds(systemParent.position, Vector3.one*0.0001f);
		float maxSize = 0;
		for (int i = 0; i < 16; i++) {
			float time = (i * increment + 0.035f);

			foreach(ParticleSystem e in particleSystems) {
				if(e.startSize > maxSize) maxSize = e.startSize; 
				e.Simulate (time, true); 
				int particleCount = e.GetParticles (ps);

				for (int i1 = 0; i1 < particleCount; i1++) {
					systemBounds.Encapsulate(ps[i].position);
				}
			}
		}
		return systemBounds.size.x;
	}
		
	static ParticleSystem[] GetAllParticleSystems (Transform p) {
		List<ParticleSystem> particleSystems = new List<ParticleSystem>(); 
		FindAllParticleSystems(particleSystems, p);
		ParticleSystem[] res = new ParticleSystem[particleSystems.Count];
		int i = 0;
		foreach(ParticleSystem ps in particleSystems) {
			res[i] = ps;
			i++;
		}
		return res;
	}
	
	static void FindAllParticleSystems(List<ParticleSystem> listP, Transform p) {
		foreach(Transform c in p) {
			FindAllParticleSystems(listP, c);
		}
		ParticleSystem ps = p.GetComponent<ParticleSystem>();
		if(ps) listP.Add(ps);
	}
}
