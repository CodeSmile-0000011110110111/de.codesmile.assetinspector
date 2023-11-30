// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmileEditor.Bindings
{
	internal class AssetDependencies : ScriptableObject
	{
		public String[] AllDependencies;
		public String[] DirectDependencies;

		public AssetDependencies Init(Asset asset)
		{
			AllDependencies = asset != null ? asset.Dependencies : new String[0];
			DirectDependencies = asset != null ? asset.DirectDependencies : new String[0];
			return this;
		}
	}
}
