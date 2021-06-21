using HananokiEditor.Extensions;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using E = HananokiEditor.BuildAssist.SettingsEditor;
using P = HananokiEditor.BuildAssist.SettingsProject;
using PB = HananokiEditor.BuildAssist.SettingsProjectBuildSceneSet;

namespace HananokiEditor.BuildAssist {

	using Item = TreeView_BuildPropertyL.Item;

	public sealed class TreeView_BuildPropertyL : HTreeView<Item> {

		public class Item : TreeViewItem {
			public PB.Profile profile;
			public int index;
		}


		TreeView_BuildPropertyR m_TreeView_BuildPropertyR;

		P.Platform m_platform;


		/////////////////////////////////////////
		public TreeView_BuildPropertyL() : base( new TreeViewState() ) {
			E.Load();

			showAlternatingRowBackgrounds = true;
			//rowHeight = EditorGUIUtility.singleLineHeight;

		}


		/////////////////////////////////////////
		public void RegisterFiles() {
			RegisterFiles( m_platform, m_TreeView_BuildPropertyR );
		}


		/////////////////////////////////////////
		public void RegisterFiles( P.Platform platform, TreeView_BuildPropertyR treeView_BuildPropertyR ) {
			m_platform = platform;
			m_TreeView_BuildPropertyR = treeView_BuildPropertyR;

			var lst = new List<Item>();
			InitID();

			int index = 0;

			lst.Add( new Item {
				id = GetID(),
				displayName = S._Configuration,
				icon = EditorIcon.icons_processed_sceneset_icon_asset,

			} );

			foreach( var p in m_platform.parameters ) {
				lst.Add( new Item {
					id = GetID(),
					displayName = p.name,
					index = index++,
				} );
			}


			m_registerItems = lst;
			ReloadAndSorting();
		}


		/////////////////////////////////////////
		public void ReloadAndSorting() {
			Reload();
			var item = RollbackLastSelect();

			//if( item != null ) {
			//	Helper.New( ref m_treeView_ProfileSceneList );
			//	m_treeView_ProfileSceneList?.RegisterFiles( item.profile );
			//}
		}


		/////////////////////////////////////////
		protected override void OnSingleClickedItem( Item item ) {
			BackupLastSelect( item );

			//Helper.New( ref m_treeView_ProfileSceneList );
			//m_treeView_ProfileSceneList.RegisterFiles( item.profile );

			P.i.selectParamsIndex = item.index;
			m_TreeView_BuildPropertyR.RegisterFiles();
			m_TreeView_BuildPropertyR.m_buildPlatformDrawer.CheckError();
		}


		/////////////////////////////////////////
		protected override void OnRowGUI( RowGUIArgs args ) {
			var item = (Item) args.item;

			if( item.id == 1 && !args.selected ) {
				HEditorStyles.sceneTopBarBg.Draw( args.rowRect );
			}

			DefaultRowGUI( args );
		}


		//////////////////////////////////////////////////////////////////////////////////
		#region Rename

		/////////////////////////////////////////
		protected override bool CanRename( TreeViewItem item ) {
			if( item == null ) return false;

			var _it = (Item) item;

			return 1 < _it.id;
		}


		/////////////////////////////////////////
		protected override void RenameEnded( RenameEndedArgs args ) {
			base.RenameEnded( args );

			if( args.newName.Length <= 0 ) goto failed;
			if( args.newName == args.originalName ) goto failed;
			args.acceptedRename = true;

			var item = ToItem( args.itemID );

			item.displayName = args.newName;
			item.profile.profileName = args.newName;
			PB.Save();
			return;

			failed:
			args.acceptedRename = false;
		}

		#endregion
	}
}

