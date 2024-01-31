// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmileEditor.Bindings;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CodeSmileEditor
{
	public class AssetInspector : EditorWindow
	{
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset;

		private VisualElement m_Document;

		private ObjectBaseClasses m_BaseClasses;
		private AssetLabels m_Labels;
		private AssetDependencies m_Dependencies;
		private AssetSubObjects m_SubObjects;
		private AssetVisibleSubObjects m_VisibleSubObjects;
		private AssetImporters m_Importers;

		[MenuItem("Window/CodeSmile/Asset Inspector", priority = 2999)]
		public static void ShowAssetInspector()
		{
			var window = GetWindow<AssetInspector>();
			window.titleContent = new GUIContent("Asset Inspector");
		}

		private static Asset GetAssetFromSelection()
		{
			Asset asset = null;

			var selectedObject = Selection.activeObject;
			if (Asset.Status.IsImported(selectedObject))
				asset = new Asset(selectedObject);

			return asset;
		}

		private static void HideObjectFieldSelector(ObjectField field)
		{
			var selector = field.Q<VisualElement>(className: "unity-object-field__selector");
			selector.visible = false;
			selector.SetEnabled(false);
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
			var selection = Selection.activeObject;

			UpdateAssetDetails(asset, selection);
			UpdateTypeDetails(asset, selection);
			UpdateFileDetails(asset, selection);
			UpdateIdentity(asset, selection);
			UpdateImporterDetails(asset);
			UpdateBundleDetails(asset);
			UpdateStatus(asset,selection);
			UpdateVersionControlStatus(asset, selection);

			UpdateAllSubAssets(asset);
			UpdateVisibleSubAssets(asset);
			UpdateLabels(asset);
			UpdateDependencies(asset);

			UpdateIconDetails(asset, selection);
		}

		private void RegisterEvents(Boolean register)
		{
			var openFile = Find<TextField>("AssetFile").Q<Button>();
			var openFolder = Find<TextField>("AssetFolder").Q<Button>();
			var inheritanceList = Find<ListView>("AssetInheritsFrom");
			var labelsList = Find<ListView>("AssetLabels");
			var directDependenciesList = Find<ListView>("AssetDirectDependencies");
			var allDependenciesList = Find<ListView>("AssetAllDependencies");
			var allObjectsList = Find<ListView>("AllSubAssets");
			var visibleObjectsList = Find<ListView>("VisibleSubAssets");

			if (register)
			{
				openFile.clicked += () => OnOpenFileClicked();
				openFolder.clicked += () => OnOpenFolderClicked();
				inheritanceList.makeItem += MakeListViewStringItem();
				labelsList.makeItem += MakeListViewStringItem();
				directDependenciesList.makeItem += MakeListViewStringItem();
				allDependenciesList.makeItem += MakeListViewStringItem();
				allObjectsList.makeItem += MakeListViewObjectItem();
				visibleObjectsList.makeItem += MakeListViewObjectItem();
			}
			else
			{
				openFile.clicked -= () => OnOpenFileClicked();
				openFolder.clicked -= () => OnOpenFolderClicked();
				inheritanceList.makeItem -= MakeListViewStringItem();
				labelsList.makeItem -= MakeListViewStringItem();
				directDependenciesList.makeItem -= MakeListViewStringItem();
				allDependenciesList.makeItem -= MakeListViewStringItem();
				allObjectsList.makeItem -= MakeListViewObjectItem();
				visibleObjectsList.makeItem -= MakeListViewObjectItem();
			}
		}

		private void OnOpenFileClicked() => GetAssetFromSelection()?.OpenExternal();

		private void OnOpenFolderClicked() => GetAssetFromSelection()?.AssetPath?.OpenExternal();

		private void OnSelectionChanged() => UpdateGui();

		private void UpdateAssetDetails(Asset asset, Object selection)
		{
			Find<TextField>("AssetName").value = selection != null ? selection.name : "<no selection>";

			var mainObject = Find<ObjectField>("AssetMainObject");
			mainObject.value = asset?.MainObject;
			HideObjectFieldSelector(mainObject);
		}

		private void UpdateTypeDetails(Asset asset, Object selection)
		{
			Find<TextField>("AssetType").value = selection != null ? selection.GetType().Name : String.Empty;
			Find<TextField>("AssetNamespace").value = selection != null ? selection.GetType().Namespace : String.Empty;
			Find<TextField>("AssetAssemblyQualifiedName").value =
				selection != null ? selection.GetType().AssemblyQualifiedName : String.Empty;

			m_BaseClasses = CreateInstance<ObjectBaseClasses>().Init(selection);
			var list = Find<ListView>("AssetInheritsFrom");
			list.bindingPath = nameof(m_BaseClasses.BaseClasses);
			list.Bind(new SerializedObject(m_BaseClasses));
		}

		private void UpdateFileDetails(Asset asset, Object selection)
		{
			var selectionPath = Asset.Path.Get(selection);
			Find<TextField>("AssetFile").value = selectionPath?.FileName;
			Find<TextField>("AssetFolder").value = selectionPath?.FolderPath;
			Find<TextField>("AssetPath").value = selectionPath?.AssetPath;
			Find<TextField>("AssetFullPath").value = selectionPath?.FullPath;
			Find<TextField>("AssetMetaPath").value = selectionPath?.MetaPath;
			if (selectionPath?.Equals("Assets") == false) // "Assets" has no .meta
				Find<TextField>("AssetMetaFullPath").value = selectionPath?.MetaPath.FullPath;
		}

		private void UpdateIdentity(Asset asset, Object selection)
		{
			var (guid, fileId) = Asset.File.GetGuidAndFileId(selection);
			var instanceId = selection != null ? selection.GetInstanceID() : 0;

			Find<TextField>("AssetGuid").value = guid.Empty() == false ? guid.ToString() : String.Empty;
			Find<TextField>("AssetLocalFileId").value = fileId != 0L ? fileId.ToString() : String.Empty;
			Find<TextField>("AssetInstanceId").value = instanceId != 0 ? instanceId.ToString() : String.Empty;
		}

		private void UpdateImporterDetails(Asset asset)
		{
#if UNITY_2022_2_OR_NEWER
			Find<TextField>("AssetDefaultImporter").value =
				asset != null ? asset.DefaultImporter?.FullName : String.Empty;
			Find<TextField>("AssetActiveImporter").value =
				asset != null ? asset.ActiveImporter?.FullName : String.Empty;
			Find<Toggle>("AssetImporterIsOverridden").value = asset != null ? asset.IsImporterOverridden : false;
#endif

			m_Importers = CreateInstance<AssetImporters>().Init(asset);
			var list = Find<ListView>("AssetAvailableImporters");
			list.bindingPath = nameof(m_Importers.AvailableImporters);
			list.Bind(new SerializedObject(m_Importers));
		}

		private void UpdateBundleDetails(Asset asset)
		{
			Find<TextField>("AssetOwningBundle").value = asset != null ? asset.OwningBundle : String.Empty;
			Find<TextField>("AssetOwningBundleVariant").value =
				asset != null ? asset.OwningBundleVariant : String.Empty;
		}

		private void UpdateStatus(Asset asset, Object selection)
		{
			var group = Find<Foldout>("AssetStatus");
			Find<Toggle>("AssetImported", group).value = selection != null ? Asset.Status.IsImported(selection) : false;
			Find<Toggle>("AssetLoaded", group).value = selection != null ? Asset.Status.IsLoaded(Asset.Path.Get(selection)) : false;
			Find<Toggle>("MainAsset", group).value = selection != null ? Asset.Status.IsMain(selection) : false;
			Find<Toggle>("SubAsset", group).value = selection != null ? Asset.Status.IsSub(selection) : false;
			Find<Toggle>("NativeAsset", group).value = selection != null ? Asset.Status.IsNative(selection) : false;
			Find<Toggle>("ForeignAsset", group).value = selection != null ? Asset.Status.IsForeign(selection) : false;
			Find<Toggle>("CanOpenAsset", group).value = selection != null ? Asset.File.CanOpenInEditor(selection) : false;
			Find<Toggle>("IsSceneAsset", group).value = selection != null ? Asset.Status.IsScene(selection) : false;
		}

		private void UpdateVersionControlStatus(Asset asset, Object selection)
		{
			var group = Find<Foldout>("AssetVersionControlStatus");
			Find<Toggle>("AssetIsEditable", group).value =
				selection != null ? Asset.VersionControl.IsEditable(selection) : false;
			Find<Toggle>("AssetIsMetaEditable", group).value =
				selection != null ? Asset.VersionControl.IsMetaEditable(selection) : false;
			Find<Toggle>("AssetCanMakeEditable", group).value =
				selection != null ? Asset.VersionControl.CanMakeEditable(selection) : false;
		}

		private void UpdateAllSubAssets(Asset asset)
		{
			m_SubObjects = CreateInstance<AssetSubObjects>().Init(asset);

			var list = Find<ListView>("AllSubAssets");
			list.bindingPath = nameof(m_SubObjects.Objects);
			list.Bind(new SerializedObject(m_SubObjects));
		}

		private void UpdateVisibleSubAssets(Asset asset)
		{
			m_VisibleSubObjects = CreateInstance<AssetVisibleSubObjects>().Init(asset);

			var list = Find<ListView>("VisibleSubAssets");
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

			{
				var list = Find<ListView>("AssetDirectDependencies");
				list.bindingPath = nameof(m_Dependencies.DirectDependencies);
				list.Bind(new SerializedObject(m_Dependencies));
			}
			{
				var list = Find<ListView>("AssetAllDependencies");
				list.bindingPath = nameof(m_Dependencies.AllDependencies);
				list.Bind(new SerializedObject(m_Dependencies));
			}
		}

		private void UpdateIconDetails(Asset asset, Object selection)
		{
			var icon = Asset.GetIcon(selection);
			Find<TextField>("AssetIcon").value = icon != null ? $"{icon.name} ({icon.width}x{icon.height})" : String.Empty;;
			Find<TextField>("AssetIconPath").value = icon != null ? $"{Asset.Path.Get(icon)}" : String.Empty;

			var image = Find<VisualElement>("AssetIconImage");
			if (icon != null)
			{
				image.style.backgroundImage = new StyleBackground(icon);
				image.style.width = new StyleLength(icon.width);
				image.style.height = new StyleLength(icon.height);
			}
			else
			{
				image.style.backgroundImage = new StyleBackground(StyleKeyword.None);
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
			HideObjectFieldSelector(field);

			field.style.fontSize = 12f;
			field.style.flexShrink = new StyleFloat(StyleKeyword.Auto);
			field.style.paddingLeft = new StyleLength(14f);
			field.focusable = false;
			return field;
		};

		private T Find<T>(String name, VisualElement element = null) where T : VisualElement =>
			element == null ? m_Document.Q<T>(name) : element.Q<T>(name);
	}
}
