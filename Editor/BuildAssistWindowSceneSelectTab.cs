using Hananoki.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using static UnityEditor.EditorGUI;
using static UnityEngine.GUILayout;
using PB = Hananoki.BuildAssist.SettingsProjectBuildSceneSet;
using SS = Hananoki.SharedModule.S;

namespace Hananoki.BuildAssist {
	public partial class BuildAssistWindow : HEditorWindow {

		public List<EditorBuildSettingsScene> m_scenePaths;
		public List<string> m_leakedScenes;

		public string SceneName( string path ) {
			var s = path.Remove( 0, 7 );
			return $"{s.DirectoryName()}/{s.FileNameWithoutExtension()}".TrimStart( '/' );
		}

		public void SceneSelectTabOnGUI() {
			PB.Load();
			using( new VerticalScope( Styles.helpBox ) ) {
				using( new HorizontalScope() ) {
					void editButton() {
						var lsss = PrefixLabel( GUILayoutUtility.GetLastRect(), EditorHelper.TempContent( S._BuildSceneSet ) );

						lsss.x -= Styles.iconEdit.width;
						lsss.x -= 2;
						lsss.width = Styles.iconEdit.width;

						if( HEditorGUI.IconButton( lsss, Styles.iconEdit, 2 ) ) {
							editMode = !editMode;
							PB.i.profileList = PB.i.profileList.Distinct( a => a.profileName ).ToList();

							Repaint();
						}
					}

					if( PB.i.profileList.Count <= PB.i.selectIndex ) PB.i.selectIndex = PB.i.profileList.Count - 1;

					var lst = PB.i.profileList.Select( x => x.profileName ).ToArray();

					BeginChangeCheck();

					if( editMode ) {
						PB.i.selectProfile.profileName = EditorGUILayout.TextField( S._BuildSceneSet, PB.i.selectProfile.profileName );
						editButton();
					}
					else {

						PB.i.selectIndex = EditorGUILayout.Popup( S._BuildSceneSet, PB.i.selectIndex, lst );

						//var ls = GUILayoutUtility.GetLastRect();

						editButton();

						if( HEditorGUILayout.IconButton( Styles.iconPlus, 4 ) ) {
							PB.i.profileList.Add( new PB.Profile( $"BuildScene ({PB.i.profileList.Count})" ) );
							PB.i.selectIndex = PB.i.profileList.Count - 1;
							s_changed = true;
						}
						BeginDisabledGroup( PB.i.selectIndex == 0 );
						if( HEditorGUILayout.IconButton( Styles.iconMinus, 4 ) ) {
							PB.i.profileList.RemoveAt( PB.i.selectIndex );
							PB.i.selectIndex = PB.i.profileList.Count - 1;
							s_changed = true;
						}
						EndDisabledGroup();
					}

					if( EndChangeCheck() ) {
						PB.Save();
					}
				}
				BeginHorizontal();
				BeginChangeCheck();
				PB.i.selectProfile.exclusionScene = EditorGUILayout.ToggleLeft( S._ExcludescenesfromthebuildthatarenotregisteredinBuildSettings, PB.i.selectProfile.exclusionScene );
				if( Button( S._Checktheorderofthebuild, ExpandWidth( false ) ) ) {
					var s = string.Join( "\n", PB.GetBuildSceneName().Select( ( x, i ) => $"{i}: {x}" ).ToArray() );
					EditorUtility.DisplayDialog( S._Checktheorderofthebuild, s, SS._OK );
				}
				if( EndChangeCheck() ) {
					PB.Save();
				}

				EndHorizontal();
			}


			// Build SettingsにセットされているScene
			if( 0 < m_scenePaths.Count ) {
				Space( 8 );
				BeginHorizontal();
				Label( S._BuildSettingsScenes, Styles.boldLabel );
				var r = EditorHelper.GetLayout( Styles.iconSettings, HEditorStyles.iconButton );
				if( HEditorGUI.IconButton( r, Styles.iconSettings, B.kBuildSettings, 1 ) ) {
					UnityEditorMenu.File_Build_Settings();
				}
				EndHorizontal();

				foreach( var p in m_scenePaths ) {
					BeginHorizontal( Styles.helpBox );
					BeginDisabledGroup( true );
					Toggle( p.enabled, "" );
					EndDisabledGroup();
					Label( EditorHelper.TempContent( SceneName( p.path ), EditorIcon.sceneAsset ), Height( 16 ) );
					if( EditorHelper.HasMouseClick( GUILayoutUtility.GetLastRect() ) ) {
						EditorHelper.PingObject( p.path );
						Event.current.Use();
					}
					FlexibleSpace();

					foreach( var pp in PB.i.profileList ) {
						BeginChangeCheck();
						var toggle = Toggle( pp.Has( p.path ), pp.profileName, "Button" );
						if( EndChangeCheck() ) {
							pp.Toggle( toggle, p.path );
							PB.Save();
						}
					}
					EndHorizontal();
				}
			}

			// Build SettingsにセットされていないScene
			Space( 8 );
			Label( S._ScenesotherthanBuildSettings, Styles.boldLabel );
			foreach( var scenePath in m_leakedScenes ) {
				BeginHorizontal( Styles.helpBox );
				Space( 20 );
				Label( EditorHelper.TempContent( scenePath, EditorIcon.sceneAsset ), Height( 16 ) );
				if( EditorHelper.HasMouseClick( GUILayoutUtility.GetLastRect() ) ) {
					EditorHelper.PingObject( scenePath );
					Event.current.Use();
				}
				FlexibleSpace();
				foreach( var pp in PB.i.profileList ) {
					BeginChangeCheck();
					var toggle = Toggle( pp.Has( scenePath ), pp.profileName, "Button" );
					if( EndChangeCheck() ) {
						pp.Toggle( toggle, scenePath );
						PB.Save();
					}
				}
				EndHorizontal();
			}
		}
	}
}