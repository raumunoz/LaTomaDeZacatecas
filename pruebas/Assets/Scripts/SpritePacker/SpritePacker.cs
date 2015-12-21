using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

//public class Int2 { 
//	public int x; 
//	public int y; public Int2 (int var1, int var2) { x = var1; y = var2; }}

public class SpritePacker : EditorWindow {
	string atlasName = "New Atlas";
	Camera renderCamera;
	float cameraPad = 1.0f;
	float lastCameraPad = 0f;
	Transform systemParent;
	BoxCollider boundsOverride;
	Transform systemTarget;
	Transform oldSystemParent;
	ParticleSystem[] particleSystems;
	ParticleEmitter[] particleEmitters;
	Animation[] animations;
	SpritePackerAnimation[] providers;
	Renderer[] renderers;
	float systemDuration;
	bool overrideSystemDuration = false;
	bool looping = false;
	bool loopConsensus = false;
	float customDuration = 2.0f;
	int numberOfColumns = 4;
	int numberOfRows = 4;
	string[] texSizeName = new string[] {"8", "16", "32", "64", "128", "256", "512", "1024", "2048"};
	int[] texSizes = new int[] {8, 16, 32, 64, 128, 256, 512, 1024, 2048};
	int atlasSizeX = 512;
	int atlasSizeY = 512;
	int cellSizeX = 0;
	int cellSizeY = 0;
	Color ambientLightColor = Color.gray;
	Color dirLightColor = Color.gray;
	bool parentIsOk = false;
	bool hasLegacy = false;
	bool hasRenderer = false;
	int useLayer = 10;
	int finalCellCount = 0;
	float increment = 0f;
	float finalTime = 0f;
	float lastUpdate = 0f;
	bool advanced = false;
	bool targetError = false;
	Vector3 viewDirection = Vector3.forward;
	Vector3 lightDirection = -Vector3.one;
	bool rotate = false;
	float rotationTime = 1f;
	Vector3 rotationAxis = Vector3.one;
	float rotationCycles = 1f;
	public float autoHueAdjust = 15f;
	Bounds systemBounds = new Bounds();
	Vector3 usedBoundsCenter;
	BackgroundColor bgColor;
	string path;
	
	Shader addAlpha;
	Shader blendAlpha;
	Shader solidAlpha;
	Shader addPlusOne;
	Shader addSoftPlusOne;
	
	enum BackgroundColor {Black, Gray}
	
	GUIStyle redBold;
	
	static bool hasGUI = false;
	
	struct OriginalRendererState {
		public Material[] materials;
		public int layer;
	}
	
	//ShapeModuleUI.cs line 29
	enum ShapeTypes { Sphere, SphereShell, HemiSphere, HemiSphereShell, Cone, Box, Mesh };

	[MenuItem ("Window/Sprite Packer")]
	static void Init () {
		SpritePacker window = (SpritePacker)EditorWindow.GetWindow (typeof (SpritePacker));
		window.Show ();
		window.Initialize();
	}
	
	public void Initialize () {
		redBold = new GUIStyle();
		redBold.normal.textColor = Color.red;
		redBold.padding.left = 10;
		path = atlasName + ".png";
		
		systemParent = Selection.activeTransform;
	}
	
	void OnDestroy() {
		hasGUI = false;
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}
	
