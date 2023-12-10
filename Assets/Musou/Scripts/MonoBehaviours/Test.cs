using System.IO;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Sprite[] sprites;

    void Start()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            var sprite = sprites[i];
            var spritePixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width,
                (int)sprite.rect.height);
            // 根据精灵生成一张png图片
            var tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
            tex.SetPixels(spritePixels);
            tex.Apply();

            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes($"{Application.dataPath}/Musou/Textures/{sprite.name}.png",
                bytes);

            Destroy(tex);
        }
    }
}