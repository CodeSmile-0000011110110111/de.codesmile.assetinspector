// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Editor.Bindings
{
	internal class AssetDependencies : ScriptableObject
	{
		public String[] Dependencies;

		public AssetDependencies Init(Asset asset)
		{
			Dependencies = asset != null ? asset.Dependencies : new String[0];
			return this;
		}
	}
}
