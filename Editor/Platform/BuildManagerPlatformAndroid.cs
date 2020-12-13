
using HananokiRuntime.Extensions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static HananokiEditor.BuildAssist.Console;
using P = HananokiEditor.BuildAssist.SettingsProject;

namespace HananokiEditor.BuildAssist {

	public class BuildPlatformAndroid : IBuildPlatform {

		public static bool s_changed {
			get {
				return BuildAssistWindow.s_changed;
			}
			set {
				BuildAssistWindow.s_changed = value;
			}
		}

		readonly string[] s_scriptingBackend = { @"Mono", @"IL2CPP" };


		public BuildReport BuildPackage( string[] scenes ) {
			var p = P.GetActiveTargetParams();
			var path = $"{p.outputDirectory}/{P.GetOutputPackageName( p )}";

			//var scenes = BuildManagerCommand.GetBuildSceneName();
			try {
				B.development = p.development;
				B.buildAppBundle = p.buildAppBundle;

				B.compressionType = p.compression;
				B.il2CppCompilerConfiguration = p.il2CppCompilerConfiguration;
				B.scriptingBackend = p.scriptingBackend;

				if( p.scriptingBackend == ScriptingImplementation.Mono2x ) {
					B.targetArchitectures = AndroidArchitecture.ARMv7;
				}
				else {
					B.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
				}

				// どちらのMinifyが利用されるかの調査、下記コードで"Release"がセットされると、後の工程で"Release"の文字列判定でReleaseが選択される模様
				//
				// string value2 = (!Unsupported.IsSourceBuild()) ? ((!development) ? "Release" : "Development") : EditorUserBuildSettings.androidBuildType.ToString();
				//
				// Unsupported.IsSourceBuild()はエディタ上ではFalseが返ってくる
				// 過去バージョンとの兼ね合いかわからないがTrueの場合はEditorUserBuildSettings.androidBuildTypeが利用されることになる
				// 2018.4.12f1時点ではEditorUserBuildSettings.androidBuildTypeは封印されているため、エディタ上からの入力は不可能
				// なのでUnsupported.IsSourceBuild()がTrueでもいいようにEditorUserBuildSettings.androidBuildTypeを書き換える
				B.androidBuildType = p.development ? AndroidBuildType.Development : AndroidBuildType.Release;
				//Log( $"path: {path}" );
				Log( $"buildTarget: {p.buildTarget.ToString()}" );
				return BuildPipeline.BuildPlayer( scenes, path, BuildTarget.Android, p.options );
			}
			catch( System.Exception e ) {
				Debug.LogException( e );
			}

			return null;
		}




		void DrawGUI_3rdpartySettings() {
			using( new GUILayout.HorizontalScope() ) {
				GUILayout.FlexibleSpace();
				if( EditorHelper.HasMenuItem( "Stan's Assets/Android/Services" ) ) {
					if( GUILayout.Button( "Android Native" ) ) {
						EditorApplication.ExecuteMenuItem( "Stan's Assets/Android/Services" );
					}
				}
				if( EditorHelper.HasMenuItem( "Window/Google Play Games/Setup/Android setup..." ) ) {
					if( GUILayout.Button( "GooglePlayGamePlugin" ) ) {
						EditorApplication.ExecuteMenuItem( "Window/Google Play Games/Setup/Android setup..." );
					}
				}
			}
		}



