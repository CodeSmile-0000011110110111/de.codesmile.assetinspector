// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmileEditor.Bindings
{
	internal class AssetSubObjects : ScriptableObject
	{
		public Object[] Objects;

		public AssetSubObjects Init(Asset asset)
		{
			Objects = asset != null ? asset.SubAssets : new Object[0];
			return this;
		}
	}
}
