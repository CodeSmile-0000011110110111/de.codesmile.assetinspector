// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.Editor.Bindings
{
	internal class ObjectBaseClasses : ScriptableObject
	{
		public List<String> BaseClasses;

		public ObjectBaseClasses Init(UnityEngine.Object obj)
		{
			BaseClasses = new List<String>();
			if (obj != null)
				AddBaseClassRecursive(obj.GetType());

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
}
