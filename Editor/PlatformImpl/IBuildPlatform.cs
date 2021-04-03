
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HananokiEditor.BuildAssist {
	using UI;

	public interface IBuildPlatform {

		BuildReport BuildPackage( string[] scenes );
	}


	public abstract class BuildPropertyBase {

		public virtual void CheckError() { }

		public virtual void DrawErrorReport( Rect rect ) { }

		public virtual List<TreeView_BuildPropertyR.Item> CreateItemList() { return null; }



		public void MessageError( ref Rect rect, string message ) {
			GUI.Label( rect, EditorHelper.TempContent( message, EditorIcon.error ), Styles.errorLabel );
			rect.y += EditorGUIUtility.singleLineHeight;
		}


		public void MessageInfo( ref Rect rect, string message ) {
			GUI.Label( rect, EditorHelper.TempContent( message, EditorIcon.info ), EditorStyles.boldLabel );
			rect.y += EditorGUIUtility.singleLineHeight;
		}

		static int id = 1;
		protected TreeView_BuildPropertyR.Item CreateItem( PropertyItem p, int depth = 0 ) {
			return new TreeView_BuildPropertyR.Item {
				id = id++,
				displayName = p.name,
				uiDraw = p,
				depth = depth,
			};
		}
	}
}
