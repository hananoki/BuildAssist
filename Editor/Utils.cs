﻿#if UNITY_2018_1_OR_NEWER
#define LOCAL_UNITY_2018_1_OR_NEWER
#endif

using Hananoki.Reflection;
using System;
using System.IO;
using UnityEditor;
using System.Reflection;
using UnityEngine;

namespace Hananoki.BuildAssist {

	[AttributeUsage( AttributeTargets.Class )]
	public class BuildAssistEvent : Attribute { public BuildAssistEvent() { } }

	[AttributeUsage( AttributeTargets.Method )]
	public class BuildAssistEventAssetBundleBuildPostProcess : Attribute { public BuildAssistEventAssetBundleBuildPostProcess() { } }

	[AttributeUsage( AttributeTargets.Method )]
	public class BuildAssistEventPackageBuildPostProcess : Attribute { public BuildAssistEventPackageBuildPostProcess() { } }


	public static class B {

		public static void CallEvent( Type tt ) {
			var t = typeof( BuildAssistEvent );
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() ) {
				foreach( var type in assembly.GetTypes() ) {
					try {
						if( type.GetCustomAttribute( t ) == null ) continue;
						foreach( var p in R.Methods( tt, type.FullName, assembly.FullName.Split( ',' )[ 0 ] ) ) p.Invoke( null, null );
					}
					catch( Exception ee ) {
						Debug.LogException( ee );
					}
				}
			}
		}

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
				return UEditorUserBuildSettings.GetCompressionType( UEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				UEditorUserBuildSettings.SetCompressionType( UEditorUserBuildSettings.activeBuildTargetGroup, value );
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
				return PlayerSettings.GetScriptingBackend( UEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				PlayerSettings.SetScriptingBackend( UEditorUserBuildSettings.activeBuildTargetGroup, value );
			}
		}

		public static Il2CppCompilerConfiguration il2CppCompilerConfiguration {
			get {
				return PlayerSettings.GetIl2CppCompilerConfiguration( UEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				PlayerSettings.SetIl2CppCompilerConfiguration( UEditorUserBuildSettings.activeBuildTargetGroup, value );
			}
		}

		public static string applicationIdentifier {
			get {
				return PlayerSettings.GetApplicationIdentifier( UEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				PlayerSettings.SetApplicationIdentifier( UEditorUserBuildSettings.activeBuildTargetGroup, value );
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
				return PlayerSettings.GetScriptingDefineSymbolsForGroup( UEditorUserBuildSettings.activeBuildTargetGroup );
			}
			set {
				PlayerSettings.SetScriptingDefineSymbolsForGroup( UEditorUserBuildSettings.activeBuildTargetGroup, value );
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

		public static bool WebGL_wasmStreaming {
			get {
				return PlayerSettings.WebGL.wasmStreaming;
			}
			set {
				PlayerSettings.WebGL.wasmStreaming = value;
			}
		}
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



	public class ScopeScriptingDefineSymbols : IDisposable {
		string saveTarget;
		BuildTargetGroup group;

		public ScopeScriptingDefineSymbols() {
			this.group = EditorUserBuildSettings.selectedBuildTargetGroup;
			saveTarget = PlayerSettings.GetScriptingDefineSymbolsForGroup( group );
		}

		public void Dispose() {
			PlayerSettings.SetScriptingDefineSymbolsForGroup( group, saveTarget );
		}
	}



	public class ScopeBuildSettings : IDisposable {
		BuildTarget saveTarget;
#if LOCAL_UNITY_2018_1_OR_NEWER
		BuildTargetGroup buildTargetGroup;
#endif
		bool development;
		bool buildAppBundle;
		ScriptingImplementation scriptingImplementation;
		Il2CppCompilerConfiguration il2CppCompilerConfiguration;
		Compression compressionType;
		AndroidArchitecture targetArchitectures;
		AndroidBuildType androidBuildType;
		WebGLCompressionFormat WebGL_compressionFormat;
		WebGLLinkerTarget WebGL_linkerTarget;
		WebGLExceptionSupport WebGL_exceptionSupport;
		int WebGL_memorySize;
#if UNITY_2019_1_OR_NEWER
		bool WebGL_threadsSupport;
		bool WebGL_wasmStreaming;
#endif

		public ScopeBuildSettings() {
			saveTarget = EditorUserBuildSettings.activeBuildTarget;
#if LOCAL_UNITY_2018_1_OR_NEWER
			buildTargetGroup = UEditorUserBuildSettings.activeBuildTargetGroup;
#endif

			scriptingImplementation = B.scriptingBackend;
			il2CppCompilerConfiguration = B.il2CppCompilerConfiguration;
			compressionType = B.compressionType;
			development = B.development;
			buildAppBundle = B.buildAppBundle;
			targetArchitectures = B.targetArchitectures;
			androidBuildType = B.androidBuildType;
			WebGL_compressionFormat = B.WebGL_compressionFormat;
			WebGL_linkerTarget = B.WebGL_linkerTarget;
			WebGL_memorySize = B.WebGL_memorySize;
			WebGL_exceptionSupport = B.WebGL_exceptionSupport;
#if UNITY_2019_1_OR_NEWER
			WebGL_threadsSupport = B.WebGL_threadsSupport;
			WebGL_wasmStreaming = B.WebGL_wasmStreaming;
#endif
		}


		public void Dispose() {
			if( saveTarget != EditorUserBuildSettings.activeBuildTarget ) {
#if LOCAL_UNITY_2018_1_OR_NEWER
				EditorUserBuildSettings.SwitchActiveBuildTarget( buildTargetGroup, saveTarget );
#else
				EUBS.SwitchActiveBuildTarget( saveTarget );
#endif
			}
			B.scriptingBackend = scriptingImplementation;
			B.il2CppCompilerConfiguration = il2CppCompilerConfiguration;
			B.compressionType = compressionType;
			B.development = development;
			B.buildAppBundle = buildAppBundle;
			B.targetArchitectures = targetArchitectures;
			B.androidBuildType = androidBuildType;
			B.WebGL_compressionFormat = WebGL_compressionFormat;
			B.WebGL_linkerTarget = WebGL_linkerTarget;
			B.WebGL_memorySize = WebGL_memorySize;
			B.WebGL_exceptionSupport = WebGL_exceptionSupport;

#if UNITY_2019_1_OR_NEWER
			B.WebGL_threadsSupport = WebGL_threadsSupport;
			B.WebGL_wasmStreaming = WebGL_wasmStreaming;
#endif
		}
	}



	public class ScopeBuildDebugAsset : IDisposable {

		bool m_yuukoo;

		public ScopeBuildDebugAsset( bool yuukou ) {
			m_yuukoo = yuukou;

			if( m_yuukoo == false ) return;

			var guids = AssetDatabase.FindAssets( "l:Debug", null );
			foreach( var guid in guids ) {
				string path = AssetDatabase.GUIDToAssetPath( guid );

				File.Move( path, path + ".bak" );
				File.Move( path + ".meta", path + ".bak.meta" );
			}
			AssetDatabase.Refresh();
		}

		public void Dispose() {
			if( m_yuukoo == false ) return;

			var guids = AssetDatabase.FindAssets( "l:Debug", null );
			foreach( var guid in guids ) {
				string path = AssetDatabase.GUIDToAssetPath( guid );

				var meta = path + ".meta";

				File.Move( path, path.Replace( ".bak", "" ) );
				File.Move( meta, meta.Replace( ".bak", "" ) );
			}
			AssetDatabase.Refresh();
		}
	}

}