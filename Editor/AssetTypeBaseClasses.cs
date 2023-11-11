﻿// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

internal class AssetTypeBaseClasses : ScriptableObject
{
	public List<String> BaseClasses;

	public AssetTypeBaseClasses Init(Asset asset)
	{
		BaseClasses = new List<String>();
		if (asset != null && asset.Type != null)
			AddBaseClassRecursive(asset.Type);

		return this;
	}

	private void AddBaseClassRecursive(Type type)
	{
		var baseType = type.BaseType;
		if (baseType != null && baseType != typeof(Object))
		{
			BaseClasses.Add(type.BaseType.FullName);
			AddBaseClassRecursive(baseType);
		}
	}
}