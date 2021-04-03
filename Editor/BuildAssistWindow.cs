using HananokiEditor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityReflection;
using EE = HananokiEditor.SharedModule.SettingsEditor;
using P = HananokiEditor.BuildAssist.SettingsProject;
using PB = HananokiEditor.BuildAssist.SettingsProjectBuildSceneSet;
using UnityDebug = UnityEngine.Debug;


namespace HananokiEditor.BuildAssist {

	public partial class BuildAssistWindow : HEditorWindow {
		//const string Window_AssetBundle_Browser = "Window/AssetBundle Browser";
		const string Window_Show_Build_Report = "Window/Show Build Report";

		[MenuItem( "Window/Hananoki/" + Package.nameNicify, false, 20 )]
		public static void Open() {
			var window = GetWindow<BuildAssistWindow>( EE.IsUtilityWindow( typeof( BuildAssistWindow ) ) );
		}

		public static BuildAssistWindow s_window;

		GUI_BuildScenes m_Drawer_ScenesInBuild;
		GUI_BuildProperty m_Drawer_BuildPlatforms;

		public static string[] s_BundleOptions;
		public static string[] s_CompressionMode;


		bool _enableAssetBundle;
		bool _enableBuildReport;

		List<BuildTargetGroup> m_supportBuildTarget;

		IBuildPlatform m_draw;
		BuildPropertyBase m_buildPlatformDrawer;



		/////////////////////////////////////////

		public static void InitLocalize() {
			s_BundleOptions = new string[] { S._ExcludeTypeInformation, S._ForceRebuild, S._IgnoreTypeTreeChanges, S._AppendHash, S._StrictMode, S._DryRunBuild, S._ClearFiles };
			s_CompressionMode = new string[] { S._NoCompression, S._StandardCompression_LZMA_, S._ChunkBasedCompression_LZ4_ };
		}



		/////////////////////////////////////////

		public static void ChangeActiveTarget() {
			if( !P.GetPlatform( P.i.selectBuildTargetGroup ).enable ) {
				//int cur = (int)P.i.selectBuildTargetGroup;
				var lst = PlatformUtils.GetSupportList();
				int cur = lst.IndexOf( P.i.selectBuildTargetGroup );
				bool finded = false;
				int idx = cur - 1;
				while( 0 <= idx ) {
					if( P.GetPlatform( lst[ idx ] ).enable ) {
						finded = true;
						goto find;
					}
					idx--;
				}
				idx = cur + 1;
				while( idx < lst.Count ) {
					if( P.GetPlatform( lst[ idx ] ).enable ) {
						finded = true;
						goto find;
					}
					idx++;
				}
			find:
				if( finded ) {
					P.i.selectBuildTargetGroup = lst[ idx ];
				}
				else {
					P.i.selectBuildTargetGroup = BuildTargetGroup.Unknown;
				}
				P.Save();
			}
			s_window.MakeDrawBuildTarget();
			Repaint();
		}


		/////////////////////////////////////////

		new public static void Repaint() {
			( (EditorWindow) s_window ).Repaint();
		}



		//void ExecuteBuildBundle() {
		//	EditorApplication.delayCall += BuildBundle;

		//	void BuildBundle() {
		//		P.SetBuildParamIndex();
		//		try {
		//			BuildCommands.Build( 0x02 );
		//		}
		//		catch( Exception e ) {
		//			UnityDebug.LogException( e );
		//		}
		//		EditorApplication.update -= BuildBundle;
		//	}
		//}



		void MakeDrawBuildTarget() {
			m_draw = null;
			switch( P.i.selectBuildTargetGroup ) {

			case BuildTargetGroup.Standalone:
				m_draw = new BuildPlatformStandard();
				m_buildPlatformDrawer = new BuildProperty_Standalone();
				break;

			case BuildTargetGroup.WebGL:
				m_draw = new BuildPlatformWebGL();
				m_buildPlatformDrawer = new BuildProperty_WebGL();
				break;

			case BuildTargetGroup.Android:
			//				m_draw = new BuildPlatformAndroid();
			//				break;
			case BuildTargetGroup.iOS:
			//				m_draw = new BuildPlatformIOS();
			//				break;
			case BuildTargetGroup.Unknown:
			//				m_draw = new BuildPlatformUnknown();
			//				break;

			default:
				m_buildPlatformDrawer = new BuildProperty_Default();
				break;
			}
		}



		/////////////////////////////////////////

		void OnFocus() {

			PB.Load();
		}


		public void OnEnable() => Init();

		//public void Reinit() => Init();

