using System.Collections.Generic;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Work", menuName = "Scriptable Objects/Work")]
public class Work : ScriptableObject
{
    public Sprite image;
    public List<Texture2D> painting;
    public List<Texture2D> masks;
    public string workName;
    public string author;
    public string age;
    public int resWidth;
    public int resHeight;

    public Question[] awnsers;
}
