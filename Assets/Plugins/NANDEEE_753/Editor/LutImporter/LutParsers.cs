using System.IO;
using UnityEngine;

namespace NANDEEE_753.Editor.LutImporter
{
    public static class LutParsers
    {
        public static LutData Parse(string path)
        {
            LutData lut = null;
            string ext = Path.GetExtension(path).ToLower();

            switch (ext.Replace(".", ""))
            {
                case "cube":
                    lut = CubeParser.Parse(path);
                    break;
                default:
                    Debug.LogError($"The .{ext} extension is not supported (parser not found).");
                    break;
            }

            return lut;
        }
    }
}