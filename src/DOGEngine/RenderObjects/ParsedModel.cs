using System.Runtime.Intrinsics.X86;
using DOGEngine.Shader;
using OpenTK.Mathematics;

namespace DOGEngine.RenderObjects;

public class ParsedModel : VertexRenderObject
{
    public ParsedModel(string filePath, Shader.Shader shader, Vector3? position = null, Vector3? orientation = null, Vector3? scale = null, Vector3? orientationOffset = null) : base(shader, position, orientation, scale, orientationOffset, ParseFile(filePath)) {}

    private static VertexDataBundle ParseFile(string filePath)
    {
        List<(float, float, float)> vertices = new List<(float, float, float)>();
        List<(float, float, float)> normals= new List<(float, float, float)>();
        List<(float, float)> textureCoords= new List<(float, float)>();
        List<((int, int?, int?), (int, int?, int?), (int, int?, int?))> faces =
            new List<((int, int?, int?), (int, int?, int?), (int, int?, int?))>();
        void ParseVertex(string[] split)
        {
            if (split.Length != 4) throw new ArgumentException("Bad line");
            if(float.TryParse(split[1], out float X) && float.TryParse(split[2], out float Y) && float.TryParse(split[3], out float Z))
                vertices.Add(new (X, Y, Z));
            else
                throw new ArgumentException("Bad line");
        }
        
        void ParseNormal(string[] split)
        {
            if (split.Length != 4) throw new ArgumentException("Bad line");
            if(float.TryParse(split[1], out float X) && float.TryParse(split[2], out float Y) && float.TryParse(split[3], out float Z))
                normals.Add(new (X, Y, Z));
            else
                throw new ArgumentException("Bad line");
        }
        
        void ParseTextureCoord(string[] split)
        {
            if (split.Length != 3) throw new ArgumentException("Bad line");
            if(float.TryParse(split[1], out float X) && float.TryParse(split[2], out float Y))
                textureCoords.Add(new (X, Y));
            else
                throw new ArgumentException("Bad line");
        }
        
        void ParseFace(string[] split)
        {
            bool TryParseVertex(string str, out (int, int?, int?) result)
            {
                result = new(0, null, null);
                if (str.Length == 0) return false;
                var indices = str.Split('/');
                if (indices.Length == 0 || indices.Length > 3) return false;
                if (int.TryParse(indices[0], out int first))
                    result.Item1 = first;
                else
                    return false;
                if (indices.Length == 1) return true;
                if (indices[1] != "" && int.TryParse(indices[1], out int second))
                    result.Item2 = second;
                else
                    return false;
                if (indices.Length == 2) return true;
                if (indices[2] != "" && int.TryParse(indices[2], out int third))
                    result.Item3 = third;
                else
                    return false;
                return true;
            }
            if (split.Length != 4) throw new ArgumentException("Bad line");
            if(TryParseVertex(split[1], out var X) && TryParseVertex(split[2], out var Y) && TryParseVertex(split[3], out var Z))
                faces.Add(new (X, Y, Z));
            else
                throw new ArgumentException("Bad line");
        }
        
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if(line.Length == 0) continue;
            if(line[0] == '#') continue;
            var split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(split.Length == 0) continue;
            switch (split[0])
            {
                case "v": ParseVertex(split); break;
                case "vt": ParseTextureCoord(split); break;
                case "vn": ParseNormal(split); break;
                case "f": ParseFace(split); break;
                case "o": break; //object name 
                case "s": break; //smooth shading
                case "mtllib": break; //referenced material file
                case "usemtl": break; //referenced material
                default: throw new ArgumentException("Bad line");
            }
        }

        float[] verts = new float[faces.Count * 9];
        float[]? coords = faces.First().Item1.Item2 is null? null : new float[faces.Count * 6];
        float[]? norms = faces.First().Item1.Item3 is null? null : new float[faces.Count * 9];

        for (int i = 0; i < faces.Count; i++)
        {
            var face = faces[i];
            var vertex1 = vertices[face.Item1.Item1 - 1];
            var vertex2 = vertices[face.Item2.Item1 - 1];
            var vertex3 = vertices[face.Item3.Item1 - 1];
            verts[i * 9 + 0] = vertex1.Item1;
            verts[i * 9 + 1] = vertex1.Item2;
            verts[i * 9 + 2] = vertex1.Item3;
            verts[i * 9 + 3] = vertex2.Item1;
            verts[i * 9 + 4] = vertex2.Item2;
            verts[i * 9 + 5] = vertex2.Item3;
            verts[i * 9 + 6] = vertex3.Item1;
            verts[i * 9 + 7] = vertex3.Item2;
            verts[i * 9 + 8] = vertex3.Item3;

            if (coords is not null)
            {
                var coord1 = textureCoords[face.Item1.Item2!.Value - 1];
                var coord2 = textureCoords[face.Item2.Item2!.Value - 1];
                var coord3 = textureCoords[face.Item3.Item2!.Value - 1];
                coords[i * 6 + 0] = coord1.Item1;
                coords[i * 6 + 1] = coord1.Item2;
                coords[i * 6 + 2] = coord2.Item1;
                coords[i * 6 + 3] = coord2.Item2;
                coords[i * 6 + 4] = coord3.Item1;
                coords[i * 6 + 5] = coord3.Item2;
            }
            if (norms is not null)
            {
                var norm1 = normals[face.Item2.Item3!.Value - 1];
                var norm2 = normals[face.Item2.Item3!.Value - 1];
                var norm3 = normals[face.Item3.Item3!.Value - 1];
                norms[i * 9 + 0] = norm1.Item1;
                norms[i * 9 + 1] = norm1.Item2;
                norms[i * 9 + 2] = norm1.Item3;
                norms[i * 9 + 3] = norm2.Item1;
                norms[i * 9 + 4] = norm2.Item2;
                norms[i * 9 + 5] = norm2.Item3;
                norms[i * 9 + 6] = norm3.Item1;
                norms[i * 9 + 7] = norm3.Item2;
                norms[i * 9 + 8] = norm3.Item3;
            }
        }

        Dictionary<Type, float[]> data = new ();
        data.Add(typeof(VertexShaderAttribute), verts);
        if(coords is not null)
            data.Add(typeof(TextureCoordShaderAttribute), coords);
        if(norms is not null)
            data.Add(typeof(NormalShaderAttribute), norms);
        return new VertexDataBundle(data, faces.Count * 3);
    }
}