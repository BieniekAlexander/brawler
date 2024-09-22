using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(ObjectColorDictionary))]
[CustomPropertyDrawer(typeof(StringColorArrayDictionary))]
[CustomPropertyDrawer(typeof(FrameCastablesDictionary))] // TODO mine
[CustomPropertyDrawer(typeof(ConditionCastablesDictionary))] // TODO mine
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}
[CustomPropertyDrawer(typeof(ColorArrayStorage))]
[CustomPropertyDrawer(typeof(CastableArrayStorage))] // TODO mine
public class AnySerializableDictionaryStoragePropertyDrawer: SerializableDictionaryStoragePropertyDrawer {}
