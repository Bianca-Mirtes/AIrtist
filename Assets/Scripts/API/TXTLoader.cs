using GLTFast.Addons;
using System.IO;
using UnityEngine;
using WorkData;

public class TXTLoader : MonoBehaviour
{
    private Infos infos;
    public static TXTLoader _instance;

    public static TXTLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TXTLoader>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject("TXTLoader");
                    _instance = singleton.AddComponent<TXTLoader>();
                    DontDestroyOnLoad(singleton);
                }
            }

            return _instance;
        }
    }

    public Infos LoadTXT(string txt)
    {
        if( txt == null || txt.Length == 0)
        {
            Debug.LogError("TXTLoader: txt is null or empty");
            return infos;
        }

        using (StringReader reader = new StringReader(txt))
        {
            int count = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Debug.Log("Read line: " + line);
                if(count == 0)
                {
                    infos.authorName = line;
                }
                if(count == 1)
                {
                    infos.workName = line;
                }
                if(count == 2)
                {
                    infos.workAge = line;
                }
                if(count == 3)
                {
                    string[] res = line.Split('x');
                    if(res.Length == 2)
                    {
                        if(int.TryParse(res[0], out int width))
                        {
                            infos.resWidth = width;
                        }
                        if(int.TryParse(res[1], out int height))
                        {
                            infos.resHeight = height;
                        }
                    }
                }
            }
            return infos;
        }
    }
}
