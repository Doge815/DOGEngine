using DOGEngine.GameObjects;
using DOGEngine.Shader;

namespace DOGEngine.GameObjects.Properties.Mesh;

public partial class TriangleMesh
{
    public static VertexDataBundle FromFile(string filePath) => FromFile(filePath, false);
    internal static VertexDataBundle FromFile(string filePath, bool verticesOnly)
    {
        if (File.Exists(filePath + ".agr")) return FromAgr(filePath + ".agr", verticesOnly);
        else if (File.Exists(filePath + ".obj"))
        {
            var data = FromObj(filePath + ".obj", verticesOnly);
            SaveAgr(in data, filePath + ".agr");
            return data;
        }
        else throw new FileNotFoundException($"Can't find {filePath}.obj or {filePath}.agr");
    }

    private static VertexDataBundle FromAgr(string filePath, bool verticesOnly)
    {
        static Type GetTye(ReadOnlySpan<char> name) => name switch
        {
            "normal" => typeof(NormalShaderAttribute),
            "vertex" => typeof(VertexShaderAttribute),
            "texture" => typeof(TextureCoordShaderAttribute),
            _ => throw new ArgumentException(nameof(name))
        };
        const int BufferSize = 1024 * 1;
        using var fileStream = File.OpenRead(filePath);
        using var streamReader = new StreamReader(fileStream, System.Text.Encoding.UTF8, true, BufferSize);
        string? line = streamReader.ReadLine();
        if (line != "AGRv1.0") throw new ArgumentException();
        Dictionary<Type, float[]> data = new ();
        while ((line = streamReader.ReadLine()) != null)
        {
            string? secondLine = streamReader.ReadLine();
            if (secondLine is null) throw new AggregateException();
            var firstSplit = line.SplitLine(',');
            firstSplit.MoveNext();
            Type cur = GetTye(firstSplit.Current);
            if(verticesOnly && cur != typeof(VertexShaderAttribute)) continue;
            firstSplit.MoveNext();
            float[] values = new float[int.Parse(firstSplit.Current)];
            int index = 0;
            foreach (ReadOnlySpan<char> value in secondLine.SplitLine(','))
            {
                values[index] = float.Parse(value);
                index++;
            }
            data.Add(cur, values);
        }

        return new VertexDataBundle(data, data[typeof(VertexShaderAttribute)].Length / 3);
    }

