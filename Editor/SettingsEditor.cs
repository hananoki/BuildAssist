
using System;

namespace Hananoki.BuildAssist {
	[Serializable]
	public class SettingsEditor {
		public const int FoldAssetBundle = ( 1 << 0 );
		public const int FoldPlatform = ( 1 << 1 );
		public const int FoldBuildSettings = ( 1 << 2 );
		public const int FoldOutputDirectory = ( 1 << 3 );


		public string adb_exe;
		public string bundletool_jar;
		public int fold;


		public static SessionStateBool connectProfiler = new SessionStateBool( "connectProfiler" );
		public static SessionStateBool autoRunPlayer = new SessionStateBool( "AutoRunPlayer" );

		public static SettingsEditor i;

		public SettingsEditor() {
			connectProfiler = new SessionStateBool( "connectProfiler" );
		}

		public static void Load() {
			if( i != null ) return;
			i = EditorPrefJson<SettingsEditor>.Get( Package.editorPrefName );
			if( i == null ) {
				i = new SettingsEditor();
				Save();
			}
		}

		public static void Save() {
			EditorPrefJson<SettingsEditor>.Set( Package.editorPrefName, i );
		}
	}
}
