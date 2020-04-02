
using Hananoki.Extensions;
using Hananoki.Reflection;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

using static Hananoki.BuildAssist.Console;
using P = Hananoki.BuildAssist.SettingsProject;

namespace Hananoki.BuildAssist {
	public static partial class BuildCommands {

		static void Batch() {
			Log( "Batch" );
			P.Load();

			Log( $"{string.Join( "; ", EditorUserBuildSettings.activeScriptCompilationDefines )}" );

			Log( $"activeBuildTargetGroup: {UEditorUserBuildSettings.activeBuildTargetGroup.ToString()}" );
			Log( $"activeBuildTarget: {EditorUserBuildSettings.activeBuildTarget.ToString()}" );

			var currentParams = P.GetActiveTargetParams();
			Log( $"{currentParams.buildTarget}" );

			foreach( var arg in Environment.GetCommandLineArgs() ) {
				if( arg.Contains( "-buildIndex" ) ) {
					int index = int.Parse( arg.Split( ':' )[ 1 ] );
					P.i.buildParamIndex = index;
					break;
				}
			}
			Build( 0x01 );
		}



		static string BuildPackage() {
			var currentParams = P.GetActiveTargetParams();

			IBuildPlatform builder = null;

			switch( UEditorUserBuildSettings.activeBuildTargetGroup ) {
				case BuildTargetGroup.Standalone:
					builder = new BuildPlatformStandard();
					break;

				case BuildTargetGroup.Android:
					builder = new BuildPlatformAndroid();
					break;

				case BuildTargetGroup.WebGL:
					builder = new BuildPlatformWebGL();
					break;
			}

			if( builder == null ) return string.Empty;

			Log( $"{builder.GetType().Name}" );
			var report = builder.BuildPackage( GetBuildSceneName() );

			B.CallEvent( typeof( BuildAssistEventPackageBuildPostProcess ) );

			if( report == null ) {
				return string.Empty;
			}

			Log( $"# BuildPipeline Result: {report.summary.result.ToString()}" );
			return report.summary.result.ToString();
		}



		static string BuildAssetBundle() {
			if( !P.i.enableAssetBundleBuild ) return string.Empty;

			var currentParams = P.GetCurrentParams();
			string result = "";

			var outputPath = "AssetBundles/" + currentParams.buildTarget.ToString();

			if( currentParams.assetBundleOption.Has( P.BUNDLE_OPTION_CLEAR_FILES ) ) {
				try {
					fs.rm( outputPath, true );
				}
				catch( Exception e ) {
					Debug.LogException( e );
				}
			}

			try {
				string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();

				AssetBundleBuild[] builds = new AssetBundleBuild[ assetBundleNames.Length ];
				for( int i = 0; i < builds.Length; i++ ) {
					builds[ i ].assetBundleName = assetBundleNames[ i ];
					builds[ i ].assetNames = AssetDatabase.GetAssetPathsFromAssetBundle( assetBundleNames[ i ] );
				}

				fs.mkdir( outputPath );

				Debug.Log( "# Start BuildPipeline.BuildAssetBundles:" );
				var manifest = BuildPipeline.BuildAssetBundles(
						outputPath,
						builds,
						currentParams.assetBundleOptions,
						currentParams.buildTarget );

				if( currentParams.assetBundleOption.Has( P.BUNDLE_OPTION_COPY_STREAMINGASSETS ) ) {
					for( int i = 0; i < builds.Length; i++ ) {
						var p = builds[ i ].assetBundleName;
						fs.cp( $"{outputPath}/{p}", $"{Application.streamingAssetsPath}/{p}", true );
					}
				}

				B.CallEvent( typeof( BuildAssistEventAssetBundleBuildPostProcess ) );

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			catch( Exception e ) {
				Debug.LogException( e );
				throw;
			}
			return result;
		}



		public static string Build( int mode ) {
			Log( "Start Build:" );
			using( new ScopeBuildSettings() )
			using( new ScopeScriptingDefineSymbols() ) {
				var currentParams = P.GetActiveTargetParams();

				if( !Application.isBatchMode ) {
					if( EditorUserBuildSettings.activeBuildTarget != currentParams.buildTarget ) {
						EditorUserBuildSettings.SwitchActiveBuildTarget( UEditorUserBuildSettings.activeBuildTargetGroup, currentParams.buildTarget );
					}
				}

				var activeBuildTargetGroup = UEditorUserBuildSettings.activeBuildTargetGroup;
				string symbol = PlayerSettings.GetScriptingDefineSymbolsForGroup( activeBuildTargetGroup );

				symbol = string.Join( ";", symbol, currentParams.scriptingDefineSymbols );

				PlayerSettings.SetScriptingDefineSymbolsForGroup( activeBuildTargetGroup, symbol );

				Log( $"ActiveBuildTargetGroup: {activeBuildTargetGroup.ToString()}" );
				Log( $"ScriptingDefineSymbols: {symbol}" );
				if( mode.Has( 0x02 ) ) {
					BuildAssetBundle();
				}
				if( mode.Has( 0x01 ) ) {
					BuildPackage();
				}
			}

			Log( "Exit Build:" );

			return "";
		}


		public static string[] GetBuildSceneName() {
			return EditorBuildSettings.scenes
						.Where( it => it.enabled )
						.Select( it => it.path )
						.ToArray();
		}
	}
}
