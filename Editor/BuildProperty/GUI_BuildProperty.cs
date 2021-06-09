using HananokiEditor.Extensions;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityReflection;
using E = HananokiEditor.BuildAssist.SettingsEditor;
using P = HananokiEditor.BuildAssist.SettingsProject;
using PB = HananokiEditor.BuildAssist.SettingsProjectBuildSceneSet;
using SS = HananokiEditor.SharedModule.S;

namespace HananokiEditor.BuildAssist {
	public class GUI_BuildProperty {

		UnityEditorSplitterState m_HorizontalSplitter;

		TreeView_BuildPropertyL m_treeViewL;
		TreeView_BuildPropertyR m_treeViewR;

		P.Platform m_platform;
		BuildPropertyBase m_buildPlatformDrawer;

		UnityEditorBuildBuildPlatforms _UnityEditorBuildBuildPlatforms;
		UnityEditorBuildBuildPlatforms m_UnityEditorBuildBuildPlatforms {
			get {
				if( _UnityEditorBuildBuildPlatforms == null ) {
					_UnityEditorBuildBuildPlatforms = new UnityEditorBuildBuildPlatforms( UnityEditorBuildBuildPlatforms.instance );
				}
				return _UnityEditorBuildBuildPlatforms;
			}
		}


		/////////////////////////////////////////
		public GUI_BuildProperty() {
			m_HorizontalSplitter = new UnityEditorSplitterState( 0.20f, 0.80f );

			m_treeViewL = new TreeView_BuildPropertyL();
			m_treeViewR = new TreeView_BuildPropertyR();
		}


		/////////////////////////////////////////
		public void SelectPlatform( P.Platform platform, BuildPropertyBase drawer ) {
			m_platform = platform;
			m_buildPlatformDrawer = drawer;
			m_treeViewL.RegisterFiles( m_platform, m_treeViewR );
			m_treeViewR.RegisterFiles( m_platform, m_buildPlatformDrawer );

			drawer.CheckError();
		}


		/////////////////////////////////////////
		public void DrawGUI() {
			UnityEditorSplitterGUILayout.BeginHorizontalSplit( m_HorizontalSplitter );
			using( new GUILayout.VerticalScope() ) {
				DrawLeftPane();
			}
			using( new GUILayout.VerticalScope( HEditorStyles.dopesheetBackground ) ) {
				DrawRightPane();
			}
			UnityEditorSplitterGUILayout.EndHorizontalSplit();
		}


		/////////////////////////////////////////
		void DrawLeftPane() {
			HGUIToolbar.Begin();
			if( HGUIToolbar.Button( EditorIcon.toolbar_plus ) ) _add();
			if( HGUIToolbar.Button( EditorIcon.toolbar_minus ) ) _minus();
			GUILayout.FlexibleSpace();
			HGUIToolbar.End();

			m_treeViewL?.DrawLayoutGUI();

			void _add() {
				m_platform.AddParams( $"New ({m_platform.parameters.Count})" );
				P.i.selectParamsIndex = m_platform.parameters.Count - 1;
				P.Save();
				m_treeViewL.RegisterFiles();
			}
			void _minus() {
				m_platform.parameters.RemoveAt( m_treeViewL.currentItem.index );
				P.Save();
				Utils.SelectItemUpdate();
				m_treeViewL.RegisterFiles();
			}
		}


		/////////////////////////////////////////
		void DrawRightPane() {
			var currentParams = P.GetCurrentParams();

			//////////////////

			HGUIToolbar.Begin();
			GUILayout.Label( EditorHelper.TempContent( $"{PlayerSettings.productName}",
				$"{S._CompanyName}\t{PlayerSettings.companyName}\n{S._ProductName}\t{PlayerSettings.productName}\n{S._Version}\t{PlayerSettings.bundleVersion}" ), EditorStyles.toolbarButton );
			GUILayout.FlexibleSpace();

			ScopeDisable.Begin( !currentParams.development );
			E.connectProfiler.Value = HEditorGUILayout.SessionToggleLeft( S._ConnectProfiler, S._ProfilingisonlyenabledinaDevelopmentPlayer, E.connectProfiler.Value );
			ScopeDisable.End();

			E.autoRunPlayer.Value = HEditorGUILayout.SessionToggleLeft( S._AutoRunPlayer, E.autoRunPlayer.Value );

			if( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup != P.i.selectBuildTargetGroup ) {
				HGUIToolbar.DropDown2( S._SwitchPlatform, EditorIcon.warning
					, OnSwitchPlatform
					, OnDropAction );

			}
			else {
				HGUIToolbar.DropDown2( E.autoRunPlayer.Value ? S._BuildAndRun : S._Build
					, OnBuild
					, OnDropAction );
			}

			OnPlayButton( currentParams );

			HGUIToolbar.End();

			//////////////////

			using( new GUILayoutScope( 1, 0 ) ) {
				m_treeViewR?.DrawLayoutGUI();

				var aa = GUILayoutUtility.GetLastRect();// .GetRect( GUIContent.none, EditorStyles.label );

				var rc = m_treeViewR.m_lastCellRect;
				rc.x = aa.x;
				rc.y += EditorStyles.toolbar.fixedHeight;
				rc.y += EditorGUIUtility.singleLineHeight;
				rc.y += 8;
				rc.height = EditorGUIUtility.singleLineHeight;

				if( Utils.IsModuleInstalled( m_platform.buildTargetGroup, currentParams.buildTarget ) ) {
					rc.height = 40;
					EditorGUI.HelpBox( rc.AlignCenterW( rc.width - 40 ), string.Format( S._No_0_moduleloaded_, m_UnityEditorBuildBuildPlatforms.GetModuleDisplayName( m_platform.buildTargetGroup, currentParams.buildTarget ) ), MessageType.Warning );
					var rc2 = rc;
					rc2.x = rc.xMax - 240;
					rc2.width = 200;
					if( GUI.Button( rc2.AlignCenterH( 20 ), S._InstallWithUnityHub ) ) {
						string playbackEngineDownloadURL = UnityEditorBuildPlayerWindow.GetUnityHubModuleDownloadURL( m_platform.buildTargetGroup, currentParams.buildTarget );
						Debug.Log( playbackEngineDownloadURL );
						Help.BrowseURL( playbackEngineDownloadURL );
					}
					rc.y += 40 + 8;

				}
				m_buildPlatformDrawer?.DrawErrorReport( rc );
			}

			var lrc = GUILayoutUtility.GetLastRect();
			lrc.y = lrc.yMax - EditorGUIUtility.singleLineHeight - 8;
			lrc.height = EditorGUIUtility.singleLineHeight;
			lrc = lrc.AlignR( B.currentOutputPackageFullName.CalcSize( HEditorStyles.treeViewLine ).x );
			lrc.x -= 8;
			GUI.Label( lrc, B.currentOutputPackageFullName, HEditorStyles.treeViewLine );


		}


