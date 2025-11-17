using UnityEngine;

[CreateAssetMenu(fileName = "Work", menuName = "Scriptable Objects/Work")]
public class Work : ScriptableObject
{
    public Sprite image;
    public Texture2DArray texture;
    public string workName;
    public string author;
    public string age;
}
