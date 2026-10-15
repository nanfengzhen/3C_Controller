using UnityEngine;
using UnityEditor;

public class FileBatchRenamer : EditorWindow
{
    // 默认你要删除的前缀
    private string prefixToRemove = "Ani_Avatar_Girl_";

    [MenuItem("Tools/文件批量重命名工具")]
    public static void ShowWindow()
    {
        GetWindow<FileBatchRenamer>("批量改名");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量去除文件名前缀", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 输入框
        prefixToRemove = EditorGUILayout.TextField("要删除的前缀:", prefixToRemove);

        EditorGUILayout.Space();

        // 显示当前选中了多少个文件
        if (Selection.objects.Length > 0)
        {
            EditorGUILayout.LabelField($"当前选中文件数: {Selection.objects.Length}");
        }
        else
        {
            EditorGUILayout.HelpBox("请先在 Project 窗口选中要改名的文件！", MessageType.Warning);
        }

        if (GUILayout.Button("开始重命名") && Selection.objects.Length > 0)
        {
            RenameSelectedAssets();
        }
    }

    private void RenameSelectedAssets()
    {
        int count = 0;
        foreach (Object obj in Selection.objects)
        {
            // 获取文件路径
            string path = AssetDatabase.GetAssetPath(obj);

            // 获取文件名（不带后缀）
            string originalName = obj.name;

            // 检查是否包含前缀
            if (originalName.StartsWith(prefixToRemove))
            {
                // 生成新名字
                string newName = originalName.Replace(prefixToRemove, "");

                // 执行重命名 (Unity 会自动处理 meta 文件和引用关系，不用担心引用丢失)
                string error = AssetDatabase.RenameAsset(path, newName);

                if (string.IsNullOrEmpty(error))
                {
                    count++;
                }
                else
                {
                    Debug.LogError($"重命名失败: {originalName} -> {error}");
                }
            }
        }

        // 刷新一下资源管理器
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"搞定！成功重命名了 {count} 个文件。");
    }
}