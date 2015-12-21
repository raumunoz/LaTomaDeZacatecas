using System;
using System.Collections.Generic;
using UnityEngine;
using Serialization;
using System.Text;


public static class JSONLevelSerializer {
	
    public static Dictionary<Type, IComponentSerializer> CustomSerializers = new Dictionary<Type, IComponentSerializer>();

	
    public static string Serialize(GameObject rootOfTree) {
		LevelData ld;
		ld = new LevelData();
		UniqueIdentifier identifier = UniqueIdentifier.identifier;
		GameObject n = identifier.gameObject;
		ld.StoredObject = new StoredItem() { Name = identifier.Id, GameObjectName = n.name };
		Component[] toBeProcessed = UniqueIdentifier.identifier.gameObject.GetComponents<Component>();
		int i = toBeProcessed.Length;
		ld.StoredItem = new List<StoredData>();
		while (i-- > 0) {
			Component cp = toBeProcessed[i];
#if UNITY_EDITOR
			if (cp is Transform) continue;
#else
			if (cp is SaveObject || cp is Transform) continue;
#endif
			StoredData sd = new StoredData() {
				Type = cp.GetType().FullName,
				Name = UniqueIdentifier.identifier.Id
			};
			if (CustomSerializers.ContainsKey(cp.GetType())) {
				sd.Data = Encoding.Default.GetString(CustomSerializers[cp.GetType()].Serialize(cp));
			} else {
				sd.Data = UnitySerializer.JSONSerializeForDeserializeInto(cp);
			}

			ld.StoredItem.Add(sd);
		}
		return UnitySerializer.JSONSerialize(ld);
    }

    public static void Deserialize(string data) {
		JSONLevelSerializer.LevelData Data =UnitySerializer.JSONDeserialize<JSONLevelSerializer.LevelData>(data);
		GameObject go = UniqueIdentifier.identifier.gameObject;
		int i = Data.StoredItem.Count;
		while (i-- > 0) {
			JSONLevelSerializer.StoredData cp = Data.StoredItem[i];
			Type type = UnitySerializer.GetTypeEx(cp.Type);
			Component component = go.GetComponent(type);
			if (JSONLevelSerializer.CustomSerializers.ContainsKey(type)) {
				JSONLevelSerializer.CustomSerializers[type].Deserialize(Encoding.Default.GetBytes(cp.Data), component);
			} else {
				UnitySerializer.JSONDeserializeInto(cp.Data, component);
			}
		}
    }

    #region Nested type: LevelData

    public class LevelData {
        public List<StoredData> StoredItem;
        public StoredItem StoredObject;
    }

    #endregion

    #region Nested type: StoredData
	
    public class StoredData {
        public string ClassId;
        public string Data;
        public string Name;
        public string Type;
    }

    #endregion

    #region Nested type: StoredItem
	
	
    public class StoredItem {
        public List<string> ChildIds = new List<string>();
        public Dictionary<string, List<string>> Children = new Dictionary<string, List<string>>();
        public string ClassId;

        public string GameObjectName;
        public string Name;
        public string ParentName;
    }

    #endregion
}

public interface IComponentSerializer {
	/// <summary>
	///   Serialize the specified component to a byte array
	/// </summary>
	/// <param name='component'> Component to be serialized </param>
	byte[] Serialize(Component component);

	/// <summary>
	///   Deserialize the specified data into the instance.
	/// </summary>
	/// <param name='data'> The data that represents the component, produced by Serialize </param>
	/// <param name='instance'> The instance to target </param>
	void Deserialize(byte[] data, Component instance);
}