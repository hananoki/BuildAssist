
using UnityEditor.Build.Reporting;

namespace Hananoki.BuildAssist {
	public interface IBuildPlatform {
		void Draw( BuildAssistWindow bmw );

		BuildReport BuildPackage( string[] scenes );
	}
}
