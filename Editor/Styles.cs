
using UnityEditor;
using UnityEngine;

namespace Hananoki.BuildAssist {
	public class Styles {
		public static GUIStyle icon => s_styles.Icon;
		public static GUIStyle helpBox => s_styles.HelpBox;
		public static GUIStyle dropDownButton => s_styles.DropDownButton;
		//public static GUIStyle minibutton => s_styles.Minibutton;
		public static GUIStyle miniPopup => s_styles.MiniPopup;

		public static GUIStyle toolbar => s_styles.Toolbar;
		public static GUIStyle toolbarbutton => s_styles.Toolbarbutton;
		public static GUIStyle toolbarbuttonActive => s_styles.ToolbarbuttonActive;
		public static GUIStyle toolbarButtonBold => s_styles.ToolbarButtonBold;

		public static GUIStyle projectBrowserHeaderBgMiddle => s_styles.ProjectBrowserHeaderBgMiddle;
		public static GUIStyle dopesheetBackground => s_styles.DopesheetBackground;
		public static GUIStyle boldLabel => EditorStyles.boldLabel;
		public static GUIStyle labelAndIcon => s_styles.LabelAndIcon;

		public static Texture2D iconAllowUp => Shared.Icon.Get( "$AllowUp" );
		public static Texture2D iconAllowDown => Shared.Icon.Get( "$AllowDown" );
		public static Texture2D iconMinus => EditorIcon.minus;
		public static Texture2D iconPlus => EditorIcon.plus;
		public static Texture2D iconEdit => s_styles.IconEdit;
		//public static Texture2D iconSettings => Shared.Icon.Get( "$Settings" );
		public static Texture2D iconHelp => Hananoki.Icon.Get( "_Help" );
		

		public GUIStyle Toolbar;
		public GUIStyle Toolbarbutton;
		public GUIStyle ToolbarbuttonActive;
		public GUIStyle ToolbarButtonBold;

		public GUIStyle ProjectBrowserHeaderBgMiddle;
		public GUIStyle DopesheetBackground;

		public GUIStyle LabelAndIcon;
		public GUIStyle HelpBox;
		public GUIStyle Minibutton;
		public GUIStyle MiniPopup;
		public GUIStyle BoldLabel;
		public GUIStyle DropDownButton;
		public GUIStyle Foldout;
		public GUIStyle Icon;


		public Texture2D IconEdit;

		public Styles() {
			LabelAndIcon = new GUIStyle( EditorStyles.label);
			LabelAndIcon.fixedHeight = 16;

			ProjectBrowserHeaderBgMiddle = new GUIStyle( "ProjectBrowserHeaderBgTop" );
			DopesheetBackground = new GUIStyle( "DopesheetBackground" );

			Toolbar = new GUIStyle( EditorStyles.toolbar );

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

			ToolbarButtonBold = new GUIStyle( Toolbarbutton );
			ToolbarButtonBold.fontStyle = FontStyle.Bold;
			ToolbarButtonBold.padding.left = 8;
			ToolbarButtonBold.padding.right = 8;

			HelpBox = new GUIStyle( EditorStyles.helpBox );

			Minibutton = new GUIStyle( "minibutton" );
			MiniPopup = new GUIStyle( "MiniPopup" );
			BoldLabel = new GUIStyle( "BoldLabel" );

			DropDownButton = new GUIStyle( "Button" );
			DropDownButton.fixedHeight = 0;
			if( UnitySymbol.Has( "UNITY_2018_3_OR_NEWER" ) ) {
				DropDownButton.padding = new RectOffset( DropDownButton.padding.left, 16, 0, 0 );
			}
			Icon = new GUIStyle();
			Icon.stretchWidth = false;
			Icon.alignment = TextAnchor.MiddleCenter;
			Icon.margin = new RectOffset( 0, 0, 4, 0 );

			IconEdit = EditorHelper.LoadIcon( "editicon.sml" );

		}

		static Styles s_styles;

		public static void Init() {
			if( s_styles == null ) {
				s_styles = new Styles();
			}
		}
	}
}
