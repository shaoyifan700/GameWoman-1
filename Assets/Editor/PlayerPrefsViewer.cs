#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Microsoft.Win32;
using System.Collections.Generic;

/// <summary>
/// PlayerPrefs 可视化查看器（仅 Windows 编辑器可用）
/// 打开方式：Unity 菜单 → Tools → PlayerPrefs Viewer
/// </summary>
public class PlayerPrefsViewer : EditorWindow
{
    private Vector2 scroll;
    private List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
    private string filter = "";

    [MenuItem("Tools/PlayerPrefs Viewer")]
    public static void Open()
    {
        GetWindow<PlayerPrefsViewer>("PlayerPrefs Viewer");
    }

    void OnEnable() => Refresh();

    void Refresh()
    {
        entries.Clear();
        string companyName = PlayerSettings.companyName;
        string productName = PlayerSettings.productName;
        string path = $@"Software\Unity\UnityEditor\{companyName}\{productName}";

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
        {
            if (key == null) return;
            foreach (string valueName in key.GetValueNames())
            {
                // Unity 在键名后追加 _h12345678 hash 后缀，去掉显示更直观
                string display = valueName;
                int hIdx = valueName.LastIndexOf("_h");
                if (hIdx > 0) display = valueName.Substring(0, hIdx);

                object value = key.GetValue(valueName);
                string strValue;
                if (value is byte[] bytes)
                    strValue = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                else
                    strValue = value?.ToString() ?? "(null)";

                entries.Add(new KeyValuePair<string, string>(display, strValue));
            }
        }
        entries.Sort((a, b) => string.Compare(a.Key, b.Key));
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新", GUILayout.Width(60))) Refresh();
        if (GUILayout.Button("全部清空", GUILayout.Width(80)))
        {
            if (EditorUtility.DisplayDialog("确认", "确定要清空所有 PlayerPrefs 吗？无法恢复！", "清空", "取消"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Refresh();
            }
        }
        EditorGUILayout.LabelField("过滤：", GUILayout.Width(40));
        filter = EditorGUILayout.TextField(filter);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"共 {entries.Count} 条记录", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var kv in entries)
        {
            if (!string.IsNullOrEmpty(filter) &&
                !kv.Key.ToLower().Contains(filter.ToLower())) continue;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField(kv.Key, GUILayout.Width(280));
            EditorGUILayout.SelectableLabel(kv.Value, GUILayout.Height(18));
            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                PlayerPrefs.DeleteKey(kv.Key);
                PlayerPrefs.Save();
                Refresh();
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
}
#endif
