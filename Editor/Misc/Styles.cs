
using HananokiRuntime;
using UnityEditor;
using UnityEngine;

namespace HananokiEditor.BuildAssist {
	public class Styles {
		public static GUIStyle icon => s_styles.Icon;
		public static GUIStyle dropDownButton => s_styles.DropDownButton;
		public static GUIStyle miniPopup => s_styles.MiniPopup;

		public static GUIStyle toolbarbutton => s_styles.Toolbarbutton;
		public static GUIStyle toolbarbuttonActive => s_styles.ToolbarbuttonActive;

		public static GUIStyle projectBrowserHeaderBgMiddle => s_styles.ProjectBrowserHeaderBgMiddle;
		public static GUIStyle labelAndIcon => s_styles.LabelAndIcon;

		public static GUIStyle errorLabel => s_styles.ErrorLabel;


		public GUIStyle Toolbarbutton;
		public GUIStyle ToolbarbuttonActive;

		public GUIStyle ProjectBrowserHeaderBgMiddle;

		public GUIStyle LabelAndIcon;
		public GUIStyle Minibutton;
		public GUIStyle MiniPopup;
		public GUIStyle DropDownButton;
		public GUIStyle Foldout;
		public GUIStyle Icon;


		public GUIStyle ErrorLabel;

		public Styles() {
			LabelAndIcon = new GUIStyle( EditorStyles.label );
			LabelAndIcon.fixedHeight = 16;

			ProjectBrowserHeaderBgMiddle = new GUIStyle( "ProjectBrowserHeaderBgTop" );
			//DopesheetBackground = new GUIStyle( "DopesheetBackground" );

			//Toolbar = new GUIStyle( EditorStyles.toolbar );

			Foldout = new GUIStyle( "Foldout" );
			Foldout.font = EditorStyles.boldLabel.font;
			Foldout.alignment = TextAnchor.MiddleLeft;
			if( UnitySymbol.Has( "UNITY_2018_3_OR_NEWER" ) ) {
				Foldout.margin = new RectOffset( Foldout.margin.left, Foldout.margin.right, 0, 0 );
			}
			Toolbarbutton = new GUIStyle( EditorStyles.toolbarButton );
			Toolbarbutton.alignment = TextAnchor.MiddleCenter;
			Toolbarbutton.padding = new RectOffset( 2, 2, 2, 2 );
			//Toolbarbutton.margin = new RectOffset( 2, 2, 2, 2 );
			ToolbarbuttonActive = new GUIStyle( Toolbarbutton );
			ToolbarbuttonActive.alignment = TextAnchor.MiddleCenter;
			ToolbarbuttonActive.padding.right += 20;// = new RectOffset( ToolbarbuttonActive.padding.left );



			Minibutton = new GUIStyle( "minibutton" );
			MiniPopup = new GUIStyle( "MiniPopup" );

			DropDownButton = new GUIStyle( "Button" );
			DropDownButton.fixedHeight = 0;
			if( UnitySymbol.Has( "UNITY_2018_3_OR_NEWER" ) ) {
				DropDownButton.padding = new RectOffset( DropDownButton.padding.left, 16, 0, 0 );
			}
			Icon = new GUIStyle();
			Icon.stretchWidth = false;
			Icon.alignment = TextAnchor.MiddleCenter;
			Icon.margin = new RectOffset( 0, 0, 4, 0 );

			ErrorLabel = new GUIStyle( EditorStyles.label );
			ErrorLabel.normal.textColor = ColorUtils.RGB( 177, 12, 12 );
			ErrorLabel.fontStyle = FontStyle.Bold;
		}

		static Styles s_styles;

		public static void Init() {
			if( s_styles == null ) {
				s_styles = new Styles();
			}
		}
	}
}
