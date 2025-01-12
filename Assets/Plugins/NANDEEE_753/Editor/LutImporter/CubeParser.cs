using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace NANDEEE_753.Editor.LutImporter
{
    public static class CubeParser
    {
        private static LutData _lut;
        private static List<Vector3> _data;
        
        public static LutData Parse(string filePath)
        {
            _lut = new LutData();
            
            string[] lines = File.ReadAllLines(filePath);
            int index = 0;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                
                // Skip whitespace
                if (trimmedLine.Length < 1) continue;
                // Skip comment
                if (trimmedLine.StartsWith('#')) continue;
                
                HandleLine(trimmedLine, ref index);
            }

            return _lut;
        }

        private static void HandleLine(string trimmedLine, ref int index)
        {
            string[] tokens = trimmedLine.Split(' ');
            
            switch (tokens[0])
            {
                case "TITLE":
                    _lut.title = trimmedLine.Substring("TITLE".Length).Trim();
                    break;
                case "LUT_1D_SIZE":
                    _lut.size = int.Parse(tokens[1]);
                    _lut.type = "1D";
                    _lut.data = new Color[_lut.size];
                    break;
                case "LUT_3D_SIZE":
                    _lut.size = int.Parse(tokens[1]);
                    _lut.type = "3D";
                    _lut.data = new Color[_lut.size * _lut.size * _lut.size];
                    break;
                case "LUT_1D_INPUT_RANGE":
                case "LUT_3D_INPUT_RANGE":
                    _lut.inputRangeMin = float.Parse(tokens[1]);
                    _lut.inputRangeMax = float.Parse(tokens[2]);
                    break;
                case "DOMAIN_MIN":
                case "DOMAIN_MAX":
                {
                    _lut.usesDomainMinMax = true;
                    float r = float.Parse(tokens[1]);
                    float g = float.Parse(tokens[2]);
                    float b = float.Parse(tokens[3]);
                    if (tokens[0] == "DOMAIN_MIN") _lut.domainMin = new Vector3(r, g, b);
                    else _lut.domainMax = new Vector3(r, g, b);
                    break;
                }
                default:
                    try
                    {
                        float r = float.Parse(tokens[0]);
                        float g = float.Parse(tokens[1]);
                        float b = float.Parse(tokens[2]);

                        if (_lut.data != null && index < _lut.data.Length) _lut.data[index++] = new Color(r, g, b);
                    }
                    catch (FormatException)
                    {
                        Debug.LogError($"FormatException on line: {trimmedLine}");
                        throw;
                    }
                    break;
            }
        }
    }
}