		/////////////////////////////////////////
		void OnSwitchPlatform() => PlatformUtils.SwitchActiveBuildTarget( P.i.selectBuildTargetGroup );


		/////////////////////////////////////////
		void OnBuild() {
			bool IsSwitchPlatformAbort() {
				if( UnityEditorEditorUserBuildSettings.activeBuildTargetGroup != P.i.selectBuildTargetGroup ) {
					bool result = EditorUtility.DisplayDialog( SS._Confirm, $"{SS._RequiresSwitchActiveBuildTarget}\n{SS._IsitOK_}", SS._OK, SS._Cancel );
					if( !result ) return true;
				}
				return false;
			}

			if( IsSwitchPlatformAbort() ) return;
			Utils.ExecuteBuildPackage();
		}


		/////////////////////////////////////////
		void OnDropAction() {
			var currentParams = P.GetCurrentParams();

			var m = new GenericMenu();
			if( Directory.Exists( P.currentOutputPackageDirectory ) ) {
				m.AddItem( SS._OpenOutputFolder, () => {
					ShellUtils.OpenDirectory( P.currentOutputPackageDirectory );
				} );
			}
			else {
				m.AddDisabledItem( $"{S.__NotFoundDirectory__}{P.currentOutputPackageDirectory.TryReplace( "/", "." )}]".content() );
			}
			m.AddSeparator( "" );

			if( PB.i.enableAssetBundleBuild ) {
				m.AddItem( S._BuildAssetBundletogether, currentParams.buildAssetBundlesTogether, () => { currentParams.buildAssetBundlesTogether = !currentParams.buildAssetBundlesTogether; } );
			}
			if( UnitySymbol.Has( "UNITY_EDITOR_WIN" ) ) {
				m.AddItem( S._CreateabuildBATfile, () => {
					var tname = $"{UnityEditorEditorUserBuildSettings.activeBuildTargetGroup.ToString()}_{currentParams.name}";
					fs.WriteFileAll( $"Build_{tname}.bat", b => {
						b.AppendLine( $"@echo off" );
						b.AppendLine( $"set PATH=%PATH%;{EditorApplication.applicationPath.GetDirectory()}" );
						b.AppendLine( $"set OPT1=-batchmode -nographics" );
						b.AppendLine( $"set BUILD=-buildTarget {B.BuildTargetToBatchName( currentParams.buildTarget )}" );
						b.AppendLine( $"set PROJ=-projectPath {Environment.CurrentDirectory}" );
						b.AppendLine( $"set LOG=-logFile \"Logs/Editor_{tname}.log\"" );
						b.AppendLine( $"set METHOD=-executeMethod Hananoki.{Package.name}.{nameof( BuildCommands )}.Batch -buildIndex:{P.i.selectParamsIndex}" );
						b.AppendLine( $"Unity.exe %OPT1% %BUILD% %PROJ% %LOG% %METHOD% -quit" );
						b.AppendLine( $"pause" );
					}, utf8bom: false );
					EditorUtility.DisplayDialog( SS._Confirm, $"{S._BuildBATcreated}\n{Environment.CurrentDirectory}/{$"Build_{tname}.bat"}", SS._OK );
				} );
			}
			m.DropDown( HEditorGUI.lastRect.PopupRect() );
		}


		/////////////////////////////////////////
		void OnPlayButton( P.Params currentParams ) {
			switch( currentParams.buildTarget ) {
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				ScopeDisable.Begin( !B.currentOutputPackageFullName.IsExistsFile() );
				if( HGUIToolbar.Button( EditorIcon.playbutton ) ) {
					System.Diagnostics.Process.Start( B.currentOutputPackageFullName );
				}
				ScopeDisable.End();
				break;
			case BuildTarget.WebGL: {
				var path = $"{B.currentOutputPackageFullName}/index.html";
				ScopeDisable.Begin( !path.IsExistsFile() );
				if( HGUIToolbar.Button( EditorIcon.playbutton ) ) {
					UnityEditorPostprocessBuildPlayer.Launch( BuildTargetGroup.WebGL, BuildTarget.WebGL, path.DirectoryName(), "", BuildOptions.None, null );
				}
				ScopeDisable.End();
				break;
			}
			default:
				ScopeDisable.Begin( true );
				HGUIToolbar.Button( EditorIcon.playbutton, "Not supported." );
				ScopeDisable.End();
				break;
			}
		}
	}
}