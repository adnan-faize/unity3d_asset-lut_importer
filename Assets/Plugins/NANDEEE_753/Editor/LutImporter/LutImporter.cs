using UnityEditor.AssetImporters;
using UnityEngine;

// https://youtu.be/c_3DXBrH-Is?si=W35gL9sUHGMoBViw

namespace NANDEEE_753.Editor.LutImporter
{
    [ScriptedImporter(1, new string[] {
        "cube"
    })]
    public class LutImporter : ScriptedImporter
    {
        [SerializeField]
        private LutData lut;
        
        [SerializeField] 
        private int size = 32;
        [SerializeField] 
        private bool use3D = false;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            lut = LutParsers.Parse(ctx.assetPath);

            // TODO : add log color transform ?? maybeeee ??

            if (use3D)
            {
                Texture3D mainTexture3D = LutData.Generate3DTexture(lut, size);
                ctx.AddObjectToAsset($"{lut.title}", mainTexture3D);
                ctx.SetMainObject(mainTexture3D);
            }
            else
            {
                Texture2D mainTexture2D = LutData.Generate2DTexture(lut, size);
                ctx.AddObjectToAsset($"{lut.title}", mainTexture2D);
                ctx.SetMainObject(mainTexture2D);
            }
            
            lut.ClearData();
        }
    }
}
