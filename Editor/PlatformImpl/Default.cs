using HananokiEditor.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityReflection;
using P = HananokiEditor.BuildAssist.SettingsProject;
using PB = HananokiEditor.BuildAssist.SettingsProjectBuildSceneSet;
using SS = HananokiEditor.SharedModule.S;
using UnityObject = UnityEngine.Object;


namespace HananokiEditor.BuildAssist {
	using UI;

	public class BuildProperty_Default : BuildPropertyBase {
		//public virtual void CheckError() { }
		//public virtual void DrawErrorReport( Rect rect ) { }
		public override List<TreeView_BuildPropertyR.Item> CreateItemList() {
			var lst = new List<TreeView_BuildPropertyR.Item>();
			lst.Add( CreateItem( new ビルドシーンセット() ) );
			return lst;
		}
	}
}



namespace HananokiEditor.BuildAssist.UI {

	public interface PropertyItem {
		/// <summary>
		/// 設定を無効にする場合はtrueを返す
		/// </summary>
		/// <param name="currentParams"></param>
		/// <returns></returns>
		bool isDisable( P.Params currentParams );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="currentParams"></param>
		/// <returns>変化があったらtrue</returns>
		bool UIDraw( Rect rect, P.Params currentParams );

		string name { get; }
	}


	/////////////////////////////////////////

	public class Group_BuildSettings : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => "Build Settings";

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			if( !HEditorGUI.IconButton( rect.W( 16 ), EditorIcon.settings ) ) return false;

			UnityEditorMenu.File_Build_Settings();

