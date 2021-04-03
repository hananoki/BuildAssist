using HananokiEditor.SharedModule;
using HananokiRuntime;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HananokiEditor.BuildAssist {
	public class GUI_EventViewer {
		[HananokiSettingsRegister]
		public static SettingsItem RegisterSetting() {
			return new SettingsItem() {
				mode = 1,
				displayName = Package.nameNicify + "/Event",
				version = "",
				gui = DrawGUI,
			};
		}
		static MethodInfo[] method1;
		static MethodInfo[] method2;
		static MethodInfo[] method3;

		public static void DrawGUI() {
			if( method1 == null ) {
				method1 = AssemblieUtils.GetAllMethodsWithAttribute<Hananoki_BuildStartProcess>().ToArray();
				method2 = AssemblieUtils.GetAllMethodsWithAttribute<Hananoki_BuildPreProcess>().ToArray();
				method3 = AssemblieUtils.GetAllMethodsWithAttribute<Hananoki_BuildPostProcess>().ToArray();
			}
			var _ = new (MethodInfo[], string)[] {
				(method1, "Hananoki_BuildStartProcess") ,
				(method2, "Hananoki_BuildPreProcess") ,
				(method3, "Hananoki_BuildPostProcess") ,
				};

			foreach( var data in _ ) {
				HEditorGUILayout.HeaderTitle( data.Item2 );
				//GUILayout.Label( data.Item2, EditorStyles.boldLabel );
				EditorGUI.indentLevel++;
				if( data.Item1.Length == 0 ) {
					EditorGUILayout.LabelField( EditorHelper.TempContent( $"“o˜^‚³‚ê‚½ƒCƒxƒ“ƒg‚Í‚ ‚è‚Ü‚¹‚ñ", EditorIcon.info ) );
				}
				else {
					foreach( var p in data.Item1 ) {
						EditorGUILayout.LabelField( EditorHelper.TempContent( $"{p.Name}", EditorIcon.assetIcon_CsScript ) );
						EditorGUI.indentLevel++;
						EditorGUILayout.LabelField( EditorHelper.TempContent( $"{p.Module.Name}", EditorIcon.icons_processed_assembly_icon_asset ) );
						EditorGUI.indentLevel--;
					}
				}
				EditorGUI.indentLevel--;
				GUILayout.Space( 8 );
			}

		}

	}
}
