using DOGEngine.Shader;
using DOGEngine.Texture;

namespace DOGEngine.GameObjects.Text;

public class Sprite2D : GameObject
{
    private static Sprite2DShader? _sprite2DShader;
    private static Sprite2DShader Shader {get {
        if (_sprite2DShader is not null) return _sprite2DShader;
        _sprite2DShader = new Sprite2DShader();
        return _sprite2DShader;
    }}
    public ITexture Texture { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Corner RelativePosition { get; set; }


    public Sprite2D(ITexture texture, Vector2 position, Vector2 size, Corner relativePosition)
    {
        Texture = texture;
        Position = position;
        Size = size;
        RelativePosition = relativePosition;
    }

    public void Draw(int width, int height)
    {
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, 0, height, 0, 1);
        Shader.SetMatrix4("projection", projection);
        Shader.UseTextShader(null, projection);
        BindBuffer();
        
            float x = Position.X ;
            float y = Position.Y ;
            float w = Size.X;
            float h = Size.Y;

            (float offX, float offY) = RenderText.GetOffset(RelativePosition, width, height);
            x += offX;
            y += offY;
            
            float[] pos = new float[] {
                x, y + h, 0, 0,
                x, y, 0, 1,
                x + w, y, 1, 1,
                x, y + h, 0, 0,
                x + w, y, 1, 1,
                x + w, y + h, 1, 0
            };
            Texture.Use();
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*pos.Length, pos);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
    
    private static (int vao, int vbo)? spriteVOs;
    private static void BindBuffer()
    {
        if (spriteVOs is null)
        {
            GL.GenVertexArrays(1, out int vao);
            GL.GenBuffers(1, out int vbo);

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 24, 0, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4* sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4* sizeof(float), 2 * sizeof(float));
            GL.BindVertexArray(0);

            spriteVOs = new(vao, vbo);
        }
        GL.BindVertexArray(spriteVOs.Value.vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, spriteVOs.Value.vbo);
    }
}