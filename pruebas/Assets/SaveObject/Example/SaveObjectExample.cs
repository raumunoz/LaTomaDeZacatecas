using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveObjectExample : MonoBehaviour {
	public GameObject saveObject;
	private string saveName = "Default";
	/**
	* In order to use the SaveObject, we need to initialize it. 
	* It will create a SaveObject instance, make sure you link 
	* correctly to the save object either with a string with the 
	* Resources folder as root or a direct link to the prefab.
	**/
	void Start () {
		
		/**
		* You need to call SaveObject.Initialize before doing anything with SaveObject.
		**/

		SaveObject.Initialize(saveObject);
		
		/**
		* Set a callback for when you load a game like this, used to do complex updates
		* when needed.
		**/
		SaveObject.SetRefreshCallback(OnRefresh);
		
		/**
		* It is recommended that you Load right after initialization, it makes sure that
		* you always have a game ready to be used. 
		**/
		SaveObject.Load("Saved Game No1");
	}

	/**
	* Use the RefreshCallback to execute the extra logic it takes
	* to properly reflect the updated values of a loaded game or
	* when you manually update values in the inspector then press
	* "Call Refresh"
	**/
	private void OnRefresh() {
		

		UpdateLogo();
		saveName = SaveObject.saveName;
	}


	/**
	* Use SaveObject.Get<T>(); to get a saved component (component of the SaveObject)
	* A game must be loaded ino order to be able to use SaveObject.Get<T>()
	**/
	private void UpdateLogo() {
		
		GetComponent<SpriteRenderer>().color = SaveObject.Get<ExampleStats>().color;
		transform.position = SaveObject.Get<ExampleStats>().position;
	}

	/**
	* Save your game like this
	**/
	public void Save() {
		SaveObject.Save();
	}
	/**
	* Load your game like this
	**/
	public void Load() {
		SaveObject.Load(saveName);
	}

	/**
	* Click on the icon on the screen, it will change, you'll then be able to save its new state.
	**/
	void Shuffle() {
		SaveObject.Get<ExampleStats>().color = new Color(Random.value, Random.value, Random.value, 0.5f + Random.value * 0.5f);
		SaveObject.Get<ExampleStats>().position = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f));
		UpdateLogo();
	}

	void OnGUI() {
		float theY = 10;
		theY = SaveGUI(theY);
		theY += 66;
		InventoryGUI(theY);
	}

	private float SaveGUI(float theY) {
		GUI.Label(new Rect(10, theY, 1600, 20), "SaveObject Example: Select the \"SavedObject\" in the scene to see the changes");
		theY += 22;
		
		saveName = GUI.TextField(new Rect(10, theY, 200, 20), saveName);
		GUI.Label(new Rect(212, theY, 1600, 20), "<<Change Save File, load after to make effective");
		theY += 22;
		if (GUI.Button(new Rect(10, theY, 200, 20), "Shuffle")) {
			Shuffle();
		}
		GUI.Label(new Rect(212, theY, 1600, 20), "<<Press to change the state of the logo");
		theY += 22;
		if (GUI.Button(new Rect(10, theY, 200, 20), "Save")) {
			Save();
		}
		GUI.Label(new Rect(212, theY, 1600, 20), "<<Press to save the state of the logo and other stats");
		theY += 22;
		if (GUI.Button(new Rect(10, theY, 200, 20), "Load")) {
			Load();
		}
		GUI.Label(new Rect(212, theY, 1600, 20), "<<Press to load the state of the logo and other stats");
		theY += 22;
		if (GUI.Button(new Rect(10, theY, 200, 20), "Alt. Save")) {
			AlternativeSave();
		}
		GUI.Label(new Rect(212, theY, 1600, 20), "<<A save module that allows you to save the way you like");
		theY += 22;
		if (GUI.Button(new Rect(10, theY, 200, 20), "Alt. Load")) {
			AlternativeLoad();
		}
		GUI.Label(new Rect(212, theY, 1600, 20), "<<A load module that allows you to load the way you like");
		return theY;
	}

	private float InventoryGUI(float theY) {
		GUI.Label(new Rect(10, theY, 1600, 20), "Golds");
		theY += 22;
		int golds = 0;
		string newGolds = GUI.TextField(new Rect(10, theY, 200, 20), SaveObject.Get<Progress>().golds.ToString());
		if (newGolds.Length == 0) {
			SaveObject.Get<Progress>().golds = 0;
		} else if (int.TryParse(newGolds, out golds)) {
			SaveObject.Get<Progress>().golds = golds;
		}
		theY += 22;
		GUI.Label(new Rect(10, theY, 1600, 20), "Inventory");
		theY += 22;
		int i = SaveObject.Get<Progress>().inventory.Count;
		while (i-- > 0) {
			SaveObject.Get<Progress>().inventory[i] = GUI.TextField(new Rect(10, theY, 200, 20), SaveObject.Get<Progress>().inventory[i]);
			if (GUI.Button(new Rect(212, theY, 20, 20), "-")) {
				SaveObject.Get<Progress>().inventory.RemoveAt(i);
			} else {
				theY += 22;
			}

		}
		if (GUI.Button(new Rect(10, theY, 200, 20), "Add")) {
			SaveObject.Get<Progress>().inventory.Insert(0, "potion");
		}
		return theY;
	}

	private void AlternativeLoad() {
		string str = PlayerPrefs.GetString("alt" + SaveObject.saveName);
		
		if (string.IsNullOrEmpty(str)) {
			Debug.LogWarning("No file found, please previously Alt Save the file you want to Alt Load (press \"Alt. Save\")");
		}
		SaveObject.LoadFromString(str);
		Debug.Log("LOAD STRING TO USE AS YOU WISH:\n" + str);
	}

	private void AlternativeSave() {
		string str = SaveObject.GetSaveString();
		PlayerPrefs.SetString("alt" + SaveObject.saveName, str);
		Debug.Log("SAVE STRING TO USE AS YOU WISH:\n" + str);
	}
}
