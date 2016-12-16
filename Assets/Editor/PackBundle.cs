using UnityEngine;
using System.Collections;
using UnityEditor;

public class PackBundle : MonoBehaviour {
	[MenuItem("Bundle/Build")]
	static void BuildAllAssetBundles()
	{
		BuildPipeline.BuildAssetBundles ("Assets/AssetBundles");
	}

	[MenuItem("Bundle/BuildForIOS")]
	static void BuildAllAssetBundlesForIOS()
	{
		BuildPipeline.BuildAssetBundles ("Assets/AssetBundles/IOS",BuildAssetBundleOptions.None,BuildTarget.iOS);
	}
}
