#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(ObjectColorDictionary))]
[CustomPropertyDrawer(typeof(StringColorArrayDictionary))]
[CustomPropertyDrawer(typeof(FrameCastablesDictionary))] // TODO mine
[CustomPropertyDrawer(typeof(ConditionCastablesDictionary))] // TODO mine
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}
[CustomPropertyDrawer(typeof(ColorArrayStorage))]
[CustomPropertyDrawer(typeof(CastableArrayStorage))] // TODO mine
public class AnySerializableDictionaryStoragePropertyDrawer: SerializableDictionaryStoragePropertyDrawer {}
#endif