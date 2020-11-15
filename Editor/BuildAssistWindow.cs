
using Hananoki.Extensions;
using Hananoki.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using E = Hananoki.BuildAssist.SettingsEditor;
using P = Hananoki.BuildAssist.SettingsProject;
using PB = Hananoki.BuildAssist.SettingsProjectBuildSceneSet;
using SS = Hananoki.SharedModule.S;
using UnityDebug = UnityEngine.Debug;
using UnityObject = UnityEngine.Object;

namespace Hananoki.BuildAssist {

	public partial class BuildAssistWindow : HEditorWindow {
		const string Window_AssetBundle_Browser = "Window/AssetBundle Browser";
		const string Window_Show_Build_Report = "Window/Show Build Report";

		[MenuItem( "Window/Hananoki/" + Package.name )]
		public static void Open() {
			var window = GetWindow<BuildAssistWindow>();
			window.SetTitle( new GUIContent( Package.name, EditorIcon.settings ) );
		}

		public static BuildAssistWindow s_window;


		public static string notDirectory => S.__NotFoundDirectory__;
		public static string[] s_BundleOptions;
		public static string[] s_CompressionMode;

		public static bool s_changed;

		P.Platform m_currentPlatform;

		bool editMode;
		bool _enableAssetBundle;
		bool _enableBuildReport;

		List<BuildTargetGroup> m_supportBuildTarget;

		IBuildPlatform m_draw;

		Vector2 m_scroll;

		public static void InitLocalize() {
			s_BundleOptions = new string[] { S._ExcludeTypeInformation, S._ForceRebuild, S._IgnoreTypeTreeChanges, S._AppendHash, S._StrictMode, S._DryRunBuild, S._ClearFiles };
			s_CompressionMode = new string[] { S._NoCompression, S._StandardCompression_LZMA_, S._ChunkBasedCompression_LZ4_ };
		}


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


		new public static void Repaint() {
			( (EditorWindow) s_window ).Repaint();
		}



