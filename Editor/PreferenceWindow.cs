//#define  HANANOKI_PREFERENCES
using UnityEditor;
using UnityEngine;

using P = Hananoki.BuildAssist.SettingsProject;
using E = Hananoki.BuildAssist.SettingsEditor;

namespace Hananoki.BuildAssist {

	public class PreferenceWindow : HSettingsEditorWindow {
		public class Styles {
			public GUIStyle boldLabel;
			public Styles() {
				boldLabel = new GUIStyle( EditorStyles.boldLabel );
			}
		}

		public static Styles s_styles;
		static Vector2 m_scroll2;

		public static void Open( EditorWindow parent ) {
			var window = GetWindow<PreferenceWindow>();
			window.SetTitle( new GUIContent( "Preference", Icon.Get( "SettingsIcon" ) ) );
			window.SetPositionCenter( parent );
			window.sectionName = Package.name;
		}

		void OnEnable() {
			drawGUI = DrawGUI;
		}



		/// <summary>
		/// 
		/// </summary>
		static void DrawGUI() {
			P.Load();
			if( s_styles == null ) s_styles = new Styles();

			EditorGUI.BeginChangeCheck();

			GUILayout.Label( S._Selectplatformtouse );

			using( var sc = new GUILayout.ScrollViewScope( m_scroll2 ) ) {
				m_scroll2 = sc.scrollPosition;

				var targetGroupList = PlatformUtils.GetSupportList();

				foreach( var t in targetGroupList ) {
					EditorGUI.BeginChangeCheck();
					var _b = HEditorGUILayout.ToggleLeft( P.GetPlatform( t ).enable, t.Icon(), t.GetName() );
					if( EditorGUI.EndChangeCheck() ) {
						P.GetPlatform( t ).enable = _b;
						BuildAssistWindow.ChangeActiveTarget();
					}
				}
			}

			if( EditorGUI.EndChangeCheck() ) {
				E.Save();
				P.Save();
			}

			GUILayout.Space( 8f );

		}


#if UNITY_2018_3_OR_NEWER && !ENABLE_LEGACY_PREFERENCE

		[SettingsProvider]
		public static SettingsProvider PreferenceView() {
			var provider = new SettingsProvider( $"Preferences/Hananoki/{Package.name}", SettingsScope.User ) {
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
