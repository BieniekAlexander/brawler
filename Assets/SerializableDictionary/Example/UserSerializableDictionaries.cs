using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable] // TODO mine
public class FrameCastablesDictionary : SerializableDictionary<int, Castable[], CastableArrayStorage> { }

[Serializable] // TODO mine
public class ConditionCastablesDictionary : SerializableDictionary<CastableCondition, Castable[], CastableArrayStorage> { }

[Serializable] // TODO also mine
public class CastableArrayStorage : SerializableDictionary.Storage<Castable[]> { }

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> {}

[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> {}

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> {}

[Serializable]
public class MyClass
{
    public int i;
    public string str;
}

[Serializable]
public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass> {}