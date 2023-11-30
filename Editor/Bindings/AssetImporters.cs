// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeSmileEditor.Bindings
{
	internal class AssetImporters : ScriptableObject
	{
		public List<String> AvailableImporters;

		public AssetImporters Init(Asset asset)
		{
			AvailableImporters = new List<String>();
			if (asset != null)
				AddAvailableImporters(asset);

			return this;
		}

		private void AddAvailableImporters(Asset asset)
		{
			foreach (var importer in Asset.Importer.GetAvailable(asset.AssetPath))
				AvailableImporters.Add(importer.FullName);
		}
	}
}