		void ExecuteBuildPackage() {
			EditorApplication.delayCall += BuildPackage;
			B.s_buildProcess = true;
			Repaint();

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
						UnityDebug.LogException( e );
					}
				}
			};
		}



		void ExecuteBuildBundle() {
			EditorApplication.delayCall += BuildBundle;

			void BuildBundle() {
				P.SetBuildParamIndex();
				try {
					BuildCommands.Build( 0x02 );
				}
				catch( Exception e ) {
					UnityDebug.LogException( e );
				}
				EditorApplication.update -= BuildBundle;
			}
		}



		public void MakeDefaultOutputDirectory() {
			var currentParams = P.GetCurrentParams();
			if( !currentParams.outputDirectoryAuto ) return;

			var s = $"{Environment.CurrentDirectory}/{currentParams.buildTarget.ToString()}";
			if( currentParams.outputUseConfiguration ) {
				s += $"/{currentParams.name}";
			}
			currentParams.outputDirectory = DirectoryUtils.Prettyfy( s );
		}


		void MakeDrawBuildTarget() {
			m_draw = null;
			switch( P.i.selectBuildTargetGroup ) {

				case BuildTargetGroup.Standalone:
					m_draw = new BuildPlatformStandard();
					break;

				case BuildTargetGroup.WebGL:
					m_draw = new BuildPlatformWebGL();
					break;

				case BuildTargetGroup.Android:
					m_draw = new BuildPlatformAndroid();
					break;
				case BuildTargetGroup.iOS:
					m_draw = new BuildPlatformIOS();
					break;
				case BuildTargetGroup.Unknown:
					m_draw = new BuildPlatformUnknown();
					break;

				default:
					m_draw = new BuildPlatformDefault();
					break;
			}
		}


		public void SelectItemUpdate() {
			if( m_currentPlatform != null && m_currentPlatform.buildTargetGroup != P.i.selectBuildTargetGroup ) {
				m_currentPlatform = null;
			}
			if( m_currentPlatform == null ) {
				m_currentPlatform = P.GetSelectPlatform();
				//BMS.i.platformList.Add( m_currentPlatform );
			}
			if( m_currentPlatform.parameters.Count == 0 ) {
				m_currentPlatform.AddParams( "Debug" );
				m_currentPlatform.AddParams( "Release" );
				m_currentPlatform.AddParams( "Master" );
				s_changed = true;
			}
			if( P.i.selectParamsIndex < 0 ) {
				P.i.selectParamsIndex = 0;
			}
			else if( m_currentPlatform.parameters.Count <= P.i.selectParamsIndex ) {
				P.i.selectParamsIndex = m_currentPlatform.parameters.Count - 1;
			}
		}


		public bool IsSwitchPlatformAbort() {
			if( UnityEditorUserBuildSettings.activeBuildTargetGroup != P.i.selectBuildTargetGroup ) {
				bool result = EditorUtility.DisplayDialog( SS._Confirm, $"{SS._RequiresSwitchActiveBuildTarget}\n{SS._IsitOK_}", SS._OK, SS._Cancel );
				if( !result ) return true;
			}
			return false;
		}


		void OnFocus() {
			m_scenePaths = EditorBuildSettings.scenes.ToList();

			PB.Load();
			m_leakedScenes = new List<string>();
			foreach( var pp in PB.i.profileList ) {
				m_leakedScenes.AddRange( pp.sceneNanes );
			}
			m_leakedScenes = m_leakedScenes.Distinct().Where( x => m_scenePaths.FindIndex( y => y.path == x ) < 0 ).ToList();
		}


		public void OnEnable() => Init();

		//public void Reinit() => Init();

		public void Init() {
			s_window = this;

			P.Load();
			m_currentPlatform = null;

			m_supportBuildTarget = PlatformUtils.GetSupportList();

			_enableAssetBundle = EditorHelper.HasMenuItem( Window_AssetBundle_Browser );
			_enableBuildReport = EditorHelper.HasMenuItem( Window_Show_Build_Report );


			MakeDrawBuildTarget();
			OnFocus();
		}



		#region OnGUI

		/// <summary>
		/// GUI プロダクト名の描画を行います
		/// </summary>
		public void DrawGUI_PackageName() {
			EditorGUI.BeginChangeCheck();
			P.i.productName = EditorGUILayout.TextField( S._ProductName, P.i.productName );
			if( EditorGUI.EndChangeCheck() ) {
				s_changed = true;
			}
			GUILayout.Space( 8 );
		}



		/// <summary>
		/// GUI 構成の描画を行います
		/// </summary>
		public void DrawGUI_ConfigurationSelect() {
			var currentParams = P.GetCurrentParams();

			using( new GUILayout.HorizontalScope() ) {
				void editBUto() {
					var lsss = EditorGUI.PrefixLabel( GUILayoutUtility.GetLastRect(), EditorHelper.TempContent( S._Configuration ) );

					lsss.x -= Styles.iconEdit.width;
					lsss.x -= 2;
					lsss.width = Styles.iconEdit.width;

					if( HEditorGUI.IconButton( lsss, Styles.iconEdit, 2 ) ) {
						editMode = !editMode;
						Repaint();
						Event.current.Use();
					}
				}

				var lst = m_currentPlatform.parameters.Select( x => x.name ).ToArray();

				if( editMode ) {
					currentParams.name = EditorGUILayout.TextField( S._Configuration, currentParams.name );
					editBUto();
				}
				else {
					EditorGUI.BeginChangeCheck();
					P.i.selectParamsIndex = EditorGUILayout.Popup( S._Configuration, P.i.selectParamsIndex, lst );
					if( EditorGUI.EndChangeCheck() ) s_changed = true;

					var ls = GUILayoutUtility.GetLastRect();

					editBUto();

					if( HEditorGUILayout.IconButton( Styles.iconPlus, 4 ) ) {
						m_currentPlatform.AddParams( $"New ({m_currentPlatform.parameters.Count})" );
						P.i.selectParamsIndex = m_currentPlatform.parameters.Count - 1;
						Event.current.Use();
						s_changed = true;
					}

					if( HEditorGUILayout.IconButton( Styles.iconMinus, 4 ) ) {
						m_currentPlatform.parameters.RemoveAt( P.i.selectParamsIndex );
						P.i.selectParamsIndex = m_currentPlatform.parameters.Count - 1;
						Event.current.Use();
						s_changed = true;
					}
				}
			}


			EditorGUILayout.LabelField( S._BuildSceneSet, PB.i.GetSelectedPopupName( currentParams ), EditorStyles.popup );
			var rrc = EditorGUI.PrefixLabel( GUILayoutUtility.GetLastRect(), S._BuildSceneSet.content() );
			if( EditorHelper.HasMouseClick( rrc ) ) {
				void OnSelect( object context ) {
					currentParams.buildSceneSetIndex = context.ToInt();
					P.Save();
				}
				var m = new GenericMenu();
				m.AddItem( S._UsethestandardBuildSettings, false, OnSelect, -1 );
				m.AddSeparator( "" );
				for( int idx = 0; idx < PB.i.profileList.Count; idx++ ) {
					var p = PB.i.profileList[ idx ];
					m.AddItem( p.profileName, false, OnSelect, idx );
				}
				m.DropDown( rrc );
				Event.current.Use();
			}
		}



		public void DrawGUI_AssetBundle() {
			if( !PB.i.enableAssetBundleBuild ) return;

			var currentParams = P.GetCurrentParams();
			int opt = currentParams.assetBundleOption;

			EditorGUI.BeginChangeCheck();
			using( new GUILayout.VerticalScope( Styles.helpBox ) ) {
				bool fold;
				using( new GUILayout.HorizontalScope() ) {
					fold = HEditorGUILayout.Foldout( E.i.fold.Has( E.FoldAssetBundle ), "Asset Bundle" );
					E.i.fold.Toggle( E.FoldAssetBundle, fold );

					GUILayout.FlexibleSpace();
					bool b7 = HEditorGUILayout.ToggleLeft( S._ClearFiles, opt.Has( P.BUNDLE_OPTION_CLEAR_FILES ) );
					opt.Toggle( P.BUNDLE_OPTION_CLEAR_FILES, b7 );

					var rc = EditorHelper.GetLayout( EditorIcon.settings, Styles.dropDownButton, GUILayout.Width( 80 ), GUILayout.Height( 16 ) );

					HEditorGUI.DropDown( rc, S._Build, Styles.dropDownButton, 18,
						() => {
							if( IsSwitchPlatformAbort() ) return;
							ExecuteBuildBundle();
						},
						() => {
							var m = new GenericMenu();
							if( Directory.Exists( P.i.outputAssetBundleDirectory ) ) {
								m.AddItem( new GUIContent( SS._OpenOutputFolder ), false, () => { EditorUtils.ShellOpenDirectory( P.i.outputAssetBundleDirectory ); } );
							}
							else {
								m.AddDisabledItem( new GUIContent( $"{notDirectory}{P.i.outputAssetBundleDirectory.Replace( "/", "." )}" ) );
							}
							m.DropDown( HEditorGUI.lastRect.PopupRect() );
						} );


					if( _enableAssetBundle ) {
						var r = EditorHelper.GetLayout( EditorIcon.settings, HEditorStyles.iconButton );
						if( HEditorGUI.IconButton( r, EditorIcon.settings, 2 ) ) {
							EditorApplication.ExecuteMenuItem( Window_AssetBundle_Browser );
							Event.current.Use();
						}
					}

					rc = HEditorGUI.lastRect;
					GUI.Label( rc.AddH( -3 ), GUIContent.none, Styles.dopesheetBackground );

				}
				GUILayout.Space( 2 );
				if( fold ) {
					EditorGUI.indentLevel++;
					bool bst = HEditorGUILayout.ToggleLeft( S._CopyingthebuildresultstoStreamingAssets, opt.Has( P.BUNDLE_OPTION_COPY_STREAMINGASSETS ) );
					currentParams.assetBundleCompressionMode = EditorGUILayout.Popup( S._Compression, currentParams.assetBundleCompressionMode, s_CompressionMode, Styles.miniPopup );
					bool b1 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 0 ], opt.Has( P.BUNDLE_OPTION_EXCLUDETYPEINFORMATION ) );
					bool b2 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 1 ], opt.Has( P.BUNDLE_OPTION_FORCEREBUILD ) );
					bool b3 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 2 ], opt.Has( P.BUNDLE_OPTION_IGNORETYPETREECHANGES ) );
					bool b4 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 3 ], opt.Has( P.BUNDLE_OPTION_APPENDHASH ) );
					bool b5 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 4 ], opt.Has( P.BUNDLE_OPTION_STRICTMODE ) );
					bool b6 = HEditorGUILayout.ToggleLeft( s_BundleOptions[ 5 ], opt.Has( P.BUNDLE_OPTION_DRYRUNBUILD ) );

					opt.Toggle( P.BUNDLE_OPTION_COPY_STREAMINGASSETS, bst );
					opt.Toggle( P.BUNDLE_OPTION_EXCLUDETYPEINFORMATION, b1 );
					opt.Toggle( P.BUNDLE_OPTION_FORCEREBUILD, b2 );
					opt.Toggle( P.BUNDLE_OPTION_IGNORETYPETREECHANGES, b3 );
					opt.Toggle( P.BUNDLE_OPTION_APPENDHASH, b4 );
					opt.Toggle( P.BUNDLE_OPTION_STRICTMODE, b5 );
					opt.Toggle( P.BUNDLE_OPTION_DRYRUNBUILD, b6 );
					GUILayout.Space( 2 );

					EditorGUI.indentLevel--;
				}
			}
			if( EditorGUI.EndChangeCheck() ) {
				currentParams.assetBundleOption = opt;
				s_changed = true;
			}
		}



		/// <summary>
		/// GUI Build Settingsの描画を行います
		/// </summary>
		public void DrawGUI_BuildSettings() {
			var currentParams = P.GetCurrentParams();

			EditorGUI.BeginChangeCheck();

			bool fold;
			using( new GUILayout.VerticalScope( Styles.helpBox ) ) {
				using( new GUILayout.HorizontalScope() ) {
					fold = HEditorGUILayout.Foldout( E.i.fold.Has( E.FoldBuildSettings ), "Build Settings" );
					E.i.fold.Toggle( E.FoldBuildSettings, fold );
					GUILayout.FlexibleSpace();
					//EditorGUI.DrawRect( GUILayoutUtility.GetLastRect(), new Color( 0, 0, 1, 0.2f ) );

					var r = EditorHelper.GetLayout( EditorIcon.settings, HEditorStyles.iconButton );
					if( HEditorGUI.IconButton( r, EditorIcon.settings, B.kBuildSettings, 1 ) ) {
						UnityEditorMenu.File_Build_Settings();
					}
				}

				if( fold ) {
					EditorGUI.indentLevel++;
					currentParams.development = EditorGUILayout.Toggle( S._DevelopmentBuild, currentParams.development );

					if( P.i.selectBuildTargetGroup == BuildTargetGroup.Android ) {
						currentParams.buildAppBundle = EditorGUILayout.Toggle( S._BuildAppBundle_GooglePlay_, currentParams.buildAppBundle );
					}

					if( P.i.selectBuildTargetGroup == BuildTargetGroup.Standalone
						|| P.i.selectBuildTargetGroup == BuildTargetGroup.Android ) {
						string[] ss = { "Default", "LZ4", "LZ4HC" };
						switch( EditorGUILayout.Popup( S._CompressionMethod, currentParams.compression.ToIndex(), ss, Styles.miniPopup ) ) {
							case 0:
								currentParams.compression = Compression.None;
								break;
							case 1:
								currentParams.compression = Compression.Lz4;
								break;
							case 2:
								currentParams.compression = Compression.Lz4HC;
								break;
						}
					}

					EditorGUI.indentLevel--;
					GUILayout.Space( 2 );
				}
			}

			if( EditorGUI.EndChangeCheck() ) {
				s_changed = true;
			}
		}



		/// <summary>
		/// GUI Player Settingsの描画を行います
		/// </summary>
		public void DrawGUI_PlayerSettings() {
			var currentParams = P.GetCurrentParams();

			int opt = currentParams.platformOption;
			bool fold;


			using( new GUILayout.VerticalScope( Styles.helpBox ) ) {
				using( new GUILayout.HorizontalScope() ) {
					EditorGUI.BeginChangeCheck();

					fold = HEditorGUILayout.Foldout( E.i.fold.Has( E.FoldPlatform ), "Player Settings" );
					E.i.fold.Toggle( E.FoldPlatform, fold );
					if( EditorGUI.EndChangeCheck() ) s_changed = true;

					GUILayout.FlexibleSpace();

					var r = EditorHelper.GetLayout( EditorIcon.settings, HEditorStyles.iconButton );

					if( HEditorGUI.IconButton( r, EditorIcon.settings, 1 ) ) {
						if( PB.i.enableOldStyleProjectSettings ) {
							Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityObject>( AssetDatabase.GUIDToAssetPath( "00000000000000004000000000000000" ) );
							HEditorWindow.Find( UnityTypes.InspectorWindow )?.Focus();
						}
						else {
							UnityEditorMenu.Edit_Project_Settings();
						}
					}
				}

				EditorGUI.BeginChangeCheck();
				if( fold ) {
					EditorGUI.indentLevel++;

					if( P.i.selectBuildTargetGroup == BuildTargetGroup.Standalone
						|| P.i.selectBuildTargetGroup == BuildTargetGroup.Android ) {
						currentParams.scriptingBackend = (ScriptingImplementation) EditorGUILayout.Popup( S._ScriptingBackend, (int) currentParams.scriptingBackend, B.kScriptingBackendNames );
					}

					bool backend = false;
					if( P.i.selectBuildTargetGroup == BuildTargetGroup.WebGL ) {
					}
					else if( currentParams.scriptingBackend == ScriptingImplementation.Mono2x ) {
						backend = true;
					}
					using( new EditorGUI.DisabledGroupScope( backend ) ) {
						currentParams.il2CppCompilerConfiguration = (Il2CppCompilerConfiguration) EditorGUILayout.EnumPopup( S._C__CompilerConfiguration, currentParams.il2CppCompilerConfiguration );
					}
					EditorGUILayout.LabelField( $"{S._ScriptingDefineSymbols} ({S._Applybuildonly})" );


					using( new GUILayout.HorizontalScope() ) {
						currentParams.scriptingDefineSymbols = EditorGUILayout.TextField( currentParams.scriptingDefineSymbols );
						var mm = R.Method( "GetSymbolList", "Hananoki.SymbolSettings.SettingsProject", "Hananoki.SymbolSettings.Editor" );
						if( mm != null ) {
							var tc = GUILayoutUtility.GetRect( EditorHelper.TempContent( Styles.iconPlus ), HEditorStyles.iconButton, GUILayout.Width( 16 ), GUILayout.Height( 16 ) );

							if( HEditorGUI.IconButton( tc, Styles.iconPlus, 3 ) ) {
								GUI.FocusControl( "" );
								void add( object obj ) {
									var s = obj as string;
									var ss = currentParams.scriptingDefineSymbols.Split( ';' );
									ArrayUtility.Add( ref ss, s );
									currentParams.scriptingDefineSymbols = string.Join( ";", ss.Where( x => !x.IsEmpty() ).Distinct().ToArray() );
								}

								var hoge = mm.Invoke( null, null );
								var lst = new List<string>();
								var m = new GenericMenu();
								foreach( var s in hoge.GetField<string[]>( "project" ) ) {
									m.AddItem( "Project/" + s, false, add, s );
								}
								foreach( var s in hoge.GetField<string[]>( "editor" ) ) {
									m.AddItem( "Editor/" + s, false, add, s );
								}
								m.DropDown( tc );
							}
						}
					}
					EditorGUILayout.LabelField( $"{S._ScriptingDefineSymbols} ({S._Current})" );
					EditorGUI.BeginDisabledGroup( true );
					EditorGUILayout.TextField( B.scriptingDefineSymbols );
					EditorGUI.EndDisabledGroup();

					EditorGUI.indentLevel--;
					GUILayout.Space( 4 );
				}
				if( EditorGUI.EndChangeCheck() ) {
					currentParams.platformOption = opt;
					s_changed = true;
				}
			}

		}



		/// <summary>
		/// 
		/// </summary>
		public void DrawGUI_OutputDirectory() {
			var currentParams = P.GetCurrentParams();
			bool fold;
			bool _outputDirectoryAuto = currentParams.outputDirectoryAuto;
			bool _outputUseConfiguration = currentParams.outputUseConfiguration;

			using( new GUILayout.VerticalScope( Styles.helpBox ) ) {
				using( new GUILayout.HorizontalScope() ) {
					EditorGUI.BeginChangeCheck();
					fold = HEditorGUILayout.Foldout( E.i.fold.Has( E.FoldOutputDirectory ), "Output Directory" );
					E.i.fold.Toggle( E.FoldOutputDirectory, fold );
					if( EditorGUI.EndChangeCheck() ) s_changed = true;

					GUILayout.FlexibleSpace();
					var r = GUILayoutUtility.GetRect( 20, 18 );
				}


				if( fold ) {
					EditorGUI.BeginChangeCheck();
					EditorGUI.indentLevel++;

					EditorGUI.BeginChangeCheck();
					using( new GUILayout.HorizontalScope() ) {
						_outputDirectoryAuto = HEditorGUILayout.ToggleLeft( S._Autosetting, _outputDirectoryAuto );
						using( new EditorGUI.DisabledGroupScope( !_outputDirectoryAuto ) ) {
							_outputUseConfiguration = HEditorGUILayout.ToggleLeft( S._UseConfigurationName, _outputUseConfiguration );
						}
						GUILayout.FlexibleSpace();
					}
					if( EditorGUI.EndChangeCheck() ) {
						if( _outputDirectoryAuto ) {
							currentParams.outputDirectoryAuto = _outputDirectoryAuto;
							currentParams.outputUseConfiguration = _outputUseConfiguration;
							MakeDefaultOutputDirectory();
						}
						s_changed = true;
					}

					string _outputDirectory = currentParams.outputDirectory;
					var rect = GUILayoutUtility.GetRect( _outputDirectory.content(), EditorStyles.label, GUILayout.Height( 16 ) );

					var r1 = rect;
					r1.width -= 16;
					EditorGUI.LabelField( r1, SS._Directory, _outputDirectory );
					if( !_outputDirectoryAuto ) {
						var r2 = rect;
						r2.x = r1.xMax;
						r2.width = 16;
						if( HEditorGUI.IconButton( r2, Icon.Get( "Folder Icon" ) ) ) {
							var _path = EditorUtility.OpenFolderPanel( S._SelectOutputDirectory, _outputDirectory, "" );
							if( !string.IsNullOrEmpty( _path ) ) {
								_outputDirectory = _path;
								s_changed = true;
							}
						}
					}

					EditorGUI.indentLevel--;
					if( EditorGUI.EndChangeCheck() || s_changed ) {
						currentParams.outputDirectoryAuto = _outputDirectoryAuto;
						currentParams.outputUseConfiguration = _outputUseConfiguration;
						currentParams.outputDirectory = _outputDirectory;
						s_changed = true;
					}
				}

			}
		}



		/// <summary>
		/// GUI 下部のビルド開始ボタン等の描画を行います
		/// </summary>
		public void DrawGUI_Bottom() {
			var currentParams = P.GetCurrentParams();

			using( new GUILayout.HorizontalScope() ) {
				GUILayout.FlexibleSpace();
				GUILayout.Label( P.currentOutputPackageFullName );
			}
			using( new GUILayout.HorizontalScope() ) {
				GUILayout.FlexibleSpace();

				using( new EditorGUI.DisabledGroupScope( !currentParams.development ) ) {
					bool b2 = HEditorGUILayout.SessionToggleLeft( S._ConnectProfiler, S._ProfilingisonlyenabledinaDevelopmentPlayer, E.connectProfiler.Value );
					E.connectProfiler.Value = b2;
				}
				bool b3 = HEditorGUILayout.SessionToggleLeft( S._AutoRunPlayer, E.autoRunPlayer.Value );
				E.autoRunPlayer.Value = b3;
				//GUILayout.Space( 16 );

				var rc = GUILayoutUtility.GetRect( EditorHelper.TempContent( S._SwitchPlatform, EditorIcon.warning ), GUI.skin.button, GUILayout.Width( 150 ) );


				void OnDropAction() {
					var m = new GenericMenu();
					if( Directory.Exists( P.currentOutputPackageDirectory ) ) {
						m.AddItem( SS._OpenOutputFolder.content(), false, () => {
							EditorUtils.ShellOpenDirectory( P.currentOutputPackageDirectory );
						} );
					}
					else {
						m.AddDisabledItem( $"{notDirectory}{P.currentOutputPackageDirectory.TryReplace( "/", "." )}]".content() );
					}
					m.AddSeparator( "" );
					if( PB.i.enableAssetBundleBuild ) {
						m.AddItem( S._BuildAssetBundletogether.content(), currentParams.buildAssetBundlesTogether, () => { currentParams.buildAssetBundlesTogether = !currentParams.buildAssetBundlesTogether; } );
					}
					if( UnitySymbol.Has( "UNITY_EDITOR_WIN" ) ) {
						m.AddItem( S._CreateabuildBATfile, false, () => {
							var tname = $"{UnityEditorUserBuildSettings.activeBuildTargetGroup.ToString()}_{currentParams.name}";
							EditorHelper.WriteFile( $"Build_{tname}.bat", b => {
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

				if( UnityEditorUserBuildSettings.activeBuildTargetGroup != P.i.selectBuildTargetGroup ) {
					HEditorGUI.DropDown( rc, EditorHelper.TempContent( S._SwitchPlatform, EditorIcon.warning ), Styles.dropDownButton, 20,
						() => PlatformUtils.SwitchActiveBuildTarget( P.i.selectBuildTargetGroup ),
						OnDropAction
						);

				}
				else {
					HEditorGUI.DropDown( rc, E.autoRunPlayer.Value ? S._BuildAndRun : S._Build, Styles.dropDownButton, 20,
							() => {
								if( IsSwitchPlatformAbort() ) return;
								ExecuteBuildPackage();
							},
							OnDropAction );


				}
				rc = HEditorGUI.lastRect;
				GUI.Label( rc.AddH( -3 ), GUIContent.none, Styles.dopesheetBackground );
				if( UnitySymbol.Has( "UNITY_EDITOR_WIN" ) ) {
					if( P.GetSelectPlatform().buildTargetGroup == BuildTargetGroup.Standalone ) {
						if( File.Exists( P.currentOutputPackageFullName ) ) {
							if( HEditorGUILayout.ImageButton( EditorIcon.playButton, GUILayout.Width( 30 ) ) ) {
								System.Diagnostics.Process.Start( P.currentOutputPackageFullName );
							}
						}
					}
					if( P.GetSelectPlatform().buildTargetGroup == BuildTargetGroup.WebGL ) {
						if( File.Exists( $"{P.currentOutputPackageFullName}/index.html" ) ) {
							if( HEditorGUILayout.ImageButton( EditorIcon.playButton, GUILayout.Width( 30 ) ) ) {
								//System.Diagnostics.Process.Start( $"{P.currentOutputPackageFullName}/index.html" );
								Application.OpenURL( "http://localhost/" );
							}
						}
					}
				}
			}
			GUILayout.Space( 10 );
		}



		void DrawGUI() {
			s_changed = false;
			SelectItemUpdate();

			var w = position.width;
			var h = position.height;

			var drawArea = new Rect( 4, EditorGUIUtility.singleLineHeight + 1 + 4, w - 4 - 4, position.height - EditorGUIUtility.singleLineHeight - 12 );
			using( new GUILayout.AreaScope( drawArea ) )
			using( var sc = new GUILayout.ScrollViewScope( m_scroll ) ) {
				m_scroll = sc.scrollPosition;

				if( P.i.selectScene ) {
					SceneSelectTabOnGUI();
					return;
				}

				m_draw?.Draw( this );

				if( s_changed ) {
					P.Save();
				}
			}
		}



		/// <summary>
		/// ツールバーを描画します
		/// </summary>
		void DrawToolBar() {
			GUILayout.BeginHorizontal( Styles.toolbar );

			EditorGUI.BeginChangeCheck();
			P.i.selectScene = HGUILayoutToolbar.Toggle( P.i.selectScene, EditorHelper.TempContent( "Scenes in Build" ), Styles.toolbarButtonBold );
			if( EditorGUI.EndChangeCheck() ) {
				P.Save();
			}

			var lst = m_supportBuildTarget.Where( x => P.GetPlatform( x ).enable ).ToArray();

			var reo = Styles.toolbarbutton.padding;
			var active = UnityEditorUserBuildSettings.activeBuildTargetGroup;

			for( int i = 0; i < lst.Length; i++ ) {
				var s = lst[ i ];
				EditorGUI.BeginChangeCheck();
				var style = Styles.toolbarbutton;
				if( active == s ) {
					style = Styles.toolbarbuttonActive;
				}
				var cont = EditorHelper.TempContent( s.GetShortName(), s.Icon() );
				var size = style.CalcSize( cont );
				size.x -= 8;
				EditorGUI.BeginChangeCheck();
				HGUILayoutToolbar.Toggle( P.i.selectBuildTargetGroup == s && P.i.selectScene == false, cont, style, GUILayout.Width( size.x ) );
				if( EditorGUI.EndChangeCheck() ) {
					P.i.selectScene = false;
				}
				if( active == s ) {
					var rc = GUILayoutUtility.GetLastRect();
					rc.x -= 4;
					rc = rc.AlignCenterH( 16 );
					if( UnitySymbol.Has( "UNITY_2019_3_OR_NEWER" ) ) {
						rc.y += 1;
					}
					GUI.DrawTexture( rc.AlignR( 16 ), EditorIcon.buildsettings_editor_small, ScaleMode.ScaleToFit );
				}

				if( EditorGUI.EndChangeCheck() ) {
					P.i.selectBuildTargetGroup = s;
					P.Save();
					ChangeActiveTarget();
				}
			}

			GUILayout.FlexibleSpace();
			if( _enableBuildReport ) {
				if( HGUILayoutToolbar.Button( "Build Report" ) ) {
					EditorApplication.ExecuteMenuItem( Window_Show_Build_Report );
				}
			}
			if( HGUILayoutToolbar.Button( EditorIcon.settings ) ) {
				SettingsProjectWindow.Open();
			}

			GUILayout.EndHorizontal();
		}



		void OnGUI() {
			Styles.Init();

			try {
				using( new EditorGUI.DisabledGroupScope( EditorHelper.IsWait() || B.s_buildProcess ) ) {
					DrawToolBar();
					GUILayout.Space( 8 );
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

