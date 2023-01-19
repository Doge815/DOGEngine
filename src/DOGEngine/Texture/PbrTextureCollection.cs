namespace DOGEngine.Texture;

public class PbrTextureCollection : ITexture
{
    public Texture AlbedoTexture { get; set; }
    public Texture NormalTexture { get; set; }
    public Texture MetallicTexture { get; set; }
    public Texture RoughnessTexture { get; set; }
    public Texture AoTexture { get; set; }

    public PbrTextureCollection(string directory)
    {
        AlbedoTexture = new Texture(directory + "/albedo.png");
        NormalTexture = new Texture(directory + "/normal.png");
        MetallicTexture = new Texture(directory + "/metallic.png");
        RoughnessTexture = new Texture(directory + "/roughness.png");
        AoTexture = new Texture(directory + "/ao.png");
    }

    public void Use()
    {
        AlbedoTexture.Use(TextureUnit.Texture0);
        NormalTexture.Use(TextureUnit.Texture1);
        MetallicTexture.Use(TextureUnit.Texture2);
        RoughnessTexture.Use(TextureUnit.Texture3);
        AoTexture.Use(TextureUnit.Texture4);
    }
}