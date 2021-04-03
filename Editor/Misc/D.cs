using UnityEditor;
using UnityReflection;
using P = HananokiEditor.BuildAssist.SettingsProject;

namespace HananokiEditor.BuildAssist {
	public static class B {

		public static bool s_buildProcess;

		public static string currentOutputPackageFullName => $"{P.currentOutputPackageDirectory}/{P.GetOutputPackageName( P.GetCurrentParams() )}";

		public static readonly string[] kScriptingBackendNames = { @"Mono", @"IL2CPP" };
		public const string kBuildSettings = "Build Settings";



		#region EditorUserBuildSettings
		public static bool development {
			get {
				return EditorUserBuildSettings.development;
			}
			set {
				EditorUserBuildSettings.development = value;
			}
		}

		public static bool buildAppBundle {
			get {
				return EditorUserBuildSettings.buildAppBundle;
			}
			set {
				EditorUserBuildSettings.buildAppBundle = value;
			}
		}

		public static Compression compressionType {
			get {
				return (Compression) UnityEditorEditorUserBuildSettings.GetCompressionType( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				UnityEditorEditorUserBuildSettings.SetCompressionType( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup, (int) value );
			}
		}

		public static AndroidBuildType androidBuildType {
			get {
				return EditorUserBuildSettings.androidBuildType;
			}
			set {
				EditorUserBuildSettings.androidBuildType = value;
			}
		}

		#endregion



		#region PlayerSettings
		public static ScriptingImplementation scriptingBackend {
			get {
				return PlayerSettings.GetScriptingBackend( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				PlayerSettings.SetScriptingBackend( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup, value );
			}
		}

		public static Il2CppCompilerConfiguration il2CppCompilerConfiguration {
			get {
				return PlayerSettings.GetIl2CppCompilerConfiguration( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				PlayerSettings.SetIl2CppCompilerConfiguration( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup, value );
			}
		}

		public static string applicationIdentifier {
			get {
				return PlayerSettings.GetApplicationIdentifier( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				PlayerSettings.SetApplicationIdentifier( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup, value );
			}
		}

		public static AndroidArchitecture targetArchitectures {
			get {
				return PlayerSettings.Android.targetArchitectures;
			}
			set {
				PlayerSettings.Android.targetArchitectures = value;
			}
		}

		public static string scriptingDefineSymbols {
			get {
				return PlayerSettings.GetScriptingDefineSymbolsForGroup( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				PlayerSettings.SetScriptingDefineSymbolsForGroup( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup, value );
			}
		}



		public static WebGLCompressionFormat WebGL_compressionFormat {
			get {
				return PlayerSettings.WebGL.compressionFormat;
			}
			set {
				PlayerSettings.WebGL.compressionFormat = value;
			}
		}

		public static WebGLLinkerTarget WebGL_linkerTarget {
			get {
				return PlayerSettings.WebGL.linkerTarget;
			}
			set {
				PlayerSettings.WebGL.linkerTarget = value;
			}
		}

		public static int WebGL_memorySize {
			get {
				return PlayerSettings.WebGL.memorySize;
			}
			set {
				PlayerSettings.WebGL.memorySize = value;
			}
		}

		public static WebGLExceptionSupport WebGL_exceptionSupport {
			get {
				return PlayerSettings.WebGL.exceptionSupport;
			}
			set {
				PlayerSettings.WebGL.exceptionSupport = value;
			}
		}

#if UNITY_2019_1_OR_NEWER
		public static bool WebGL_threadsSupport {
			get {
				return PlayerSettings.WebGL.threadsSupport;
			}
			set {
				PlayerSettings.WebGL.threadsSupport = value;
			}
		}

		//public static bool WebGL_wasmStreaming {
		//	get {
		//		return PlayerSettings.WebGL.wasmStreaming;
		//	}
		//	set {
		//		PlayerSettings.WebGL.wasmStreaming = value;
		//	}
		//}
#endif

		#endregion



		public static string BuildTargetToBatchName( BuildTarget target ) {
			switch( target ) {
			case BuildTarget.StandaloneWindows:
				return "Win";
			case BuildTarget.StandaloneWindows64:
				return "Win64";
			case BuildTarget.StandaloneOSX:
				return "Linux64";
			case BuildTarget.StandaloneLinux64:
				return "OSXUniversal";
			case BuildTarget.iOS:
				return "iOS";
			case BuildTarget.Android:
				return "Android";
			case BuildTarget.WebGL:
				return "WebGL";
			case BuildTarget.XboxOne:
				return "XboxOne";
			case BuildTarget.PS4:
				return "PS4";
			case BuildTarget.WSAPlayer:
				return "WindowsStoreApps";
			case BuildTarget.Switch:
				return "Switch";
			case BuildTarget.tvOS:
				return "tvOS";
			}
			return "Standalone";
		}
	}

}