			return false;
		}
	}

	/////////////////////////////////////////

	public class Group_PlayerSettings : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => "Player Settings";

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			if( !HEditorGUI.IconButton( rect.W( 16 ), EditorIcon.settings ) ) return false;

			if( PB.i.enableOldStyleProjectSettings ) {
				Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityObject>( AssetDatabase.GUIDToAssetPath( "00000000000000004000000000000000" ) );
				EditorWindowUtils.Find( UnityTypes.UnityEditor_InspectorWindow )?.Focus();
			}
			else {
				//UnityEditorMenu.Edit_Project_Settings();
				UnityEditorSettingsWindow.Show( SettingsScope.Project, "Project/Player" );
			}

			return false;
		}
	}

	/////////////////////////////////////////

	public class Group_OutputDirectory : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => "Output Directory";

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			//if( !HEditorGUI.IconButton( rect.W( 16 ), EditorIcon.settings ) ) return false;

			//UnityEditorMenu.File_Build_Settings();

			return false;
		}
	}




	/////////////////////////////////////////

	public class ビルドシーンセット : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._BuildSceneSet;

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			EditorGUI.LabelField( rect, PB.i.GetSelectedPopupName( currentParams ), EditorStyles.popup );

			if( EditorHelper.HasMouseClick( rect ) ) {
				void OnSelect( object context ) {
					currentParams.buildSceneSetIndex = (int) context;
					P.Save();
				}
				var m = new GenericMenu();
				m.AddItem( S._UsethestandardBuildSettings, false, OnSelect, -1 );
				m.AddSeparator( "" );
				for( int idx = 0; idx < PB.i.profileList.Count; idx++ ) {
					var p = PB.i.profileList[ idx ];
					m.AddItem( p.profileName, false, OnSelect, idx );
				}
				m.DropDownPopupRect( rect );
			}
			return false;
		}
	}

	/////////////////////////////////////////

	public class 開発ビルド : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._DevelopmentBuild;

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			ScopeChange.Begin();
			currentParams.development = EditorGUI.Toggle( rect.W( 16 ), currentParams.development );
			if( ScopeChange.End() ) {
				P.Save();
				return true;
			}
			return false;
		}
	}

	/////////////////////////////////////////

	public class 圧縮方法 : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._CompressionMethod;

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			string[] ss = { "Default", "LZ4", "LZ4HC" };
			var index = currentParams.compression.ToIndex();
			ScopeChange.Begin();
			var _index = EditorGUI.Popup( rect, index, ss, Styles.miniPopup );
			if( ScopeChange.End() ) {
				switch( _index ) {
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
				P.Save();
				return true;
			}
			return false;
		}
	}

	/////////////////////////////////////////

	public class アーキテクチャ : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._Architecture;

		List<(GUIContent, BuildTarget)> m_arch;


		public bool UIDraw( Rect rect, P.Params currentParams ) {
			if( m_arch == null ) {
				var dic = DesktopStandaloneBuildWindowExtension.GetArchitecturesForPlatform( currentParams.buildTarget );
				if( dic != null ) {
					m_arch = new List<(GUIContent, BuildTarget)>();
					foreach( var p in dic.Keys ) {
						m_arch.Add( ((GUIContent) p, (BuildTarget) dic[ p ]) );
					}
				}
				else {
					return false;
				}
			}
			if( m_arch.Count == 0 ) return false;


			int index = m_arch.FindIndex( x => x.Item2 == currentParams.buildTarget );
			ScopeChange.Begin();
			index = EditorGUI.Popup( rect, index, m_arch.Select( x => x.Item1 ).ToArray() );
			if( ScopeChange.End() ) {
				currentParams.buildTarget = m_arch[ index ].Item2;
				Utils.MakeDefaultOutputDirectory();
				Utils.s_changed = true;
				return true;
			}
			return false;
		}
	}


	/////////////////////////////////////////

	public class スクリプティングバックエンド : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._ScriptingBackend;

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			ScopeChange.Begin();
			currentParams.scriptingBackend = (ScriptingImplementation) EditorGUI.Popup( rect, (int) currentParams.scriptingBackend, B.kScriptingBackendNames );
			if( ScopeChange.End() ) {
				P.Save();
				return true;
			}
			return false;
		}
	}

	/////////////////////////////////////////

	public class CPPコンパイラ設定 : PropertyItem {
		public bool isDisable( P.Params currentParams ) {
			// WebGLはIL2CPP固定なのでチェックしない
			if( P.i.selectBuildTargetGroup == BuildTargetGroup.WebGL ) return false;

			return currentParams.scriptingBackend == ScriptingImplementation.Mono2x;
		}

		public string name => S._C__CompilerConfiguration;

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			ScopeChange.Begin();
			currentParams.il2CppCompilerConfiguration = (Il2CppCompilerConfiguration) EditorGUI.EnumPopup( rect, currentParams.il2CppCompilerConfiguration );
			if( ScopeChange.End() ) {
				P.Save();
				return true;
			}
			return false;
		}
	}

	/////////////////////////////////////////

	public class 自動設定 : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => S._Autosetting;

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			bool _outputDirectoryAuto = currentParams.outputDirectoryAuto;
			bool _outputUseConfiguration = currentParams.outputUseConfiguration;
			ScopeChange.Begin();
			_outputDirectoryAuto = EditorGUI.Toggle( rect.W( 16 ), _outputDirectoryAuto );
			if( ScopeChange.End() ) {
				currentParams.outputDirectoryAuto = _outputDirectoryAuto;
				Utils.MakeDefaultOutputDirectory();
				P.Save();
				return true;
			}
			return false;
		}
	}

	/////////////////////////////////////////

	public class 構成の値を使用する : PropertyItem {
		public bool isDisable( P.Params currentParams ) => !currentParams.outputDirectoryAuto;
		public string name => S._UseConfigurationName;

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			bool _outputDirectoryAuto = currentParams.outputDirectoryAuto;
			bool _outputUseConfiguration = currentParams.outputUseConfiguration;
			ScopeChange.Begin();
			_outputUseConfiguration = EditorGUI.Toggle( rect.W( 16 ), _outputUseConfiguration );
			if( ScopeChange.End() ) {
				currentParams.outputUseConfiguration = _outputUseConfiguration;
				Utils.MakeDefaultOutputDirectory();
				P.Save();
				return true;
			}
			return false;
		}
	}

	/////////////////////////////////////////

	public class 出力ディレクトリ : PropertyItem {
		public bool isDisable( P.Params currentParams ) => false;
		public string name => SS._Directory;

		public bool UIDraw( Rect rect, P.Params currentParams ) {
			string _outputDirectory = currentParams.outputDirectory;
			EditorGUI.LabelField( rect, _outputDirectory );

			if( !currentParams.outputDirectoryAuto ) {
				var r2 = rect.AlignR( 16 );
				r2.x -= 4;
				if( HEditorGUI.IconButton( r2, EditorIcon.folder ) ) {
					var _path = EditorUtility.OpenFolderPanel( S._SelectOutputDirectory, _outputDirectory, "" );
					if( !string.IsNullOrEmpty( _path ) ) {
						currentParams.outputDirectory = _path;
						P.Save();
						return true;
					}
				}
			}
			return false;
		}
	}
}
