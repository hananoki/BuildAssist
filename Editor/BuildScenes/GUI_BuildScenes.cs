using UnityEngine;
using UnityReflection;
using PB = HananokiEditor.BuildAssist.SettingsProjectBuildSceneSet;

namespace HananokiEditor.BuildAssist {
	public class GUI_BuildScenes {

		UnityEditorSplitterState m_HorizontalSplitter;

		TreeView_BuildScenesL m_treeView;


		/////////////////////////////////////////
		public GUI_BuildScenes() {
			m_HorizontalSplitter = new UnityEditorSplitterState( 0.20f, 0.80f );

			m_treeView = new TreeView_BuildScenesL();
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

			m_treeView.DrawLayoutGUI();

			void _add() {
				PB.i.profileList.Add( new PB.Profile( $"BuildScene ({PB.i.profileList.Count})" ) );
				PB.Save();
				m_treeView.RegisterFiles();
			}
			void _minus() {
				PB.i.profileList.Remove( m_treeView.currentItem.profile );
				PB.Save();

				m_treeView.RegisterFiles();
			}
		}


		/////////////////////////////////////////
		void DrawRightPane() {
			HGUIToolbar.Begin();
			if( HGUIToolbar.Button( EditorIcon.settings ) ) UnityEditorMenu.File_Build_Settings();
			//if( HGUIToolbar.Button( EditorIcon.search_icon ) ) ProjectBrowserUtils.SetSearch( "t:Scene" );
			GUILayout.FlexibleSpace();
			HGUIToolbar.End();

			m_treeView.DrawItem();
		}
	}
}
