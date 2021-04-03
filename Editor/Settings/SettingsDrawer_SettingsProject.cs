using HananokiEditor.Extensions;
using HananokiEditor.SharedModule;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using P = HananokiEditor.BuildAssist.SettingsProject;
using PB = HananokiEditor.BuildAssist.SettingsProjectBuildSceneSet;

namespace HananokiEditor.BuildAssist {

	public class SettingsDrawer_SettingsProject {

		public class SettingsProjectEvent {
			[HananokiSettingsRegister]
			public static SettingsItem RegisterSettings() {
				return new SettingsItem() {
					mode = 1,
					displayName = Package.nameNicify,
					version = Package.version,
					gui = DrawGUI,
				};
			}
		}


		static bool s_changed;

		static GUIContent[] s_exclusionContents;



		static void DrawContentConfig() {
			ScopeChange.Begin();

			//PB.i.enableAssetBundleBuild = HEditorGUILayout.ToggleLeft( S._EnableAssetBundleBuild, PB.i.enableAssetBundleBuild );
			//HEditorGUI.DrawDebugRectAtLastRect();
			PB.i.enableOldStyleProjectSettings = HEditorGUILayout.ToggleLeft( S._Usingtheold_styleProjectSettings, PB.i.enableOldStyleProjectSettings );
			GUILayout.Space( 8 );
			PB.i.enableExlusionAssets = HEditorGUILayout.ToggleLeft( S._Enablingassetexclusionatbuildtime, PB.i.enableExlusionAssets );

			GUILayout.Label( S._ExclusionAssetsList, EditorStyles.boldLabel );

			if( s_exclusionContents == null ) {
				if( PB.i.exclusionAssets == null ) {
					PB.i.exclusionAssets = new List<PB.ExclusionSets>();
				}

				//foreach(var p in PB.i.exclusionAssets ) {
				//	Debug.Log( GUIDUtils.GetAssetPath( p.GUID ) );
				//}
				s_exclusionContents = PB.i.exclusionAssets.Select( x => x.GUID.ToAssetPath() ).OrderBy( value => value ).Select( x => new GUIContent( x, AssetDatabase.GetCachedIcon( x ) ) ).ToArray();
			}

			int removeIndex = -1;
			ScopeVertical.Begin( EditorStyles.helpBox );
			for( int i = 0; i < s_exclusionContents.Length; i++ ) {
				var s = s_exclusionContents[ i ];
				ScopeHorizontal.Begin();
				GUILayout.Label( EditorHelper.TempContent( "" ), GUILayout.ExpandWidth( true ), GUILayout.Height( 18 ) );
				ScopeHorizontal.End();

				var rc = GUILayoutUtility.GetLastRect();
				GUI.Box( rc, "", Styles.projectBrowserHeaderBgMiddle );
				GUI.Label( rc, s, Styles.labelAndIcon );
				rc.x = rc.xMax - 16;
				rc.width = 16;
				if( HEditorGUI.IconButton( rc, EditorIcon.minus ) ) {
					removeIndex = i;
				}
			}
			GUILayout.FlexibleSpace();
			if( 0 <= removeIndex ) {
				var findGUID = s_exclusionContents[ removeIndex ].text.ToGUID();
				var rIndex = PB.i.exclusionAssets.FindIndex( x => x.GUID == findGUID );
				PB.i.exclusionAssets.RemoveAt( rIndex );
				s_exclusionContents = null;
				s_changed = true;
			}
			ScopeVertical.End();

			var dropRc = GUILayoutUtility.GetLastRect();
			var evt = Event.current;
			switch( evt.type ) {
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if( !dropRc.Contains( evt.mousePosition ) ) break;

				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				void AddFiles( params string[] paths ) {
					PB.i.exclusionAssets.AddRange( paths.Select( x => new PB.ExclusionSets( x.ToGUID(), x ) ).ToArray() );
					PB.i.exclusionAssets = PB.i.exclusionAssets.Distinct( x => x.GUID ).ToList();
					PB.Save();
				}
				string[] DirFiles( string path ) {
					return DirectoryUtils.GetFiles( path, "*", SearchOption.AllDirectories ).Where( x => x.Extension() != ".meta" ).ToArray();
				}

				if( evt.type == EventType.DragPerform ) {
					DragAndDrop.AcceptDrag();
					if( DragAndDrop.paths.Length == 1 ) {
						if( Directory.Exists( DragAndDrop.paths[ 0 ] ) ) {
							var m = new GenericMenu();
							m.AddItem( S._Registerasafolder, false, ( context ) => {
								AddFiles( (string) context );
							}, DragAndDrop.paths[ 0 ] );
							m.AddItem( S._Registeringfilescontainedinafolder, false, ( context ) => {
								AddFiles( DirFiles( (string) context ) );
								;
							}, DragAndDrop.paths[ 0 ] );
							m.DropDownAtMousePosition();
						}
						else {
							AddFiles( DragAndDrop.paths );
						}
					}
					else {
						bool dirChekc = false;
						foreach( var s in DragAndDrop.paths ) {
							if( Directory.Exists( s ) ) {
								dirChekc = true;
								break;
							}
						}
						if( dirChekc ) {
							var m = new GenericMenu();
							m.AddItem( S._Registerasafolder, false, ( context ) => {
								AddFiles( (string[]) context );
							}, DragAndDrop.paths );
							m.AddItem( S._Registeringfilescontainedinafolder, false, ( context ) => {
								foreach( var s in ( (string[]) context ) ) {
									if( Directory.Exists( s ) ) {
										AddFiles( DirFiles( s ) );
									}
									else {
										AddFiles( s );
									}
								}
							}, DragAndDrop.paths );
							m.DropDownAtMousePosition();
						}
						else {
							AddFiles( DragAndDrop.paths );
						}
					}

					DragAndDrop.activeControlID = 0;

					s_exclusionContents = null;
				}
				s_changed = true;
				Event.current.Use();
				break;
			}

			if( ScopeChange.End() ) {
				s_changed = true;
			}
		}


		public static void DrawGUI() {
			P.Load();
			PB.Load();
			Styles.Init();

			DrawContentConfig();

			if( s_changed ) {
				P.Save();
				PB.Save();
				BuildAssistWindow.Repaint();
			}
		}
	}
}
