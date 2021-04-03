#if UNITY_2018_1_OR_NEWER
#define LOCAL_UNITY_2018_1_OR_NEWER
#endif

using HananokiRuntime.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityReflection;

namespace HananokiEditor.BuildAssist {

	/////////////////////////////////////////

	public class BuildProcessScope : GUI.Scope {
		public BuildProcessScope() {
			B.s_buildProcess = true;
		}
		protected override void CloseScope() {
			B.s_buildProcess = false;
		}
	}


	/////////////////////////////////////////

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


	/////////////////////////////////////////

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
			buildTargetGroup = UnityEditorEditorUserBuildSettings.activeBuildTargetGroup;
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
			//WebGL_wasmStreaming = B.WebGL_wasmStreaming;
#endif
		}


		public void Dispose() {
			if( saveTarget != EditorUserBuildSettings.activeBuildTarget ) {
#if LOCAL_UNITY_2018_1_OR_NEWER
				EditorUserBuildSettings.SwitchActiveBuildTarget( buildTargetGroup, saveTarget );
#else
				EditorUserBuildSettings.SwitchActiveBuildTarget( saveTarget );
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
			//B.WebGL_wasmStreaming = WebGL_wasmStreaming;
#endif
		}
	}


	/////////////////////////////////////////

	public class ScopeBuildExclusionAssets : IDisposable {

		bool enabled;
		List<string> fileList;

		public ScopeBuildExclusionAssets( bool enabled, string[] fileList ) {
			this.enabled = enabled;
			if( this.enabled == false ) return;

			this.fileList = new List<string>( fileList );

			foreach( var path in fileList ) {
				if( path.IsEmpty() ) continue;
				try {
					File.Move( path, $"{path}~" );
					File.Move( $"{path}.meta", $"{path}.meta~" );
				}
				catch( Exception e ) {
					Debug.LogException( e );
				}
			}
			AssetDatabase.Refresh();
		}

		public void Dispose() {
			if( enabled == false ) return;

			foreach( var path in fileList ) {
				if( path.IsEmpty() ) continue;
				var f1 = $"{path}~";
				var f2 = $"{path}.meta~";
				try {
					File.Move( f1, f1.TrimEnd( '~' ) );
					File.Move( f2, f2.TrimEnd( '~' ) );
				}
				catch( Exception e ) {
					Debug.LogException( e );
				}
			}
			AssetDatabase.Refresh();
		}
	}

}
