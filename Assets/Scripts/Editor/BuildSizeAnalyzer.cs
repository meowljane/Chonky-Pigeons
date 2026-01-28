using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// 빌드 크기 분석 및 Resources 폴더 에셋 크기 확인 도구
/// </summary>
public class BuildSizeAnalyzer : EditorWindow
{
    private Dictionary<string, long> assetSizes = new Dictionary<string, long>();
    private Vector2 scrollPosition;
    private long totalResourcesSize = 0;
    private string sizeUnit = "MB";

    [MenuItem("Tools/Build Size Analyzer")]
    public static void ShowWindow()
    {
        GetWindow<BuildSizeAnalyzer>("Build Size Analyzer");
    }

    private void OnGUI()
    {
        GUILayout.Label("빌드 크기 분석 도구", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Resources 폴더의 모든 에셋은 빌드에 포함됩니다.\n" +
            "사용하지 않는 에셋을 삭제하면 빌드 크기를 줄일 수 있습니다.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Resources 폴더 크기 분석", GUILayout.Height(30)))
        {
            AnalyzeResourcesFolder();
        }

        EditorGUILayout.Space();

        if (assetSizes.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"총 Resources 크기: {FormatSize(totalResourcesSize)}");
            if (GUILayout.Button("크기순 정렬", GUILayout.Width(100)))
            {
                SortBySize();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
            
            foreach (var kvp in assetSizes.OrderByDescending(x => x.Value))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(400));
                EditorGUILayout.LabelField(FormatSize(kvp.Value), GUILayout.Width(100));
                
                if (GUILayout.Button("선택", GUILayout.Width(60)))
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(kvp.Key);
                    if (obj != null)
                    {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 타일 에셋 통계
            var tileAssets = assetSizes.Where(kvp => kvp.Key.Contains("Image/Object/Map"));
            if (tileAssets.Any())
            {
                long tileSize = tileAssets.Sum(kvp => kvp.Value);
                int tileCount = tileAssets.Count();
                EditorGUILayout.HelpBox(
                    $"타일 에셋 통계:\n" +
                    $"  - 개수: {tileCount}개\n" +
                    $"  - 총 크기: {FormatSize(tileSize)}\n" +
                    $"  - 평균 크기: {FormatSize(tileSize / tileCount)}",
                    MessageType.Info);
            }
        }
    }

    private void AnalyzeResourcesFolder()
    {
        assetSizes.Clear();
        totalResourcesSize = 0;

        string resourcesPath = "Assets/Resources";
        if (!Directory.Exists(resourcesPath))
        {
            EditorUtility.DisplayDialog("오류", "Resources 폴더를 찾을 수 없습니다.", "확인");
            return;
        }

        // Resources 폴더의 모든 파일 찾기
        string[] allFiles = Directory.GetFiles(resourcesPath, "*", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".meta"))
            .ToArray();

        foreach (string filePath in allFiles)
        {
            string assetPath = filePath.Replace('\\', '/');
            FileInfo fileInfo = new FileInfo(filePath);
            
            if (fileInfo.Exists)
            {
                long size = fileInfo.Length;
                assetSizes[assetPath] = size;
                totalResourcesSize += size;
            }
        }

        Debug.Log($"Resources 폴더 분석 완료: {assetSizes.Count}개 파일, 총 {FormatSize(totalResourcesSize)}");
        EditorUtility.DisplayDialog("완료", 
            $"분석 완료!\n" +
            $"파일 수: {assetSizes.Count}개\n" +
            $"총 크기: {FormatSize(totalResourcesSize)}", 
            "확인");
    }

    private void SortBySize()
    {
        assetSizes = assetSizes.OrderByDescending(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} KB";
        else
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }
}
