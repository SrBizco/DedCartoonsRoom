using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildWebGL
{
    [MenuItem("Ded Cartoons/Build WebGL for Website")]
    public static void BuildForWebsite()
    {
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var outputPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "DedCartoons", "public", "room"));
        BuildAt(outputPath);
    }

    public static void Build()
    {
        var outputPath = GetCommandLineArgument("-webglOutput");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Missing -webglOutput <absolute path>.");
        }

        BuildAt(outputPath);
    }

    private static void BuildAt(string outputPath)
    {
        var scenes = EditorBuildSettings.scenes;
        Directory.CreateDirectory(outputPath);

        var report = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.WebGL, BuildOptions.None);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"WebGL build failed: {report.summary.result}.");
        }

        CopyWebGlVideos(outputPath);
        CopyEnterVideoForWebsite();
        AddWebGlEntryBridge(outputPath);
    }

    private static string GetCommandLineArgument(string argumentName)
    {
        var arguments = Environment.GetCommandLineArgs();
        for (var index = 0; index < arguments.Length - 1; index++)
        {
            if (arguments[index] == argumentName)
            {
                return arguments[index + 1];
            }
        }

        return null;
    }

    private static void CopyWebGlVideos(string outputPath)
    {
        var sourceDirectory = Path.Combine(Application.dataPath, "Assets");
        var destinationDirectory = Path.Combine(outputPath, "StreamingAssets", "Videos");
        Directory.CreateDirectory(destinationDirectory);

        CopyVideo(sourceDirectory, "BG", "FONDO WEB.mp4", destinationDirectory, "background.mp4");
        CopyVideo(sourceDirectory, "BG", "WEBSITE INTRO OFICIAL.mp4", destinationDirectory, "intro.mp4");
        CopyVideo(sourceDirectory, "Dander", "DANDER TALK MP4.mp4", destinationDirectory, "dander-talk.mp4");
    }

    private static void CopyVideo(string sourceDirectory, string folder, string sourceFileName, string destinationDirectory, string destinationFileName)
    {
        var sourcePath = Path.Combine(sourceDirectory, folder, sourceFileName);
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"WebGL video not found: {sourcePath}", sourcePath);
        }

        File.Copy(sourcePath, Path.Combine(destinationDirectory, destinationFileName), true);
    }

    private static void CopyEnterVideoForWebsite()
    {
        var sourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Enter input.mp4");
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Entry video not found: {sourcePath}", sourcePath);
        }

        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var destinationDirectory = Path.GetFullPath(Path.Combine(projectRoot, "..", "DedCartoons", "public", "assets"));
        Directory.CreateDirectory(destinationDirectory);
        File.Copy(sourcePath, Path.Combine(destinationDirectory, "Enter-input.mp4"), true);
    }

    private static void AddWebGlEntryBridge(string outputPath)
    {
        var indexPath = Path.Combine(outputPath, "index.html");
        var html = File.ReadAllText(indexPath);
        html = Regex.Replace(html, @"canvas\.style\.width = ""\d+px"";\s*canvas\.style\.height = ""\d+px"";", @"canvas.style.width = ""960px"";
        canvas.style.height = ""540px"";");
        html = html.Replace("// config.matchWebGLToCanvasSize = false;", "config.matchWebGLToCanvasSize = false;");
        const string instanceHook = "window.dedUnityInstance = unityInstance;";

        if (!html.Contains(instanceHook))
        {
            html = html.Replace("}).then((unityInstance) => {", $"}}).then((unityInstance) => {{\n                {instanceHook}\n                window.dedOnUnityReady?.();");
        }

        const string integrationStyleMarker = "/* Ded Cartoons WebGL integration */";
        if (!html.Contains(integrationStyleMarker))
        {
            var integrationStyle = $@"
    <style>
      {integrationStyleMarker}
      html, body {{ width: 100%; height: 100%; margin: 0; overflow: hidden; background: #000; }}
      #unity-container.unity-desktop {{ transform: translate(-50%, -50%) scale(1.35); }}
      #unity-footer {{ display: none; }}
    </style>";
            html = html.Replace("</head>", $"{integrationStyle}\n  </head>");
        }

        const string bridgeMarker = "<!-- Ded Cartoons entry bridge -->";
        if (!html.Contains(bridgeMarker))
        {
            var bridge = $@"
    {bridgeMarker}
    <script>
      (() => {{
        let startRequested = false;
        const startIntro = () => {{
          startRequested = true;
          if (window.dedUnityInstance) {{
            window.dedUnityInstance.SendMessage('IntroScreen', 'RestartIntroAfterUserGesture');
            startRequested = false;
          }}
        }};

        window.dedOnUnityReady = () => {{
          if (startRequested) startIntro();
        }};

        window.addEventListener('message', (event) => {{
          if (event.data === 'ded-cartoons-enter') startIntro();
        }});
      }})();
    </script>";
            html = html.Replace("</body>", $"{bridge}\n  </body>");
        }

        File.WriteAllText(indexPath, html);
    }
}
