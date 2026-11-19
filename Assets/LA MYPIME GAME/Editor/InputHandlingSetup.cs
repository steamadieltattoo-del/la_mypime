#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Configura automáticamente Active Input Handling = Both al importar el proyecto
/// y abre la escena demo cuando el Editor termina de compilar/importar.
/// Usa reflexión para no depender del nombre exacto del enum entre versiones de Unity.
/// </summary>
[InitializeOnLoad]
public static class InputHandlingSetup
{
    const string EscenaDemo = "Assets/LA MYPIME GAME/Scenes/DemoMypime.unity";
    static int framesEspera;

    static InputHandlingSetup()
    {
        ConfigurarInput();

        // En el Editor (nunca en batchmode) abrimos la escena demo si no hay ninguna abierta.
        if (!Application.isBatchMode)
        {
            framesEspera = 0;
            EditorApplication.update += IntentarAbrirEscenaDemo;
        }
    }

    static void ConfigurarInput()
    {
        try
        {
            Type enumType = typeof(PlayerSettings).Assembly.GetType("UnityEditor.ActiveInputHandler");
            if (enumType == null) return;

            object both = Enum.Parse(enumType, "Both");
            PropertyInfo prop = typeof(PlayerSettings).GetProperty(
                "activeInputHandler", BindingFlags.Public | BindingFlags.Static);
            if (prop == null) return;

            if (!Equals(prop.GetValue(null, null), both))
            {
                prop.SetValue(null, both, null);
                AssetDatabase.SaveAssets();
                Debug.Log("[LA MYPIME GAME] Active Input Handling = Both configurado automáticamente.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[LA MYPIME GAME] No se pudo configurar el input automáticamente: " + e.Message);
        }
    }

    static void IntentarAbrirEscenaDemo()
    {
        // Esperar a que Unity termine de compilar e importar.
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            framesEspera = 0;
            return;
        }

        framesEspera++;
        if (framesEspera < 5) return;

        // Si ya hay una escena abierta, no tocamos nada.
        if (!string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
        {
            EditorApplication.update -= IntentarAbrirEscenaDemo;
            return;
        }

        // La escena debe existir ya en la base de datos de assets.
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(EscenaDemo) == null)
            return; // todavía no importada: seguir esperando

        EditorApplication.update -= IntentarAbrirEscenaDemo;
        AbrirEscenaDemo();
    }

    /// <summary>Abre la escena demo (también disponible en el menú superior).</summary>
    [MenuItem("LA MYPIME GAME/Abrir escena demo")]
    public static void AbrirEscenaDemo()
    {
        EditorSceneManager.OpenScene(EscenaDemo);
        Debug.Log("[LA MYPIME GAME] Escena abierta: " + EscenaDemo);
    }
}
#endif