	void OnGUI () {
		
		if(!hasGUI) {
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			hasGUI = true;
		}
		if(cameraPad != lastCameraPad) {
			if(SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.Repaint();	
		}
		lastCameraPad = cameraPad;
		
		GUILayout.Space (10);
		systemParent = EditorGUILayout.ObjectField ("Source: ", systemParent, typeof (Transform), true) as Transform;
		
		float curTime = Time.realtimeSinceStartup;
		if(systemParent != oldSystemParent || curTime - lastUpdate > 0.5f) {
			if(systemParent != oldSystemParent) systemTarget = null;
			UpdateParent(systemParent);
			lastUpdate = curTime;
		}
		
		if(hasLegacy) {
			GUILayout.BeginHorizontal();
			GUILayout.Label ("Warning: Legacy particles cannot be baked.", redBold);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button ("Convert to Shuriken")) {
				ConvertLegacyParticles();
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
		
		if (parentIsOk) {
			if(systemTarget) {
				systemTarget = EditorGUILayout.ObjectField ("Destination: ", systemTarget, typeof (Transform), true) as Transform;
				targetError = false;
				
				//PrefabType t = PrefabUtility.GetPrefabType(systemTarget);
				//if(t != PrefabType.None &&  t != PrefabType.PrefabInstance) {
				if(EditorUtility.IsPersistent(systemTarget)) {
					systemTarget = null;
					targetError = true;
				}
			}
			if(targetError) {
				GUILayout.Label ("Destination cannot be a prefab.\nMake it a prefab it after it is generated.", EditorStyles.boldLabel);
			}
			
			GUILayout.Label ("Bake settings:");
			overrideSystemDuration = EditorGUILayout.Toggle ("Override System Duration", overrideSystemDuration);
			if (!overrideSystemDuration)
				GUILayout.Label ("System Duration:    " + systemDuration.ToString ());
			if (overrideSystemDuration)
				customDuration = EditorGUILayout.FloatField ("Custom Duration", customDuration);
			
			if(!loopConsensus) looping = EditorGUILayout.Toggle ("Loop", looping);
			
			numberOfColumns = EditorGUILayout.IntField ("Columns: ", numberOfColumns);
			numberOfRows = EditorGUILayout.IntField ("Rows: ", numberOfRows);
			
			atlasSizeX = EditorGUILayout.IntPopup ("Texture Width: ", atlasSizeX, texSizeName, texSizes);
			atlasSizeY = EditorGUILayout.IntPopup ("Texture Height: ", atlasSizeY, texSizeName, texSizes);
			autoHueAdjust = EditorGUILayout.Slider ("Automatic Hue Adjustment", autoHueAdjust, 0f, 100f);
			
			GUILayout.Space (5);
			GUILayout.Label ("Camera settings:");
			boundsOverride = EditorGUILayout.ObjectField ("Bounds Override: ", boundsOverride, typeof (BoxCollider), true) as BoxCollider;
			cameraPad = EditorGUILayout.FloatField ("Camera Size Scale: ", cameraPad);
			
			GUILayout.Space (5);
			//advanced = EditorGUILayout.Toggle ("Advanced options", advanced);
			
			//if(advanced) {
				//bgColor = (BackgroundColor)EditorGUILayout.IntPopup ("Base Render On: ", (int)bgColor, new string[] {"Black (best color)", "Gray (supports more shaders)"}, new int[] {(int)BackgroundColor.Black, (int)BackgroundColor.Gray});				
				//viewDirection = EditorGUILayout.Vector3Field ("View Direction: ", viewDirection);
				GUILayout.Space (5);
				GUILayout.Label ("Settings for solid objects:", EditorStyles.label);
				lightDirection = EditorGUILayout.Vector3Field ("Light Direction: ", lightDirection);
				dirLightColor = EditorGUILayout.ColorField("Light Color: ", dirLightColor);
				ambientLightColor = EditorGUILayout.ColorField("Ambient Color: ", ambientLightColor);
				GUILayout.Space (5);
				rotate = EditorGUILayout.Toggle ("Rotate: ", rotate);
				if(rotate) {
					rotationTime = EditorGUILayout.FloatField ("Duration: ", rotationTime);
					rotationAxis = EditorGUILayout.Vector3Field ("Rotation Axis: ", rotationAxis);
					rotationCycles = EditorGUILayout.FloatField ("Cycles: ", rotationCycles);
				}
			//}
			
			GUILayout.Space (10);
			
			finalCellCount = numberOfColumns * numberOfRows;
			finalTime = (overrideSystemDuration) ? customDuration : systemDuration;
			//	This would normally be: finalTime / (finalCellCount - 1); but we don't want the last frame.
			increment = finalTime / finalCellCount;
			cellSizeX = (atlasSizeX/numberOfColumns);// * 2;
			cellSizeY = (atlasSizeY/numberOfRows)	;// * 2;
			
			GUILayout.Label (finalCellCount + " " + (atlasSizeX/numberOfColumns) + "x" + (atlasSizeY/numberOfRows) + " frames at " +  (1f/increment).ToString ("##") + " fps.");
			
			if(viewDirection == Vector3.zero) {
				GUILayout.Label ("View Direction cannot be zero!!", redBold);
			} else if (GUILayout.Button ("Bake Atlas", GUILayout.MaxWidth (200))) {
				BakeAtlas ();
			}
		} else if(!hasLegacy) {
			if(!hasRenderer) {
				GUILayout.Label ("Please assign a Source that contains a renderer", EditorStyles.boldLabel);
			} else {
				GUILayout.Label ("Please assign a Source that contains an\nanimation or particle system or enable rotation", EditorStyles.boldLabel);	
				rotate = EditorGUILayout.Toggle ("Rotate: ", rotate);
			}
		}
		oldSystemParent = systemParent;
	}
	
	//http://answers.unity3d.com/questions/58018/drawing-to-the-scene-from-an-editorwindow.html
	public void OnSceneGUI (SceneView scnView) {
		
		bool found = false;
		Transform t = Selection.activeTransform;
		if(t == systemParent || (t && t.GetComponent<BoxCollider>() == boundsOverride)) found = true;
		while(t != null) {
			t = t.parent;
			if(t == systemParent) found = true;
		}
		
		if(!found) return;
		
		Color hc = Handles.color;
		
		if(viewDirection != Vector3.zero) {
			Vector3 camPos = usedBoundsCenter - viewDirection.normalized*systemBounds.size.z*1.4f * cameraPad;
			float aspect = (float)cellSizeX / cellSizeY;
			float orthoSize = Mathf.Max(systemBounds.extents.x/aspect, systemBounds.extents.y) * cameraPad;
	        float nearClip = systemBounds.size.z * 0.25f * cameraPad;
	        float farClip = systemBounds.size.z * 3 * cameraPad;

			
			Vector3[] cornersN = {new Vector3(1,1,1), new Vector3(1,-1,1), new Vector3(-1,-1,1), new Vector3(-1,1,1)};
			Vector3[] cornersF = {new Vector3(1,1,1), new Vector3(1,-1,1), new Vector3(-1,-1,1), new Vector3(-1,1,1)};
			
			for(int i = 0; i < 4; i++) cornersN[i] = camPos+Quaternion.LookRotation(viewDirection)*Vector3.Scale(cornersN[i], new Vector3(orthoSize*aspect, orthoSize, nearClip));
			for(int i = 0; i < 4; i++) cornersF[i] = camPos+Quaternion.LookRotation(viewDirection)*Vector3.Scale(cornersF[i], new Vector3(orthoSize*aspect, orthoSize,  farClip));
			
			Handles.color = new Color(1,1,1,1f);
			
			for(int i = 0; i < 4; i++) {
				int p = i+1; if(p == 4) p = 0;
				Handles.DrawLine(cornersN[i], cornersN[p]);	
				Handles.DrawLine(cornersF[i], cornersF[p]);	
				Handles.DrawLine(cornersN[i], cornersF[i]);	
			}
			Handles.CircleCap(0, camPos, Quaternion.LookRotation(viewDirection), orthoSize * 0.3f);
		}
		if(boundsOverride) {
			Handles.color = new Color(0.5f,1,0.5f,0.7f);
			Vector3[] cornersN = {new Vector3(1,1,1), new Vector3(1,-1,1), new Vector3(-1,-1,1), new Vector3(-1,1,1)};
			Vector3[] cornersF = {new Vector3(1,1,1), new Vector3(1,-1,1), new Vector3(-1,-1,1), new Vector3(-1,1,1)};
			
			for(int i = 0; i < 4; i++) cornersN[i] = usedBoundsCenter+Vector3.Scale(cornersN[i], new Vector3(systemBounds.extents.x, systemBounds.extents.y, -systemBounds.extents.z));
			for(int i = 0; i < 4; i++) cornersF[i] = usedBoundsCenter+Vector3.Scale(cornersF[i], new Vector3(systemBounds.extents.x, systemBounds.extents.y,  systemBounds.extents.z));
			
			for(int i = 0; i < 4; i++) {
				int p = i+1; if(p == 4) p = 0;
				Handles.DrawLine(cornersN[i], cornersN[p]);	
				Handles.DrawLine(cornersF[i], cornersF[p]);	
				Handles.DrawLine(cornersN[i], cornersF[i]);	
			}
			
			GetBounds();
		}
		Handles.color = hc;
	}
	
	void UpdateParent(Transform parent) {
		
		solidAlpha = Shader.Find ("Hidden/SolidAlpha");
		blendAlpha = Shader.Find ("Hidden/BlendAlpha");
		addAlpha = Shader.Find ("Hidden/AddAlpha");
		addPlusOne = Shader.Find ("Hidden/Additive+1");
		addSoftPlusOne = Shader.Find ("Hidden/Additive (Soft)+1");
		
		List<Renderer> theRenderers = new List<Renderer>();
		List<Animation> theAnimations = new List<Animation>();
		List<SpritePackerAnimation> theProviders = new List<SpritePackerAnimation>();
		List<ParticleSystem> theParticleSystems = new List<ParticleSystem>();
		List<ParticleEmitter> theParticleEmitters = new List<ParticleEmitter>();
		if(parent != null) FindAllInterestingComponents(theRenderers, theAnimations, theProviders, theParticleSystems, parent);
		if(parent != null) FindAllLegacyParticles(theParticleEmitters, parent);
		renderers = new Renderer[theRenderers.Count];
		animations = new Animation[theAnimations.Count];
		providers = new SpritePackerAnimation[theProviders.Count];
		particleSystems = new ParticleSystem[theParticleSystems.Count];
		particleEmitters = new ParticleEmitter[theParticleEmitters.Count];
		int i0 = 0;
	    foreach(Renderer r in theRenderers) 			{ renderers 	 [i0] = r; i0++; } i0 = 0;
		foreach(Animation a in theAnimations) 			{ animations	 [i0] = a; i0++; } i0 = 0;
		foreach(SpritePackerAnimation s in theProviders) { providers[i0] = s; i0++; } i0 = 0;
		foreach(ParticleSystem p in theParticleSystems) { particleSystems[i0] = p; i0++; } i0 = 0;
		foreach(ParticleEmitter p in theParticleEmitters) { particleEmitters[i0] = p; i0++; }	
		
		parentIsOk = (renderers.Length > 0 && (animations.Length > 0 || providers.Length > 0  || particleSystems.Length > 0 || rotate));
		//Debug.Log ("leg" + Random.value + "   " + particleEmitters.Length);
		hasLegacy = particleEmitters.Length > 0;
		hasRenderer = renderers.Length > 0;
		
		//Debug.Log (parentIsOk + " PArent is ok "+ Random.value);
		
		if(parentIsOk) {
			systemDuration = 0;
			bool newLoop = false;
			loopConsensus = true;
			if(particleSystems.Length > 0) newLoop = particleSystems[0].loop;
			else if(providers.Length > 0) newLoop = providers[0].loop;
			else if(animations.Length > 0) newLoop = (animations[0].wrapMode == WrapMode.Loop || animations[0].wrapMode == WrapMode.PingPong);
			else newLoop = rotate;
			if(rotate) systemDuration = rotationTime;
			foreach(Animation a in animations) {
				if( a.clip && (a.wrapMode == WrapMode.Loop || a.wrapMode == WrapMode.PingPong) != newLoop) loopConsensus = false;
				if( a.clip && a.clip.length > 	systemDuration) systemDuration = a.clip.length;
			}
			foreach(SpritePackerAnimation a in providers) {
				if(a.loop != newLoop) loopConsensus = false;
				if(a.animationLength > 	systemDuration) systemDuration = a.animationLength;
			}
			foreach(ParticleSystem e in particleSystems) {
				if(e.loop != newLoop) loopConsensus = false;
				float d = (looping ? 0 : e.startDelay) + (e.emissionRate == 0 && !looping ? 0 : e.duration) + (looping ? 0 : e.startLifetime);
				if(d > systemDuration) systemDuration = d;
			}
			if(loopConsensus) looping = newLoop;
		}
		
		GetBounds();
		
		if(SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.Repaint();
	}
	
	
	
	void BakeAtlas () {
		
		UpdateParent(systemParent);
		
		if(!parentIsOk) return;
		if(viewDirection == Vector3.zero) return;
		
		finalTime = (overrideSystemDuration) ? customDuration : systemDuration;
       
        GameObject cam = new GameObject("__Camera", typeof(Camera));
        cam.transform.rotation = Quaternion.LookRotation(viewDirection);
		renderCamera = cam.GetComponent<Camera>();
        renderCamera.orthographic = true;
        renderCamera.backgroundColor = bgColor == BackgroundColor.Black ? new Color(0, 0, 0, 0) : new Color(0.5f, 0.5f, 0.5f, 0);
       	renderCamera.cullingMask = 1 << useLayer;
       
        GameObject lighte = new GameObject("__Light", typeof(Light));
        lighte.transform.rotation = Quaternion.LookRotation(lightDirection);
        lighte.GetComponent<Light>().renderMode = LightRenderMode.ForcePixel;
        lighte.GetComponent<Light>().type = LightType.Directional;
        lighte.GetComponent<Light>().color = dirLightColor;
		
        RenderTexture rt = new RenderTexture(cellSizeX, cellSizeY, 16);
        rt.isPowerOfTwo = true;
        renderCamera.targetTexture = rt;
		
		bool oldfog = RenderSettings.fog;
		Color oldAmbient = RenderSettings.ambientLight;
		RenderSettings.fog = false;
		RenderSettings.ambientLight = ambientLightColor;
		
		bool hasAlpha = false;
		bool hasSolid = false;
		bool hasAdditive = false;
		OriginalRendererState[] oldStates = new OriginalRendererState[renderers.Length];
		for(int i1 = 0; i1 < renderers.Length; i1++) {
			OriginalRendererState s = new OriginalRendererState();
			s.layer = renderers[i1].gameObject.layer;
			s.materials = renderers[i1].sharedMaterials;
			List<Material> newMaterials = new List<Material>();
			foreach (Material m in renderers[i1].sharedMaterials) {
				if(!hasAlpha) hasAlpha = m.shader.name.Contains("Alpha Blended");
				if(!hasSolid) hasSolid = !m.shader.name.Contains("Particle") && !m.shader.name.Contains("Transparent");
				if(!hasAdditive) hasAdditive = m.shader.name.Contains("Additive");
			}
			foreach (Material m in renderers[i1].sharedMaterials) {
				bool bg = m.shader.name.Contains("Particle") && m.shader.name.Contains("Additive") && !hasAlpha;
				if(bg) {
					Material newMat = new Material(m);
					if(newMat.HasProperty("_Color")) newMat.color = Color.Lerp(newMat.color, Color.clear, 0.3f);
					if(newMat.HasProperty("_TintColor")) newMat.SetColor("_TintColor", Color.Lerp(newMat.GetColor("_TintColor"), Color.clear, 0.3f));
					newMat.shader = Shader.Find("Particles/Alpha Blended");
					newMaterials.Add(newMat);
				}
				Material newMat1 = new Material(m);
				if(newMat1.shader.name == "Particles/Additive") newMat1.shader = addPlusOne;
				if(newMat1.shader.name == "Particles/Additive (Soft)") newMat1.shader = addSoftPlusOne;
				if(bg) {
					if(newMat1.HasProperty("_Color")) newMat1.color = Color.Lerp(newMat1.color, Color.clear, 0.3f);
					if(newMat1.HasProperty("_TintColor")) newMat1.SetColor("_TintColor", Color.Lerp(newMat1.GetColor("_TintColor"), Color.clear, 0.3f));
				}
				newMaterials.Add (newMat1);
			}
			renderers[i1].sharedMaterials = newMaterials.ToArray();
        	oldStates[i1] = s;
			renderers[i1].gameObject.layer = useLayer;
        }
		
		
		//	sets systemBounds & usedBoundsCenter
		GetBounds();
		
		cam.transform.position = usedBoundsCenter - viewDirection.normalized*systemBounds.size.z*1.4f * cameraPad;
		renderCamera.aspect = (float)cellSizeX / cellSizeY;
		renderCamera.orthographicSize = Mathf.Max(systemBounds.extents.x/renderCamera.aspect, systemBounds.extents.y) * cameraPad;
        renderCamera.nearClipPlane = systemBounds.size.z * 0.25f * cameraPad;
        renderCamera.farClipPlane = systemBounds.size.z * 3 * cameraPad;
		

        Texture2D copyTex = new Texture2D( cellSizeX, cellSizeY, TextureFormat.RGB24, false );
		//Texture2D copyTex2 = new Texture2D( cellSizeX*0.5f, cellSizeY*0.5f, TextureFormat.RGB24, false );
        Texture2D tex = new Texture2D( atlasSizeX, atlasSizeY, TextureFormat.ARGB32, false );
        Texture2D texA = new Texture2D( atlasSizeX, atlasSizeY, TextureFormat.ARGB32, false );
		
		RenderSheet(copyTex, renderCamera, tex);
		
		renderCamera.backgroundColor = Color.black;
		for(int i1 = 0; i1 < renderers.Length; i1++) {
			List<Material> newMaterials = new List<Material>();
			foreach (Material m in oldStates[i1].materials) {
				Material newMat = new Material(m);
				if(m.shader.name.Contains("Particle")) {
					newMat.shader = m.shader.name.Contains("Additive") ? addAlpha : blendAlpha;
				} else {
					newMat.shader = solidAlpha;
				}
				newMaterials.Add(newMat);
			}
			renderers[i1].sharedMaterials = newMaterials.ToArray();
        }
		
		RenderSheet(copyTex, renderCamera, texA);
		
		for(var i1 = 0; i1 < renderers.Length; i1++) { 
			renderers[i1].gameObject.layer = oldStates[i1].layer;
			renderers[i1].sharedMaterials = oldStates[i1].materials;
        }
		
        int x = 0;
        int y = 0;
		float avgHueTilt=0f;
		float avgHueDist=0f;
        while(x < tex.width) {
            y = 0;
            while(y < tex.height) {
                Color c = tex.GetPixel(x, y);
				HSBColor hsb = HSBColor.FromColor(c);
				Color cA = texA.GetPixel(x, y);
				float a = (cA.r + cA.g + cA.b) * 0.33f;
				Vector4 cv = new Vector4(c.r, c.g, c.b, 1);
				if(bgColor == BackgroundColor.Gray) {
					Vector4 delta = new Vector4(c.r - 0.5f, c.g - 0.5f, c.b - 0.5f, 1);
					if(a != 0)  cv += delta / a;
				} else {
					cv /= a;	
				}
				Color nc = new Color(cv.x, cv.y, cv.z, a*a);
				HSBColor nhsb = HSBColor.FromColor(nc);
				nhsb.h = hsb.h;
				float tilt = nhsb.h - (float)((int)nhsb.h % 60);
				avgHueTilt += tilt;
				avgHueDist += Mathf.Abs (tilt);
				tex.SetPixel(x, y, HSBColor.ToColor(nhsb));
                y++;   
            }      
            x++;
        }
		if(autoHueAdjust > 1f) {
			avgHueDist /= (tex.width*tex.height);
			for(x = 0; x < tex.width; x++) {
				for(y=0; y<tex.height; y++) {
					Color c = tex.GetPixel(x, y);
					HSBColor hsb = HSBColor.FromColor(c);
					hsb.h += avgHueDist * (autoHueAdjust*0.01f) * (avgHueTilt > 0 ? -1 : 1);
					tex.SetPixel(x, y, HSBColor.ToColor(hsb));
				}
			}
		}
        tex.Apply();
        
	    path = EditorUtility.SaveFilePanelInProject("Save texture as PNG", path, "png", "Please enter a file name to save the texture to");
	    if(path.Length != 0) {
	        byte[] bytes = tex.EncodeToPNG();
	        if(bytes != null) {
				File.WriteAllBytes(path, bytes);
	            AssetDatabase.Refresh();
	        }
	    }

        RenderTexture.active = null;

        DestroyImmediate (copyTex);
        DestroyImmediate (tex);
		DestroyImmediate (texA);
        DestroyImmediate (rt);
		DestroyImmediate (lighte);

        RenderSettings.fog = oldfog;
        RenderSettings.ambientLight = oldAmbient;
       
		float origSize = renderCamera.orthographicSize*2;
		
		Vector3 pos = systemParent.position + Vector3.right * origSize;
		Quaternion rot = systemParent.rotation;
		Vector3 localScale = Vector3.one; 
		Transform p = null;
				
		if(systemTarget) {
			pos = systemTarget.position; 
			rot = systemTarget.rotation;
			localScale = systemTarget.localScale;
			p = systemTarget.parent;
			DestroyImmediate(systemTarget.gameObject);
		}

		systemTarget = Instantiate(systemParent, pos, rot) as Transform;
		systemTarget.gameObject.name = systemParent.gameObject.name + " packed";
		systemTarget.parent = p;
		systemTarget.localScale = localScale;
		
		List<Renderer> theRenderers = new List<Renderer>();
		List<Animation> theAnimations = new List<Animation>();
		List<SpritePackerAnimation> theProviders = new List<SpritePackerAnimation>();
		List<ParticleSystem> theParticleSystems = new List<ParticleSystem>();
		//List<ParticleEmitter> theParticleEmitters = new List<ParticleEmitter>();
		//FindAllInterestingComponents(theRenderers, theAnimations, theProviders, theParticleSystems, theParticleEmitters, systemTarget);
		FindAllInterestingComponents(theRenderers, theAnimations, theProviders, theParticleSystems, systemTarget);
		foreach(Renderer r in theRenderers)  DestroyImmediate(r);
		foreach(Animation r in theAnimations)  DestroyImmediate(r);
		//foreach(SpritePackerAnimation r in theProviders)  Destroy(r);
		foreach(ParticleSystem r in theParticleSystems)  DestroyImmediate(r);
		//foreach(ParticleEmitter r in theParticleEmitters)  DestroyImmediate(r);
		List<GameObject> objects = new List<GameObject>();
		FindAllGameObjects(objects, systemTarget);
		foreach(GameObject g in objects) {
			g.transform.parent = systemTarget;	
		}
		foreach(GameObject g in objects) {
			Component[] cs = g.GetComponents<Component>();
			if(cs.Length == 1) DestroyImmediate(g);
		}
		
		GameObject go = new GameObject(systemParent.name + " sprite");
		go.transform.parent = systemTarget;
		go.transform.localPosition = systemParent.InverseTransformPoint(usedBoundsCenter);
		
		ParticleSystem ps = go.AddComponent<ParticleSystem>();
		ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
		SerializedObject psSerial = new SerializedObject (ps);
		SerializedObject psrSerial = new SerializedObject (psr);
		
		psSerial.Update ();
		psrSerial.Update ();
				
		psSerial.FindProperty("UVModule.enabled").boolValue = true;
		psSerial.FindProperty("UVModule.tilesX").intValue = numberOfColumns;
		psSerial.FindProperty("UVModule.tilesY").intValue = numberOfRows;
	
		psSerial.FindProperty("EmissionModule.m_BurstCount").intValue = 1;
		psSerial.FindProperty("EmissionModule.time0").floatValue = 0;
		psSerial.FindProperty("EmissionModule.cnt0").intValue = 1;
		LegacyToShurikenConverter.SetShurikenMinMaxCurve(psSerial, "EmissionModule.rate", 0, 0, true);

		psSerial.FindProperty("lengthInSec").floatValue = finalTime;
		LegacyToShurikenConverter.SetShurikenMinMaxCurve(psSerial, "InitialModule.startLifetime", finalTime-0.01f, finalTime-0.01f, true);
		LegacyToShurikenConverter.SetShurikenMinMaxCurve(psSerial, "InitialModule.startSize", origSize, origSize, true);

		psSerial.FindProperty("ShapeModule.radius").floatValue = 0;
		
		psSerial.FindProperty("ShapeModule.type").intValue = (int)ShapeTypes.Sphere;
		
		LegacyToShurikenConverter.SetShurikenMinMaxCurve(psSerial, "InitialModule.startSpeed", 0, 0, true);
		
		psSerial.ApplyModifiedProperties();
		psrSerial.ApplyModifiedProperties();
	
		string name1 = hasAlpha || hasSolid || !hasAdditive ? "SpritePacker/PackedBlend" : "SpritePacker/PackedAdditive";
		if(hasSolid && !hasAlpha && !hasAdditive) name1 = "SpritePacker/PackedCutout";
		Material mat = new Material(Shader.Find(name1));
		mat.mainTexture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
		
		psr.sharedMaterial = mat;
		
		DestroyImmediate (cam);
		
        Debug.Log("Create Particle Animation done, saved to:  " + path);
		
		if(path.LastIndexOf("/") != -1) path = path.Substring(path.LastIndexOf("/")+1);
	}

	
	void ConvertLegacyParticles () {
		systemParent = LegacyToShurikenConverter.Convert(systemParent);
		UpdateParent(systemParent);
	}
	
	void RenderSheet(Texture2D copyTex, Camera renderCamera, Texture2D texture) {
		float timeValue = 0.0f;
		int j, k;													//	Establish a variable for our columns & rows 
		
		int timeSamples = 5;
		foreach(SpritePackerAnimation a in providers) {
			a.Init();
			a.Sample(0, Vector3.zero);
		}
		foreach(Animation a in animations) {
			a.clip.SampleAnimation(a.gameObject, 0);
		}
		foreach(ParticleSystem e in particleSystems) {
			e.Clear(true);
			e.Pause();
		}
		
		for (int i = 0; i < finalCellCount*timeSamples; i++) {

			float time = i * (increment/timeSamples) + 0.035f;
			if(rotate) {
				systemParent.rotation = Quaternion.AngleAxis(time/finalTime * 360.0f * rotationCycles, rotationAxis.normalized);	
			}
			foreach(ParticleSystem e in particleSystems) {
				e.Simulate (time, true);
			}
			foreach(Animation a in animations) {
				a.clip.SampleAnimation(a.gameObject, time*a.clip.length);
			}
			foreach(SpritePackerAnimation a in providers) {
				a.Sample(time, renderCamera.transform.position);
			}

			//	SnapShot
			if((i/timeSamples)*timeSamples == i) {
				int usedI = i/timeSamples;
				j = usedI %	numberOfColumns;									//	... find the remainder by width to get the column...
				k = usedI / numberOfColumns;								//	... devide by width to get the row...
				
				renderCamera.Render ();
				RenderTexture.active = renderCamera.targetTexture;
				copyTex.ReadPixels (new Rect (0, 0, cellSizeX, cellSizeY), 0, 0);
				Color[] pixels = copyTex.GetPixels (0, 0, cellSizeX, cellSizeY);
				texture.SetPixels (j * cellSizeX, ((numberOfRows - k) * cellSizeY) - cellSizeY, cellSizeX, cellSizeY, pixels);
			}
		}
		foreach(ParticleSystem e in particleSystems) {
			e.Clear(true);
			e.Pause();
		}
		foreach(SpritePackerAnimation a in providers) {
			a.Sample(0, Vector3.zero);
		}
	}
	
	//void FindAllInterestingComponents(List<Renderer> listR, List<Animation> listA, List<SpritePackerAnimation> listS, List<ParticleSystem> listP, List<ParticleEmitter> listPl, Transform p) {
	void FindAllInterestingComponents(List<Renderer> listR, List<Animation> listA, List<SpritePackerAnimation> listS, List<ParticleSystem> listP, Transform p) {
		foreach(Transform c in p) {
			//FindAllInterestingComponents(listR, listA, listS, listP, listPl, c);
			FindAllInterestingComponents(listR, listA, listS, listP, c);
		}
		Renderer r = p.GetComponent<Renderer>();
		if(r) listR.Add(r);
		Animation a = p.GetComponent<Animation>();
		if(a && a.clip) listA.Add(a);
		SpritePackerAnimation s = p.GetComponent<SpritePackerAnimation>();
		if(s) listS.Add(s);
		ParticleSystem pe = p.GetComponent<ParticleSystem>();
		if(pe) listP.Add(pe);
		//ParticleEmitter pel = p.particleEmitter;
		//if(pel) listPl.Add(pel);
	}
	
	void FindAllLegacyParticles(List<ParticleEmitter> listP, Transform p) {
		foreach(Transform c in p) {
			FindAllLegacyParticles(listP, c);
		}
		ParticleEmitter pe = p.GetComponent<ParticleEmitter>();
		//Debug.Log (p.gameObject.name);
		if(pe) listP.Add(pe);
	}
	
	void FindAllGameObjects(List<GameObject> list, Transform p) {
		foreach(Transform c in p) {
			FindAllGameObjects(list, c);
			list.Add(c.gameObject);
		}
	}
	
	void GetBounds () {
		if(boundsOverride == null) {	
			if(systemParent == null) return;
			systemBounds = new Bounds(systemParent.position, Vector3.one * 0.001f);

			ParticleSystem.Particle[] ps = new ParticleSystem.Particle[10000];
			float maxSize = 0;
			
			foreach(SpritePackerAnimation a in providers) {
				a.Init();
			}
			
			for (int i = 0; i < finalCellCount; i++) {
				float time = (i * increment + 0.035f);
				if(rotate) {
					systemParent.rotation = Quaternion.AngleAxis(((float)i*finalCellCount) *360, rotationAxis);	
				}
				foreach(Animation a in animations) {
					a[a.clip.name].time = time;
					a.Sample();
				}
				foreach(SpritePackerAnimation a in providers) {
					a.Sample(time, renderCamera.transform.position);
				}
				
				foreach(ParticleSystem e in particleSystems) {
					if(e.startSize > maxSize) maxSize = e.startSize; 
					e.Simulate (time, true); 
					int particleCount = e.GetParticles (ps);

					for (int i1 = 0; i1 < particleCount; i1++) {
						systemBounds.Encapsulate(ps[i].position);
					}
				}
				
				/*
				foreach(ParticleEmitter e in particleEmitters) {
					if(e.maxSize > maxSize) maxSize = e.maxSize; 
					e.Simulate (time); 
					Particle[] ps1 = e.particles;

					for (int i1 = 0; i1 < ps1.Length; i1++) {
						systemBounds.Encapsulate(ps1[i].position);
					}
				}
				*/
				
				foreach(Renderer r in renderers) {
					//if(!r.GetComponent<ParticleSystem>() && !r.GetComponent<ParticleEmitter>()) {
					if(!r.GetComponent<ParticleSystem>()) {
						systemBounds.Encapsulate(r.bounds.max);
						systemBounds.Encapsulate(r.bounds.min);
					}
				}
				
			}
			systemBounds.size += Vector3.one*maxSize;
		} else {
			systemBounds.center = boundsOverride.transform.TransformPoint(boundsOverride.center);
			systemBounds.size = Vector3.Scale(boundsOverride.transform.localScale, boundsOverride.size);
		}
		
		usedBoundsCenter = (boundsOverride != null ? systemBounds.center : systemParent.position);	
	}
}