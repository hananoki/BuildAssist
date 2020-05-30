
using Hananoki.Extensions;
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

using static Hananoki.BuildAssist.Console;
using E = Hananoki.BuildAssist.SettingsEditor;
using P = Hananoki.BuildAssist.SettingsProject;
using SS = Hananoki.SharedModule.S;

namespace Hananoki.BuildAssist {

	public class BuildPlatformWebGL : IBuildPlatform {

		public static bool s_changed {
			get {
				return BuildAssistWindow.s_changed;
			}
			set {
				BuildAssistWindow.s_changed = value;
			}
		}


		public BuildReport BuildPackage( string[] scenes ) {
			var p = P.GetActiveTargetParams();
			var path = $"{p.outputDirectory}/{P.GetOutputPackageName( p )}";

			try {
				B.development = p.development;

				B.compressionType = p.compression;
				B.il2CppCompilerConfiguration = p.il2CppCompilerConfiguration;
				B.WebGL_compressionFormat = p.WebGL_compressionFormat;
				B.WebGL_linkerTarget = p.WebGL_linkerTarget;
				B.WebGL_memorySize = p.WebGL_memorySize;
				B.WebGL_exceptionSupport = p.WebGL_exceptionSupport;
#if UNITY_2019_1_OR_NEWER
				B.WebGL_threadsSupport = p.WebGL_threadsSupport;
				B.WebGL_wasmStreaming = p.WebGL_wasmStreaming;
#endif
				Log( $"path: {path}" );
				Log( $"buildTarget: {p.buildTarget.ToString()}" );
				Log( $"options: {p.options.ToString()}" );
				return BuildPipeline.BuildPlayer( scenes, path, BuildTarget.WebGL, p.options );
			}
			catch( Exception e ) {
				Debug.LogException( e );
			}

			return null;
		}

