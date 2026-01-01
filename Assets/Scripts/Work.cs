using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Work", menuName = "Scriptable Objects/Work")]
public class Work : ScriptableObject
{
    public Sprite image;
    public Texture2DArray[] painting;
    public Texture2DArray[] masks;
    public string workName;
    public string author;
    public string age;

    public Question[] awnsers;
    public int resWidth;
    public int resHeight;
    public TextAsset vectors;
}
