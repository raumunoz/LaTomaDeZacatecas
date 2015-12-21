#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

/// <summary>
///   Indicates that a property or field should not be serialized
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Event)]
public class DoNotSerialize : Attribute {
}

namespace Serialization
{
    public class SerializePrivateFieldOfType
    {
        private static readonly Index<string, List<SerializePrivateFieldOfType>> privateFields =
            new Index<string, List<SerializePrivateFieldOfType>>();

        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var name = "";
            if (privateFields.ContainsKey(type.Name))
            {
                name = type.Name;
            }
            if (privateFields.ContainsKey(type.FullName))
            {
                name = type.FullName;
            }
			if (name == "") { 
				
			}
            return new FieldInfo[0];
        }
    }

    /// <summary>
    ///   Used to set an order for deserialiation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SerializationPriorityAttribute : Attribute
    {
        public readonly int Priority;

        public SerializationPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
	
	[AttributeUsage(AttributeTargets.Class)]
	public class SerializerPriorityAttribute : Attribute
	{
		public readonly int Priority;
		
		
		public static int GetPriority(MemberInfo source)
		{
			var attr = Attribute.GetCustomAttribute(source, typeof(SerializerPriorityAttribute));
			return attr != null ? ((SerializerPriorityAttribute)attr).Priority : 1;
		}
	}
	
	

    public interface IProvideAttributeList
    {
        bool AllowAllSimple(Type tp);
        IEnumerable<string> GetAttributeList(Type tp);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AttributeListProvider : Attribute
    {
        public readonly Type AttributeListType;

        public AttributeListProvider(Type attributeListType)
        {
            AttributeListType = attributeListType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DeferredAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class Specialist : Attribute
    {
        public readonly Type Type;

        public Specialist(Type type)
        {
            Type = type;
        }
    }

    public interface ISerializeObjectEx : ISerializeObject
    {
        bool CanSerialize(Type targetType, object instance);
    }

    public interface ISpecialist
    {
        object Serialize(object value);
        object Deserialize(object value);
    }

    public interface ISerializeObject
    {
        object[] Serialize(object target);
        object Deserialize(object[] data, object instance);
    }

    public interface ICreateObject
    {
        object Create(Type itemType);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SerializerAttribute : Attribute
    {
        internal readonly Type SerializesType;

        public SerializerAttribute(Type serializesType)
        {
            SerializesType = serializesType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OnlyInterfaces : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SubTypeSerializerAttribute : Attribute
    {
        internal readonly Type SerializesType;

        public SubTypeSerializerAttribute(Type serializesType)
        {
            SerializesType = serializesType;
        }
    }

    /// <summary>
    ///   .NET compatible binary serializer with suppression support
    ///   produces compact representations, suitable for further compression
    /// </summary>
    //
    public static class UnitySerializer
    {
		
        private static readonly Dictionary<RuntimeTypeHandle, IEnumerable<FieldInfo>> FieldLists =
            new Dictionary<RuntimeTypeHandle, IEnumerable<FieldInfo>>();

        private static readonly Dictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> PropertyLists =
            new Dictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();


        internal static List<RuntimeTypeHandle> _knownTypesList;
        internal static Dictionary<RuntimeTypeHandle, ushort> _knownTypesLookup;
        private static Dictionary<object, int> _seenObjects;
        private static Dictionary<Type, bool> _seenTypes;
        private static Dictionary<int, object> _loadedObjects;
        internal static List<string> _propertyList;
        internal static Dictionary<string, ushort> _propertyLookup;


        private static Stack<List<DeferredSetter>> _deferredStack;
        private static Stack<List<Action>> _finalActions;
        private static Stack<Dictionary<int, object>> _loadedObjectStack;
        private static Stack<Dictionary<Type, bool>> _seenTypesStack;
        private static Stack<Dictionary<object, int>> _storedObjectsStack;
        private static Stack<KnownTypesStackEntry> _knownTypesStack;
        private static Stack<PropertyNameStackEntry> _propertyNamesStack;
        private static Stack<int> _idStack;
        private static int _nextId;
        //Holds a reference to the custom serializers
        private static readonly Dictionary<Type, ISerializeObject> Serializers =
            new Dictionary<Type, ISerializeObject>();

        //Specialist serializers
        internal static readonly Dictionary<Type, ISpecialist> Specialists = new Dictionary<Type, ISpecialist>();
        //Holds a reference to the custom serializers
		public class SubTypeEntry
		{
			public int priority;
			public Type type;
			public ISerializeObject serializer;
		}
        private static  List<SubTypeEntry> SubTypeSerializers = new List<SubTypeEntry>();

        //Holds a reference to the custom object creators
        private static readonly Dictionary<Type, ICreateObject> Creators = new Dictionary<Type, ICreateObject>();
        //Holds a reference to the custom object attribute list providers
        private static readonly Dictionary<Type, IProvideAttributeList> AttributeLists =
            new Dictionary<Type, IProvideAttributeList>();


        /// <summary>
        ///   Write all types, even if they are known, often used with Loud mode
        /// </summary>
        public static bool Verbose;


        public static readonly List<Type> PrewarmLookup = new List<Type>();


		private static readonly Dictionary<RuntimeTypeHandle, ushort> FullPrewarmedTypes =
            new Dictionary<RuntimeTypeHandle, ushort>();

        public static readonly List<Type> FullPrewarmLookup = new List<Type>();

		
		private static readonly Dictionary<string, ushort> PrewarmedNames = new Dictionary<string, ushort>();
        private static readonly HashSet<Type> privateTypes = new HashSet<Type>();
        private static readonly Stack<Type> currentTypes = new Stack<Type>();
        public static int currentVersion;

        /// <summary>
        ///   Cache for property name to item lookups
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<string, EntryConfiguration>> StoredTypes =
            new Dictionary<Type, Dictionary<string, EntryConfiguration>>();

		static Dictionary<object,Type> _typeCache = new Dictionary<object, Type>();
		
        public static Type GetTypeEx(object fullTypeName)
        {
			Type returnValue;
			if(!_typeCache.TryGetValue(fullTypeName, out returnValue))
			{
				if(fullTypeName is string)
				{
		            var type = Type.GetType((string)fullTypeName);
		            if (type != null)
		            {
		                returnValue = type;
		            }
					else
					{
			            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetType((string)fullTypeName) != null);
			            returnValue = assembly != null ? assembly.GetType((string)fullTypeName) : null;
					}
				}
				else if(fullTypeName is ushort)
				{
					if((ushort)fullTypeName >= 60000)
					{
						returnValue = UnitySerializer.currentVersion >= 11 ? FullPrewarmLookup[(ushort)fullTypeName - 60000] : PrewarmLookup[(ushort)fullTypeName-60000];
					}
					else
					{
						returnValue = Type.GetTypeFromHandle( _knownTypesList[(ushort)fullTypeName]);
					}
				}
				_typeCache[fullTypeName] = returnValue;
			}
			return returnValue;
        }
         
        

        public static void AddFixup(DeferredSetter setter)
        {
            lock (FixupFunctions)
            {
                FixupFunctions.Add(setter);
            }
        }


        public static event Func<Type, bool> CanSerialize;

        internal static bool CanSerializeType(Type tp)
        {
            if (CanSerialize != null)
            {
                return CanSerialize(tp);
            }
            else
            {
                return true;
            }
        }

        internal static void PushPropertyNames(bool clear)
        {
			if(SerializationScope.IsPrimaryScope)
			{
	            _propertyNamesStack.Push(new PropertyNameStackEntry {propertyList = _propertyList, propertyLookup = _propertyLookup});
	            if (clear)
	            {
	                _propertyList = new List<string>();
	                _propertyLookup = new Dictionary<string, ushort>();
	            }
			}
			else
			{
				_propertyList = _propertyList ?? new List<string>();
				_propertyLookup = _propertyLookup ?? new Dictionary<string, ushort>();
			}
        }

        internal static void PushPropertyNames()
        {
            PushPropertyNames(true);
        }

        internal static void PopPropertyNames()
        {
			if(SerializationScope.IsPrimaryScope)
			{
	            var stackEntry = _propertyNamesStack.Pop();
	            _propertyList = stackEntry.propertyList;
	            _propertyLookup = stackEntry.propertyLookup;
			}
        }


        /// <summary>
        ///   Event that is fired if a particular type cannot be instantiated
        /// </summary>
        public static event EventHandler<ObjectMappingEventArgs> CreateType;


        private static void InvokeCreateType(ObjectMappingEventArgs e)
        {
            var handler = CreateType;
            if (handler != null)
            {
                handler(null, e);
            }
        }


        /// <summary>
        ///   Event that is fired if a particular type cannot be found
        /// </summary>
        public static event EventHandler<TypeMappingEventArgs> MapMissingType;


        internal static void InvokeMapMissingType(TypeMappingEventArgs e)
        {
            var handler = MapMissingType;
            if (handler != null)
            {
                handler(null, e);
            }
        }

		
				/// <summary>

        public static T JSONDeserialize<T>(string data) where T : class
        {
			return JSONDeserialize(data) as T;
        }

        /// <summary>
        ///   Caches and returns property info for a type
        /// </summary>
        /// <param name="itm"> The type that should have its property info returned </param>
        /// <returns> An enumeration of PropertyInfo objects </returns>
        /// <remarks>
        ///   It should be noted that the implementation converts the enumeration returned from reflection to an array as this more than double the speed of subsequent reads
        /// </remarks>
        internal static IEnumerable<PropertyInfo> GetPropertyInfo(RuntimeTypeHandle itm)
        {
			lock (PropertyLists)
            {
                string[] validNames;
                IEnumerable<PropertyInfo> ret;
               
				if (!PropertyLists.TryGetValue(itm, out ret))
                {
                    var tp = Type.GetTypeFromHandle(itm);
                    var allowSimple = true;
                    validNames = AttributeLists
                        .Where(p => p.Key.IsAssignableFrom(tp))
                        .SelectMany(p =>
                                        {
                                            allowSimple = allowSimple && p.Value.AllowAllSimple(tp);
                                            return p.Value.GetAttributeList(tp);
                                        }).ToArray();
                    if (validNames.FirstOrDefault() == null)
                    {
                        validNames = null;
                    }
                    var containingType = Type.GetTypeFromHandle(itm);

                    ret = containingType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                        .Where(
                            p =>
                            !typeof (Component).IsAssignableFrom(tp) || tp == typeof (Component) || !componentNames.ContainsKey(p.Name))
                        .Where(
                            p => p.GetGetMethod() != null &&
                                    !(p.GetIndexParameters().Any()) &&
                                    (p.GetSetMethod() != null &&
                                    CanSerializeType(p.PropertyType)) &&
                                    ((p.PropertyType.IsValueType && allowSimple) || validNames == null ||
                                    validNames.Any(n => n == p.Name))
                        ).ToArray();
                    PropertyLists[itm] = ret;
                }
                
                var propertyInfos = ret as PropertyInfo[] ?? ret.ToArray();
				return propertyInfos;
            }
        }

        /// <summary>
        ///   Caches and returns field info for a type
        /// </summary>
        /// <param name="itm"> The type that should have its field info returned </param>
        /// <returns> An enumeration of FieldInfo objects </returns>
        /// <remarks>
        ///   It should be noted that the implementation converts the enumeration returned from reflection to an array as this more than double the speed of subsequent reads
        /// </remarks>
        internal static IEnumerable<FieldInfo> GetFieldInfo(RuntimeTypeHandle itm)
        {
            lock (FieldLists)
            {
                IEnumerable<FieldInfo> ret;


                if (FieldLists.ContainsKey(itm))
                {
                    ret = FieldLists[itm];
                }
                else
                {
                    var tp = Type.GetTypeFromHandle(itm);
                    var allowSimple = true;
                    var validNames = AttributeLists
                        .Where(p => p.Key.IsAssignableFrom(tp))
                        .SelectMany(p =>
                                        {
                                            allowSimple = allowSimple && p.Value.AllowAllSimple(tp);
                                            return p.Value.GetAttributeList(tp);
                                        }).ToList();

                    if (validNames.FirstOrDefault() == null)
                    {
                        validNames = null;
                    }
                    var parents = new List<Type>();
                    var allParents = new List<Type>();
                    var scan = tp;
                    var addAll = false;
                    do
                    {
                        if (privateTypes.Any(currentTypes.Contains) || addAll || (scan.GetInterface("IEnumerator") != null))
                        {
                            if ((scan.GetInterface("IEnumerator") != null))
                            {
                                if (!addAll)
                                {
                                    addAll = true;
                                    privateTypes.Add(scan);
                                }
                            }
                            parents.Add(scan);
                        }
                        //So we can check for SerializeThis
                        allParents.Add(scan);
                        scan = scan.BaseType;
                    } while (scan != null);


                    ret = FieldLists[itm] = tp
                                                .GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                           BindingFlags.SetField | BindingFlags.Static)
                                                .Concat(
                                                    parents.SelectMany(
                                                        p =>
                                                        p.GetFields(BindingFlags.Instance | BindingFlags.NonPublic |
                                                                    BindingFlags.SetField)))
                                                .Concat(SerializePrivateFieldOfType.GetFields(tp))
                                                .Where(
                                                    p =>
                                                    !p.IsLiteral &&
                                                    CanSerializeType(p.FieldType) &&
                                                    (
                                                        (p.FieldType.IsValueType && allowSimple) ||
                                                        validNames == null ||
                                                        (validNames.Any(n => n == p.Name))
                                                    ))
                                                .Where(
                                                    p =>
                                                    !typeof (Component).IsAssignableFrom(tp) || tp == typeof (Component) || !componentNames.ContainsKey(p.Name))
                                                .ToArray();
                }

                return ret;
            }
        }

        /// <summary>
        ///   Returns a token that represents the name of the property
        /// </summary>
        /// <param name="name"> The name for which to return a token </param>
        /// <returns> A 2 byte token representing the name </returns>
        internal static ushort GetPropertyDefinitionId(string name)
        {
            ushort id;
            if (!PrewarmedNames.TryGetValue(name, out id))
            {
#if US_LOGGING
				Radical.Log("Prewarm miss on {0}", name);
#endif
                if (!_propertyLookup.TryGetValue(name, out id))
                {
                    id = (ushort) _propertyLookup.Count;
                    _propertyLookup[name] = id;
                }
            }
            return id;
        }

        public static object JSONDeserialize(Stream inputStream)
        {
            return JSONDeserialize(inputStream, null);
        }


        public static object JSONDeserialize(Stream inputStream, object instance)
        {
            // this version always uses the BinarySerializer
            using (new SerializationScope())
            {
                var v = Verbose;
                CreateStacks();
                try
                {
                    PushKnownTypes();
                    PushPropertyNames();
                    var serializer = new JSONSerializer(new StreamReader(inputStream).ReadToEnd());
                    serializer.StartDeserializing();
                    var ob = DeserializeObject(new Entry()
                                                   {
                                                       Name = "root",
                                                       Value = instance
                                                   }, serializer);
                    serializer.FinishedDeserializing();
                    return ob;
                }
                catch {
                    
                    return null;
                }
                finally
                {
                    PopKnownTypes();
					PopPropertyNames();
                    Verbose = v;
                }
            }
        }

        /// <summary>
        ///   Escape the specified input for JSON.
        /// </summary>
        /// <param name='input'> Input. </param>
        public static string Escape(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        /// <summary>
        ///   Unescape a JSON string
        /// </summary>
        /// <returns> The escape. </returns>
        /// <param name='input'> Input. </param>
        public static string UnEscape(string input)
        {
            return input.Contains("\"") ? input : input.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        internal static void PopKnownTypes()
        {
			if(SerializationScope.IsPrimaryScope)
			{
	            var stackEntry = _knownTypesStack.Pop();
	            _knownTypesList = stackEntry.knownTypesList;
	            _knownTypesLookup = stackEntry.knownTypesLookup;
			}
        }

        private static void PushKnownTypes(bool clear)
        {
			if(SerializationScope.IsPrimaryScope)
			{
	            _knownTypesStack.Push(new KnownTypesStackEntry {knownTypesList = _knownTypesList, knownTypesLookup = _knownTypesLookup});
	            if (!clear)
	            {
	                return;
	            }
	            _knownTypesList = new List<RuntimeTypeHandle>();
	            _knownTypesLookup = new Dictionary<RuntimeTypeHandle, ushort>();
			}
			else
			{
	            _knownTypesList = _knownTypesList ?? new List<RuntimeTypeHandle>();
	            _knownTypesLookup = _knownTypesLookup ?? new Dictionary<RuntimeTypeHandle, ushort>();
			}
        }

        internal static void PushKnownTypes()
        {
            PushKnownTypes(true);
        }

        public static object JSONDeserialize(string json)
        {
            using (new SerializationScope())
            {
                using (var inputStream = new MemoryStream(Encoding.Default.GetBytes(UnEscape(json))))
                {
                    return JSONDeserialize(inputStream);
                }
            }
        }

        /// <summary>
        ///   Convert a previously serialized object from a byte array 
        ///   back into a .NET object
        /// </summary>
        /// <param name="json"> </param>
        /// <param name="instance"> </param>
        /// <returns> The rehydrated object represented by the data supplied </returns>
        public static void JSONDeserializeInto(string json, object instance)
        {
            using (new SerializationScope())
            {
                using (var inputStream = new MemoryStream(Encoding.Default.GetBytes(UnEscape(json))))
                {
                    JSONDeserialize(inputStream, instance);
                }
            }
        }

        /// <summary>
        ///   Creates a set of stacks on the current thread
        /// </summary>
        private static void CreateStacks()
        {
            if (_propertyNamesStack == null)
            {
                _propertyNamesStack = new Stack<PropertyNameStackEntry>();
            }
            if (_knownTypesStack == null)
            {
                _knownTypesStack = new Stack<KnownTypesStackEntry>();
            }
            if (_loadedObjectStack == null)
            {
                _loadedObjectStack = new Stack<Dictionary<int, object>>();
            }
            if (_storedObjectsStack == null)
            {
                _storedObjectsStack = new Stack<Dictionary<object, int>>();
            }
            if (_seenTypesStack == null)
            {
                _seenTypesStack = new Stack<Dictionary<Type, bool>>();
            }
            if (_deferredStack == null)
            {
                _deferredStack = new Stack<List<DeferredSetter>>();
            }
            if (_finalActions == null)
            {
                _finalActions = new Stack<List<Action>>();
            }
            if (_idStack == null)
            {
                _idStack = new Stack<int>();
            }
        }

        public static void JSONSerialize(object item, Stream outputStream)
        {
            JSONSerialize(item, outputStream, false);
        }

        public static void JSONSerialize(object item, Stream outputStream, bool forDeserializeInto)
        {
            CreateStacks();


            using (new SerializationScope())
            {
				SerializationScope.SetPrimaryScope();
                //var serializer = Activator.CreateInstance(SerializerType) as IStorage;
                var serializer = new JSONSerializer();
                //BinarySerializer serializer = new BinarySerializer();
                serializer.StartSerializing();
                SerializeObject(new Entry()
                                    {
                                        Name = "root",
                                        Value = item
                                    }, serializer, forDeserializeInto);
                serializer.FinishedSerializing();
                var outputWr = new StreamWriter(outputStream);
                outputWr.Write(serializer.Data);
                outputWr.Flush();
                outputStream.Flush();
            }
        }

        /// <summary>
        ///   Serialize an object into an array of bytes
        /// </summary>
        /// <param name="item"> The object to serialize </param>
        /// <returns> A byte array representation of the item </returns>
        public static string JSONSerialize(object item)
        {
            using (new SerializationScope())
            {
                using (var outputStream = new MemoryStream())
                {
                    JSONSerialize(item, outputStream);
                    //Reset the verbose mode
                    return Encoding.Default.GetString(outputStream.ToArray());
                }
            }
        }


        public static string JSONSerializeForDeserializeInto(object item)
        {
            using (new SerializationScope())
            {
			    using (var outputStream = new MemoryStream())
                {
                    JSONSerialize(item, outputStream, true);
                    //Reset the verbose mode
                    return Encoding.Default.GetString(outputStream.ToArray());
                }
			
            }
        }

        /// <summary>
        ///   Return whether the type specified is a simple type that can be serialized fast
        /// </summary>
        /// <param name="tp"> The type to check </param>
        /// <returns> True if the type is a simple one and can be serialized directly </returns>
        private static bool IsSimpleType(Type tp)
        {
            return tp.IsPrimitive || tp == typeof (string) || tp.IsEnum || tp == typeof (DateTime) ||
                   tp == typeof (TimeSpan) || tp == typeof (Guid) || tp == typeof (decimal);
        }
		
		public static object currentlySerializingObject;
		
        private static void SerializeObjectAndProperties(object item, Type itemType, IStorage storage)
        {

			var last = currentlySerializingObject;
			currentlySerializingObject = item;
            WriteFields(itemType, item, storage);
            WriteProperties(itemType, item, storage);
			currentlySerializingObject = last;

        }

        /// <summary>
        ///   Create an instance of a type
        /// </summary>
        /// <param name="itemType"> The type to construct </param>
        /// <returns> </returns>
        internal static object CreateObject(Type itemType)
        {
            try
            {
                if (Creators.ContainsKey(itemType))
                {
                    return Creators[itemType].Create(itemType);
                }
                if (typeof (Component).IsAssignableFrom(itemType))
                {
                    return null;
                }
                if (itemType.IsSubclassOf(typeof (ScriptableObject)))
                {
                    return ScriptableObject.CreateInstance(itemType);
                }
                return Activator.CreateInstance(itemType);
            }
            catch {
                try
                {
                    var constructorInfo =
                        itemType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
                                                null, new Type[] {}, null);
                    return constructorInfo != null ? constructorInfo.Invoke(new object[] {}) : CreateInstance(itemType);
                }
                catch
                {
                    return CreateInstance(itemType);
                }
            }
        }

        private static object CreateInstance(Type itemType)
        {
            //Raise an event to construct the object
            var ct = new ObjectMappingEventArgs {TypeToConstruct = itemType};
            if (typeof (MulticastDelegate).IsAssignableFrom(itemType))
            {
                return
                    (object)
                    Delegate.CreateDelegate(typeof (Action),
                                            typeof (UnitySerializer).GetMethod("DummyAction",
                                                                               BindingFlags.Public | BindingFlags.Static));
            }
            InvokeCreateType(ct);
            //Check if we created the right thing
            if (ct.Instance != null &&
                (ct.Instance.GetType() == itemType || ct.Instance.GetType().IsSubclassOf(itemType)))
            {
                return ct.Instance;
            }

            var error =
                string.Format(
                    "Could not construct an object of type '{0}', it must be creatable in this scope and have a default parameterless constructor or you should handle the CreateType event on UnitySerializer to construct the object",
                    itemType.FullName);
            throw new MissingConstructorException(error);
        }
		
        /// <summary>
        ///   Logs a type and returns a unique token for it
        /// </summary>
        /// <param name="tp"> The type to retrieve a token for </param>
        /// <returns> A 2 byte token representing the type </returns>
        internal static ushort GetTypeId(RuntimeTypeHandle tp)
        {
            ushort tpId;
            if (!FullPrewarmedTypes.TryGetValue(tp, out tpId))
            {
                if (!_knownTypesLookup.TryGetValue(tp, out tpId))
                {
                    tpId = (ushort) _knownTypesLookup.Count;
                    _knownTypesLookup[tp] = tpId;
                }
            }
            return tpId;
        }

        /// <summary>
        ///   Gets a property setter and a standard default type for an entry
        /// </summary>
        /// <param name="entry"> </param>
        private static void UpdateEntryWithName(Entry entry)
        {
            
            Dictionary<string, EntryConfiguration> configurations;
            if (!StoredTypes.TryGetValue(entry.OwningType, out configurations))
            {
                configurations = new Dictionary<string, EntryConfiguration>();
                StoredTypes[entry.OwningType] = configurations;
            }

            EntryConfiguration entryConfiguration;
            if (!configurations.TryGetValue(entry.Name, out entryConfiguration))
            {
                entryConfiguration = new EntryConfiguration();

                var pi = entry.OwningType.GetProperty(entry.Name,
                                                      BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (pi != null)
                {
                    entryConfiguration.Type = pi.PropertyType;
                    entryConfiguration.Setter = new GetSetGeneric(pi);
                }
                else
                {
                    var fi = GetField(entry.OwningType, entry.Name);
                    if (fi != null)
                    {
                        entryConfiguration.Type = fi.FieldType;
                        entryConfiguration.Setter = new GetSetGeneric(fi);
                    }
                }
                configurations[entry.Name] = entryConfiguration;
            }
            entry.StoredType = entryConfiguration.Type;
            entry.Setter = entryConfiguration.Setter;
        }

        private static FieldInfo GetField(Type tp, string name)
        {
            FieldInfo fi = null;
            while (tp != null &&
                   (fi =
                    tp.GetField(name,
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.Static)) == null)
            {
                tp = tp.BaseType;
            }
            return fi;
        }

        #region Serialization

        private static readonly Dictionary<Type, ISerializeObject> cachedSerializers =
            new Dictionary<Type, ISerializeObject>();

        private static void SerializeObject(Entry entry, IStorage storage)
        {
            SerializeObject(entry, storage, false);
        }

        private static bool CompareToNull(object o)
        {
            return (o is Object) ? !(bool) ((Object) o) : o == null;
        }

        private static void SerializeObject(Entry entry, IStorage storage, bool first)
        {
            var item = entry.Value;
            var objectId = _nextId++;
            if (CompareToNull(item))
            {
                entry.Value = new Nuller();
                item = entry.Value;
            }

            if (storage.StartSerializing(entry, objectId))
            {
                _seenObjects[item] = objectId;
                return;
            }

            var itemType = item.GetType();
            using (new TypePusher(itemType))
            {
                //Check for simple types again
                if (IsSimpleType(itemType))
                {
                    storage.WriteSimpleValue(itemType.IsEnum
                                                 ? Convert.ChangeType(item, Enum.GetUnderlyingType(itemType),
                                                                      CultureInfo.InvariantCulture)
                                                 : item);
                    return;
                }

                //Check whether this object has been seen
                if (!(itemType.IsValueType) && _seenObjects.ContainsKey(item))
                {
                    storage.BeginWriteObject(_seenObjects[item], item.GetType(), true);
                    storage.EndWriteObject();
                    return;
                }


                var skipRecord = false;

                if (!first)
                {
                    //Check for custom serialization
                    if (Serializers.ContainsKey(itemType))
                    {
                        //If we have a custom serializer then use it!
                        storage.BeginWriteObject(objectId, itemType, false);
                        storage.BeginWriteProperty("data", typeof (object[]));
                        var serializer = Serializers[itemType];
                        var data = serializer.Serialize(item);
						using(new SerializationSplitScope())
						{
	                        SerializeObject(new Entry()
	                                            {
	                                                Name = "data",
	                                                Value = data,
	                                                StoredType = typeof (object[])
	                                            }, storage);
						}
                        storage.EndWriteProperty();
                        storage.EndWriteObject();
						_seenObjects[item] = objectId;
                        return;
                    }

                    ISerializeObject serializeObject;
                    if (!cachedSerializers.TryGetValue(itemType, out serializeObject))
                    {
                        foreach (var tp in SubTypeSerializers)
                        {
                            if (!tp.type.IsAssignableFrom(itemType) ||
                                (tp.serializer.GetType().IsDefined(typeof (OnlyInterfaces), false) && !itemType.IsInterface))
                            {
                                continue;
                            }
                            serializeObject = tp.serializer;
                            break;
                        }
                        cachedSerializers[itemType] = serializeObject;
                    }
                    if (serializeObject != null)
                    {
                        if (!(serializeObject is ISerializeObjectEx) || (serializeObject as ISerializeObjectEx).CanSerialize(itemType, entry.Value))
                        {
                            //If we have a custom serializer then use it!
                            storage.BeginWriteObject(objectId, itemType, false);
                            storage.BeginWriteProperty("data", typeof (object[]));
                            var data = serializeObject.Serialize(item);
							using(new SerializationSplitScope())
							{
	                            SerializeObject(new Entry()
	                                                {
	                                                    Name = "data",
	                                                    Value = data,
	                                                    StoredType = typeof (object[])
	                                                }, storage);
							}
                            storage.EndWriteProperty();
                            storage.EndWriteObject();
							_seenObjects[item] = objectId;
                        }
                        return;
                    }
                }
                else
                {
                    skipRecord = true;
                }


                //We are going to serialize an object
                if (!skipRecord && !itemType.IsValueType)
                {
					//Debug.Log(objectId.ToString() + " : " + item.ToString());
                    _seenObjects[item] = objectId;
                }
                storage.BeginWriteObject(objectId, itemType, false);

                //Check for collection types)
                if (item is Array)
                {
                    if (((Array) item).Rank == 1)
                    {
                        SerializeArray(item as Array, itemType, storage);
                    }
                    else
                    {
                        SerializeMultiDimensionArray(item as Array, itemType, storage);
                    }
                    storage.EndWriteObject();
                    return;
                }
                else if (item is IDictionary)
                {
                    SerializeDictionary(item as IDictionary, itemType, storage);
                }
                else if (item is IList)
                {
                    SerializeList(item as IList, itemType, storage);
                }

                //storageOtherwise we are serializing an object
                SerializeObjectAndProperties(item, itemType, storage);
                storage.EndWriteObject();
            }
        }

        private static void SerializeList(ICollection item, Type tp, IStorage storage)
        {
            Type valueType = null;
            //Try to optimize the storage of types based on the type of list
            if (tp.IsGenericType)
            {
                var types = tp.GetGenericArguments();
                valueType = types[0];
            }

            //storage.WriteValue("no_items", item.Count);
            storage.BeginWriteList(item.Count, item.GetType());
            var entry = new Entry();

            var id = 0;
            foreach (var val in item)
            {
                entry.Value = val;
                entry.StoredType = valueType;
                if(!storage.BeginWriteListItem(id++, val))
                    SerializeObject(entry, storage);
                storage.EndWriteListItem();
            }
            storage.EndWriteList();
        }

        private static void SerializeDictionary(IDictionary item, Type tp, IStorage storage)
        {
            Type keyType = null;
            Type valueType = null;
            //Try to optimise storage based on the type of dictionary
            if (tp.IsGenericType)
            {
                var types = tp.GetGenericArguments();
                keyType = types[0];
                valueType = types[1];
            }

            //storage.WriteValue("no_items", item.Count);
            storage.BeginWriteDictionary(item.Count, item.GetType());
			storage.BeginWriteDictionaryKeys();
            //Serialize the pairs
            var id = 0;
            foreach (var key in item.Keys)
            {
                if(!storage.BeginWriteDictionaryKey(id++,key))
                    SerializeObject(new Entry
                                    {
                                        StoredType = keyType,
                                        Value = key
                                    }, storage);
                storage.EndWriteDictionaryKey();
            }
			storage.EndWriteDictionaryKeys();
			storage.BeginWriteDictionaryValues();
            id = 0;
            foreach (var val in item.Values)
            {
                if(!storage.BeginWriteDictionaryValue(id++, val))
                    SerializeObject(new Entry
                                    {
                                        StoredType = valueType,
                                        Value = val
                                    }, storage);
                storage.EndWriteDictionaryValue();
            }
			storage.EndWriteDictionaryValues();
            storage.EndWriteDictionary();
        }

        private static void SerializeArray(Array item, Type tp, IStorage storage)
        {
            var elementType = tp.GetElementType();
            if (IsSimpleType(elementType))
            {
                storage.WriteSimpleArray(item.Length, item);
            }
            else
            {
                var length = item.Length;
                storage.BeginWriteObjectArray(length, item.GetType());
                for (var l = 0; l < length; l++)
                {
                    var val = item.GetValue(l);
                    if(!storage.BeginWriteObjectArrayItem(l, val))
                        SerializeObject(new Entry()
                                        {
                                            Value = item.GetValue(l),
                                            StoredType = elementType
                                        }, storage);
                    storage.EndWriteObjectArrayItem();
                }
                storage.EndWriteObjectArray();
            }
        }

        private static void SerializeMultiDimensionArray(Array item, Type tp, IStorage storage)
        {
            // Multi-dimension serializer data is:
            // Int32: Ranks
            // Int32 (x number of ranks): length of array dimension 

            var dimensions = item.Rank;

            var length = item.GetLength(0);

            // Determine the number of cols being populated
            //var cols = item.GetLength(item.Rank - 1);

            // Explicitly write this value, to denote that this is a multi-dimensional array
            // so it doesn't break the deserializer when reading values for existing arrays

            storage.BeginMultiDimensionArray(item.GetType(), dimensions, length);


            var indicies = new int[dimensions];

            // Write out the length of each array, if we are dealing with the first array
            for (var arrayStartIndex = 0; arrayStartIndex < dimensions; arrayStartIndex++)
            {
                indicies[arrayStartIndex] = 0;
                //storage.WriteValue("dim_len" + arrayStartIndex, item.GetLength(arrayStartIndex));
                storage.WriteArrayDimension(arrayStartIndex, item.GetLength(arrayStartIndex));
            }

            SerializeArrayPart(item, 0, indicies, storage);

            storage.EndMultiDimensionArray();
        }

        private static void SerializeArrayPart(Array item, int i, int[] indices, IStorage storage)
        {
            var length = item.GetLength(i);
            for (var l = 0; l < length; l++)
            {
                indices[i] = l;
                if (i != item.Rank - 2)
                {
                    SerializeArrayPart(item, i + 1, indices, storage);
                }
                else
                {
                    var arrayType = item.GetType().GetElementType();
                    var cols = item.GetLength(i + 1);

                    var baseArray = Array.CreateInstance(arrayType, cols);

                    // Convert the whole multi-dimensional array to be 'row' based
                    // and serialize using the existing code
                    for (var arrayStartIndex = 0; arrayStartIndex < cols; arrayStartIndex++)
                    {
                        indices[i + 1] = arrayStartIndex;
                        baseArray.SetValue(item.GetValue(indices), arrayStartIndex);
                    }

                    SerializeArray(baseArray, baseArray.GetType(), storage);
                }
            }
        }


        private static void WriteProperties(Type itemType, object item, IStorage storage)
        {
            var seen = _seenTypes.ContainsKey(itemType) && _seenTypes[itemType];
            _seenTypes[itemType] = true;
            var propList = GetWritableAttributes.GetProperties(item, seen);
            storage.BeginWriteProperties(propList.Length);

            foreach (var entry in propList)
            {
                storage.BeginWriteProperty(entry.Name, entry.PropertyInfo.PropertyType);
                SerializeObject(entry, storage, false);
                storage.EndWriteProperty();
            }
            storage.EndWriteProperties();
        }


        private static void WriteFields(Type itemType, object item, IStorage storage)
        {
            var seen = _seenTypes.ContainsKey(itemType);
            if (!seen)
            {
                _seenTypes[itemType] = false;
            }
            var fieldList = GetWritableAttributes.GetFields(item, seen);
            storage.BeginWriteFields(fieldList.Length);
            foreach (var entry in fieldList)
            {
                storage.BeginWriteField(entry.Name, entry.FieldInfo.FieldType);
                SerializeObject(entry, storage, false);
                storage.EndWriteField();
            }
            storage.EndWriteFields();
        }

        #region Nested type: Nuller

        public class Nuller
        {
        }

        #endregion

        #region Nested type: TypePusher

        private class TypePusher : IDisposable
        {
            public TypePusher(Type t)
            {
                currentTypes.Push(t);
            }

            #region IDisposable Members

            public void Dispose()
            {
                currentTypes.Pop();
            }

            #endregion
        }

        #endregion

        #endregion

        #region New Deserialization

        #region Delegates

        public delegate object GetData(Dictionary<string, object> parameters);

        #endregion

        public static object DeserializingObject;
        private static readonly Stack<object> DeserializingStack = new Stack<object>();
        private static readonly List<DeferredSetter> FixupFunctions = new List<DeferredSetter>();

        /// <summary>
        ///   Deserializes an object or primitive from the stream
        /// </summary>
        /// <param name="entry"> </param>
        /// <param name="storage"> </param>
        /// <returns> The value read from the file </returns>
        /// <remarks>
        ///   The function is supplied with the type of the property that the object was stored in (if known) this enables
        ///   a compact format where types only have to be specified if they differ from the expected one
        /// </remarks>
        internal static object DeserializeObject(Entry entry, IStorage storage)
        {
            try
            {
                var objectID = _nextId++;
                //Get a name for the item
                storage.DeserializeGetName(entry);
                //Update the core info including a property getter
                if (entry.MustHaveName)
                {
                    UpdateEntryWithName(entry);
                }
                //Start to deserialize
                var candidate = storage.StartDeserializing(entry);
                if (candidate != null)
                {
                    storage.FinishDeserializing(entry);
                    return candidate;
                }


                var itemType = entry.StoredType;

                if (itemType == null)
                {
                    return null;
                }

                object obj = null, result2 = null;

                //Check if this is a simple value and read it if so
                if (IsSimpleType(itemType))
                {
                    if (itemType.IsEnum)
                    {
                        return Enum.Parse(itemType, storage.ReadSimpleValue(Enum.GetUnderlyingType(itemType)).ToString(),
                                          true);
                    }
                    return storage.ReadSimpleValue(itemType);
                }

                //See if we should lookup this object or create a new one
                bool isReference;
                var existingId = storage.BeginReadObject(out isReference);
                if (existingId != -1)
                {
                    if (isReference)
                    {
                        try
                        {
                            var o = _loadedObjects[existingId];
							storage.EndReadObject();
							return o;
                        }
                        catch
                        {
                            throw new SerializationException(
                                "Error when trying to link to a previously seen object. The stream gave an object id of " +
                                existingId + " but that was not found.  It is possible that an" +
                                "error has caused the data stream to become corrupt and that this id is wildly out of range.  Ids should be sequential numbers starting at 1 for the first object or value seen and then incrementing thereafter.");
                        }
                    }
                }
				if(entry.Value!=null)
                	_loadedObjects[objectID] = entry.Value;

                //Only custom serialize if the object hasn't already been created
                //this is normally only tr
                if (entry.Value == null)
                {
                    //Check for custom serialization
                    if (Serializers.ContainsKey(itemType))
                    {
                        //Read the serializer and its data
                        var serializer = Serializers[itemType];
                        var nentry = new Entry
                                         {
                                             Name = "data",
                                             StoredType = typeof (object[])
                                         };
                        storage.BeginReadProperty(nentry);
						object[] data = null;
						using(new SerializationSplitScope())
						{
	                        data =
	                            (object[])
	                            DeserializeObject(nentry, storage);
						}
                        var result = serializer.Deserialize(data, entry.Value);
                        storage.EndReadProperty();
                        storage.EndReadObject();
                        _loadedObjects[objectID] = result;
                        storage.FinishDeserializing(entry);
                        return result;
                    }
                    ISerializeObject serializeObject;
                    if (!cachedSerializers.TryGetValue(itemType, out serializeObject))
                    {
                        serializeObject = null;
                        foreach (var tp in SubTypeSerializers)
                        {
                            if (!tp.type.IsAssignableFrom(itemType) ||
                                (tp.serializer.GetType().IsDefined(typeof (OnlyInterfaces), false) && !itemType.IsInterface))
                            {
                                continue;
                            }
                            serializeObject = tp.serializer;
                            break;
                        }
                        cachedSerializers[itemType] = serializeObject;
                    }
                    if (serializeObject != null)
                    {
                        if (
                            !(serializeObject is ISerializeObjectEx) || (serializeObject as ISerializeObjectEx).CanSerialize(itemType, entry.Value))
                        {
                            var nentry = new Entry
                                             {
                                                 Name = "data",
                                                 StoredType = typeof (object[])
                                             };
                            storage.BeginReadProperty(nentry);
                            //If we have a custom serializer then use it!
							object[] data;
							using(new SerializationSplitScope())
							{
	                            data =
	                                (object[])
	                                DeserializeObject(nentry, storage);
	
							}
                            var result = serializeObject.Deserialize(data, entry.Value);
                            storage.EndReadProperty();
                            storage.EndReadObject();
                            _loadedObjects[objectID] = result;
                            storage.FinishDeserializing(entry);
                            return result;
                        }
                    }
                }

                //Otherwise create the object
                if (itemType.IsArray)
                {
                    int baseCount;
                    var isMultiDimensionArray = storage.IsMultiDimensionalArray(out baseCount);

                    if (isMultiDimensionArray)
                    {
                        var result = DeserializeMultiDimensionArray(itemType, storage, objectID);
                        storage.EndReadObject();
                        _loadedObjects[objectID] = result;
                        storage.FinishDeserializing(entry);
                        return result;
                    }
                    else
                    {
                        var result = DeserializeArray(itemType, storage, baseCount, objectID);
                        storage.EndReadObject();
                        _loadedObjects[objectID] = result;
                        storage.FinishDeserializing(entry);
                        return result;
                    }
                }

                obj = entry.Value ?? CreateObject(itemType);
                if (itemType.IsValueType)
                {
                    obj = RuntimeHelpers.GetObjectValue(obj);
                }
                _loadedObjects[objectID] = obj;

                //Check for collection types)
                if (obj is IDictionary)
                {
                    DeserializeDictionary(obj as IDictionary, itemType, storage);
                }
                if (obj is IList)
                {
                    DeserializeList(obj as IList, itemType, storage);
                }

                //Otherwise we are serializing an object
                result2 = DeserializeObjectAndProperties(obj, itemType, storage);
                storage.EndReadObject();

                //Check for null
                if (obj is Nuller)
                {
                    return null;
                }
                return result2;
            }
            finally
            {
#if US_LOGGING
				if(Radical.IsLogging())
				{
					Radical.OutdentLog ();
					Radical.Log ("</Object {0}>", entry.Name);
				}
#endif
            }
        }
		


        /// <summary>
        ///   Deserializes an array of values
        /// </summary>
        /// <param name="itemType"> The type of the array </param>
        /// <param name="storage"> </param>
        /// <param name="count"> </param>
        /// <returns> The deserialized array </returns>
        /// <remarks>
        ///   This routine optimizes for arrays of primitives and bytes
        /// </remarks>
        private static object DeserializeArray(Type itemType, IStorage storage, int count, int objectID)
        {
            var elementType = itemType.GetElementType();
            Array result = null;
		

            if (IsSimpleType(elementType))
            {
                result = storage.ReadSimpleArray(elementType, count);
                _loadedObjects[objectID] = result;
            }
            else
            {
                if (count == -1)
                {
                    count = storage.BeginReadObjectArray(itemType);
                }
                result = Array.CreateInstance(elementType, count);
                _loadedObjects[objectID] = result;

                for (var l = 0; count==-1 ? storage.HasMore() : l < count; l++)
                {
					var entry = new Entry()
                                                      {
                                                          StoredType = elementType
                                                      };
                    var value = storage.BeginReadObjectArrayItem(l, entry);
                    value = value ?? DeserializeObject(entry, storage);
                    if (value != null && value.GetType().IsDefined(typeof (DeferredAttribute), true))
                    {
                        var toSet = value;
                        value = new DeferredSetter(d => toSet);
                    }

                    if (value is DeferredSetter)
                    {
                        var st = value as DeferredSetter;
                        var pos = l;
                        var nd = new DeferredSetter(st.deferredRetrievalFunction) {enabled = st.enabled};
                        nd._setAction = () =>
                                           {
                                               if (result != null)
                                               {
                                                   result.SetValue(nd.deferredRetrievalFunction(st.parameters), pos);
                                               }
                                           };
                        AddFixup(nd);
                    }
                    else
                    {
                        result.SetValue(value, l);
                    }

                    storage.EndReadObjectArrayItem();
                }
                if (count != -1)
                {
                    storage.EndReadObjectArray();
                }
            }


            return result;
        }


        /// <summary>
        ///   Deserializes a multi-dimensional array of values
        /// </summary>
        /// <param name="itemType"> The type of the array </param>
        /// <param name="storage"> </param>
        /// <param name="objectID"> </param>
        /// <returns> The deserialized array </returns>
        /// <remarks>
        ///   This routine deserializes values serialized on a 'row by row' basis, and
        ///   calls into DeserializeArray to do this
        /// </remarks>
        private static object DeserializeMultiDimensionArray(Type itemType, IStorage storage, int objectID)
        {
            //Read the number of dimensions the array has
            //var dimensions = storage.ReadValue<int>("dimensions");
            //var totalLength = storage.ReadValue<int>("length");
            int dimensions, totalLength;
            storage.BeginReadMultiDimensionalArray(out dimensions, out totalLength);

            // Establish the length of each array element
            // and get the total 'row size'
            var lengths = new int[dimensions];
            var indices = new int[dimensions];

            for (var item = 0; item < dimensions; item++)
            {
                lengths[item] = storage.ReadArrayDimension(item); //.ReadValue<int>("dim_len" + item);
                indices[item] = 0;
            }
            //Get the expected element type
            var elementType = itemType.GetElementType();

            var sourceArrays = Array.CreateInstance(elementType, lengths);
            DeserializeArrayPart(sourceArrays, 0, indices, itemType, storage, objectID);
            return sourceArrays;
        }

        private static void DeserializeArrayPart(Array sourceArrays, int i, int[] indices, Type itemType,
                                                 IStorage storage, int objectID)
        {
            var length = sourceArrays.GetLength(i);
            for (var l = 0; l < length; l++)
            {
                indices[i] = l;
                if (i != sourceArrays.Rank - 2)
                {
                    DeserializeArrayPart(sourceArrays, i + 1, indices, itemType, storage, objectID);
                }
                else
                {
                    var sourceArray = (Array) DeserializeArray(itemType, storage, -1, objectID);
                    var cols = sourceArrays.GetLength(i + 1);
                    for (var arrayStartIndex = 0; arrayStartIndex < cols; arrayStartIndex++)
                    {
                        indices[i + 1] = arrayStartIndex;
                        sourceArrays.SetValue(sourceArray.GetValue(arrayStartIndex), indices);
                    }
                }
            }
        }

        /// <summary>
        ///   Deserializes a dictionary from storage, handles generic types with storage optimization
        /// </summary>
        /// <param name="o"> The newly created dictionary </param>
        /// <param name="itemType"> The type of the dictionary </param>
        /// <param name="storage"> </param>
        /// <returns> The dictionary object updated with the values from storage </returns>
        private static object DeserializeDictionary(IDictionary o, Type itemType, IStorage storage)
        {
            Type keyType = null;
            Type valueType = null;
            if (itemType.IsGenericType)
            {
                var types = itemType.GetGenericArguments();
                keyType = types[0];
                valueType = types[1];
            }

            var count = storage.BeginReadDictionary(keyType, valueType);
			storage.BeginReadDictionaryKeys();
            var list = new List<object>();
            for (var i = 0; count==-1 ? storage.HasMore() : i < count; i++)
            {
				var entry = new Entry()
                                                  {
                                                      StoredType = keyType
                                                  };
                var value = storage.BeginReadDictionaryKeyItem(i, entry) ??
                    DeserializeObject(entry, storage);
                if (value.GetType().IsDefined(typeof (DeferredAttribute), true))
                {
                    var toSet = value;
                    value = new DeferredSetter(d => toSet);
                }

                if (value is DeferredSetter)
                {
                    var st = value as DeferredSetter;
                    var nd = new DeferredSetter(st.deferredRetrievalFunction) {enabled = st.enabled};
                    list.Add(null);
                    var c = list.Count - 1;
                    nd._setAction = () =>
                                       {
                                           if (list.Count > c)
                                           {
                                               list[c] = nd.deferredRetrievalFunction(st.parameters);
                                           }
                                       };
                    AddFixup(nd);
                }
                else
                {
                    list.Add(value);
                }
                storage.EndReadDictionaryKeyItem();
            }
			storage.EndReadDictionaryKeys();
			storage.BeginReadDictionaryValues();
            for (var i = 0; count==-1 ? storage.HasMore() : i < count; i++)
            {
				var entry = new Entry()
                                                  {
                                                      StoredType = valueType
                                                  };
                var value = storage.BeginReadDictionaryValueItem(i, entry) ??
                    DeserializeObject(entry, storage);
                if (value != null && value.GetType().IsDefined(typeof (DeferredAttribute), true) || list[i] == null)
                {
                    var toSet = value;
                    value = new DeferredSetter(d => toSet);
                }

                if (value is DeferredSetter)
                {
                    var st = value as DeferredSetter;
                    var nd = new DeferredSetter(st.deferredRetrievalFunction) {enabled = st.enabled};
                    var index = i;
                    nd._setAction = () =>
                                       {
                                           if (o != null && list != null)
                                           {
                                               o[list[index]] = nd.deferredRetrievalFunction(st.parameters);
                                           }
                                       };
                    AddFixup(nd);
                }
                else
                {
                    o[list[i]] = value;
                }
                storage.EndReadDictionaryValueItem();
            }
			storage.EndReadDictionaryValues();
            storage.EndReadDictionary();

            if (currentVersion >= 7 && currentVersion < 9)
            {
                DeserializeObjectAndProperties(o, itemType, storage);
            }

            return o;
        }

        /// <summary>
        ///   Deserialize a list from the data stream
        /// </summary>
        /// <param name="o"> The newly created list </param>
        /// <param name="itemType"> The type of the list </param>
        /// <param name="storage"> </param>
        /// <returns> The list updated with values from the stream </returns>
        private static object DeserializeList(IList o, Type itemType, IStorage storage)
        {
            Type valueType = null;
            if (itemType.IsGenericType)
            {
                var types = itemType.GetGenericArguments();
                valueType = types[0];
            }

            var count = storage.BeginReadList(valueType);
            for (var i = 0; count==-1 ? storage.HasMore() : i < count; i++)
            {
				var entry = new Entry()
                                                  {
                                                      StoredType = valueType,
                                                  };
                var value = storage.BeginReadListItem(i, entry) ?? 
                     DeserializeObject(entry, storage);
                if (value != null && value.GetType().IsDefined(typeof (DeferredAttribute), true))
                {
                    var toSet = value;
                    value = new DeferredSetter(d => toSet);
                }

                if (value is DeferredSetter)
                {
                    var st = value as DeferredSetter;
                    var nd = new DeferredSetter(st.deferredRetrievalFunction) {enabled = st.enabled};
                    nd._setAction = () =>
                                       {
                                           if (o != null)
                                           {
                                               o.Add(nd.deferredRetrievalFunction(st.parameters));
                                           }
                                       };
                    AddFixup(nd);
                }
                else
                {
                    o.Add(value);
                }
                storage.EndReadListItem();
            }
            if (currentVersion >= 7 && currentVersion < 9)
            {
                DeserializeObjectAndProperties(o, itemType, storage);
            }

            storage.EndReadList();
            return o;
        }

        /// <summary>
        ///   Deserializes a class based object that is not a collection, looks for both public properties and fields
        /// </summary>
        /// <param name="o"> The object being deserialized </param>
        /// <param name="itemType"> The type of the object </param>
        /// <param name="storage"> </param>
        /// <returns> The object updated with values from the stream </returns>
        private static object DeserializeObjectAndProperties(object o, Type itemType, IStorage storage)
        {
            DeserializingStack.Push(DeserializingObject);
            try
            {
				var last = currentlySerializingObject;
				currentlySerializingObject = o;
                DeserializingObject = o;
#if US_LOGGING
				Radical.Log ("[! {0}]", o.GetType().FullName);
				Radical.IndentLog();
#endif
                DeserializeFields(storage, itemType, o);
                DeserializeProperties(storage, itemType, o);
				currentlySerializingObject = last;
#if US_LOGGING
	            Radical.OutdentLog();
				Radical.Log ("[/! {0}]", o.GetType().FullName);
#endif
                return o;
            }
            finally
            {
                DeserializingObject = DeserializingStack.Pop();
            }
        }

        /// <summary>
        ///   Deserializes the properties of an object from the stream
        /// </summary>
        /// <param name="storage"> </param>
        /// <param name="itemType"> The type of the object </param>
        /// <param name="o"> The object to deserialize </param>
        private static void DeserializeProperties(IStorage storage, Type itemType, object o)
        {
            //Get the number of properties
            //var propCount = storage.ReadValue<byte>("property_count");
            var propCount = storage.BeginReadProperties();

            for (var i = 0; propCount !=-1 ? i < propCount : storage.HasMore(); i++)
            {
                //Deserialize the value
                var entry = storage.BeginReadProperty(new Entry
                                                          {
                                                              OwningType = itemType,
                                                              MustHaveName = true
                                                          });
                var value = DeserializeObject(entry, storage);
#if US_LOGGING
				if(Radical.IsLogging())
				{
					Radical.Log (string.Format("Property {0} : {1}", entry.Name, value.GetType().FullName));
				}
#endif
                if (entry.Setter != null && value != null)
                {
                    try
                    {
                        if (value.GetType().IsDefined(typeof (DeferredAttribute), true))
                        {
                            var toSet = value;
                            value = new DeferredSetter(d => toSet);
                        }
                        if (value is DeferredSetter)
                        {
                            //We need to account for there being multiple items with the same 
                            var setter = value as DeferredSetter;
                            var ns = new DeferredSetter(setter.deferredRetrievalFunction) {enabled = setter.enabled};
                            ns._setAction = () => entry.Setter.Set(o,
                                                                  setter.deferredRetrievalFunction(
                                                                      setter.parameters));
                            if (entry.OwningType.IsValueType)
                            {
                                ns._setAction();
                            }
                            else
                            {
                                AddFixup(ns);
                            }
                        } 
                        else
                        {
                            entry.Setter.Set(o, value);
                        }
                    }
                    catch
                    {
                        try
                        {
                            // if the property is nullable enum we need to handle it differently because a straight ChangeType doesn't work
                            // TODO maybe adjust persistence to have a nullable bit in propertyindex?
                            var type = Nullable.GetUnderlyingType(entry.Setter.Info.PropertyType);
                            if (type != null && type.IsEnum)
                            {
                                entry.Setter.Info.SetValue(o, Enum.Parse(type, value.ToString(), true), null);
                            }
                            else
                            {
                                entry.Setter.Info.SetValue(o,
                                                           Convert.ChangeType(value, entry.Setter.Info.PropertyType,
                                                                              null), null);
                            }
                        }
                        catch 
                        {
                           
                        }
                    }
                }
                storage.EndReadProperty();
            }
            storage.EndReadProperties();
        }

        /// <summary>
        ///   Deserializes the fields of an object from the stream
        /// </summary>
        /// <param name="storage"> </param>
        /// <param name="itemType"> The type of the object </param>
        /// <param name="o"> The object to deserialize </param>
        private static void DeserializeFields(IStorage storage, Type itemType, object o)
        {
            var fieldCount = storage.BeginReadFields();

            for (var i = 0; fieldCount==-1 ?  storage.HasMore() : i < fieldCount; i++)
            {
                var entry = storage.BeginReadField(new Entry()
                                                       {
                                                           OwningType = itemType,
                                                           MustHaveName = true
                                                       });
                var value = DeserializeObject(entry, storage);
#if US_LOGGING
				if(Radical.IsLogging())
				{
					Radical.Log (string.Format("Field {0} : {1}", entry.Name, value == null ? "null" : value.GetType().FullName));
				}
#endif
                if (entry.Setter != null && value != null)
                {
                    try
                    {
                        if (value.GetType().IsDefined(typeof (DeferredAttribute), true))
                        {
                            var toSet = value;
                            value = new DeferredSetter(d => toSet);
                        }

                        if (value is DeferredSetter)
                        {
                            //We need to account for there being multiple items with the same 
                            var setter = value as DeferredSetter;
                            var ns = new DeferredSetter(setter.deferredRetrievalFunction)
                                         {
                                             enabled = setter.enabled,
                                             _setAction = () =>
                                                             {
                                                                 if (entry.Setter != null)
                                                                 {
                                                                     entry.Setter.Set(o,
                                                                                      setter.deferredRetrievalFunction(
                                                                                          setter.parameters));
                                                                 }
                                                             }
                                         };
                            if (entry.OwningType.IsValueType)
                            {
                                ns._setAction();
                            }
                            else
                            {
                                AddFixup(ns);
                            }
                        }
                        else
                        {
                            entry.Setter.Set(o, value);
                        }
                    }
                    catch
                    {
                        try
                        {
                            // if the property is nullable enum we need to handle it differently because a straight ChangeType doesn't work
                            var type = Nullable.GetUnderlyingType(entry.Setter.FieldInfo.FieldType);
                            if (type != null && type.IsEnum)
                            {
                                entry.Setter.FieldInfo.SetValue(o, Enum.Parse(type, value.ToString(), true));
                            }
                            else
                            {
                                entry.Setter.FieldInfo.SetValue(o,
                                                                Convert.ChangeType(value,
                                                                                   entry.Setter.FieldInfo.FieldType,
                                                                                   null));
                            }
                        }
                        catch
                        {
                           
                        }
                    }
                }
                storage.EndReadField();
            }
            storage.EndReadFields();
        }

        public class DeferredSetter
        {
			public int priority = 0;
            public readonly GetData deferredRetrievalFunction;
            public bool enabled = true;
            internal readonly Dictionary<string, object> parameters = new Dictionary<string, object>();
            internal Action _setAction;

            public DeferredSetter(GetData retrievalFunction)
            {
                deferredRetrievalFunction = retrievalFunction;
            }
        }

        #endregion

        #region Nested type: EntryConfiguration

        /// <summary>
        ///   Stores configurations for entries
        /// </summary>
        private class EntryConfiguration
        {
            public GetSet Setter;
            public Type Type;
        }

        #endregion

        #region Nested type: KnownTypesStackEntry

        private class KnownTypesStackEntry
        {
            public List<RuntimeTypeHandle> knownTypesList;
            public Dictionary<RuntimeTypeHandle, ushort> knownTypesLookup;
        }

        #endregion

        #region Nested type: MissingConstructorException

        public class MissingConstructorException : Exception
        {
            public MissingConstructorException(string message)
                : base(message)
            {
            }
        }

        #endregion

        #region Basic IO

        #region Delegates

        public delegate object ReadAValue(BinaryReader reader);

        #endregion

        private static readonly Dictionary<Type, WriteAValue> Writers = new Dictionary<Type, WriteAValue>();
        public static readonly Dictionary<Type, ReadAValue> Readers = new Dictionary<Type, ReadAValue>();
        private static readonly Dictionary<string, bool> componentNames = new Dictionary<string, bool>();


        static UnitySerializer()
        {
            componentNames = typeof (Component).GetFields().Cast<MemberInfo>()
                .Concat(typeof (Component).GetProperties().Cast<MemberInfo>())
                .Select(m => m.Name)
                .ToDictionary(m => m, m => true);
            Writers[typeof (string)] = StringWriter;
            Writers[typeof (Decimal)] = DecimalWriter;
            Writers[typeof (float)] = FloatWriter;
            Writers[typeof (byte[])] = ByteArrayWriter;
            Writers[typeof (bool)] = BoolWriter;
            Writers[typeof (Guid)] = GuidWriter;
            Writers[typeof (DateTime)] = DateTimeWriter;
            Writers[typeof (TimeSpan)] = TimeSpanWriter;
            Writers[typeof (char)] = CharWriter;
            Writers[typeof (ushort)] = UShortWriter;
            Writers[typeof (double)] = DoubleWriter;
            Writers[typeof (ulong)] = ULongWriter;
            Writers[typeof (int)] = IntWriter;
            Writers[typeof (uint)] = UIntWriter;
            Writers[typeof (byte)] = ByteWriter;
            Writers[typeof (long)] = LongWriter;
            Writers[typeof (short)] = ShortWriter;
            Writers[typeof (sbyte)] = SByteWriter;

            Readers[typeof (string)] = AStringReader;
            Readers[typeof (Decimal)] = DecimalReader;
            Readers[typeof (float)] = FloatReader;
            Readers[typeof (byte[])] = ByteArrayReader;
            Readers[typeof (bool)] = BoolReader;
            Readers[typeof (Guid)] = GuidReader;
            Readers[typeof (DateTime)] = DateTimeReader;
            Readers[typeof (TimeSpan)] = TimeSpanReader;
            Readers[typeof (char)] = CharReader;
            Readers[typeof (ushort)] = UShortReader;
            Readers[typeof (double)] = DoubleReader;
            Readers[typeof (ulong)] = ULongReader;
            Readers[typeof (int)] = IntReader;
            Readers[typeof (uint)] = UIntReader;
            Readers[typeof (byte)] = ByteReader;
            Readers[typeof (long)] = LongReader;
            Readers[typeof (short)] = ShortReader;
            Readers[typeof (sbyte)] = SByteReader;
        }

        private static object ShortReader(BinaryReader reader)
        {
            return reader.ReadInt16();
        }

        private static object LongReader(BinaryReader reader)
        {
            return reader.ReadInt64();
        }

        private static object GuidReader(BinaryReader reader)
        {
			if(currentVersion >=10)
			{
				return new Guid(reader.ReadBytes(16));
			}
            return new Guid(reader.ReadString());
        }

        private static object SByteReader(BinaryReader reader)
        {
            return reader.ReadSByte();
        }

        private static object ByteReader(BinaryReader reader)
        {
            return reader.ReadByte();
        }

        private static object UIntReader(BinaryReader reader)
        {
            return reader.ReadUInt32();
        }

        private static object IntReader(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        private static object ULongReader(BinaryReader reader)
        {
            return reader.ReadUInt64();
        }

        private static object DoubleReader(BinaryReader reader)
        {
            return reader.ReadDouble();
        }

        private static object UShortReader(BinaryReader reader)
        {
            return reader.ReadUInt16();
        }

        private static object CharReader(BinaryReader reader)
        {
            return reader.ReadChar();
        }

        private static object FloatReader(BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        private static object TimeSpanReader(BinaryReader reader)
        {
            return new TimeSpan(reader.ReadInt64());
        }

        private static object DateTimeReader(BinaryReader reader)
        {
            return new DateTime(reader.ReadInt64());
        }

        private static object ByteArrayReader(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            return reader.ReadBytes(len);
        }

        private static object DecimalReader(BinaryReader reader)
        {
            var array = new int[4];
            array[0] = (int) reader.ReadInt32();
            array[1] = (int) reader.ReadInt32();
            array[2] = (int) reader.ReadInt32();
            array[3] = (int) reader.ReadInt32();

            return new Decimal(array);
        }

        private static object BoolReader(BinaryReader reader)
        {
            return reader.ReadChar() == 'Y';
        }

        private static object AStringReader(BinaryReader reader)
        {
            var retString = reader.ReadString();

            return retString == "~~NULL~~"
                       ? null
                       : retString;
        }

        private static void SByteWriter(BinaryWriter writer, object value)
        {
            writer.Write((sbyte) value);
        }

        private static void ShortWriter(BinaryWriter writer, object value)
        {
            writer.Write((short) value);
        }

        private static void LongWriter(BinaryWriter writer, object value)
        {
            writer.Write((long) value);
        }

        private static void ByteWriter(BinaryWriter writer, object value)
        {
            writer.Write((byte) value);
        }

        private static void UIntWriter(BinaryWriter writer, object value)
        {
            writer.Write((uint) value);
        }

        private static void IntWriter(BinaryWriter writer, object value)
        {
            writer.Write((int) value);
        }

        private static void ULongWriter(BinaryWriter writer, object value)
        {
            writer.Write((ulong) value);
        }

        private static void DoubleWriter(BinaryWriter writer, object value)
        {
            writer.Write((double) value);
        }

        private static void UShortWriter(BinaryWriter writer, object value)
        {
            writer.Write((ushort) value);
        }

        private static void CharWriter(BinaryWriter writer, object value)
        {
            writer.Write((char) value);
        }

        private static void TimeSpanWriter(BinaryWriter writer, object value)
        {
            writer.Write(((TimeSpan) value).Ticks);
        }

        private static void DateTimeWriter(BinaryWriter writer, object value)
        {
            writer.Write(((DateTime) value).Ticks);
        }

        private static void GuidWriter(BinaryWriter writer, object value)
        {
            writer.Write(((Guid)value).ToByteArray());
        }

        private static void BoolWriter(BinaryWriter writer, object value)
        {
            writer.Write((bool) value
                             ? 'Y'
                             : 'N');
        }

        private static void ByteArrayWriter(BinaryWriter writer, object value)
        {
            var array = value as byte[];
            writer.Write((int) array.Length);
            writer.Write(array);
        }

        private static void FloatWriter(BinaryWriter writer, object value)
        {
            writer.Write((float) value);
        }

        private static void DecimalWriter(BinaryWriter writer, object value)
        {
            var array = Decimal.GetBits((Decimal) value);
            writer.Write(array[0]);
            writer.Write(array[1]);
            writer.Write(array[2]);
            writer.Write(array[3]);
        }

        private static void StringWriter(BinaryWriter writer, object value)
        {
            writer.Write((string) value);
        }


        /// <summary>
        ///   Write a basic untyped value
        /// </summary>
        /// <param name="writer"> The writer to commit byte to </param>
        /// <param name="value"> The value to write </param>
        internal static void WriteValue(BinaryWriter writer, object value)
        {
            WriteAValue write;

            if (!Writers.TryGetValue(value.GetType(), out write))
            {
                writer.Write((int) value);
                return;
            }
            write(writer, value);
        }

        private delegate void WriteAValue(BinaryWriter writer, object value);

        #endregion

        #region Nested type: ObjectMappingEventArgs

        /// <summary>
        ///   Arguments for object creation event
        /// </summary>
        public class ObjectMappingEventArgs : EventArgs
        {
            /// <summary>
            ///   Supply a type to use instead
            /// </summary>
            public object Instance = null;

            /// <summary>
            ///   The type that cannot be
            /// </summary>
            public Type TypeToConstruct;
        }

        #endregion

        #region Nested type: PropertyNameStackEntry

        internal class PropertyNameStackEntry
        {
            public List<string> propertyList;
            public Dictionary<string, ushort> propertyLookup;
        }

        #endregion

        #region Nested type: ScanTypeFunction

        internal delegate void ScanTypeFunction(Type type, Attribute attribute);

        #endregion

        #region Nested type: SerializationScope
		
		
        public class SerializationScope : IDisposable
        {
			static Stack<bool> _primaryScopeStack = new Stack<bool>();
			static bool _hasSetPrimaryScope;
			static bool _primaryScope;
            private static int _counter = 0;

            public static bool IsPrimaryScope
            {
                get
                {
                    return _primaryScope || true;
                }
            }
			
			public static void SetPrimaryScope()
			{
				if(_hasSetPrimaryScope)
					return;
				_primaryScope = true;
				_hasSetPrimaryScope = true;
			}
			
            public SerializationScope()
            {
				_primaryScopeStack.Push(_primaryScope);
				_primaryScope = false;
                if (_seenObjects == null)
                {
                    _seenObjects = new Dictionary<object, int>();
                }
                if (_loadedObjects == null)
                {
                    _loadedObjects = new Dictionary<int, object>();
                }
                if (_seenTypes == null)
                {
                    _seenTypes = new Dictionary<Type, bool>();
                }
                if (_counter == 0)
                {
                    _seenObjects.Clear();
                    _loadedObjects.Clear();
                    _seenTypes.Clear();
                    _nextId = 0;
                }
                _counter++;
            }

            public void Dispose()
            {
				_primaryScope = _primaryScopeStack.Pop();
                if (--_counter != 0)
                {
                    return;
                }
				_hasSetPrimaryScope = false;
                _nextId = 0;
                _seenObjects.Clear();
                _loadedObjects.Clear();
                _seenTypes.Clear();
				if(_knownTypesLookup != null) _knownTypesLookup.Clear();
				if(_knownTypesList != null) _knownTypesList.Clear();
				if(_propertyLookup != null) _propertyLookup.Clear();
				if(_propertyList != null) _propertyList.Clear();
				
            }
        }

        #endregion

        #region Nested type: SerializationSplitScope

        public class SerializationSplitScope : IDisposable
        {
            public SerializationSplitScope()
            {
                CreateStacks();
                if (_seenObjects == null)
                {
                    _seenObjects = new Dictionary<object, int>();
                }
                if (_loadedObjects == null)
                {
                    _loadedObjects = new Dictionary<int, object>();
                }
                if (_seenTypes == null)
                {
                    _seenTypes = new Dictionary<Type, bool>();
                }
                _seenTypesStack.Push(_seenTypes);
                _storedObjectsStack.Push(_seenObjects);
                _loadedObjectStack.Push(_loadedObjects);
                _idStack.Push(_nextId);
                _nextId = 0;
                _seenObjects = new Dictionary<object, int>();
                _loadedObjects = new Dictionary<int, object>();
                _seenTypes = new Dictionary<Type, bool>();
            }

            #region IDisposable Members

            public void Dispose()
            {
                _seenObjects = _storedObjectsStack.Pop();
                _loadedObjects = _loadedObjectStack.Pop();
                _seenTypes = _seenTypesStack.Pop();
                _nextId = _idStack.Pop();
            }

            #endregion
        }

        #endregion

        #region Nested type: TypeMappingEventArgs

        /// <summary>
        ///   Arguments for a missing type event
        /// </summary>
        public class TypeMappingEventArgs : EventArgs
        {
            /// <summary>
            ///   The missing types name
            /// </summary>
            public string TypeName = String.Empty;

            /// <summary>
            ///   Supply a type to use instead
            /// </summary>
            public Type UseType = null;
        }

        #endregion
    }
}