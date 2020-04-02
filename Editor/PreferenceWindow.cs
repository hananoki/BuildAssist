//#define  HANANOKI_PREFERENCES
using UnityEditor;
using UnityEngine;

using P = Hananoki.BuildAssist.SettingsProject;
using E = Hananoki.BuildAssist.SettingsEditor;

namespace Hananoki.BuildAssist {

	public class PreferenceWindow : HSettingsEditorWindow {

		static Vector2 m_scroll2;

		public static void Open( EditorWindow parent ) {
			var window = GetWindow<PreferenceWindow>();
			window.SetTitle( new GUIContent( "Project Settings", Styles.iconSettings ) );
			window.SetPositionCenter( parent );
			window.sectionName = Package.name;
		}


		void OnEnable() {
			m_settingsProvider = PreferenceView();
		}


		static void DrawContent() {
			P.i.enableAssetBundleBuild = EditorGUILayout.ToggleLeft( S._EnableAssetBundleBuild, P.i.enableAssetBundleBuild );

			GUILayout.Space( 8 );

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
		}

		/// <summary>
		/// 
		/// </summary>
		static void DrawGUI() {
			P.Load();
			Styles.Init();

			EditorGUI.BeginChangeCheck();

			using( var sc = new GUILayout.ScrollViewScope( m_scroll2 ) ) {
				m_scroll2 = sc.scrollPosition;

				DrawContent();
			}

			if( EditorGUI.EndChangeCheck() ) {
				E.Save();
				P.Save();
				BuildAssistWindow.Repaint();
			}

			GUILayout.Space( 8f );

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
