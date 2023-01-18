using DOGEngine.Shader;

namespace DOGEngine.RenderObjects;

public readonly struct VertexDataBundle
{
    internal Dictionary<Type, float[]> Data { get; }
    public int Rows { get; }
    public float[] CreateVertices(Shader.Shader shader)
    {
        int columns = 0;
        foreach (IShaderAttribute attribute in shader.Attributes)
            columns += attribute.Size;

        float[] verts = new float[columns * Rows];
        foreach (IShaderAttribute attribute in shader.Attributes)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j <  attribute.Size; j++)
                {
                    int writePosition = i * columns + j + attribute.Offset;
                    if (Data.TryGetValue(attribute.GetType(), out var attributeData))
                        verts[writePosition] = attributeData[i * attribute.Size + j];
                    else
                        verts[writePosition] = 0;
                }
            }
        }
        return verts;
    }

    public VertexDataBundle(Dictionary<Type, float[]> data, int rows)
    {
        Data = data;
        Rows = rows;
    }
}