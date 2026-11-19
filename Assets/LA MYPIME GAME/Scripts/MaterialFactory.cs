using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crea materiales de color y los reutiliza para no generar cientos de materiales distintos.
/// Funciona tanto con URP como con el pipeline estándar.
/// </summary>
public static class MaterialFactory
{
    private static Shader shader;
    private static readonly Dictionary<Color, Material> cache = new Dictionary<Color, Material>();

    public static Shader CurrentShader
    {
        get
        {
            if (shader != null) return shader;

            string[] candidates =
            {
                "Universal Render Pipeline/Lit",
                "Standard",
                "Diffuse",
                "Sprites/Default"
            };

            foreach (string name in candidates)
            {
                shader = Shader.Find(name);
                if (shader != null) return shader;
            }
            return null;
        }
    }

    public static Material Get(Color color)
    {
        color.a = 1f;

        Material material;
        if (!cache.TryGetValue(color, out material) || material == null)
        {
            Shader s = CurrentShader;
            if (s == null)
            {
                Debug.LogWarning("No se encontró ningún shader válido.");
                return null;
            }
            material = new Material(s);
            material.color = color;
            cache[color] = material;
        }
        return material;
    }
}
