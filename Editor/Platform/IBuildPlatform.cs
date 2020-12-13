
using UnityEditor.Build.Reporting;

namespace HananokiEditor.BuildAssist {
	public interface IBuildPlatform {
		void Draw( BuildAssistWindow bmw );

		BuildReport BuildPackage( string[] scenes );
	}
}