		public void Init() {
			s_window = this;
			s_window.SetTitle( new GUIContent( Package.nameNicify, EditorIcon.buildsettings_psm_small ) );

			m_Drawer_ScenesInBuild = new GUI_BuildScenes();
			m_Drawer_BuildPlatforms = new GUI_BuildProperty();

			P.Load();
			Utils.s_currentPlatform = null;

			m_supportBuildTarget = PlatformUtils.GetSupportList();

			//_enableAssetBundle = EditorHelper.HasMenuItem( Window_AssetBundle_Browser );
			_enableBuildReport = EditorHelper.HasMenuItem( Window_Show_Build_Report );


			MakeDrawBuildTarget();
			OnFocus();

			Utils.SelectItemUpdate();
			ChangeActiveTarget();
			m_Drawer_BuildPlatforms.SelectPlatform( Utils.s_currentPlatform, m_buildPlatformDrawer );
		}



		#region OnGUI




		//public void DrawGUI_AssetBundle() {
		//	if( !PB.i.enableAssetBundleBuild ) return;

		//	var currentParams = P.GetCurrentParams();
		//	int opt = currentParams.assetBundleOption;

		//	EditorGUI.BeginChangeCheck();
		//	using( new GUILayout.VerticalScope( EditorStyles.helpBox ) ) {
		//		bool fold;
		//		using( new GUILayout.HorizontalScope() ) {
		//			fold = HEditorGUILayout.Foldout( E.i.fold.Has( E.FoldAssetBundle ), "Asset Bundle" );
		//			E.i.fold.Toggle( E.FoldAssetBundle, fold );

		//			GUILayout.FlexibleSpace();
		//			bool b7 = HEditorGUILayout.ToggleLeft( S._ClearFiles, opt.Has( P.BUNDLE_OPTION_CLEAR_FILES ) );
		//			opt.Toggle( P.BUNDLE_OPTION_CLEAR_FILES, b7 );

		//			var rc = EditorHelper.GetLayout( EditorIcon.settings, Styles.dropDownButton, GUILayout.Width( 80 ), GUILayout.Height( 16 ) );

		//			HEditorGUI.DropDown( rc, S._Build, Styles.dropDownButton, 18,
		//				() => {
		//					if( Utils.IsSwitchPlatformAbort() ) return;
		//					ExecuteBuildBundle();
		//				},
		//				() => {
		//					var m = new GenericMenu();
		//					if( Directory.Exists( P.i.outputAssetBundleDirectory ) ) {
		//						m.AddItem( new GUIContent( SS._OpenOutputFolder ), false, () => { ShellUtils.OpenDirectory( P.i.outputAssetBundleDirectory ); } );
		//					}
		//					else {
		//						m.AddDisabledItem( new GUIContent( $"{notDirectory}{P.i.outputAssetBundleDirectory.Replace( "/", "." )}" ) );
		//					}
		//					m.DropDown( HEditorGUI.lastRect.PopupRect() );
		//				} );


		//			if( _enableAssetBundle ) {
		//				var r = EditorHelper.GetLayout( EditorIcon.settings, HEditorStyles.iconButton );
		//				if( HEditorGUI.IconButton( r, EditorIcon.settings, 2 ) ) {
		//					EditorApplication.ExecuteMenuItem( Window_AssetBundle_Browser );
		//					Event.current.Use();
		//				}
		//			}

		//			rc = HEditorGUI.lastRect;
		//			GUI.Label( rc.AddH( -3 ), GUIContent.none, HEditorStyles.dopesheetBackground );

		//		}
		//		GUILayout.Space( 2 );
		//		if( fold ) {
		//			EditorGUI.indentLevel++;
		//			bool bst = HEditorGUILayout.ToggleLeft( S._CopyingthebuildresultstoStreamingAssets, opt.Has( P.BUNDLE_OPTION_COPY_STREAMINGASSETS ) );
		//			currentParams.assetBundleCompressionMode = EditorGUILayout.Popup( S._Compression, currentParams.assetBundleCompressionMode, s_CompressionMode, Styles.miniPopup );
		//			bool b1 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 0 ], opt.Has( P.BUNDLE_OPTION_EXCLUDETYPEINFORMATION ) );
		//			bool b2 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 1 ], opt.Has( P.BUNDLE_OPTION_FORCEREBUILD ) );
		//			bool b3 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 2 ], opt.Has( P.BUNDLE_OPTION_IGNORETYPETREECHANGES ) );
		//			bool b4 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 3 ], opt.Has( P.BUNDLE_OPTION_APPENDHASH ) );
		//			bool b5 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 4 ], opt.Has( P.BUNDLE_OPTION_STRICTMODE ) );
		//			bool b6 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 5 ], opt.Has( P.BUNDLE_OPTION_DRYRUNBUILD ) );

