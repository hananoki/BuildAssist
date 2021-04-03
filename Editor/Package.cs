
using System;
using UnityEditor;

namespace HananokiEditor.BuildAssist {
  public static class Package {
    public const string name = "BuildAssist";
    public const string nameNicify = "Build Assist";
    public const string editorPrefName = "Hananoki.BuildAssist";
    public const string version = "2.0.0";
    public static string projectSettingsPath => $"{SharedModule.SettingsEditor.projectSettingDirectory}/BuildAssist.json";
  }
}
