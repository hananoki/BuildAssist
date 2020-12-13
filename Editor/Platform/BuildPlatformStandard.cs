
using HananokiEditor.Extensions;
using HananokiRuntime.Extensions;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityReflection;

using static HananokiEditor.BuildAssist.Console;
using P = HananokiEditor.BuildAssist.SettingsProject;

namespace HananokiEditor.BuildAssist {

	public class BuildPlatformStandard : IBuildPlatform {
		public BuildReport BuildPackage( string[] scenes ) {
			var p = P.GetActiveTargetParams();
			var path = $"{p.outputDirectory}/{P.GetOutputPackageName( p )}";

			//var scenes = BuildManagerCommand.GetBuildSceneName();
			try {
				B.development = p.development;
				//B.buildAppBundle = p.buildAppBundle;

				B.compressionType = p.compression;
				B.il2CppCompilerConfiguration = p.il2CppCompilerConfiguration;
				B.scriptingBackend = p.scriptingBackend;

				Log( $"path: {path}" );
				Log( $"buildTarget: {p.buildTarget.ToString()}" );
				return BuildPipeline.BuildPlayer( scenes, path, p.buildTarget, p.options );
			}
			catch( System.Exception e ) {
				Debug.LogException( e );
			}

			return null;
		}



		static (GUIContent, BuildTarget)[] GetArchitecturesForPlatform( BuildTarget target ) {
			System.ValueTuple<GUIContent, BuildTarget>[] result;
			if( target != BuildTarget.StandaloneWindows && target != BuildTarget.StandaloneWindows64 ) {
				if( target != BuildTarget.StandaloneLinux64 ) {
					result = null;
				}
				else {
					result = new System.ValueTuple<GUIContent, BuildTarget>[] {
						( UnityEditorEditorGUIUtility.TrTextContent( "x86_64" ), BuildTarget.StandaloneLinux64 )
					};
				}
			}
			else {
				result = new System.ValueTuple<GUIContent, BuildTarget>[] {
					( UnityEditorEditorGUIUtility.TrTextContent("x86"), BuildTarget.StandaloneWindows ),
					( UnityEditorEditorGUIUtility.TrTextContent("x86_64"), BuildTarget.StandaloneWindows64 )
				};
			}
			return result;
		}


		public void Draw( BuildAssistWindow parent ) {

			parent.DrawGUI_PackageName();
			parent.DrawGUI_ConfigurationSelect();

			parent.DrawGUI_AssetBundle();
			parent.DrawGUI_BuildSettings();
			parent.DrawGUI_PlayerSettings();
			parent.DrawGUI_OutputDirectory();


			var currentParams = P.GetCurrentParams();

			// アーキテクチャ

			(GUIContent, BuildTarget)[] array = GetArchitecturesForPlatform( currentParams.buildTarget );
			if( array != null ) {
				int index = ArrayUtility.FindIndex( array, x => x.Item2 == currentParams.buildTarget );
				EditorGUI.BeginChangeCheck();
				index = EditorGUILayout.Popup( S._Architecture.content(), index, array.Select( x => x.Item1 ).ToArray() );
				if( EditorGUI.EndChangeCheck() ) {
					//EditorHelper.SwitchActiveBuildTarget();
					currentParams.buildTarget = array[ index ].Item2;
					parent.MakeDefaultOutputDirectory();
					BuildAssistWindow.s_changed = true;
				}
			}

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

			if( currentParams.scriptingBackend == ScriptingImplementation.IL2CPP ) {
				var ss = (string) R.Method( "GetTargetStringFrom", "UnityEditor.Modules.ModuleManager" ).Invoke( null, new object[] { UnityEditorEditorUserBuildSettings.activeBuildTargetGroup, EditorUserBuildSettings.activeBuildTarget } );
				object obj = R.Method( "GetBuildWindowExtension", "UnityEditor.Modules.ModuleManager" ).Invoke( null, new object[] { ss } );
				try {
					var sss = R.MethodInvoke<string>( obj, "GetCannotBuildIl2CppPlayerInCurrentSetupError" );
					if( !sss.IsEmpty() ) {
						errorTitle();
						errorLabel( sss );
					}
				}
				catch(System.Exception) {
					// スタンドアロン以外がビルドターゲットだとメソッドが見つからない

				}
				//GUILayout.Label( $"{ss}\nm_HasIl2CppPlayers: {sss}" );
			}
			GUILayout.FlexibleSpace();
			parent.DrawGUI_Bottom();
		}
	}
}


