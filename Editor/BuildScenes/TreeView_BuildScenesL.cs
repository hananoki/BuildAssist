using HananokiEditor.Extensions;
using HananokiRuntime;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using E = HananokiEditor.BuildAssist.SettingsEditor;
using PB = HananokiEditor.BuildAssist.SettingsProjectBuildSceneSet;

namespace HananokiEditor.BuildAssist {

	using Item = TreeView_BuildScenesL.Item;

	public sealed class TreeView_BuildScenesL : HTreeView<Item> {

		public class Item : TreeViewItem {
			public PB.Profile profile;
		}

		TreeView_BuildScenesR m_treeView_ProfileSceneList;


		/////////////////////////////////////////
		public TreeView_BuildScenesL() : base( new TreeViewState() ) {
			E.Load();

			showAlternatingRowBackgrounds = true;
			//rowHeight = EditorGUIUtility.singleLineHeight;

			RegisterFiles();
		}


		/////////////////////////////////////////
		public void RegisterFiles() {

			var lst = new List<Item>();
			InitID();


			lst.Add( new Item {
				id = GetID(),
				displayName = S._BuildSceneSet,
				icon = EditorIcon.icons_processed_sceneset_icon_asset,
				//depth = 1,
			} );

			PB.Load();

			foreach( var p in PB.i.profileList ) {
				lst.Add( new Item {
					id = GetID(),
					displayName = p.profileName,
					profile = p,
				} );
			}


			m_registerItems = lst;
			ReloadAndSorting();
		}


		/////////////////////////////////////////
		public void ReloadAndSorting() {
			Reload();
			var item = RollbackLastSelect();

			if( item != null ) {
				Helper.New( ref m_treeView_ProfileSceneList );
				m_treeView_ProfileSceneList?.RegisterFiles( item.profile );
			}
		}


		/////////////////////////////////////////
		protected override void OnSingleClickedItem( Item item ) {
			BackupLastSelect( item );

			Helper.New( ref m_treeView_ProfileSceneList );
			m_treeView_ProfileSceneList.RegisterFiles( item.profile );
		}


		/////////////////////////////////////////
		protected override void OnRowGUI( RowGUIArgs args ) {
			var item = (Item) args.item;

			if( item.id == 1 && !args.selected ) {
				HEditorStyles.sceneTopBarBg.Draw( args.rowRect );
			}

			DefaultRowGUI( args );
		}


		/////////////////////////////////////////
		public void DrawItem() {
			using( new GUILayoutScope( 1, 0 ) ) {
				Helper.New( ref m_treeView_ProfileSceneList );
				m_treeView_ProfileSceneList.DrawLayoutGUI();
			}
		}


		/////////////////////////////////////////
		public string SceneName( string path ) {
			var s = path.Remove( 0, 7 );
			return $"{s.DirectoryName()}/{s.FileNameWithoutExtension()}".TrimStart( '/' );
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

