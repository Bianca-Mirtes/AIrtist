using System;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

namespace WorkData
{
    [System.Serializable]
    public class Infos
    {
        public int resWidth { get; set; }
        public int resHeight { get; set; }
        public string authorName { get; set; }
        public string workName { get; set; }
        public string workAge { get; set; }
    }

    [Serializable]
    public class ArtWorkContext
    {
        public string artistName;
        public string title;
        public string style;
        public string genre;
        public string sourceUrl;
        public string dimensions;
    }

    [System.Serializable]
    public class WorkAPI
    {
        public Sprite image;
        public List<ZipArchiveEntry> painting;
        public List<ZipArchiveEntry> masks;
        public string workName;
        public string author;
        public string age;
        public string txtInfos;
        public int resWidth;
        public int resHeight;

        public ArtWorkContext artWorkContext;
    }
}