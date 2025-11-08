using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Security.Cryptography;
using TMPro;

[InitializeOnLoad]
public class pablo : MonoBehaviour, IPostprocessBuildWithReport
{
    public int callbackOrder => 100;

    [SerializeField]
        Sprite pablo_png;


    public static TextMeshProUGUI pablo_test_ui;

    [RuntimeInitializeOnLoadMethod]
    static void Startup()
    {
    }

    public void OnPostprocessBuild(BuildReport report)
    {
    }
}
