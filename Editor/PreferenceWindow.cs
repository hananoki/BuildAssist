//#define  HANANOKI_PREFERENCES

using Hananoki.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

using P = Hananoki.BuildAssist.SettingsProject;
using PB = Hananoki.BuildAssist.SettingsProjectBuildSceneSet;

namespace Hananoki.BuildAssist {

	public class PreferenceWindow : HSettingsEditorWindow {

		static PreferenceWindow s_window;

		static Vector2 m_scroll2;
		static Vector2 m_scroll3;
		static bool s_changed;

		static GUIContent[] s_exclusionContents;

		public static void Open( EditorWindow parent ) {
			s_window = GetWindow<PreferenceWindow>( typeof( BuildAssistWindow ) );
			s_window.SetTitle( new GUIContent( "Project Settings", Styles.iconSettings ) );
			//s_window.SetPositionCenter( parent );
			s_window.sectionName = Package.name;
		}


		void OnEnable() {
			m_settingsProvider = PreferenceView();
		}


		static void DrawContentPlatfom() {
			EditorGUI.BeginChangeCheck();

			var targetGroupList = PlatformUtils.GetSupportList();

			GUILayout.BeginVertical( EditorStyles.helpBox );
			GUILayout.Label( S._Selectplatformtouse );
			foreach( var t in targetGroupList ) {
				EditorGUI.BeginChangeCheck();
				var _b = HEditorGUILayout.ToggleLeft( P.GetPlatform( t ).enable, t.Icon(), t.GetName() );
				if( EditorGUI.EndChangeCheck() ) {
					P.GetPlatform( t ).enable = _b;
					BuildAssistWindow.ChangeActiveTarget();
				}
			}
			GUILayout.EndVertical();

			if( EditorGUI.EndChangeCheck() ) {
				s_changed = true;
			}
		}


		static void DrawContentConfig() {
			EditorGUI.BeginChangeCheck();

			PB.i.enableAssetBundleBuild = HEditorGUILayout.ToggleLeft( S._EnableAssetBundleBuild, PB.i.enableAssetBundleBuild );
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
				s_exclusionContents = PB.i.exclusionAssets.Select( x => GUIDUtils.GetAssetPath( x.GUID ) ).OrderBy( value => value ).Select( x => new GUIContent( x, AssetDatabase.GetCachedIcon( x ) ) ).ToArray();
			}

			int removeIndex = -1;
			using( new GUILayout.VerticalScope( Styles.helpBox ) ) {
				for( int i = 0; i < s_exclusionContents.Length; i++ ) {
					var s = s_exclusionContents[ i ];
					using( new GUILayout.HorizontalScope() ) {
						GUILayout.Label( EditorHelper.TempContent( "" ), GUILayout.ExpandWidth( true ), GUILayout.Height( 18 ) );
					}

					var rc = GUILayoutUtility.GetLastRect();
					GUI.Box( rc, "", Styles.projectBrowserHeaderBgMiddle );
					GUI.Label( rc, s, Styles.labelAndIcon );
					rc.x = rc.xMax - 16;
					rc.width = 16;
					if( HEditorGUI.IconButton( rc, Styles.iconMinus ) ) {
						removeIndex = i;
					}
				}
				GUILayout.FlexibleSpace();
				if( 0 <= removeIndex ) {
					var findGUID = GUIDUtils.ToGUID( s_exclusionContents[ removeIndex ].text );
					var rIndex = PB.i.exclusionAssets.FindIndex( x => x.GUID == findGUID );
					PB.i.exclusionAssets.RemoveAt( rIndex );
					s_exclusionContents = null;
					s_changed = true;
					s_window?.Repaint();
				}
			}

			var dropRc = GUILayoutUtility.GetLastRect();
			var evt = Event.current;
			switch( evt.type ) {
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if( !dropRc.Contains( evt.mousePosition ) ) break;

					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

					void AddFiles( params string[] paths ) {
						PB.i.exclusionAssets.AddRange( paths.Select( x => new PB.ExclusionSets( GUIDUtils.ToGUID( x ), x ) ).ToArray() );
						PB.i.exclusionAssets = PB.i.exclusionAssets.Distinct( x => x.GUID ).ToList();
						PB.Save();
					}
					string[] DirFiles( string path ) {
						return DirectoryUtils.GetFiles( path.ToCast<string>(), "*", SearchOption.AllDirectories ).Where( x => x.Extension() != ".meta" ).ToArray();
					}

					if( evt.type == EventType.DragPerform ) {
						DragAndDrop.AcceptDrag();
						if( DragAndDrop.paths.Length == 1 ) {
							if( Directory.Exists( DragAndDrop.paths[ 0 ] ) ) {
								var m = new GenericMenu();
								m.AddItem( S._Registerasafolder, false, ( context ) => {
									AddFiles( context.ToCast<string>() );
								}, DragAndDrop.paths[ 0 ] );
								m.AddItem( S._Registeringfilescontainedinafolder, false, ( context ) => {
									AddFiles( DirFiles( context.ToCast<string>() ) );
									;
								}, DragAndDrop.paths[ 0 ] );
								m.DropDown();
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
									AddFiles( context.ToCast<string[]>() );
								}, DragAndDrop.paths );
								m.AddItem( S._Registeringfilescontainedinafolder, false, ( context ) => {
									foreach( var s in context.ToCast<string[]>() ) {
										if( Directory.Exists( s ) ) {
											AddFiles( DirFiles( s ) );
										}
										else {
											AddFiles( s );
										}
									}
								}, DragAndDrop.paths );
								m.DropDown();
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

			if( EditorGUI.EndChangeCheck() ) {
				s_changed = true;
			}
		}




		static string[] toolName = { "Platform", "Config" };



		/// <summary>
		/// 
		/// </summary>
		static void DrawGUI() {
			P.Load();
			PB.Load();
			Styles.Init();

			using( new PreferenceLayoutScope( ref m_scroll2 ) ) {

				PB.i.selectTool = GUILayout.Toolbar( PB.i.selectTool, toolName );
				GUILayout.Space( 8 );

				if( PB.i.selectTool == 0 ) DrawContentPlatfom();
				else DrawContentConfig();
			}

			if( s_changed ) {
				P.Save();
				PB.Save();
				BuildAssistWindow.Repaint();

			}
		}


#if UNITY_2018_3_OR_NEWER && !ENABLE_LEGACY_PREFERENCE

		[SettingsProvider]
		public static SettingsProvider PreferenceView() {
			var provider = new SettingsProvider( $"Hananoki/{Package.name}", SettingsScope.Project ) {
				label = $"{Package.name}",
				guiHandler = PreferencesGUI,
				titleBarGuiHandler = () => GUILayout.Label( $"{Package.version}", EditorStyles.miniLabel ),
			};
			return provider;
		}
		public static void PreferencesGUI( string searchText ) {
#else
		[PreferenceItem( Package.name )]
		public static void PreferencesGUI() {
#endif
			DrawGUI();
		}
	}
}
