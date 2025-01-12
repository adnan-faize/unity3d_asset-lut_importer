using System;
using UnityEngine;

namespace NANDEEE_753.Editor.LutImporter
{
    [Serializable]
    public class LutData
    {
        public string title = "Unknown";
        public string type = "1D";
        public int size = 0;
        
        public float inputRangeMin = 0;
        public float inputRangeMax = 1;

        public bool usesDomainMinMax = false;
        public Vector3 domainMin = Vector3.zero;
        public Vector3 domainMax = Vector3.one;
        
        public Color[] data;

        public void ClearData() => data = Array.Empty<Color>();

        private static Color Map1D(LutData lut, Color input)
        {
            // Normalize the input value to the range [0,1]
            float normalizedInput = (input.r - lut.inputRangeMin) / (lut.inputRangeMax - lut.inputRangeMin);
            
            // Find the index of the LUT entry that corresponds to the normalized input value
            int value = Mathf.FloorToInt(normalizedInput * (lut.data.Length - 1));
            int index = Mathf.Clamp(value, 0, lut.data.Length - 2);
            
            // Find the two LUT entries that the input value falls between
            Color lower = lut.data[index];
            Color upper = lut.data[index + 1];
            
            // Interpolate between the two LUT entries to find the remapped value
            float t = normalizedInput * (lut.data.Length - 1) - index;
            Color result = Color.Lerp(lower, upper, t);
            
            return result;
        }
        
        private static Color Map3D(LutData lut, Color input)
        {
            // Normalize the input value to the range [0,1]
            Vector3 normalizedInput;
            
            if (lut.usesDomainMinMax)
            {
                float x = (input.r - lut.domainMin.x) / (lut.domainMax.x - lut.domainMin.x);
                float y = (input.g - lut.domainMin.y) / (lut.domainMax.y - lut.domainMin.y);
                float z = (input.b - lut.domainMin.z) / (lut.domainMax.z - lut.domainMin.z);
                normalizedInput = new Vector3(x, y, z);
            }
            else
            {
                float x = (input.r - lut.inputRangeMin) / (lut.inputRangeMax - lut.inputRangeMin);
                float y = (input.g - lut.inputRangeMin) / (lut.inputRangeMax - lut.inputRangeMin);
                float z = (input.b - lut.inputRangeMin) / (lut.inputRangeMax - lut.inputRangeMin);
                normalizedInput = new Vector3(x, y, z);
            }
            
            // Find the indices of the LUT entries that correspond to the normalized input value
            Vector3 indices = normalizedInput * (lut.size - 1);
            Vector3Int lowerIndices = Vector3Int.FloorToInt(indices);
            Vector3Int upperIndices = Vector3Int.CeilToInt(indices);
            
            // Clamp the indices to the valid range
            Vector3Int lowerIndicesLhsRhs = new Vector3Int(lut.size - 2, lut.size - 2, lut.size - 2);
            Vector3Int lowerIndicesLhs = Vector3Int.Min(lowerIndices, lowerIndicesLhsRhs);
            lowerIndices = Vector3Int.Max(lowerIndicesLhs, Vector3Int.zero);
            
            Vector3Int upperIndicesLhsRhs = new Vector3Int(lut.size - 1, lut.size - 1, lut.size - 1);
            Vector3Int upperIndicesLhs = Vector3Int.Min(upperIndices, upperIndicesLhsRhs);
            upperIndices = Vector3Int.Max(upperIndicesLhs, Vector3Int.one);

            // Find the eight LUT entries that the input value falls between
            Color c000 = lut.data[lowerIndices.x + lowerIndices.y * lut.size + lowerIndices.z * lut.size * lut.size];
            Color c001 = lut.data[lowerIndices.x + lowerIndices.y * lut.size + upperIndices.z * lut.size * lut.size];
            Color c010 = lut.data[lowerIndices.x + upperIndices.y * lut.size + lowerIndices.z * lut.size * lut.size];
            Color c011 = lut.data[lowerIndices.x + upperIndices.y * lut.size + upperIndices.z * lut.size * lut.size];
            Color c100 = lut.data[upperIndices.x + lowerIndices.y * lut.size + lowerIndices.z * lut.size * lut.size];
            Color c101 = lut.data[upperIndices.x + lowerIndices.y * lut.size + upperIndices.z * lut.size * lut.size];
            Color c110 = lut.data[upperIndices.x + upperIndices.y * lut.size + lowerIndices.z * lut.size * lut.size];
            Color c111 = lut.data[upperIndices.x + upperIndices.y * lut.size + upperIndices.z * lut.size * lut.size];
            
            // Interpolate between the eight LUT entries to find the remapped value
            Vector3 t = indices - lowerIndices;
            Color a = Color.Lerp(Color.Lerp(c000, c100, t.x), Color.Lerp(c010, c110, t.x), t.y);
            Color b = Color.Lerp(Color.Lerp(c001, c101, t.x), Color.Lerp(c011, c111, t.x), t.y);
            Color result = Color.Lerp(a, b, t.z);
            
            return result;
        }

        private static Color Map(LutData lut, Color color) => lut.type.Equals("1D") ? Map1D(lut, color) : Map3D(lut, color);

        public static Texture3D Generate3DTexture(LutData lut, int size)
        {
            Texture3D texture = new Texture3D(size, size, size, TextureFormat.RGBAHalf, false)
            {
                anisoLevel = 0,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            
            Color[] colors = new Color[size * size * size];

            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float r = (float)x / (size - 1);
                        float g = (float)y / (size - 1);
                        float b = (float)z / (size - 1);
                        
                        Color color = new Color(r, g, b);

                        if (lut.size != 0) color = Map(lut, color);
                        
                        colors[x + y * size + z * size * size] = color;
                    }
                }
            }
            texture.SetPixels(colors, 0);
            texture.Apply();
            
            return texture;
        }

        public static Texture2D Generate2DTexture(LutData lut, int size)
        {
            Texture2D texture = new Texture2D(size * size, size, TextureFormat.RGB24, false, true);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        float r = x / (float)(size - 1);
                        float g = y / (float)(size - 1);
                        float b = z / (float)(size - 1);
                        Color color = new Color(r, g, b);
                        
                        if (lut.size != 0) color = Map(lut, color);

                        texture.SetPixel(z * size + x, y, color);
                    }
                }
            }
            
            texture.Apply();
            
            return texture;
        }
    }
}
