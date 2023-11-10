// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Editor;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class AssetInspector : EditorWindow
{
	[SerializeField] private VisualTreeAsset m_VisualTreeAsset;

	private VisualElement m_Document;

	[MenuItem("CodeSmile/Asset Inspector")]
	public static void ShowAssetInspector()
	{
		var window = GetWindow<AssetInspector>();
		window.titleContent = new GUIContent("Asset Inspector");
	}

	private void OnEnable()
	{
		Debug.Log("enable");
		Selection.selectionChanged += OnSelectionChanged;
	}

	private void OnDisable()
	{
		Debug.Log("disable");
		Selection.selectionChanged -= OnSelectionChanged;
	}

	public void CreateGUI()
	{
		m_Document = m_VisualTreeAsset.Instantiate();
		rootVisualElement.Add(m_Document);
	}

	private void OnSelectionChanged() => UpdateGui(Selection.activeObject);

	private void UpdateGui(Object active)
	{
		Asset asset = null;
		if (Asset.Exists(active))
			asset = new Asset(active);

		UpdateName(active, asset);
		UpdateType(active, asset);
		UpdatePath(active, asset);
		UpdateGuid(active, asset);
		UpdateInstanceId(active, asset);
		//UpdateLabels(active, asset);
		UpdateIcon(active, asset);
	}

	private void UpdateName(Object active, Asset asset) =>
		Find<TextField>("AssetName").value = active != null ? active.name : "<no selection>";

	private void UpdateType(Object active, Asset asset) =>
		Find<TextField>("AssetType").value = asset != null ? asset.Type.FullName : String.Empty;

	private void UpdatePath(Object active, Asset asset) =>
		Find<TextField>("AssetPath").value = asset != null ? asset.AssetPath : String.Empty;

	private void UpdateGuid(Object active, Asset asset) =>
		Find<TextField>("AssetGuid").value = asset != null ? asset.Guid.ToString() : String.Empty;

	private void UpdateInstanceId(Object active, Asset asset) => Find<TextField>("AssetInstanceId").value =
		asset != null ? asset.MainObject.GetInstanceID().ToString() : String.Empty;

	private void UpdateLabels(Object active, Asset asset) =>
		Find<TextField>("AssetLabels").value = asset != null ? asset.AssetPath : String.Empty;

	private void UpdateIcon(Object active, Asset asset)
	{
		var field = Find<TextField>("AssetIcon");

		var iconAsset = (Asset)asset.Icon;

		field.value = asset != null && asset.Icon != null
			? $"{asset.Icon.name} {iconAsset.AssetPath.FileName} ({asset.Icon.width}x{asset.Icon.height})"
			: String.Empty;

		UpdateIconImage(asset);
	}

	private void UpdateIconImage(Asset asset)
	{
		var image = Find<VisualElement>("AssetIconImage");
		if (asset != null && asset.Icon is Texture2D texture)
		{
			image.style.backgroundImage = new StyleBackground(texture);
			image.style.backgroundSize = new BackgroundSize(texture.width, texture.height);
			image.style.width = new StyleLength(texture.width);
			image.style.height = new StyleLength(texture.height);
		}
		else
		{
			image.style.backgroundImage = new StyleBackground(StyleKeyword.None);
			image.style.backgroundSize = new BackgroundSize(16, 16);
			image.style.width = new StyleLength(0f);
			image.style.height = new StyleLength(0f);
		}
	}

	private T Find<T>(String name) where T : VisualElement => m_Document.Q<T>(name);
}
