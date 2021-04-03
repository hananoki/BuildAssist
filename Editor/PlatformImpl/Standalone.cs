using HananokiRuntime.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityReflection;
using static HananokiEditor.BuildAssist.Console;
using P = HananokiEditor.BuildAssist.SettingsProject;

namespace HananokiEditor.BuildAssist {
	using UI;

	public class BuildProperty_Standalone : BuildPropertyBase {

		string[] errorMessages;

		public BuildProperty_Standalone() {
		}

		public override List<TreeView_BuildPropertyR.Item> CreateItemList() {
			var lst = new List<TreeView_BuildPropertyR.Item>();

			lst.Add( CreateItem( new ビルドシーンセット() ) );

			var item = CreateItem( new Group_BuildSettings() );
			lst.Add( item );
			item.AddChild( CreateItem( new 開発ビルド(), 1 ) );
			item.AddChild( CreateItem( new 圧縮方法(), 1 ) );
			item.AddChild( CreateItem( new アーキテクチャ(), 1 ) );

			var item2 = CreateItem( new Group_PlayerSettings() );
			lst.Add( item2 );
			item2.AddChild( CreateItem( new スクリプティングバックエンド(), 1 ) );
			item2.AddChild( CreateItem( new CPPコンパイラ設定(), 1 ) );

			var item3 = CreateItem( new Group_OutputDirectory() );
			lst.Add( item3 );
			item3.AddChild( CreateItem( new 自動設定(), 1 ) );
			item3.AddChild( CreateItem( new 構成の値を使用する(), 1 ) );
			item3.AddChild( CreateItem( new 出力ディレクトリ(), 1 ) );

			return lst;
		}


		public override void CheckError() {
			var currentParams = P.GetCurrentParams();
			var lst = new List<string>( 32 );
			if( currentParams.scriptingBackend == ScriptingImplementation.IL2CPP ) {
				var ss = UnityEditorModulesModuleManager.GetTargetStringFrom( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup, EditorUserBuildSettings.activeBuildTarget );
				object obj = UnityEditorModulesModuleManager.GetBuildWindowExtension( ss );
				var ext = new DesktopStandaloneBuildWindowExtension( obj );
				var mes = ext.GetCannotBuildIl2CppPlayerInCurrentSetupError();
				if( !mes.IsEmpty() ) {
					lst.Add( mes );
				}
			}
			errorMessages = lst.ToArray();
		}


		public override void DrawErrorReport( Rect rect ) {
			if( errorMessages.IsEmpty() ) return;

			//MessageError( ref rect, "PlayerSettings.Standalone settings are incomplete" );
			foreach( var p in errorMessages ) {
				MessageError( ref rect, p );
			}
		}
	}



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




		public void Draw( BuildAssistWindow parent ) {
			var currentParams = P.GetCurrentParams();
		}
	}
}