		//			opt.Toggle( P.BUNDLE_OPTION_COPY_STREAMINGASSETS, bst );
		//			opt.Toggle( P.BUNDLE_OPTION_EXCLUDETYPEINFORMATION, b1 );
		//			opt.Toggle( P.BUNDLE_OPTION_FORCEREBUILD, b2 );
		//			opt.Toggle( P.BUNDLE_OPTION_IGNORETYPETREECHANGES, b3 );
		//			opt.Toggle( P.BUNDLE_OPTION_APPENDHASH, b4 );
		//			opt.Toggle( P.BUNDLE_OPTION_STRICTMODE, b5 );
		//			opt.Toggle( P.BUNDLE_OPTION_DRYRUNBUILD, b6 );
		//			GUILayout.Space( 2 );

		//			EditorGUI.indentLevel--;
		//		}
		//	}
		//	if( EditorGUI.EndChangeCheck() ) {
		//		currentParams.assetBundleOption = opt;
		//		Utils.s_changed = true;
		//	}
		//}


		/////////////////////////////////////////

		void DrawToolBar() {
			HGUIToolbar.Begin();

			if( HGUIToolbar.Button( EditorIcon.settings ) ) SharedModule.SettingsWindow.OpenProject( Package.nameNicify );

			if( HGUIToolbar.Toggle( P.i.selectScene, "Scenes in Build", EditorIcon.sceneasset ) ) {
				P.i.selectScene = true;
				P.Save();
			}

			var lst = m_supportBuildTarget.Where( x => P.GetPlatform( x ).enable ).ToArray();

			var reo = Styles.toolbarbutton.padding;
			var active = UnityEditorEditorUserBuildSettings.activeBuildTargetGroup;

			for( int i = 0; i < lst.Length; i++ ) {
				var s = lst[ i ];

				var style = active == s ? Styles.toolbarbuttonActive : Styles.toolbarbutton;

				var cont = EditorHelper.TempContent( s.GetShortName(), s.Icon() );
				var size = style.CalcSize( cont );
				size.x -= 8;

				bool bb = P.i.selectScene || P.i.selectScene;

				ScopeChange.Begin();
				if( HGUIToolbar.Toggle( P.i.selectBuildTargetGroup == s && !bb, cont, style, GUILayout.Width( size.x ) ) ) {
					P.i.selectScene = false;
					P.Save();

					P.i.selectBuildTargetGroup = s;
					Utils.SelectItemUpdate();
					ChangeActiveTarget();
					m_Drawer_BuildPlatforms.SelectPlatform( Utils.s_currentPlatform, m_buildPlatformDrawer );
				}
				if( ScopeChange.End() ) {
					P.Save();
				}

				if( active == s ) {
					var rc = GUILayoutUtility.GetLastRect();
					EditorGUI.DrawRect( rc, new Color( 0, 0, 1, 0.1f ) );

					rc.x -= 4;
					rc = rc.AlignCenterH( 16 );
					if( UnitySymbol.Has( "UNITY_2019_3_OR_NEWER" ) ) {
						rc.y += 1;
					}
					GUI.DrawTexture( rc.AlignR( 16 ), EditorIcon.buildsettings_editor_small, ScaleMode.ScaleToFit );
				}
			}

			GUILayout.FlexibleSpace();
			if( _enableBuildReport ) {
				if( HGUIToolbar.Button( "Build Report" ) ) {
					EditorApplication.ExecuteMenuItem( Window_Show_Build_Report );
				}
			}


			HGUIToolbar.End();
		}


		/////////////////////////////////////////

		void DrawGUI() {
			Utils.s_changed = false;
			Utils.SelectItemUpdate();

			var w = position.width;
			var h = position.height;

			var drawArea = new Rect( 0, EditorStyles.toolbar.fixedHeight, w, position.height - EditorStyles.toolbar.fixedHeight );
			using( new GUILayout.AreaScope( drawArea ) ) {

				if( P.i.selectScene ) {
					m_Drawer_ScenesInBuild.DrawGUI();
					return;
				}
				else {
					m_Drawer_BuildPlatforms.DrawGUI();
				}

				if( Utils.s_changed ) {
					P.Save();
				}
			}
		}

		/////////////////////////////////////////

		public override void OnDefaultGUI() {
			Styles.Init();

			try {
				using( new EditorGUI.DisabledGroupScope( EditorHelper.IsWait() || B.s_buildProcess ) ) {
					DrawToolBar();
					DrawGUI();
				}
			}
			catch( Exception e ) {
				UnityDebug.LogException( e );
			}
		}

		#endregion


	}
}

