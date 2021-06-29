using HananokiEditor.Extensions;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using E = HananokiEditor.BuildAssist.SettingsEditor;
using P = HananokiEditor.BuildAssist.SettingsProject;
//using SS = HananokiEditor.SharedModule.S;


//using SS = HananokiEditor.SharedModule.S;

namespace HananokiEditor.BuildAssist {
	using UI;
	using Item = TreeView_BuildPropertyR.Item;


	public sealed class TreeView_BuildPropertyR : HTreeView<Item> {

		public class Item : TreeViewItem {
			public bool toggle;
			public string path;
			public bool title;
			public PropertyItem uiDraw;
		}


		P.Platform m_platform;
		public BuildPropertyBase m_buildPlatformDrawer;

		public Rect m_lastCellRect;


		/////////////////////////////////////////
		public TreeView_BuildPropertyR() : base( new TreeViewState() ) {
			E.Load();

			showAlternatingRowBackgrounds = true;
			//rowHeight = 16;// EditorGUIUtility.singleLineHeight;

			//RegisterFiles();
		}


		/////////////////////////////////////////
		public void RegisterFiles(  ) {
			RegisterFiles( m_platform , m_buildPlatformDrawer );
		}


		/////////////////////////////////////////
		public void RegisterFiles( P.Platform platform, BuildPropertyBase drawer ) {
			var lst = new List<Item>();
			InitID();

			m_platform = platform;
			m_buildPlatformDrawer = drawer;

			m_registerItems = m_buildPlatformDrawer.CreateItemList();

			ReloadAndSorting();
			ExpandAll();
		}


		/////////////////////////////////////////
		public void ReloadAndSorting() {
			Reload();
			//RollbackLastSelect();
		}


		/////////////////////////////////////////
		protected override void OnSingleClickedItem( Item item ) {
			BackupLastSelect( item );
		}


		/////////////////////////////////////////
		protected override void OnRowGUI( Item item, RowGUIArgs args ) {

			m_lastCellRect = args.rowRect;

			if( item.uiDraw != null ) {
				var p = P.GetCurrentParams();
				ScopeDisable.Begin( item.uiDraw.isDisable( p ) );

				DefaultRowGUI( args );

				var rect = args.rowRect;
				ScopeChange.Begin();
				if( item.uiDraw.UIDraw( rect.TrimL( 250 ), p ) ) {
					m_buildPlatformDrawer.CheckError();
				}
				ScopeChange.End();

				ScopeDisable.End();
			}
			else {
				DefaultRowGUI( args );
			}
		}
	}
}

