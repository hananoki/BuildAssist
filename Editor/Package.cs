
using System;
using UnityEditor;

namespace HananokiEditor.BuildAssist {
  public static class Package {
    public const string name = "BuildAssist";
    public const string nameNicify = "Build Assist";
    public const string editorPrefName = "Hananoki.BuildAssist";
    public const string version = "1.3.8";
    public static string projectSettingsPath => $"{SharedModule.SettingsEditor.projectSettingDirectory}/BuildAssist.json";
  }
}
