
#if UNITY_EDITOR

using Hananoki.BuildAssist;
using UnityEngine;

[BuildAssistEvent]
public class Addons {
	[BuildAssistEventAssetBundleBuildPostProcess]
	static void AssetBundleBuild() {
		Debug.Log( "AssetBundleBuild" );
	}

	[BuildAssistEventPackageBuildPostProcess]
	static void PackageBuild() {
		Debug.Log( "PackageBuild" );
	}
}

#endif
