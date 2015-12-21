
using UnityEngine;
using System;

[ExecuteInEditMode]
public class SaveObject : MonoBehaviour {
	#region hidden
	private static Action refreshCallback;
	private static GameObject resetOriginal;
	private static string _saveName = "Default";
	private static SaveObject _instance;
	private static SaveObject instance {
		get {
			if (_instance == null) {
				Debug.LogError("SaveObject not initialized");
			}
			return _instance;
		}
	}
	void Awake() {
		if (resetOriginal != null) {
			name = resetOriginal.name;
		}
		UniqueIdentifier.identifier = new UniqueIdentifier();
		UniqueIdentifier.identifier.Id = name;
		UniqueIdentifier.identifier.gameObject = gameObject;
		_instance = this;
		DontDestroyOnLoad(gameObject);
	}
	private void Refresh() {
		if (refreshCallback != null) {
			refreshCallback();
		}
	}
	#endregion

	public static string saveName {
		get {
			return _saveName;
		}
	}
	public string currentSaveString {
		get { return JSONLevelSerializer.Serialize(gameObject); }
	}

	/// <summary>
	///	Initialize Unity Saver
	/// </summary>
	/// <param name="originalPath"> The path to the original save object prefab in the Resources folder</param>
	public static void Initialize(string originalPath) {
		Initialize(Resources.Load(originalPath) as GameObject);
	}

	/// <summary>
	/// Initialize Unity Saver
	/// </summary>
	/// <param name="original"> The original save object prefab</param>
	public static void Initialize(GameObject original) {
		resetOriginal = original;
		Instantiate(original).name = original.name;
		SetRefreshCallback(refreshCallback);
		Save();
	}

	/// <summary>
	/// Set the action that will be called when you load a game or press refresh on the SaveObject Component
	/// </summary>
	/// <param name="refreshCallback">The action that will be called</param>
	public static void SetRefreshCallback(Action refreshCallback) {
		SaveObject.refreshCallback = refreshCallback;
	}

	/// <summary>
	/// Load a game
	/// </summary>
	/// <param name="saveName"> The name of the "file" you want to load, will load the default or currently loaded "file" if left blank</param>
	public static void Load(string saveName = "") {
		if (!string.IsNullOrEmpty(saveName)) {
			_saveName = saveName;
		}
		
		if (UnityEngine.PlayerPrefs.HasKey(_saveName)) {
			string save = UnityEngine.PlayerPrefs.GetString(_saveName);
			JSONLevelSerializer.Deserialize(save);
			instance.Refresh();
		} else {
			NewGame();
		}
		
		
	}

	/// <summary>
	/// Load a game from a provided string
	/// </summary>
	/// <param name="save"> Serialization string of the saved gameObject</param>
	public static void LoadFromString(string save) {
		if (string.IsNullOrEmpty(save)) {
			Debug.LogError("No save string provided. Please load a string previously output by \"SaveObject.GetSaveString()\"");
			return;
		}
		try {
			JSONLevelSerializer.Deserialize(save);
		} catch {
			Debug.LogError("Incompatible save string. Please load a string previously output by \"SaveObject.GetSaveString()\"");
		}
		instance.Refresh();
	}

	/// <summary>
	/// Save a game
	/// </summary>
	public static void Save() {
		string save = instance.currentSaveString;
		PlayerPrefs.SetString(_saveName, save);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Gets the string used to save the game
	/// </summary>
	/// <returns>the string used to save the game</returns>
	public static string GetSaveString() {
		return instance.currentSaveString;
	}

	/// <summary>
	/// Revert the load state to prefab original state
	/// </summary>
	public static void NewGame() {
		Destroy(instance.gameObject);
		Instantiate(resetOriginal);
		instance.Refresh();
	}

	/// <summary>
	/// Get a Component of the save object
	/// </summary>
	public static T Get<T>() where T : Component {
		Component comp = null;
		comp = instance.GetComponent<T>();
		if (comp == null) {
			Debug.LogError(typeof(T) + " not present in SaveObject");
		}
		return (T)comp;
	}
}

public class UniqueIdentifier {
	public string Id;
	public GameObject gameObject;
	public static UniqueIdentifier identifier;
}