    private static void SaveAgr(in VertexDataBundle data, string filePath)
    {
        string GetName(Type t)
        {
            if (t == typeof(VertexShaderAttribute)) return "vertex";
            else if (t == typeof(TextureCoordShaderAttribute)) return "texture";
            else if (t == typeof(NormalShaderAttribute)) return "normal";
            throw new ArgumentException(nameof(t));
        }
        using StreamWriter file = new(filePath);
        file.WriteLine("AGRv1.0");
        foreach (var pair in data.Data)
        {
            file.WriteLine($"{GetName(pair.Key)},{pair.Value.Length}");
            foreach (float value in pair.Value)
            {
                file.Write(value);
                file.Write(',');
            }
            file.WriteLine();
        }
        file.Close();
    }
    private static VertexDataBundle FromObj(string filePath, bool verticesOnly)
    {
        List<(float, float, float)> vertices = new ();
        List<(float, float, float)> normals= new ();
        List<(float, float)> textureCoords= new ();
        List<((int, int?, int?), (int, int?, int?), (int, int?, int?))> faces = new ();
        
        void ParseVertex(string[] split)
        {
            if (split.Length != 4) throw new ArgumentException("Bad line");
            if (float.TryParse(split[1], out float X) && float.TryParse(split[2], out float Y) &&
                float.TryParse(split[3], out float Z))

                lock (vertices)
                    vertices.Add(new (X, Y, Z));
            else
                throw new ArgumentException("Bad line");
        }
        
        void ParseNormal(string[] split)
        {
            if(verticesOnly) return;
            if (split.Length != 4) throw new ArgumentException("Bad line");
            if (float.TryParse(split[1], out float X) && float.TryParse(split[2], out float Y) &&
                float.TryParse(split[3], out float Z))
                lock (normals)
                    normals.Add(new (X, Y, Z));
            else
                throw new ArgumentException("Bad line");
        }
        
        void ParseTextureCoord(string[] split)
        {
            if(verticesOnly) return;
            if (split.Length != 3) throw new ArgumentException("Bad line");
            if (float.TryParse(split[1], out float X) && float.TryParse(split[2], out float Y))
                lock (textureCoords)
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
                lock (faces)
                    faces.Add(new (X, Y, Z));
            else
                throw new ArgumentException("Bad line");
        }

        void ParseLine(string line)
        {
            if(line.Length == 0) return;
            if(line[0] == '#') return;
            var split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(split.Length == 0) return;
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
        
        foreach (string line in File.ReadLines(filePath))
            ParseLine(line);

        float[] verts = new float[faces.Count * 9];
        float[]? coords = verticesOnly ? null : faces.First().Item1.Item2 is null? null : new float[faces.Count * 6];
        float[]? norms = verticesOnly ? null : faces.First().Item1.Item3 is null? null : new float[faces.Count * 9];

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
    
    public static readonly VertexDataBundle Triangle = new VertexDataBundle(new Dictionary<Type, float[]>()
    {
        { typeof(VertexShaderAttribute), new[]
            {
                -0.5f, -0.5f, 0f, 
                0.5f, -0.5f, 0f, 
                0.5f, 0.5f, 0f, 
            }
        },
        { typeof(TextureCoordShaderAttribute), new[]
            {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
            }
        },
    }, 3);
    public static readonly VertexDataBundle Cube = new VertexDataBundle(new Dictionary<Type, float[]>()
    {
        { typeof(VertexShaderAttribute), new[]
            {
                -0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, -0.5f, 
                0.5f, 0.5f, -0.5f, 
                0.5f, 0.5f, -0.5f, 
                -0.5f, 0.5f, -0.5f, 
                -0.5f, -0.5f, -0.5f, 

                -0.5f, -0.5f, 0.5f, 
                0.5f, -0.5f, 0.5f, 
                0.5f, 0.5f, 0.5f, 
                0.5f, 0.5f, 0.5f, 
                -0.5f, 0.5f, 0.5f, 
                -0.5f, -0.5f, 0.5f, 

                -0.5f, 0.5f, 0.5f, 
                -0.5f, 0.5f, -0.5f, 
                -0.5f, -0.5f, -0.5f, 
                -0.5f, -0.5f, -0.5f, 
                -0.5f, -0.5f, 0.5f, 
                -0.5f, 0.5f, 0.5f, 

                0.5f, 0.5f, 0.5f, 
                0.5f, 0.5f, -0.5f, 
                0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, 0.5f, 
                0.5f, 0.5f, 0.5f, 

                -0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, 0.5f, 
                0.5f, -0.5f, 0.5f, 
                -0.5f, -0.5f, 0.5f, 
                -0.5f, -0.5f, -0.5f, 

                -0.5f, 0.5f, -0.5f, 
                0.5f, 0.5f, -0.5f, 
                0.5f, 0.5f, 0.5f, 
                0.5f, 0.5f, 0.5f, 
                -0.5f, 0.5f, 0.5f, 
                -0.5f, 0.5f, -0.5f, 
            }
        },
        { typeof(TextureCoordShaderAttribute), new[]
            {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 0.0f,

                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 0.0f,

                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 0.0f,
                1.0f, 0.0f,

                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 0.0f,
                1.0f, 0.0f,

                0.0f, 1.0f,
                1.0f, 1.0f,
                1.0f, 0.0f,
                1.0f, 0.0f,
                0.0f, 0.0f,
                0.0f, 1.0f,

                0.0f, 1.0f,
                1.0f, 1.0f,
                1.0f, 0.0f,
                1.0f, 0.0f,
                0.0f, 0.0f,
                0.0f, 1.0f
            }
        },
    }, 36);
}