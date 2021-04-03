using HananokiEditor.Extensions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static HananokiEditor.BuildAssist.Console;
using P = HananokiEditor.BuildAssist.SettingsProject;
using SS = HananokiEditor.SharedModule.S;

namespace HananokiEditor.BuildAssist {
	using UI;

	public class BuildProperty_WebGL : BuildPropertyBase {
		//public virtual void CheckError() { }

		public override void DrawErrorReport( Rect rect ) {
			var currentParams = P.GetCurrentParams();

			if( currentParams.development ) {
				MessageInfo( ref rect, SS._Info );
				MessageInfo( ref rect, S._NotethatWebGLdevelopmentbuildsaremuchlargerthanreleasebuildsandshoundnotbepublicsed );
			}
		}

		public override List<TreeView_BuildPropertyR.Item> CreateItemList() {
			var lst = new List<TreeView_BuildPropertyR.Item>();

			lst.Add( CreateItem( new ビルドシーンセット() ) );

			var item = CreateItem( new Group_BuildSettings() );
			lst.Add( item );
			item.AddChild( CreateItem( new 開発ビルド(), 1 ) );

			var item2 = CreateItem( new Group_PlayerSettings() );
			lst.Add( item2 );
			item2.AddChild( CreateItem( new CPPコンパイラ設定(), 1 ) );

			var item3 = CreateItem( new Group_WebGL() );
			lst.Add( item3 );
			item3.AddChild( CreateItem( new WebGL圧縮形式(), 1 ) );
			item3.AddChild( CreateItem( new WebGLリンカーターゲット(), 1 ) );
			item3.AddChild( CreateItem( new WebGLメモリーサイズ(), 1 ) );
			item3.AddChild( CreateItem( new WebGL例外サポート(), 1 ) );
			item3.AddChild( CreateItem( new WebGLマルチスレッド(), 1 ) );

			var item4 = CreateItem( new Group_OutputDirectory() );
			lst.Add( item4 );
			item4.AddChild( CreateItem( new 自動設定(), 1 ) );
			item4.AddChild( CreateItem( new 構成の値を使用する(), 1 ) );
			item4.AddChild( CreateItem( new 出力ディレクトリ(), 1 ) );

			return lst;
		}
	}

	public class BuildPlatformWebGL : IBuildPlatform {


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
				//B.WebGL_wasmStreaming = p.WebGL_wasmStreaming;
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



		/////////////////////////////////

	}


}

namespace HananokiEditor.BuildAssist.UI {

	public class Group_WebGL : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => "WebGL";

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			//if( !HEditorGUI.IconButton( rect.W( 16 ), EditorIcon.settings ) ) return false;

			//UnityEditorMenu.File_Build_Settings();

			return false;
		}
	}

	/////////////////////////////////////////


	public class WebGL圧縮形式 : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._CompressionFormat;

		string infoText = $@"Brotli (default: 2019.1～):
{S._WebGLresourcesarestoredusingBrotlicompression_}

Gzip (default: ～2018.4):
{S._WebGLresourcesarestoredusingGzipcompression_}

Disabled:
{S._WebGLresourcesareuncompressed_}";



		public bool UIDraw( Rect rect, P.Params currentParams ) {
			ScopeChange.Begin();
			currentParams.WebGL_compressionFormat = (WebGLCompressionFormat) EditorGUI.EnumPopup( rect.TrimR( 20 ), currentParams.WebGL_compressionFormat );
			if( HEditorGUI.IconButton( rect.AlignR( 16 ).AddX( -4 ), EditorIcon._help ) ) {
				EditorUtility.DisplayDialog( SS._Info, infoText, SS._OK );
			}
			if( ScopeChange.End() ) {
				P.Save();
				return true;
			}
			return false;
		}
	}

	/////////////////////////////////////////


	public class WebGLリンカーターゲット : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._LinkerTarget;

		string infoText = $@"Asm:
{S._Onlyasm_jsoutputwillbegenerated_Thissettinghasbeendeprecated_}

