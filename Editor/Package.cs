
using System;
using UnityEditor;

namespace Hananoki.BuildAssist {
  public static class Package {
    public const string name = "BuildAssist";
    public const string editorPrefName = "Hananoki.BuildAssist";
    public const string version = "1.3.2";
    public static string projectSettingsPath => $"{SharedModule.SettingsEditor.projectSettingDirectory}/BuildAssist.json";
  }
  
#if UNITY_EDITOR
  [EditorLocalizeClass]
  public class LocalizeEvent {
    [EditorLocalizeMethod]
    public static void Changed() {
      foreach( var filename in DirectoryUtils.GetFiles( AssetDatabase.GUIDToAssetPath( "3b63796fcdc63d441842ccad9baad1b2" ), "*.csv" ) ) {
        if( filename.Contains( EditorLocalize.GetLocalizeName() ) ) {
          EditorLocalize.Load( Package.name, AssetDatabase.AssetPathToGUID( filename ), "3dde87905623cf44f8233a6e89e15db5" );
          BuildAssistWindow.InitLocalize();
        }
      }
    }
  }
#endif
}
