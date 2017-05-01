
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TextMeshProReplacer
{
    internal class TextReplacer
    {
        [MenuItem("Text Mesh Replacer/Replace Current Scene")]
        internal static void ReplaceCurrentScene()
        {

            Text[] allText = Object.FindObjectsOfType<Text>();
            ReplaceUnityText(allText);
        }
        [MenuItem("Text Mesh Replacer/Replace All Scene")]
        internal static void ReplaceAllScene()
        {
            SceneAsset[] scenes = FindAssets<SceneAsset>();
            for (int i = 0; i < scenes.Length; i++)
            {
                SceneAsset scene = scenes[i];
                EditorUtility.DisplayProgressBar("Replacing in scene...", scene.name, (float)i / scenes.Length);
                Scene loadedScene=EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scene),OpenSceneMode.Single);
                while (!loadedScene.isLoaded)
                {
                    //wait
                }

                ReplaceCurrentScene();

                EditorSceneManager.SaveScene(loadedScene);
            }
            EditorUtility.ClearProgressBar();
        }
        [MenuItem("Text Mesh Replacer/Test/Test Canvas")]
        internal static void GenerateDemoCanvas()
        {
            //generate new canvas
            GameObject newCanvas=new GameObject("TestCanvas");
            Canvas canvas=newCanvas.AddComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            
            //find all fonts
            Font[] allFonts = FindAssets<Font>();
            //generate texts inside canvas
            StringBuilder stringBuilder=new StringBuilder();

            for (int i = 0; i < 100; i++)
            {
                Text testText=new GameObject("test text").AddComponent<Text>();
                testText.transform.SetParent(newCanvas.transform,false);
                testText.rectTransform.anchoredPosition=new Vector2(Random.Range(-300,300), Random.Range(-300, 300));
                if (allFonts.Length > 0)
                    testText.font = allFonts[Random.Range(0, allFonts.Length)];
                //generate random string
                int stringLength = Random.Range(5, 20);
                stringBuilder.Length = 0;
                while (stringLength>0)
                {
                    stringBuilder.Append(char.ConvertFromUtf32(Random.Range(48, 123)));
                    stringLength--;
                }
                testText.text = stringBuilder.ToString();

            }
            
            EditorUtility.SetDirty(canvas);

            ReplaceUnityText(canvas.GetComponentsInChildren<Text>());
        }
        internal static void ReplaceUnityText(Text[] unityTexts)
        {
            TMP_FontAsset[] fonts = FindAssets<TMP_FontAsset>();
            if(fonts.Length==0)
                return;
            List<Font> missingFonts=new List<Font>();
            for (int i = 0; i < unityTexts.Length; i++)
            {
                Text text = unityTexts[i];
                
                if (!missingFonts.Contains(text.font))
                {
                    TMP_FontAsset font = GetTMPFont(text.font, fonts);
                    if (font != null)
                        ReplaceUnityText(text, font);
                    else
                        missingFonts.Add(text.font);
                }
            }
            //log missing fonts if any
            if (missingFonts.Count==0)
                return;
            Debug.LogWarningFormat("Missing {0} fonts",missingFonts.Count);
            for (int i = 0; i < missingFonts.Count; i++)
            {
                Debug.LogWarningFormat("Text Mesh pro Font {0} is missing", missingFonts[i].name);
            }
        }

        internal static void ReplaceUnityText(Text unityText)
        {
            TMP_FontAsset[] fonts = FindAssets<TMP_FontAsset>();
            if (fonts.Length == 0)
                return;
            TMP_FontAsset font = GetTMPFont(unityText.font, fonts);
            if (font != null)
                ReplaceUnityText(unityText, font);
            else
                Debug.LogWarningFormat("Text Mesh pro Font {0} is missing",unityText.font.name);
        }
        private static void ReplaceUnityText(Text unityText,TMP_FontAsset font)
        {
            Text currentText = unityText;
            //check if tmpro text already exist, if not dont replace
            
            //for some reason adding tmpro component messed up the rect transform size
            //this will fix it
            Vector2 size = currentText.rectTransform.sizeDelta;

            TextData textData = new TextData();
            GameObject obj = unityText.gameObject;
            textData.write(currentText,font);

            //Selection.activeObject = obj;
            Object.DestroyImmediate(currentText);


            TextMeshProUGUI tmproText = obj.AddComponent<TextMeshProUGUI>();
            tmproText.autoSizeTextContainer = false;
            textData.read(tmproText);
            tmproText.rectTransform.sizeDelta = size;

            EditorUtility.SetDirty(tmproText);
        }
        private static T[] FindAssets<T>() where T : Object
        {
            List<T> result = new List<T>();
            string[] assetList = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name));
            for (int i = 0; i < assetList.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetList[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    result.Add(asset);
                }
            }
            return result.ToArray();
        }

        private static TMP_FontAsset GetTMPFont(Font m_font, TMP_FontAsset[] fonts)
        {

            for (int i = 0; i < fonts.Length; i++)
            {
                TMP_FontAsset fontAsset = fonts[i];
                if (fontAsset.fontInfo.Name.Equals(m_font.name, System.StringComparison.OrdinalIgnoreCase))
                    return fontAsset;

                if (System.Array.Exists(m_font.fontNames,(s)=>s.Equals(fontAsset.fontInfo.Name,System.StringComparison.OrdinalIgnoreCase)))
                {
                    return fontAsset;
                }
            }
            return null;
        }
    }
    internal struct TextData
    {
        private string text;
        private TextAlignmentOptions anchor;
        private TMP_FontAsset tmProfont;
        private Color color;
        private float fontSize;
        private FontStyles fontStyle;
        public void write(Text textObject,TMP_FontAsset tmpFont)
        {

            color = textObject.color;
            fontSize = textObject.fontSize;
            fontStyle = GetFontStyle(textObject.fontStyle);
            tmProfont = tmpFont;
            anchor = GetTextAlignment(textObject.alignment);
            if (textObject.fontStyle == FontStyle.BoldAndItalic)
                text = string.Format("<i>{0}</i>", textObject.text);
            else
                text = textObject.text;
        }

        public void read(TextMeshProUGUI textObject)
        {
            textObject.text = text;
            textObject.color = color;
            textObject.fontSize = fontSize;
            textObject.fontStyle = fontStyle;

            textObject.alignment = anchor;

            if (tmProfont != null)
                textObject.font = tmProfont;

        }
        private FontStyles GetFontStyle(FontStyle style)
        {
            switch (style)
            {
                case FontStyle.Bold:
                    return FontStyles.Bold;
                case FontStyle.Italic:
                    return FontStyles.Italic;
                case FontStyle.BoldAndItalic:
                    //this might not work because it's protected inside tmpro internal
                    //and i'm too lazy too look it up :P
                    //so i added <i> tags around it
                    return FontStyles.Bold;
            }
            return FontStyles.Normal;
        }
        private TextAlignmentOptions GetTextAlignment(TextAnchor m_anchor)
        {
            switch (m_anchor)
            {
                case TextAnchor.LowerCenter:
                    return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerLeft:
                    return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerRight:
                    return TextAlignmentOptions.BottomRight;
                case TextAnchor.MiddleCenter:
                    return TextAlignmentOptions.Center;
                case TextAnchor.MiddleLeft:
                    return TextAlignmentOptions.MidlineLeft;
                case TextAnchor.MiddleRight:
                    return TextAlignmentOptions.MidlineRight;
                case TextAnchor.UpperCenter:
                    return TextAlignmentOptions.Top;
                case TextAnchor.UpperLeft:
                    return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperRight:
                    return TextAlignmentOptions.TopRight;
                default:
                    return TextAlignmentOptions.Baseline;
            }
        }

    }
}
