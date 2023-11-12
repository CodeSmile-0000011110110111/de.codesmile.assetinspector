// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Editor.Bindings
{
	internal class AssetSubObjects : ScriptableObject
	{
		public Object[] Objects;

		public AssetSubObjects Init(Asset asset)
		{
			Objects = asset != null ? asset.SubObjects : new Object[0];
			return this;
		}
	}
}
