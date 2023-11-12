// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Editor.Bindings;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.Editor
{
	public class AssetInspector : EditorWindow
	{
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset;

		private VisualElement m_Document;

		private AssetTypeBaseClasses m_BaseClasses;
		private AssetLabels m_Labels;
		private AssetDependencies m_Dependencies;
		private AssetSubObjects m_SubObjects;
		private AssetVisibleSubObjects m_VisibleSubObjects;

		[MenuItem("CodeSmile/Asset Inspector")]
		public static void ShowAssetInspector()
		{
			var window = GetWindow<AssetInspector>();
			window.titleContent = new GUIContent("Asset Inspector");
		}

		private static Asset GetAssetFromSelection()
		{
			Asset asset = null;

			var active = Selection.activeObject;
			if (Asset.IsImported(active))
				asset = new Asset(active);

			return asset;
		}

		public void CreateGUI()
		{
			m_Document = m_VisualTreeAsset.Instantiate();
			rootVisualElement.Add(m_Document);

			RegisterEvents(true);
		}

		private void OnEnable() => Selection.selectionChanged += OnSelectionChanged;

		private void OnDisable()
		{
			Selection.selectionChanged -= OnSelectionChanged;

			RegisterEvents(false);
		}

		private void UpdateGui()
		{
			var asset = GetAssetFromSelection();

			UpdateName(asset);
			// rename button

			// Type Details
			UpdateType(asset);
			UpdateNamespace(asset);
			UpdateFullyQualifiedName(asset);
			UpdateInheritsFrom(asset);

			// File Details
			UpdateFile(asset);
			UpdateFolder(asset);
			UpdatePath(asset);
			UpdateFullPath(asset);

			// Identity
			UpdateGuid(asset);
			UpdateInstanceId(asset);
			UpdateLocalFileId(asset);

			UpdateStatus(asset);
			UpdateAllObjects(asset);
			UpdateVisibleObjects(asset);
			UpdateLabels(asset);

			// Dependencies
			UpdateDependencies(asset);

			// Icon Details
			UpdateIcon(asset);
			UpdateIconPath(asset);
			UpdateIconImage(asset);
		}

		private void RegisterEvents(Boolean register)
		{
			var openFile = Find<TextField>("AssetFile").Q<Button>();
			var openFolder = Find<TextField>("AssetFolder").Q<Button>();
			var inheritanceList = Find<ListView>("AssetInheritsFrom");
			var labelsList = Find<ListView>("AssetLabels");
			var dependenciesList = Find<ListView>("AssetDependencies");
			var allObjectsList = Find<ListView>("AssetSubObjects");
			var visibleObjectsList = Find<ListView>("AssetVisibleSubObjects");

			if (register)
			{
				openFile.clicked += () => OnOpenFileClicked();
				openFolder.clicked += () => OnOpenFolderClicked();
				inheritanceList.makeItem += MakeListViewStringItem();
				labelsList.makeItem += MakeListViewStringItem();
				dependenciesList.makeItem += MakeListViewStringItem();
				allObjectsList.makeItem += MakeListViewObjectItem();
				visibleObjectsList.makeItem += MakeListViewObjectItem();
			}
			else
			{
				openFile.clicked -= () => OnOpenFileClicked();
				openFolder.clicked -= () => OnOpenFolderClicked();
				inheritanceList.makeItem -= MakeListViewStringItem();
				labelsList.makeItem -= MakeListViewStringItem();
				dependenciesList.makeItem -= MakeListViewStringItem();
				allObjectsList.makeItem -= MakeListViewObjectItem();
				visibleObjectsList.makeItem -= MakeListViewObjectItem();
			}
		}

		private void OnOpenFileClicked() => GetAssetFromSelection()?.OpenInDefaultApplication();

		private void OnOpenFolderClicked() => GetAssetFromSelection()?.AssetPath?.OpenFolder();

		private void OnSelectionChanged() => UpdateGui();

		private void UpdateName(Asset asset) => Find<TextField>("AssetName").value =
			asset != null ? asset.MainObject.name : "<no selection>";

		private void UpdateType(Asset asset) => Find<TextField>("AssetType").value =
			asset != null ? Asset.GetMainType(asset.AssetPath).Name : String.Empty;

		private void UpdateNamespace(Asset asset) => Find<TextField>("AssetNamespace").value =
			asset != null ? Asset.GetMainType(asset.AssetPath).Namespace : String.Empty;

		private void UpdateFullyQualifiedName(Asset asset) => Find<TextField>("AssetAssemblyQualifiedName").value =
			asset != null ? Asset.GetMainType(asset.AssetPath).AssemblyQualifiedName : String.Empty;

		private void UpdateInheritsFrom(Asset asset)
		{
			m_BaseClasses = CreateInstance<AssetTypeBaseClasses>().Init(asset);

			var list = Find<ListView>("AssetInheritsFrom");
			list.bindingPath = nameof(m_BaseClasses.BaseClasses);
			list.Bind(new SerializedObject(m_BaseClasses));
		}

		private void UpdateFile(Asset asset)
		{
			var file = Find<TextField>("AssetFile");
			file.value = asset != null ? asset.AssetPath.FileName : String.Empty;
		}

		private void UpdateFolder(Asset asset)
		{
			var folder = Find<TextField>("AssetFolder");
			folder.value = asset != null ? asset.AssetPath.FolderPath : String.Empty;
		}

		private void UpdatePath(Asset asset) =>
			Find<TextField>("AssetPath").value = asset != null ? asset.AssetPath : String.Empty;

		private void UpdateFullPath(Asset asset) => Find<TextField>("AssetFullPath").value =
			asset != null ? asset.AssetPath.FullPath : String.Empty;

		private void UpdateGuid(Asset asset) =>
			Find<TextField>("AssetGuid").value = asset != null ? asset.Guid.ToString() : String.Empty;

		private void UpdateInstanceId(Asset asset) => Find<TextField>("AssetInstanceId").value =
			asset != null ? asset.MainObject.GetInstanceID().ToString() : String.Empty;

		private void UpdateLocalFileId(Asset asset) => Find<TextField>("AssetLocalFileId").value =
			asset != null ? asset.LocalFileId.ToString() : String.Empty;

		private void UpdateStatus(Asset asset)
		{
			var group = Find<Foldout>("AssetStatus");
			var imported = Find<Toggle>("AssetImported", group);
			imported.value = asset != null ? Asset.IsImported(asset) : false;
			var loaded = Find<Toggle>("AssetLoaded", group);
			loaded.value = asset != null ? Asset.IsLoaded(asset) : false;
			var main = Find<Toggle>("MainAsset", group);
			main.value = asset != null ? Asset.IsMain(asset) : false;
			var sub = Find<Toggle>("SubAsset", group);
			sub.value = asset != null ? Asset.IsSub(asset) : false;
			var native = Find<Toggle>("NativeAsset", group);
			native.value = asset != null ? Asset.IsNative(asset) : false;
			var foreign = Find<Toggle>("ForeignAsset", group);
			foreign.value = asset != null ? Asset.IsForeign(asset) : false;
		}

		private void UpdateAllObjects(Asset asset)
		{
			m_SubObjects = CreateInstance<AssetSubObjects>().Init(asset);

			var list = Find<ListView>("AssetSubObjects");
			list.bindingPath = nameof(m_SubObjects.Objects);
			list.Bind(new SerializedObject(m_SubObjects));
		}

		private void UpdateVisibleObjects(Asset asset)
		{
			m_VisibleSubObjects = CreateInstance<AssetVisibleSubObjects>().Init(asset);

			var list = Find<ListView>("AssetVisibleSubObjects");
			list.bindingPath = nameof(m_VisibleSubObjects.Objects);
			list.Bind(new SerializedObject(m_VisibleSubObjects));
		}

		private void UpdateLabels(Asset asset)
		{
			m_Labels = CreateInstance<AssetLabels>().Init(asset);

			var list = Find<ListView>("AssetLabels");
			list.bindingPath = nameof(m_Labels.Labels);
			list.Bind(new SerializedObject(m_Labels));
		}

		private void UpdateDependencies(Asset asset)
		{
			m_Dependencies = CreateInstance<AssetDependencies>().Init(asset);

			var list = Find<ListView>("AssetDependencies");
			list.bindingPath = nameof(m_Dependencies.Dependencies);
			list.Bind(new SerializedObject(m_Dependencies));
		}

		private void UpdateIcon(Asset asset)
		{
			var field = Find<TextField>("AssetIcon");
			field.value = asset != null && asset.Icon != null
				? $"{asset.Icon.name} ({asset.Icon.width}x{asset.Icon.height})"
				: String.Empty;
		}

		private void UpdateIconPath(Asset asset)
		{
			var field = Find<TextField>("AssetIconPath");
			field.value = asset != null && asset.Icon != null ? $"{Asset.Path.Get(asset.Icon)}" : String.Empty;
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

		private Func<VisualElement> MakeListViewStringItem() => () =>
		{
			var label = new Label();
			label.style.fontSize = 12f;
			label.style.flexShrink = new StyleFloat(StyleKeyword.Auto);
			label.style.paddingLeft = new StyleLength(18f);
			label.style.color = new StyleColor(new Color(.8f, .8f, 1f));
			return label;
		};

		private Func<VisualElement> MakeListViewObjectItem() => () =>
		{
			var field = new ObjectField();
			var selector = field.Q<VisualElement>(className: "unity-object-field__selector");
			selector.visible = false;
			selector.SetEnabled(false);

			field.style.fontSize = 12f;
			field.style.flexShrink = new StyleFloat(StyleKeyword.Auto);
			field.style.paddingLeft = new StyleLength(14f);
			field.focusable = false;
			//field.SetEnabled(false);
			return field;
		};

		private T Find<T>(String name, VisualElement element = null) where T : VisualElement =>
			element == null ? m_Document.Q<T>(name) : element.Q<T>(name);
	}
}
