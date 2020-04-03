#if !ENABLE_WARNING
#pragma warning disable 649
#endif

using Hananoki.Extensions;
using Hananoki.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hananoki.BuildAssist {

	[Serializable]
	public class SettingsProject {

		public const int BUNDLE_OPTION_EXCLUDETYPEINFORMATION = ( 1 << 0 );
		public const int BUNDLE_OPTION_FORCEREBUILD = ( 1 << 1 );
		public const int BUNDLE_OPTION_IGNORETYPETREECHANGES = ( 1 << 2 );
		public const int BUNDLE_OPTION_APPENDHASH = ( 1 << 3 );
		public const int BUNDLE_OPTION_STRICTMODE = ( 1 << 4 );
		public const int BUNDLE_OPTION_DRYRUNBUILD = ( 1 << 5 );
		public const int BUNDLE_OPTION_CLEAR_FILES = ( 1 << 6 );

		public const int BUNDLE_OPTION_COPY_STREAMINGASSETS = ( 1 << 10 );

		// BuildOptions

		public const int OPT_DEVELOPMENT = ( 1 << 0 );
		public const int OPT_ANDROID_BUILD_APP_BUNDLE = ( 1 << 1 );

		public const int OPT_BUILD_ASSET_BUNDLES_TOGETHER = ( 1 << 16 );



		[Serializable]
		public class Params {
			public string name;
			public int buildSceneSetIndex =-1;
			public BuildTarget buildTarget;
			public ScriptingImplementation scriptingBackend;
			public Il2CppCompilerConfiguration il2CppCompilerConfiguration;
			public Compression compression = Compression.Lz4;
			public string scriptingDefineSymbols;

			public WebGLCompressionFormat WebGL_compressionFormat;
			public WebGLLinkerTarget WebGL_linkerTarget = WebGLLinkerTarget.Wasm;
			public int WebGL_memorySize = 32;
			public WebGLExceptionSupport WebGL_exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
			public bool WebGL_threadsSupport = false;
			public bool WebGL_wasmStreaming = false;

			public int assetBundleOption;
			public int assetBundleCompressionMode = 2;

			public bool outputDirectoryAuto;
			public bool outputUseConfiguration;
			public string outputDirectory;

			public int platformOption;
			public bool development { get { return platformOption.Has( OPT_DEVELOPMENT ); } set { platformOption.Toggle( OPT_DEVELOPMENT, value ); } }
			public bool buildAssetBundlesTogether { get { return platformOption.Has( OPT_BUILD_ASSET_BUNDLES_TOGETHER ); } set { platformOption.Toggle( OPT_BUILD_ASSET_BUNDLES_TOGETHER, value ); } }
			public bool buildAppBundle { get { return platformOption.Has( OPT_ANDROID_BUILD_APP_BUNDLE ); } set { platformOption.Toggle( OPT_ANDROID_BUILD_APP_BUNDLE, value ); } }


			public BuildOptions options {
				get {
					BuildOptions opt = BuildOptions.None;

					if( SettingsEditor.autoRunPlayer.Value ) {
						opt |= BuildOptions.AutoRunPlayer;
					}
					if( B.development ) {
						if( SettingsEditor.connectProfiler.Value ) {
							opt |= BuildOptions.ConnectWithProfiler;
						}
					}
					if( B.development ) {
						opt |= BuildOptions.Development;
					}
					if( B.compressionType == Compression.Lz4 ) {
						opt |= BuildOptions.CompressWithLz4;
					}
					if( B.compressionType == Compression.Lz4HC ) {
						opt |= BuildOptions.CompressWithLz4HC;
					}
					return opt;
				}
			}


			public BuildAssetBundleOptions assetBundleOptions {
				get {
					BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;

					if( assetBundleCompressionMode == 2 ) {
						opt |= BuildAssetBundleOptions.ChunkBasedCompression;
					}
					else if( assetBundleCompressionMode == 0 ) {
						opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
					}

					if( assetBundleOption.Has( BUNDLE_OPTION_FORCEREBUILD ) ) {
						opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
					}
					if( assetBundleOption.Has( BUNDLE_OPTION_EXCLUDETYPEINFORMATION ) ) {
						opt |= BuildAssetBundleOptions.DisableWriteTypeTree;
					}
					if( assetBundleOption.Has( BUNDLE_OPTION_IGNORETYPETREECHANGES ) ) {
						opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
					}
					if( assetBundleOption.Has( BUNDLE_OPTION_APPENDHASH ) ) {
						opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
					}
					if( assetBundleOption.Has( BUNDLE_OPTION_STRICTMODE ) ) {
						opt |= BuildAssetBundleOptions.StrictMode;
					}
					if( assetBundleOption.Has( BUNDLE_OPTION_DRYRUNBUILD ) ) {
						opt |= BuildAssetBundleOptions.DryRunBuild;
					}
					return opt;
				}
			}


			public Params( BuildTargetGroup b, string name ) {
				this.name = name;

				buildTarget = UnityEditorUserBuildSettingsUtils.CalculateSelectedBuildTarget( b );
			}
		}


		[Serializable]
		public class Platform {
			public bool enable = true;
			public BuildTargetGroup buildTargetGroup;
			public List<Params> parameters;

			public Platform( BuildTargetGroup buildTargetGroup ) {
				this.buildTargetGroup = buildTargetGroup;
				parameters = new List<Params>();
			}
			public void AddParams( string name ) {
				parameters.Add( new Params( buildTargetGroup, name ) );
			}
		}


		public string productName;
		public int selectParamsIndex;

		public bool enableAssetBundleBuild = true;

		[NonSerialized]
		public int buildParamIndex;

		public BuildTargetGroup selectBuildTargetGroup;
		public bool selectScene;

		public List<Platform> platformList;


		public static SettingsProject i;



		public static string currentOutputPackageDirectory => GetCurrentParams().outputDirectory;

		public static string currentOutputPackageFullName => $"{currentOutputPackageDirectory}/{GetOutputPackageName( GetCurrentParams() )}";

		public string outputAssetBundleDirectory => "AssetBundles/" + GetCurrentParams().buildTarget.ToString();

		public static Platform GetSelectPlatform() => GetPlatform( i.selectBuildTargetGroup );

		public static Platform GetPlatform( BuildTargetGroup b ) => i.platformList[ (int) b ];

		public static void SetBuildParamIndex() => i.buildParamIndex = i.selectParamsIndex;


		public static Params GetCurrentParams() {
			try {
				var p = GetPlatform( i.selectBuildTargetGroup ).parameters;
				if( p.Count <= i.selectParamsIndex ) return null;
				return GetPlatform( i.selectBuildTargetGroup ).parameters[ i.selectParamsIndex ];
			}
			catch( Exception e ) {
				Debug.LogException( e );
			}
			return null;
		}


		public static Params GetActiveTargetParams() {
			var parameters = GetPlatform( UnityEditorUserBuildSettings.activeBuildTargetGroup ).parameters;
			return parameters[ i.buildParamIndex ];
		}


		public static string GetOutputPackageName( Params param ) {
			if( param.buildTarget == BuildTarget.StandaloneWindows || param.buildTarget == BuildTarget.StandaloneWindows64 ) {
				return $"{i.productName}.exe";
			}
			else if( param.buildTarget == BuildTarget.Android ) {
				var fname = $"{i.productName}_{PlayerSettings.bundleVersion}_v{PlayerSettings.Android.bundleVersionCode}";
				if( param.buildAppBundle ) {
					fname += ".aab";
				}
				else {
					fname += ".apk";
				}
				return fname;
			}
			return i.productName;
		}


		public SettingsProject() {
			selectBuildTargetGroup = UnityEditorUserBuildSettings.activeBuildTargetGroup;
			platformList = new List<Platform>();
			Resize();
		}


		public void Resize() {
			int max = (int) EnumUtils.GetArray<BuildTargetGroup>().Max();

			for( int ii = 0; ii < max; ii++ ) {
				if( ii < platformList.Count ) continue;
				platformList.Add( new Platform( (BuildTargetGroup) ii ) );
			}
		}


		public static void Load() {
			SettingsEditor.Load();

			if( i != null ) {
				i.Resize();
				return;
			}

			i = JsonUtility.FromJson<SettingsProject>( fs.ReadAllText( Package.projectSettingsPath ) );
			if( i == null ) {
				i = new SettingsProject();
				Save();
			}
			i.Resize();
		}


		public static void Save() {
			File.WriteAllText( Package.projectSettingsPath, JsonUtility.ToJson( i, true ) );
			SettingsEditor.Save();
		}
	}



	[Serializable]
	public class SettingsProjectBuildSceneSet {
		public static string projectSettingsPath => $"{Environment.CurrentDirectory}/ProjectSettings/BuildAssistBuildSceneSet.json";

		[Serializable]
		public class Profile {
			public string profileName;
			public List<string> sceneNanes;
			public bool exclusionScene;

			public Profile( string profileName ) {
				this.profileName = profileName;
				sceneNanes = new List<string>();
			}

			public void Toggle( bool toggle, string sceneName ) {
				if( toggle ) {
					sceneNanes.Add( sceneName );
				}
				else {
					sceneNanes.Remove( sceneName );
				}
			}
			public bool Has( string sceneName ) {
				if( sceneNanes == null ) return false;
				return 0 <= sceneNanes.IndexOf( sceneName ) ? true : false;
			}

		}

		public static SettingsProjectBuildSceneSet i;

		public List<Profile> profileList;
		public int selectIndex;


		public Profile selectProfile => profileList[ selectIndex ];

		SettingsProjectBuildSceneSet() {
			profileList = new List<Profile>();
			profileList.Add( new Profile( "Default" ) );
		}

		public string GetSelectedPopupName( SettingsProject.Params currentParams ) {
			if( currentParams.buildSceneSetIndex < 0 ) return S._UsethestandardBuildSettings;
			if( profileList.Count <= currentParams.buildSceneSetIndex ) {
				currentParams.buildSceneSetIndex = profileList.Count - 1;
			}
			return profileList[ currentParams.buildSceneSetIndex ].profileName;
		}



		public static string[] GetBuildSceneName() {
			Load();
			return GetBuildSceneName( i.selectProfile );
		}
		
		public static string[] GetBuildSceneName( Profile profile ) {
			Load();
			var scenes = EditorBuildSettings.scenes.Select( x => x.path ).ToList();
			var lst = new List<string>();
			foreach( var s in scenes ) {
				if( 0 <= profile.sceneNanes.IndexOf( s ) ) {
					lst.Add( s );
				}
			}
			if( !profile.exclusionScene ) {
				lst.AddRange( profile.sceneNanes );
			}
			return lst.Distinct().ToArray();
		}

		public static string[] GetBuildSceneName( SettingsProject.Params c ) {
			if( 0 <= c.buildSceneSetIndex ) {
				return GetBuildSceneName( i.profileList[ c.buildSceneSetIndex ] );
			}
			return EditorBuildSettings.scenes.Where( x => x.enabled ).Select( x => x.path ).ToArray();
		}


		public static void Load() {
			if( i != null ) return;
			i = JsonUtility.FromJson<SettingsProjectBuildSceneSet>( fs.ReadAllText( projectSettingsPath ) );
			if( i == null ) {
				i = new SettingsProjectBuildSceneSet();
				Save();
			}
		}

		public static void Save() {
			File.WriteAllText( projectSettingsPath, JsonUtility.ToJson( i, true ) );
			SettingsEditor.Save();
		}
	}
}