		void DrawGUI_Android( P.Params currentParams ) {
			int opt = currentParams.platformOption;
			//bool fold;


			//using( new GUILayout.VerticalScope( s_styles.HelpBox ) ) {
			//	using( new GUILayout.HorizontalScope() ) {
			//		EditorGUI.BeginChangeCheck();
			//		fold = EditorGUILayout.Foldout( BuildManagerSettingsEditor.i.fold.Has( BuildManagerSettingsEditor.FoldPlatform ), "Player Settings", s_styles.Foldout );
			//		BuildManagerSettingsEditor.i.fold.Toggle( BuildManagerSettingsEditor.FoldPlatform, fold );
			//		if( EditorGUI.EndChangeCheck() ) s_changed = true;

			//		GUILayout.FlexibleSpace();
			//		var r = GUILayoutUtility.GetRect( 20, 18 );
			//		GUI.Label( r, s_styles.Settings, s_styles.Icon );
			//		if( EditorHelper.HasMouseClick( GUILayoutUtility.GetLastRect() ) ) {
			//			Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityObject>( AssetDatabase.GUIDToAssetPath( "00000000000000004000000000000000" ) );
			//			EditorUtils.InspectorWindow().Focus();
			//			Event.current.Use();
			//		}
			//	}

			//	EditorGUI.BeginChangeCheck();
			//	if( fold ) {
			//		EditorGUI.indentLevel++;
			//		currentParams.scriptingBackend = (ScriptingImplementation) EditorGUILayout.Popup( L.Tr( "ScriptingBackend" ), (int) currentParams.scriptingBackend, s_scriptingBackend );
			//		using( new EditorGUI.DisabledGroupScope( currentParams.scriptingBackend == ScriptingImplementation.Mono2x ) ) {
			//			currentParams.il2CppCompilerConfiguration = (Il2CppCompilerConfiguration) EditorGUILayout.EnumPopup( L.Tr( "C++ Compiler Configuration" ), currentParams.il2CppCompilerConfiguration );
			//		}
			//		EditorGUILayout.LabelField( $"{L.Tr( "Scripting Define Symbols" )} ({L.Tr( "Additional" )})" );
			//		currentParams.scriptingDefineSymbols = EditorGUILayout.TextField( currentParams.scriptingDefineSymbols );
			//		EditorGUILayout.LabelField( $"{L.Tr( "Scripting Define Symbols" )} ({L.Tr( "Current" )})" );
			//		EditorGUI.BeginDisabledGroup( true );
			//		EditorGUILayout.TextField( B.scriptingDefineSymbols );
			//		EditorGUI.EndDisabledGroup();
			//		EditorGUI.indentLevel--;
			//		GUILayout.Space( 4 );
			//	}
			//	if( EditorGUI.EndChangeCheck() ) {
			//		currentParams.platformOption = opt;
			//		s_changed = true;
			//	}
			//}

			GUILayout.Space( 4 );
			DrawGUI_3rdpartySettings();
#if false //保留
			currentParams.DEBUG_LOG = EGL.Toggle( "DEBUG_LOG", currentParams.DEBUG_LOG );
			BuildManagerPreference.i.adb_exe = EGL.TextField( "adb.exe", BuildManagerPreference.i.adb_exe );
			BuildManagerPreference.i.bundletool_jar = EGL.TextField( "bundletool", BuildManagerPreference.i.bundletool_jar );
			using( new GL.HorizontalScope() ) {
				GL.FlexibleSpace();
				GL.Button( "Install on device" );
				GL.Button( "Launch on device" );
			}
#endif
			GUILayout.Space( 4 );

			EditorGUI.BeginChangeCheck();
			using( new GUILayout.HorizontalScope() ) {
				GUILayout.Label( $"Version", EditorStyles.label );
				GUILayout.Label( $"{PlayerSettings.bundleVersion}", EditorStyles.label );
				var a = GUILayoutUtility.GetLastRect();
				a.x += a.width;

				a.width = 8;
				a.height = 8;
				//a.x -= 6;
				var rcU = a;
				rcU.y -= 0;
				var rcD = a;
				rcD.y += 0 + a.height;

				if( GUI.Button( rcU, Styles.iconAllowUp, Styles.icon ) ) {
					var f = float.Parse( PlayerSettings.bundleVersion );
					int i = (int) ( ( f * 100.0f + 0.5f ) );
					i += 100;
					f = ( (float) i ) / 100.0f;
					PlayerSettings.bundleVersion = f.ToString( "F2" );
				}
				if( GUI.Button( rcD, Styles.iconAllowDown, Styles.icon ) ) {
					var f = float.Parse( PlayerSettings.bundleVersion );
					int i = (int) ( ( f * 100.0f + 0.5f ) );
					i -= 100;
					f = ( (float) i ) / 100.0f;
					PlayerSettings.bundleVersion = f.ToString( "F2" );
				}
				rcU.x += 8;
				rcD.x += 8;
				if( GUI.Button( rcU, Styles.iconAllowUp, Styles.icon ) ) {
					var f = float.Parse( PlayerSettings.bundleVersion );
					int i = (int) ( ( f * 100.0f + 0.5f ) );
					i++;
					f = ( (float) i ) / 100.0f;
					PlayerSettings.bundleVersion = f.ToString( "F2" );
				}
				if( GUI.Button( rcD, Styles.iconAllowDown, Styles.icon ) ) {
					var f = float.Parse( PlayerSettings.bundleVersion );
					int i = (int) ( ( f * 100.0f + 0.5f ) );
					i--;
					f = ( (float) i ) / 100.0f;
					PlayerSettings.bundleVersion = f.ToString( "F2" );
				}

				GUILayout.Space( 20 );

				GUILayout.Label( $"Bundle Version Code", EditorStyles.label );
				GUILayout.Label( $"{PlayerSettings.Android.bundleVersionCode}", EditorStyles.label );
				a = GUILayoutUtility.GetLastRect();
				a.x += a.width;
				a.width = 8;
				a.height = 8;

				rcU = a;
				rcU.y -= 0;
				rcD = a;
				rcD.y += 0 + a.height;

				if( GUI.Button( rcU, Styles.iconAllowUp, Styles.icon ) ) {
					PlayerSettings.Android.bundleVersionCode++;
				}
				if( GUI.Button( rcD, Styles.iconAllowDown, Styles.icon ) ) {
					PlayerSettings.Android.bundleVersionCode--;
				}

				GUILayout.FlexibleSpace();
			}

			if( EditorGUI.EndChangeCheck() ) {
				s_changed = true;
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
				errorLabel( S._PlayerSettings_Androidsettingsareincomplete, "console.erroricon.sml" );
				once = true;
			}

			if( PlayerSettings.Android.bundleVersionCode == 0 ) {
				errorTitle();
				errorLabel( S._IfBundleVersionCodeis0_abuilderroroccurs );
			}

			if( B.applicationIdentifier.IsEmpty() ) {
				errorTitle();
				errorLabel( S._PackageNameofIdentificationisempty );
			}
			else if( B.applicationIdentifier == "com.Company.ProductName" ) {
				errorTitle();
				errorLabel( S._AnerroroccursifPackageNameis_com_Company_ProductName_ );
			}
			else if( !B.applicationIdentifier.Contains( "." ) ) {
				errorTitle();
				errorLabel( S._PackageNamemustbeseparatedbyatleastone__Dot_ );
			}
			else if( B.applicationIdentifier[ 0 ] == '.' ) {
				errorTitle();
				errorLabel( S._AnerroroccursifPackageNamestartswitha__Dot_ );
			}
			else if( B.applicationIdentifier[ B.applicationIdentifier.Length - 1 ] == '.' ) {
				errorTitle();
				errorLabel( S._AnerroroccursiftheendofPackageNameis__Dot_ );
			}
		}



		public void Draw( BuildAssistWindow parent ) {
			var currentParams = P.GetCurrentParams();
			if( currentParams == null ) return;

			parent.DrawGUI_PackageName();
			parent.DrawGUI_ConfigurationSelect();
			parent.DrawGUI_AssetBundle();
			parent.DrawGUI_BuildSettings();
			parent.DrawGUI_PlayerSettings();
			parent.DrawGUI_OutputDirectory();

			// PlayerS
			DrawGUI_Android( currentParams );

			GUILayout.FlexibleSpace();
			parent.DrawGUI_Bottom();
		}

	}
}
