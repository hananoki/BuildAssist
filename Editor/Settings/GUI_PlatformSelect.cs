using HananokiEditor.Extensions;
using HananokiEditor.SharedModule;
using UnityEngine;
using P = HananokiEditor.BuildAssist.SettingsProject;

namespace HananokiEditor.BuildAssist {
	public class GUI_PlatformSelect {
		[HananokiSettingsRegister]
		public static SettingsItem RegisterSetting() {
			return new SettingsItem() {
				mode = 1,
				displayName = Package.nameNicify + "/Platform",
				version = "",
				gui = DrawGUI,
			};
		}


		/////////////////////////////////////////

		public static void DrawGUI() {
			//E.Load();
			P.Load();
			var targetGroupList = PlatformUtils.GetSupportList();

			ScopeIsCompile.Begin();

			ScopeVertical.Begin();
			HEditorGUILayout.HeaderTitle( "Platform" );
			GUILayout.Space( 8 );
			foreach( var t in targetGroupList ) {
				ScopeChange.Begin();

				var _b = HEditorGUILayout.ToggleBox( P.GetPlatform( t ).enable, t.Icon(), t.GetName() );
				if( ScopeChange.End() ) {
					P.GetPlatform( t ).enable = _b;
					P.Save();
					BuildAssistWindow.ChangeActiveTarget();
				}
			}
			ScopeVertical.End();

			ScopeIsCompile.End();
		}
	}
}