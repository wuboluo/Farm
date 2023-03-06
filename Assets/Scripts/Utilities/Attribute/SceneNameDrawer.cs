using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    // 需要从路径中切割舍去的东西
    private readonly string[] scenePathSplit = {"/", ".unity"};
    
    // 记录当前场景序号
    private int sceneIndex = -1;
    
    // 下拉菜单中的待选项
    private GUIContent[] sceneNames;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 先判断 BuildSettings 中是否有场景，没有则 return
        if (EditorBuildSettings.scenes.Length == 0) return;

        // 当前场景没有设置的时候，才允许选择
        if (sceneIndex == -1)
            GetSceneNameArray(property);

        int oldIndex = sceneIndex;

        // 下拉单格式
        sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);

        // 点按了新的序号时，更新显示的文本
        if (oldIndex != sceneIndex)
            property.stringValue = sceneNames[sceneIndex].text;
    }

    private void GetSceneNameArray(SerializedProperty property)
    {
        // 类型为 EditorBuildSettingsScene的数组，此类型存在一个属性为 path，意味着可以返回场景文件在资源文件夹下的目录路径（ *.unity ）
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        
        // 初始化数组，把 BuildSettings里存在的场景添加进去
        sceneNames = new GUIContent[scenes.Length];

        for (int i = 0; i < sceneNames.Length; i++)
        {
            // 获得当前场景的路径
            string path = scenes[i].path;
            
            // 根据 scenePathSplit 为分界，把路径拆分为字符串数组，舍去空白内容
            string[] splitPath = path.Split(scenePathSplit, StringSplitOptions.RemoveEmptyEntries);

            // 如果 splitPath 长度为 0，就是该场景从 settings 里移除了，避免报空
            // 否则取最后一位：该场景的文件名称（例如：01.Field）
            string sceneName = splitPath.Length > 0 ? splitPath[^1] : "(Deleted Scene)";
            
            // 给第 i项实例出来
            sceneNames[i] = new GUIContent(sceneName);
        }

        // 如果 BuildSettings 里没有场景，则提示去检查并添加需要的场景
        if (sceneNames.Length == 0) sceneNames = new[] {new GUIContent("Check Your Build Settings")};

        // 如果这个 property 开始时已经被设置了
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            // 设置一个临时变量，标记已配置的场景名称是否在 settings的列表中找到
            bool nameFound = false;

            // 循环判断，若找到，则返回此场景的序号，并更新标记，结束循环
            for (int i = 0; i < sceneNames.Length; i++)
                if (sceneNames[i].text == property.stringValue)
                {
                    sceneIndex = i;
                    nameFound = true;
                    break;
                }

            // 没找到则返回第一项
            if (nameFound == false)
                sceneIndex = 0;
        }
        
        // 如果 property 开始时被设置为 empty，则返回第一项
        else
        {
            sceneIndex = 0;
        }

        // 设置 property 的值，为当前场景的名称
        property.stringValue = sceneNames[sceneIndex].text;
    }
}