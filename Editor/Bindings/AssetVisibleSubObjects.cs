// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmileEditor.Bindings
{
	internal class AssetVisibleSubObjects : ScriptableObject
	{
		public Object[] Objects;

		public AssetVisibleSubObjects Init(Asset asset)
		{
			Objects = asset != null ? asset.VisibleSubAssets : new Object[0];
			return this;
		}
	}
}
