using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

public class Engine
{
    #region Setup
    public static int Width = 84;
    public static int Height = 24;
    public static int DialogueHeight = 5;
    public static int TargetFPS = 12;
    private static int frameCount = 1;

    public static void PrepareConsoleForUnicode()
    {
        int windowY = Height + DialogueHeight + 4;
        int windowX = Width + 2;
        Console.OutputEncoding = Encoding.UTF8;
        Console.SetWindowSize(windowX, windowY);
    }
    #endregion

    #region Main Loop
    public static void Run(Script script)
    {
        PrepareConsoleForUnicode();
        script.Start();

        Stopwatch stopwatch = new Stopwatch();
        float targetFrameTime = 1000f / TargetFPS;

        while (!script.IsDone)
        {
            stopwatch.Restart();
            script.Update();
            stopwatch.Stop();

            float frameTime = stopwatch.ElapsedMilliseconds;
            float delay = targetFrameTime - frameTime;

            if (delay > 0)
            {
                Thread.Sleep((int)delay);
            }
            frameCount++;
        }
    }
    #endregion

    #region Drawing
    public static void DrawCanvas(ConsoleColor color = ConsoleColor.Red)
    {
        Console.ForegroundColor = color;
        StringBuilder canvasBuilder = new StringBuilder();
        canvasBuilder.AppendLine(new string('#', Width + 2));

        for (int i = 0; i < Height; i++)
        {
            canvasBuilder.Append('#').Append(new string(' ', Width)).AppendLine("#");
        }

        if (Height > 1)
        {
            canvasBuilder.AppendLine(new string('#', Width + 2));
        }

        for (int i = 0; i < DialogueHeight; i++)
        {
            canvasBuilder.Append('#').Append(new string(' ', Width)).AppendLine("#");
        }

        if (DialogueHeight > 1)
        {
            canvasBuilder.AppendLine(new string('#', Width + 2));
        }

        Console.Write(canvasBuilder.ToString());
    }

    public static void DrawImage(string[] image, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;

        for (int y = 0; y < Height; y++)
        {
            char[] characters = image[y % image.Length].ToCharArray();

            for (int x = 0; x < Width; x++)
            {
                DrawLine(x, y, RenderUnicodeChar(characters[x % characters.Length]));
            }
        }
    }

    public static void DisplayText(string text)
    {
        StringBuilder textBuilder = new StringBuilder();
        foreach (char c in text)
        {
            textBuilder.Append(RenderUnicodeChar(c));
        }
        DrawLine(2, Height + 2, textBuilder.ToString());
    }

    private static void DrawLine(int x, int y, string line)
    {
        Console.SetCursorPosition(x + 1, y + 1);
        Console.Write(line);
    }

    private static string RenderUnicodeChar(char c)
    {
        return c == '\0' || char.IsWhiteSpace(c) ? " " : c.ToString();
    }
    #endregion

    #region File Handling
    public static string[] GetObjectInFile(string fileName, string objectName)
    {
        string[] contents = File.ReadAllLines(fileName + ".txt", Encoding.UTF8);
        List<string> objectContent = new List<string>();
        bool insideTargetObject = false;

        foreach (string line in contents)
        {
            if (line.StartsWith(objectName + ":"))
            {
                insideTargetObject = true;
                continue;
            }

            if (insideTargetObject)
            {
                if (line == "{") continue;
                if (line == "}") break;
                objectContent.Add(line);
            }
        }

        return objectContent.ToArray();
    }
    #endregion

    #region Virtual Methods
    public virtual void Start()
    {
        DrawCanvas();
        DrawImage(GetObjectInFile("Backgrounds", "Hallway"));
    }

    public virtual void Update()
    {
        DrawImage(GetObjectInFile("Characters", "Avery"));
        DisplayText("This is a very long sentence");
    }
    #endregion
}
