using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// 미사용 타일 에셋을 찾고 삭제하는 유틸리티
/// </summary>
public class UnusedTileAssetCleaner : EditorWindow
{
    private string searchPath = "Assets/Resources/Image/Object/Map";
    private List<string> unusedAssets = new List<string>();
    private Vector2 scrollPosition;
    private bool showUnusedAssets = false;

    [MenuItem("Tools/Unused Tile Asset Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<UnusedTileAssetCleaner>("Unused Tile Cleaner");
    }

    private void OnGUI()
    {
        GUILayout.Label("미사용 타일 에셋 정리 도구", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "이 도구는 프로젝트에서 사용되지 않는 타일 에셋을 찾아줍니다.\n" +
            "주의: 실제로 사용 중인 에셋도 감지하지 못할 수 있으니\n" +
            "삭제 전에 반드시 확인하세요!",
            MessageType.Warning);

        EditorGUILayout.Space();

        searchPath = EditorGUILayout.TextField("검색 경로:", searchPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("미사용 에셋 찾기", GUILayout.Height(30)))
        {
            FindUnusedAssets();
        }

        EditorGUILayout.Space();

        if (unusedAssets.Count > 0)
        {
            EditorGUILayout.HelpBox(
                $"총 {unusedAssets.Count}개의 미사용 에셋을 찾았습니다.",
                MessageType.Info);

            showUnusedAssets = EditorGUILayout.Foldout(showUnusedAssets, "미사용 에셋 목록 보기");

            if (showUnusedAssets)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                foreach (var asset in unusedAssets)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(asset);
                    if (GUILayout.Button("선택", GUILayout.Width(60)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(asset);
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("선택된 에셋만 삭제", GUILayout.Height(30)))
            {
                DeleteSelectedAssets();
            }
            if (GUILayout.Button("모든 미사용 에셋 삭제", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                    "경고",
                    $"정말로 {unusedAssets.Count}개의 에셋을 모두 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다!",
                    "삭제",
                    "취소"))
                {
                    DeleteAllUnusedAssets();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void FindUnusedAssets()
    {
        unusedAssets.Clear();

        if (!Directory.Exists(searchPath))
        {
            EditorUtility.DisplayDialog("오류", "지정한 경로가 존재하지 않습니다.", "확인");
            return;
        }

        // 모든 .asset 파일 찾기
        string[] allAssets = Directory.GetFiles(searchPath, "*.asset", SearchOption.AllDirectories)
            .Select(path => path.Replace('\\', '/'))
            .ToArray();

        Debug.Log($"총 {allAssets.Length}개의 에셋을 검사합니다...");

        int unusedCount = 0;

        foreach (string assetPath in allAssets)
        {
            if (IsAssetUnused(assetPath))
            {
                unusedAssets.Add(assetPath);
                unusedCount++;
            }
        }

        Debug.Log($"검사 완료: {unusedCount}개의 미사용 에셋을 찾았습니다.");
        EditorUtility.DisplayDialog("완료", $"검사 완료!\n{unusedCount}개의 미사용 에셋을 찾았습니다.", "확인");
    }

    private bool IsAssetUnused(string assetPath)
    {
        // 에셋을 로드
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (asset == null) return false;

        // 1. 씬에서 참조 확인
        if (IsReferencedInScenes(asset)) return false;

        // 2. Prefab에서 참조 확인
        if (IsReferencedInPrefabs(asset)) return false;

        // 3. 다른 에셋에서 참조 확인 (ScriptableObject, TilePalette 등)
        if (IsReferencedInAssets(asset)) return false;

        // 4. Resources.Load나 Addressables로 로드되는지 확인
        // (이 부분은 정확하지 않을 수 있으므로 주의)

        return true;
    }

    private bool IsReferencedInScenes(Object asset)
    {
        string[] scenePaths = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories);
        
        foreach (string scenePath in scenePaths)
        {
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            if (dependencies.Contains(AssetDatabase.GetAssetPath(asset)))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsReferencedInPrefabs(Object asset)
    {
        string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
        
        foreach (string prefabPath in prefabPaths)
        {
            string[] dependencies = AssetDatabase.GetDependencies(prefabPath, true);
            if (dependencies.Contains(AssetDatabase.GetAssetPath(asset)))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsReferencedInAssets(Object asset)
    {
        string assetPath = AssetDatabase.GetAssetPath(asset);
        
        // 모든 에셋에서 참조 확인
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        
        foreach (string path in allAssetPaths)
        {
            if (path == assetPath) continue;
            if (path.EndsWith(".cs")) continue; // 스크립트는 제외
            
            string[] dependencies = AssetDatabase.GetDependencies(path, false);
            if (dependencies.Contains(assetPath))
            {
                return true;
            }
        }
        return false;
    }

    private void DeleteSelectedAssets()
    {
        if (Selection.objects.Length == 0)
        {
            EditorUtility.DisplayDialog("알림", "삭제할 에셋을 선택해주세요.", "확인");
            return;
        }

        List<string> pathsToDelete = new List<string>();
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (unusedAssets.Contains(path))
            {
                pathsToDelete.Add(path);
            }
        }

        if (pathsToDelete.Count == 0)
        {
            EditorUtility.DisplayDialog("알림", "선택한 에셋 중 미사용 에셋이 없습니다.", "확인");
            return;
        }

        if (EditorUtility.DisplayDialog(
            "확인",
            $"{pathsToDelete.Count}개의 에셋을 삭제하시겠습니까?",
            "삭제",
            "취소"))
        {
            DeleteAssets(pathsToDelete);
        }
    }

    private void DeleteAllUnusedAssets()
    {
        DeleteAssets(unusedAssets);
    }

    private void DeleteAssets(List<string> paths)
    {
        int deletedCount = 0;
        foreach (string path in paths)
        {
            if (AssetDatabase.DeleteAsset(path))
            {
                deletedCount++;
            }
        }

        AssetDatabase.Refresh();
        unusedAssets.Clear();
        
        EditorUtility.DisplayDialog("완료", $"{deletedCount}개의 에셋을 삭제했습니다.", "확인");
        Debug.Log($"{deletedCount}개의 미사용 에셋을 삭제했습니다.");
    }
}
