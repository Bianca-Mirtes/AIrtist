using UnityEngine;

namespace WorkData
{
    [System.Serializable]
    public struct Infos
    {
        public int resWidth { get; set; }
        public int resHeight { get; set; }
        public string authorName { get; set; }
        public string workName { get; set; }
        public string workAge { get; set; }
    }
}