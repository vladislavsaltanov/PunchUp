using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Security.Cryptography;
using TMPro;

public class pablo : MonoBehaviour, IPostprocessBuildWithReport
{
    public int callbackOrder => 100;

    [SerializeField]
        Sprite pablo_png;

    public bool isPablo;

    [SerializeField]
    TextMeshProUGUI pablo_test_ui;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        isPablo = File.Exists(AssetDatabase.GetAssetPath(pablo_png));
        Debug.Log(AssetDatabase.GetAssetPath(pablo_png));
        // 4e2f67a627694d7782492483c81a70f8
        Debug.Log(CalculateMD5(AssetDatabase.GetAssetPath(pablo_png)));
        pablo_test_ui.text = CalculateMD5(AssetDatabase.GetAssetPath(pablo_png));
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log(report.summary.outputPath);

        if (!isPablo)
        {
            Debug.LogError("imagine no pablo...");
            return;
        }

        File.Copy(AssetDatabase.GetAssetPath(pablo_png), report.summary.outputPath);
    }

    string CalculateMD5(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
