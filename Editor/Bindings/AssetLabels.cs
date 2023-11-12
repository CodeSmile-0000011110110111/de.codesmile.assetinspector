// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Editor.Bindings
{
	internal class AssetLabels : ScriptableObject
	{
		public String[] Labels;

		public AssetLabels Init(Asset asset)
		{
			Labels = asset != null ? asset.Labels : new String[0];
			return this;
		}
	}
}