Wasm (default):
{S._OnlyWebAssemblyoutputwillbegenerated_ThiswillrequireabrowserwithWebAssemblysupporttorunthegeneratedcontent_}

Both:
{S._Bothasm_jsandWebAssemblyoutputwillbegenerated_TheWebAssemblyversionofthegeneratedcontentwillbeusedifsupportedbythebrowser_otherwise_theasm_jsversionwillbeused_Thissettinghasbeendeprecated_}";


		public bool UIDraw( Rect rect, P.Params currentParams ) {
			ScopeChange.Begin();
			currentParams.WebGL_linkerTarget = (WebGLLinkerTarget) EditorGUI.EnumPopup( rect.TrimR( 20 ), currentParams.WebGL_linkerTarget );
			if( HEditorGUI.IconButton( rect.AlignR( 16 ).AddX( -4 ), EditorIcon._help ) ) {
				EditorUtility.DisplayDialog( SS._Info, infoText, SS._OK );
			}
			if( ScopeChange.End() ) {
				P.Save();
				return true;
			}
			return false;
		}
	}


	/////////////////////////////////////////


	public class WebGLメモリーサイズ : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._MemorySize;

		string[] m_memorySizePopup = { "16MB", "32MB", "64MB", "128MB", "256MB", "512MB", "1GB", "2GB", "4GB", "8GB" };
		int[] m_memorySize = { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			int idx = ArrayUtility.IndexOf( m_memorySize, currentParams.WebGL_memorySize );
			if( idx < 0 ) idx = 1;
			ScopeChange.Begin();
			idx = EditorGUI.Popup( rect.TrimR( 20 ), idx, m_memorySizePopup );

			if( ScopeChange.End() ) {
				currentParams.WebGL_memorySize = m_memorySize[ idx ];
				P.Save();
				return true;
			}
			return false;
		}
	}

	/////////////////////////////////////////


	public class WebGL例外サポート : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._EnableExceptions;

		string infoText = $@"None:
{S._Disableexceptionsupport_}

Explicitly Thrown Exceptions Only (default):
{S._Enablethrowsupport_}

Full Without Stacktrace:
{S._Enableexceptionsupportforallexceptions_withoutstacktraceinformation_}

Full With Stacktrace:
{S._Enableexceptionsupportforallexceptions_includingstacktraceinformation_}";



		public bool UIDraw( Rect rect, P.Params currentParams ) {
			ScopeChange.Begin();
			currentParams.WebGL_exceptionSupport = (WebGLExceptionSupport) EditorGUI.EnumPopup( rect.TrimR( 20 ), currentParams.WebGL_exceptionSupport );
			if( HEditorGUI.IconButton( rect.AlignR( 16 ).AddX( -4 ), EditorIcon._help ) ) {
				EditorUtility.DisplayDialog( SS._Info, infoText, SS._OK );
			}
			if( ScopeChange.End() ) {
				P.Save();
				return true;
			}
			return false;
		}
	}


	/////////////////////////////////////////


	public class WebGLマルチスレッド : PropertyItem {

		public bool isDisable( P.Params currentParams ) => !UnitySymbol.Has( "UNITY_2019_1_OR_NEWER" );
		public string name => S._EnableMultiThread;

		string infoText = $@"{S._EnableMultithreadingsupport_}
{S._Whenenabled_Unityoutputsabuildwithmultithreadingsupport_ThegeneratedcontentrequiresabrowserthatsupportsWebAssemblythreads_Thisisanexperimentalfeatureandshouldonlybeusedfortestingpurposes_}";

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			ScopeChange.Begin();
			currentParams.WebGL_threadsSupport = EditorGUI.Toggle( rect.W( 16 ), currentParams.WebGL_threadsSupport );
			if( HEditorGUI.IconButton( rect.AlignR( 16 ).AddX( -4 ), EditorIcon._help ) ) {
				EditorUtility.DisplayDialog( SS._Info, infoText, SS._OK );
			}
			if( ScopeChange.End() ) {
				P.Save();
				return true;
			}
			return false;
		}
	}

}


