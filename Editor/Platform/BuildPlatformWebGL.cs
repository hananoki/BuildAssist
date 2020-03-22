
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

using SS = Hananoki.SharedModule.S;
using static Hananoki.BuildAssist.Console;

namespace Hananoki.BuildAssist {

	public class BuildPlatformWebGL : IBuildPlatform {
		public BuildReport BuildPackage( string[] scenes ) {
			var p = SettingsProject.GetActiveTargetParams();
			var path = $"{p.outputDirectory}";

			try {
				B.development = p.development;

				B.compressionType = p.compression;
				B.il2CppCompilerConfiguration = p.il2CppCompilerConfiguration;
				B.WebGL_compressionFormat = p.WebGL_compressionFormat;

				Log( $"path: {path}" );
				Log( $"buildTarget: {p.buildTarget.ToString()}" );
				Log( $"options: {p.options.ToString()}" );
				return BuildPipeline.BuildPlayer( scenes, path, BuildTarget.WebGL, p.options );
			}
			catch( System.Exception e ) {
				Debug.LogException( e );
			}

			return null;
		}


		public void Draw( BuildAssistWindow parent ) {

			parent.DrawGUI_PackageName();
			parent.DrawGUI_ConfigurationSelect();

			parent.DrawGUI_AssetBundle();
			parent.DrawGUI_BuildSettings();
			parent.DrawGUI_PlayerSettings();
			parent.DrawGUI_OutputDirectory();

			var currentParams = SettingsProject.GetCurrentParams();

			bool once = false;
			void errorLabel( string s, string icon = "" ) {
				var c = EditorStyles.label.normal.textColor;
				EditorStyles.label.normal.textColor = Color.red;
				EditorStyles.label.fontStyle = FontStyle.Bold;

				GUILayout.Label( EditorHelper.TempContent( s, Icon.Get( icon ) ), EditorStyles.label );

				EditorStyles.label.fontStyle = FontStyle.Normal;
				EditorStyles.label.normal.textColor = c;
			}
			void errorTitle() {
				if( once ) return;
				errorLabel( "PlayerSettings.Standalone settings are incomplete", "console.erroricon.sml" );
				once = true;
			}



			if( currentParams.development ) {
				HEditorGUILayout.BoldLabel( SS._Info, EditorIcon.Info );
				HEditorGUILayout.BoldLabel( S._NotethatWebGLdevelopmentbuildsaremuchlargerthanreleasebuildsandshoundnotbepublicsed );
			}

			GUILayout.FlexibleSpace();
			parent.DrawGUI_Bottom();
		}
	}
}


