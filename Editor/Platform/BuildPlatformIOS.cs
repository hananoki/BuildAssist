
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Hananoki.BuildAssist {
	public class BuildPlatformIOS : IBuildPlatform {
		public BuildReport BuildPackage( string[] scenes ) { return null; }
		public void Draw( BuildAssistWindow window ) {
			EditorGUILayout.HelpBox( "I don't have a mac.", MessageType.Warning );
		}
	}
}

