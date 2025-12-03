using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class pablo_checker : MonoBehaviour
{
    public static bool checkPablo = true;
    public static bool isPablo => File.Exists(pablo_path);
    public static string pablo_path => Path.Combine(Application.streamingAssetsPath, "pablo.png");

    public static bool isPabloValid => ("4e2f67a627694d7782492483c81a70f8" == CalculateMD5(pablo_path));

    void Start()
    {
        if (checkPablo)
            if (!isPablo || !isPabloValid)
                Application.Quit();
    }

    public static string CalculateMD5(string filename)
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
