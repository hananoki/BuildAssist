
using System;
using UnityEditor;

namespace Hananoki.BuildAssist {
  public static class Package {
    public const string name = "BuildAssist";
    public const string editorPrefName = "Hananoki.BuildAssist";
    public const string version = "1.3.6";
    public static string projectSettingsPath => $"{SharedModule.SettingsEditor.projectSettingDirectory}/BuildAssist.json";
  }
}
