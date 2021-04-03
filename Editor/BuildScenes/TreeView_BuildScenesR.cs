using HananokiEditor.Extensions;
using HananokiRuntime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using E = HananokiEditor.BuildAssist.SettingsEditor;
using PB = HananokiEditor.BuildAssist.SettingsProjectBuildSceneSet;

namespace HananokiEditor.BuildAssist {

	using Item = TreeView_BuildScenesR.Item;

	public sealed class TreeView_BuildScenesR : HTreeView<Item> {

		public class Item : TreeViewItem {
			public bool toggle;
			public string path;
			public bool title;
			public bool scene;
			public bool sceneReg;
		}


		PB.Profile m_profile;
		static int s_buildNo;



		/////////////////////////////////////////
		public TreeView_BuildScenesR() : base( new TreeViewState() ) {
			E.Load();

			showAlternatingRowBackgrounds = true;
		}

		/////////////////////////////////////////
		public void RegisterFiles( PB.Profile profile ) {

			var lst = new List<Item>();
			InitID();

			m_profile = profile;

			if( profile == null ) {
				foreach( var p in AssetDatabase.FindAssets( "t:Scene" ).Select( x => x.ToAssetPath() ) ) {
					var check = EditorBuildSettings.scenes.Select( x => x.path ).ToList();
					lst.Add( new Item {
						id = GetID(),
						displayName = p,
						path = p,
						icon = EditorIcon.sceneasset,
						scene = true,
						sceneReg = 0 <= check.IndexOf( p ),
					} );
				}
			}
			else {
				var scenes = new List<string>( m_profile.sceneNanes );

				// Build SettingsにセットされているScene
				lst.Add( new Item {
					id = GetID(),
					displayName = S._BuildSettingsScenes,
					icon = EditorIcon.sceneasset,
					title = true,
				} );

				foreach( var p in EditorBuildSettings.scenes ) {
					bool toggle = false;
					var index = scenes.FindIndex( x => x == p.path );
					if( 0 <= index ) {
						scenes.RemoveAt( index );
						toggle = true;
					}

					lst.Add( new Item {
						id = GetID(),
						displayName = Utils.MakeSceneName( p.path ),
						path = p.path,
						toggle = toggle,
					} );
				}


				// Build SettingsにセットされていないScene

				lst.Add( new Item {
					id = GetID(),
					displayName = S._ScenesotherthanBuildSettings,
					icon = EditorIcon.sceneasset,
					title = true,
				} );

				foreach( var p in scenes ) {
					lst.Add( new Item {
						id = GetID(),
						displayName = Utils.MakeSceneName( p ),
						path = p,
						toggle = true,
					} );
				}
			}

			m_registerItems = lst;
			ReloadAndSorting();
		}


		/////////////////////////////////////////
		public void ReloadAndSorting() {
			Reload();
			//RollbackLastSelect();
		}


		/////////////////////////////////////////
		protected override void SingleClickedItem( int id ) {
			var item = ToItem( id );
			BackupLastSelect( item );
		}


		/////////////////////////////////////////
		void OnRowGUISceneFiles( RowGUIArgs args, Item item ) {
			var rect = args.rowRect;

			if( item.sceneReg ) {
				var col1 = ColorUtils.RGB( 169, 201, 255 );
				col1.a = 0.5f;
				EditorGUI.DrawRect( args.rowRect, col1 );
			}

			rect.x += 4;
			if( HEditorGUI.IconButton( rect.W( 16 ), EditorIcon.plus ) ) {
				EditorHelper.AddSceneToBuildSetting( item.path );
				item.sceneReg = true;
			}
			rect.x += 16;
			if( HEditorGUI.IconButton( rect.W( 16 ), item.icon ) ) {
				EditorHelper.PingObject( item.path );
			}
			rect.x += 20;
			Label( args, rect.TrimR( 40 ), item.displayName );
		}


		/////////////////////////////////////////
		void OnRowGUISceneSet( RowGUIArgs args, Item item ) {
			if( item.title ) {
				if( item.id == 1 ) {
					s_buildNo = 0;
				}
				if( !args.selected ) {
					HEditorStyles.sceneTopBarBg.Draw( args.rowRect );
				}
				DefaultRowGUI( args );
				return;
			}

			//DefaultRowGUI( args );
			var rect = args.rowRect;
			rect.x += 4;

			if( HEditorGUI.IconButton( rect.W( 16 ), EditorIcon.minus ) ) {
				EditorHelper.RemoveSceneToBuildSetting( item.path );
				m_registerItems.Remove( item );
				ReloadAndSorting();
				//EditorApplication.delayCall += () => RegisterFiles( m_profile );
			}

			rect.x += 16;
			if( HEditorGUI.IconButton( rect.W( 16 ), EditorIcon.sceneasset ) ) {
				EditorHelper.PingObject( item.path );
			}

			rect.x += 16;
			ScopeChange.Begin();
			item.toggle = EditorGUI.Toggle( rect.W( 16 ), item.toggle );
			if( ScopeChange.End() ) {
				m_profile.Toggle( item.toggle, item.path );
				PB.Save();
			}

			rect.x += 20;

			Label( args, rect, item.displayName );

			if( item.toggle ) {
				var ss = $"{s_buildNo}";
				var sz = ss.CalcSize( HEditorStyles.treeViewLine );
				rect = args.rowRect.AlignR( sz.x );
				rect.x -= 4;
				Label( args, rect, ss );
				s_buildNo++;
			}
		}


		/////////////////////////////////////////
		protected override void OnRowGUI( RowGUIArgs args ) {
			var item = (Item) args.item;

			if( item.scene ) {
				OnRowGUISceneFiles( args, item );
			}
			else {
				OnRowGUISceneSet( args, item );
			}
		}
	}
}

