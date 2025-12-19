using UnityEngine;

[CreateAssetMenu(menuName = "Scratch/Frame Diff Data")]
public class FrameDiffData : ScriptableObject
{
    public int framesPerArray;
    public float[] diffs; // tamanho = framesPerArray*2 - 1
}

