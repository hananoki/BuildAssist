
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace HananokiEditor.BuildAssist {
	public class BuildPlatformUnknown : IBuildPlatform {
		public BuildReport BuildPackage( string[] scenes ) { return null; }
		public void Draw( BuildAssistWindow window ) {
			EditorGUILayout.HelpBox( S._Chooseyourplatformfrompreferences_, MessageType.Info );
		}
	}
}
