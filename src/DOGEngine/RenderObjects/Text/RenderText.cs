using DOGEngine.Shader;
using DOGEngine.Texture;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;

namespace DOGEngine.RenderObjects.Text;

public class RenderText : GameObject
{
    private static TextShader? _textShader;
    private static TextShader Shader {get {
        if (_textShader is not null) return _textShader;
        _textShader = new TextShader();
        return _textShader;
    }}
    public Vector3 Color { get; set; }
    public string Text { get; set; }
    public Font Font { get; set; }
    public float Scale { get; set; }
    public Vector2 Position { get; set; }
    public Corner RelativePosition { get; set; }

    public RenderText(Font font, string? text = null, Vector2? position = null, Corner? relativePosition = null, Vector3? color = null,
        float? scale = null)
    {
        Font = font;
        Text = text ?? string.Empty;
        Position = position ?? Vector2.Zero;
        RelativePosition = relativePosition ?? Corner.BottomLeft;
        Color = color ?? Vector3.Zero;
        Scale = scale ?? 1;
    }
    
    public void Draw(int width, int height)
    {
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, 0, height, 0, 1);
        Shader.UseTextShader(null, Color, projection);
        RenderCharacterProvider.BindBuffer();

        int offset = 0;
        foreach (char c in Text)
        {

            RenderCharacter ch = RenderCharacterProvider.GetRenderCharacter(Font, c);
            float x = Position.X + offset + ch.Bearing.X * Scale;
            float y = Position.Y - (ch.Size.Y - ch.Bearing.Y) * Scale;
            float w = ch.Size.X * Scale;
            float h = ch.Size.Y * Scale;

            if (RelativePosition == Corner.TopLeft) y += height;
            if (RelativePosition == Corner.BottomRight) x += width;
            if (RelativePosition == Corner.TopRight)
            {
                y += height;
                x += width;
            }
            
            float[] pos = new float[] {
                x, y + h, 0, 0,
                x, y, 0, 1,
                x + w, y, 1, 1,
                x, y + h, 0, 0,
                x + w, y, 1, 1,
                x + w, y + h, 1, 0
            };
            Shader.UseTextShader(ch);
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*pos.Length, pos);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            offset += (int)(ch.Offset * Scale);
        }
    }
}

public enum Corner
{
    TopLeft,
    BottomLeft,
    TopRight,
    BottomRight
}

internal static class RenderCharacterProvider
{
    private static (int vao, int vbo)? textVOs;
    private static Dictionary<Font, Dictionary<char, RenderCharacter>> fonts { get; set; } =
        new Dictionary<Font, Dictionary<char, RenderCharacter>>();
    internal static RenderCharacter GetRenderCharacter(Font font, char character)
    {
        if (fonts.TryGetValue(font, out var fontFamily))
        {
            if (fontFamily.TryGetValue(character, out var renderCharacter)) return renderCharacter;
            throw new AggregateException(nameof(character) + " out of range");
        }
        else
        {
            createFont(font);
            return GetRenderCharacter(font, character);
        }
    }

    private static void createFont(Font font)
    {
        var characters = new Dictionary<char, RenderCharacter>();
        FreeTypeSharp.FreeTypeLibrary lib = new FreeTypeSharp.FreeTypeLibrary();
        FT_Error error;

        error = FT_New_Face(lib.Native, font.FontFile, 0, out var face);
        if(error != 0) throw new AggregateException(error.ToString());
        FreeTypeSharp.FreeTypeFaceFacade faceWrapper = new FreeTypeSharp.FreeTypeFaceFacade(lib, face);
        
        error = FT_Set_Pixel_Sizes(faceWrapper.Face, 0, font.Height);
        if(error != 0) throw new AggregateException(error.ToString());
        
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

        RenderCharacter createCharacter(char c){
            error = FT_Load_Char(faceWrapper.Face, Convert.ToUInt32(c), FT_LOAD_RENDER);
            if(error != 0) throw new AggregateException(error.ToString());

            GL.GenTextures(1, out int texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.CompressedRed, (int)faceWrapper.GlyphBitmap.width, (int)faceWrapper.GlyphBitmap.rows, 0, PixelFormat.Red, PixelType.UnsignedByte, faceWrapper.GlyphBitmap.buffer);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            return new RenderCharacter( texture, new Vector2(faceWrapper.GlyphBitmap.width, faceWrapper.GlyphBitmap.rows), new Vector2(faceWrapper.GlyphBitmapLeft, faceWrapper.GlyphBitmapTop),  (uint) faceWrapper.GlyphMetricHorizontalAdvance);
        }
        
        for (byte b = 0; b < 255; b++)
        {
            characters.Add((char)b, createCharacter(((char)b)));
        }
        FT_Done_Face(faceWrapper.Face);
        FT_Done_FreeType(lib.Native);
        
        fonts.Add(font, characters);
    }

    internal static void BindBuffer()
    {
        if (textVOs is null)
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

            textVOs = new(vao, vbo);
        }
        GL.BindVertexArray(textVOs.Value.vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, textVOs.Value.vbo);
    }
}
