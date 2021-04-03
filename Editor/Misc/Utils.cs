#if UNITY_2018_1_OR_NEWER
#define LOCAL_UNITY_2018_1_OR_NEWER
#endif

using HananokiEditor.Extensions;
using HananokiRuntime.Extensions;
using System;
using UnityEditor;
using UnityEngine;
using UnityReflection;
using P = HananokiEditor.BuildAssist.SettingsProject;
using SS = HananokiEditor.SharedModule.S;

namespace HananokiEditor.BuildAssist {

	public static class Utils {

		/////////////////////////////////////////
		public static bool s_changed;
		public static P.Platform s_currentPlatform;


		/////////////////////////////////////////

		public static void CallEvent( Type tt ) {
			foreach( var func in AssemblieUtils.GetAllMethodsWithAttribute( tt ) ) {
				func.Invoke( null, null );
			}
		}

		/////////////////////////////////////////

		public static bool IsModuleInstalled( BuildTargetGroup buildTargetGroup, BuildTarget buildTarget ) {
			bool flag = UnityEditorBuildPipeline.LicenseCheck( buildTarget );
			string targetStringFrom = UnityEditorModulesModuleManager.GetTargetStringFrom( buildTargetGroup, buildTarget );
			return flag
				&& !string.IsNullOrEmpty( targetStringFrom )
				&& UnityEditorModulesModuleManager.GetBuildPostProcessor( targetStringFrom ) == null
				&& ( buildTargetGroup != BuildTargetGroup.Standalone || !UnityEditorBuildPlayerWindow.IsAnyStandaloneModuleLoaded() );
		}

		/////////////////////////////////////////

		public static void SelectItemUpdate() {
			if( s_currentPlatform != null && s_currentPlatform.buildTargetGroup != P.i.selectBuildTargetGroup ) {
				s_currentPlatform = null;
			}
			if( s_currentPlatform == null ) {
				s_currentPlatform = P.GetSelectPlatform();
				//BMS.i.platformList.Add( m_currentPlatform );
			}
			if( s_currentPlatform.parameters.Count == 0 ) {
				if( s_currentPlatform.buildTargetGroup != BuildTargetGroup.Unknown ) {
					s_currentPlatform.AddParams( "Debug" );
					s_currentPlatform.AddParams( "Release" );
					s_currentPlatform.AddParams( "Master" );
					s_changed = true;
				}
			}
			if( P.i.selectParamsIndex < 0 ) {
				P.i.selectParamsIndex = 0;
			}
			else if( s_currentPlatform.parameters.Count <= P.i.selectParamsIndex ) {
				P.i.selectParamsIndex = s_currentPlatform.parameters.Count - 1;
			}
		}


		/////////////////////////////////////////

		public static bool IsSwitchPlatformAbort() {
			if( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup != P.i.selectBuildTargetGroup ) {
				bool result = EditorUtility.DisplayDialog( SS._Confirm, $"{SS._RequiresSwitchActiveBuildTarget}\n{SS._IsitOK_}", SS._OK, SS._Cancel );
				if( !result ) return true;
			}
			return false;
		}

		/////////////////////////////////////////

		public static string MakeSceneName( string path ) {
			if( path.IsEmpty() ) return string.Empty;
			var s = path.Remove( 0, 7 );
			return $"{s.DirectoryName()}/{s.FileNameWithoutExtension()}".TrimStart( '/' );
		}

		/////////////////////////////////////////

		public static void ExecuteBuildPackage() {
			EditorApplication.delayCall += BuildPackage;
			B.s_buildProcess = true;
			BuildAssistWindow.Repaint();

			void BuildPackage() {
				using( new BuildProcessScope() ) {
					P.SetBuildParamIndex();
					try {
						var flag = 0x01;
						if( P.GetCurrentParams().buildAssetBundlesTogether ) {
							flag |= 0x02;
						}
						BuildCommands.Build( flag );
					}
					catch( Exception e ) {
						Debug.LogException( e );
					}
				}
			};
		}

		/////////////////////////////////////////

		public static void MakeDefaultOutputDirectory() {
			var currentParams = P.GetCurrentParams();
			if( !currentParams.outputDirectoryAuto ) return;

			var s = $"{Environment.CurrentDirectory}/{currentParams.buildTarget.ToString()}";
			if( currentParams.outputUseConfiguration ) {
				s += $"/{currentParams.name}";
			}
			currentParams.outputDirectory = DirectoryUtils.Prettyfy( s );
		}
	}


	[InitializeOnLoad]
	public static class Console {
		static Action<string> s_log;
		static Console() {
			if( UnityEngine.Application.isBatchMode ) {
				s_log = BatchLog;
			}
			else {
				s_log = EditorLog;
			}
		}
		public static void BatchLog( string s ) {
			System.Console.WriteLine( s );
		}
		public static void EditorLog( string s ) {
			UnityEngine.Debug.Log( s );
		}

		public static void Log( string s ) => s_log( $"### {s}" );
	}

}