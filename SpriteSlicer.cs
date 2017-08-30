using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class SpriteSlicer
{
    [MenuItem("Assets/Tools/拆分图片")]
    public static void SliceSpriteWithXML()
    {
        //获取所有选中的资源
        var allSelections = Selection.objects;

        //获取选中资源的数量
        int allSelectionsCount = allSelections.Length;

        if(allSelectionsCount <= 0) {
            Debug.Log("没有选中任何资源.");
            return;
        }
        List<string> slicedPics = new List<string>();
        List<string> slicedXML = new List<string>();
        //遍历选中的资源
        for(int i = 0; i < allSelectionsCount; i++) {
            //获取路径
            string path = AssetDatabase.GetAssetPath(allSelections[i]);
            //获取扩展名
            string extension = Path.GetExtension(path);
            //暂时只支持png图片, 如果不是进入下一循环
            if(!extension.Equals(".png")) {
                continue;
            }

            Texture2D selectTexture = allSelections[i] as Texture2D;
            //找到同名的xml文件
            string xmlFilePath = path.Replace(".png", ".xml");
            //如果同名xml文件不存在, 进入下一循环
            if(!File.Exists(xmlFilePath)) {
                //弹出一个警告窗口
                EditorUtility.DisplayDialog("出毛病了!", xmlFilePath + "不存在", "好吧~");
                continue;
            }

            slicedPics.Add(path);
            slicedXML.Add(xmlFilePath);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            //解析xml文件, 获取到所有子图的信息
            importer.spritesheet = GetSubTexturesFrom(xmlFilePath).ToArray();
            AssetDatabase.StartAssetEditing();
            AssetDatabase.ImportAsset(importer.assetPath);
            AssetDatabase.StopAssetEditing();
        }

        if(slicedPics.Count > 0) {
            string str = "";
            foreach(string s in slicedPics) {
                str += (s + "\n");
            }
            if(EditorUtility.DisplayDialog("完成", str + "拆分完成, 是否删除对应的xml文件", "删除", "保留")) {
                foreach(string s in slicedXML) {
                    if(File.Exists(s)) {
                        File.Delete(s);
                    }
                }
                AssetDatabase.Refresh();
            }
        }
    }

    private static List<SpriteMetaData> GetSubTexturesFrom(string _xml)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(_xml);
        if(xml == null) {
            Debug.Log("打开xml文件失败" + _xml);
            return null;
        }

        XmlElement doc = xml.DocumentElement;
        if(doc == null) {
            Debug.Log("没有找到根节点" + _xml);
            return null;
        }
        int height = Convert.ToInt32(doc.Attributes["height"].Value);
        int width = Convert.ToInt32(doc.Attributes["width"].Value);
        List<SpriteMetaData> spriteMetaData = new List<SpriteMetaData>();
        XmlElement spriteEl = doc.FirstChild as XmlElement;
        while(spriteEl != null) {

            SpriteMetaData d = new SpriteMetaData();
            int x = Convert.ToInt32(spriteEl.Attributes["x"].Value);
            int y = Convert.ToInt32(spriteEl.Attributes["y"].Value);
            int w = Convert.ToInt32(spriteEl.Attributes["w"].Value);
            int h = Convert.ToInt32(spriteEl.Attributes["h"].Value);
            d.alignment = (int)SpriteAlignment.Center;
            d.pivot = new Vector2(0.5f, 0.5f);
            d.border = Vector4.zero;
            d.name = spriteEl.Attributes["n"].Value.Replace(".png", ""); //去掉名字中的后缀
            d.rect = new Rect(x, height - (y + h), w, h);
            spriteMetaData.Add(d);
            spriteEl = spriteEl.NextSibling as XmlElement;
        }

        return spriteMetaData;
    }
}