		void DrawGUI_WebGL( P.Params currentParams ) {
			int opt = currentParams.platformOption;

			using( new GUILayout.VerticalScope( Styles.helpBox ) ) {
				EditorGUI.BeginChangeCheck();
				var fold = HEditorGUILayout.Foldout( E.i.fold.Has( E.FoldPlayerSettingsWebGL ), "WebGL" );
				E.i.fold.Toggle( E.FoldPlayerSettingsWebGL, fold );
				if( EditorGUI.EndChangeCheck() ) s_changed = true;

				EditorGUI.BeginChangeCheck();
				if( fold ) {
					EditorGUI.indentLevel++;
					using( new GUILayout.HorizontalScope() ) {
						currentParams.WebGL_compressionFormat = (WebGLCompressionFormat) EditorGUILayout.EnumPopup( S._CompressionFormat, currentParams.WebGL_compressionFormat );
						if( HEditorGUILayout.IconButton( Styles.iconHelp, 3 ) ) {
							EditorUtility.DisplayDialog( SS._Info, $@"Brotli (default: 2019.1～):
{S._WebGLresourcesarestoredusingBrotlicompression_}

Gzip (default: ～2018.4):
{S._WebGLresourcesarestoredusingGzipcompression_}

Disabled:
{S._WebGLresourcesareuncompressed_}", SS._OK );
						}
					}
					using( new GUILayout.HorizontalScope() ) {
						currentParams.WebGL_linkerTarget = (WebGLLinkerTarget) EditorGUILayout.EnumPopup( S._LinkerTarget, currentParams.WebGL_linkerTarget );
						if( HEditorGUILayout.IconButton( Styles.iconHelp, 3 ) ) {
							EditorUtility.DisplayDialog( SS._Info, $@"Asm:
{S._Onlyasm_jsoutputwillbegenerated_Thissettinghasbeendeprecated_}

Wasm (default):
{S._OnlyWebAssemblyoutputwillbegenerated_ThiswillrequireabrowserwithWebAssemblysupporttorunthegeneratedcontent_}

Both:
{S._Bothasm_jsandWebAssemblyoutputwillbegenerated_TheWebAssemblyversionofthegeneratedcontentwillbeusedifsupportedbythebrowser_otherwise_theasm_jsversionwillbeused_Thissettinghasbeendeprecated_}", SS._OK );
						}
					}
					string[] memS = { "16MB", "32MB", "64MB", "128MB", "256MB", "512MB", "1GB", "2GB", "4GB", "8GB" };
					int[] memI = { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
					int idx = ArrayUtility.IndexOf( memI, currentParams.WebGL_memorySize );
					if( idx < 0 ) idx = 1;
					idx = EditorGUILayout.Popup( S._MemorySize, idx, memS );
					currentParams.WebGL_memorySize = memI[ idx ];
					using( new GUILayout.HorizontalScope() ) {
						currentParams.WebGL_exceptionSupport = (WebGLExceptionSupport) EditorGUILayout.EnumPopup( S._EnableExceptions, currentParams.WebGL_exceptionSupport );
						if( HEditorGUILayout.IconButton( Styles.iconHelp, 3 ) ) {
							EditorUtility.DisplayDialog( SS._Info, $@"None:
{S._Disableexceptionsupport_}

Explicitly Thrown Exceptions Only (default):
{S._Enablethrowsupport_}

Full Without Stacktrace:
{S._Enableexceptionsupportforallexceptions_withoutstacktraceinformation_}

Full With Stacktrace:
{S._Enableexceptionsupportforallexceptions_includingstacktraceinformation_}
", SS._OK );
						}
					}

					if( UnitySymbol.Has( "UNITY_2019_1_OR_NEWER" ) ) {
						using( new GUILayout.HorizontalScope() ) {
							currentParams.WebGL_wasmStreaming = EditorGUILayout.ToggleLeft( S._WebAssemblyStreaming, currentParams.WebGL_wasmStreaming );
							if( HEditorGUILayout.IconButton( Styles.iconHelp, 3 ) ) {
								EditorUtility.DisplayDialog( SS._Info, $@"{S._EnableWebAssemblystreamingcompilation_}
{S._Whenenabled_UnitycompilestheWebAssemblybinaryfilewhilethefiledownloads_Thissettingrequiresan_application_wasm_mimetype_sosetuptheserveraccordingly_}", SS._OK );
							}
						}
						using( new GUILayout.HorizontalScope() ) {
							currentParams.WebGL_threadsSupport = EditorGUILayout.ToggleLeft( S._EnableMultiThread, currentParams.WebGL_threadsSupport );
							if( HEditorGUILayout.IconButton( Styles.iconHelp, 3 ) ) {
								EditorUtility.DisplayDialog( SS._Info, $@"{S._EnableMultithreadingsupport_}
{S._Whenenabled_Unityoutputsabuildwithmultithreadingsupport_ThegeneratedcontentrequiresabrowserthatsupportsWebAssemblythreads_Thisisanexperimentalfeatureandshouldonlybeusedfortestingpurposes_}", SS._OK );
							}
						}
					}
					EditorGUI.indentLevel--;
				}
				if( EditorGUI.EndChangeCheck() ) {
					currentParams.platformOption = opt;
					s_changed = true;
				}
			}
		}



		public void Draw( BuildAssistWindow parent ) {

			var currentParams = P.GetCurrentParams();

			parent.DrawGUI_PackageName();
			parent.DrawGUI_ConfigurationSelect();

			parent.DrawGUI_AssetBundle();
			parent.DrawGUI_BuildSettings();
			parent.DrawGUI_PlayerSettings();

			DrawGUI_WebGL( currentParams );

			parent.DrawGUI_OutputDirectory();



			//bool once = false;
			//void errorLabel( string s, string icon = "" ) {
			//	var c = EditorStyles.label.normal.textColor;
			//	EditorStyles.label.normal.textColor = Color.red;
			//	EditorStyles.label.fontStyle = FontStyle.Bold;

			//	GUILayout.Label( EditorHelper.TempContent( s, Icon.Get( icon ) ), EditorStyles.label );

			//	EditorStyles.label.fontStyle = FontStyle.Normal;
			//	EditorStyles.label.normal.textColor = c;
			//}
			//void errorTitle() {
			//	if( once ) return;
			//	errorLabel( "PlayerSettings.Standalone settings are incomplete", "console.erroricon.sml" );
			//	once = true;
			//}



			if( currentParams.development ) {
				HEditorGUILayout.BoldLabel( SS._Info, EditorIcon.info );
				HEditorGUILayout.BoldLabel( S._NotethatWebGLdevelopmentbuildsaremuchlargerthanreleasebuildsandshoundnotbepublicsed );
			}

			GUILayout.FlexibleSpace();
			parent.DrawGUI_Bottom();
		}
	}
}


