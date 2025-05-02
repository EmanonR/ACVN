using System;
using System.IO;
using System.Text;
using System.Media;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

public class Engine
{
    #region Setup
    static SoundPlayer bgmSoundPlayer = new SoundPlayer();

    public static int Width = 84;
    public static int Height = 21;
    public static int DialogueHeight = 5;

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
        Console.CursorVisible = false;
        PrepareConsoleForUnicode();
        script.Start();
    }
    #endregion

    #region Drawing
    public static void DrawCanvas(ConsoleColor color = ConsoleColor.Red)
    {
        // Canvas color
        Console.ForegroundColor = color;    
        StringBuilder canvasBuilder = new StringBuilder();

        // Top Line
        canvasBuilder.AppendLine(new string('#', Width + 2));

        // For each Height Line
        for (int i = 0; i < Height; i++)
        {
            canvasBuilder.Append('#').Append(new string(' ', Width)).AppendLine("#");
        }

        // Sepperator Line
        canvasBuilder.AppendLine(new string('#', Width + 2));

        // DialogueBox
        for (int i = 0; i < DialogueHeight; i++)
        {
            canvasBuilder.Append('#').Append(new string(' ', Width)).AppendLine("#");
        }

        // Bottom Line
        canvasBuilder.AppendLine(new string('#', Width + 2));


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

    public static void DrawSprite(string[] image, ConsoleColor color = ConsoleColor.Green)
    {
        Console.ForegroundColor = color;

        for (int y = 0; y < image.Length; y++)
        {
            char[] characters = image[y].ToCharArray();

            for (int x = 0; x < characters.Length; x++)
            {
                DrawLine((Width / 2) - (characters.Length / 2) + x, Height - image.Length + y, RenderUnicodeChar(characters[x]));
            }
        }
    }

    public static void ClearTextBox()
    {
        for (int y = 0; y < DialogueHeight; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                DrawLine(x, Height + 1 + y, " ");
            }
        }
    }

    public static void ClearCanvas()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                DrawLine(x, y, " ");
            }
        }
    }

    public static void PrintNewLine(string text, bool autoForward = false, ConsoleColor color = ConsoleColor.Green)
    {
        Console.ForegroundColor = color;

        ClearTextBox();
        PrintText(text);

        //Console.SetCursorPosition(3, Height + DialogueHeight);

        if (!autoForward)
            Console.ReadKey(true);
    }

    public static void PrintText(string text)
    {
        StringBuilder textBuilder = new StringBuilder();

        // Counting character in string
        int characterCounter = 0;

        // All lines, devided by length, Auto wrapping
        List<string> lines = new List<string>();

        // For each char in whole text
        for (int c = 0; c < text.Length; c++)
        {
            textBuilder.Append(RenderUnicodeChar(text[c]));
            characterCounter++;

            // counter by width - 4
            if (characterCounter == Width - 4)
            {
                // reset counter
                characterCounter = 0;

                // Add to list of Lines
                lines.Add(textBuilder.ToString());
                textBuilder.Clear();
            }

            //If at end of for loop
            if (c == text.Length - 1)
            {
                // reset counter
                characterCounter = 0;

                // Add to list of Lines
                lines.Add(textBuilder.ToString());
                textBuilder.Clear();
            }

            if (lines.Count == DialogueHeight - 2)
            {
                break;
            }
        }


        for (int l = 0; l < lines.Count; l++)
        {
            DrawLine(2, Height + 2 + l, lines[l]);
        }

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

    public static void DisplayChoices(List<string> choices, ConsoleColor color = ConsoleColor.Red)
    {
        Console.ForegroundColor = color;

        // Save the screen as is

        // Display Options as boxes
        for (int i = 0; i < choices.Count; i++)
        {
            int x = (Width / 2) - ((choices[i].Length + 3) / 2);
            int y = (Height / 2) - (choices.Count * 6 / 2) + (i * 6);

            int boxWidth = 32;
            int boxX = (Width / 2) - (boxWidth / 2);

            //Draw Box
            for (int b = 0; b < 5; b++)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("#");

                for (int j = 0; j < boxWidth; j++)
                {
                    if (b == 0 || b == 4)
                        sb.Append("#");
                    else
                        sb.Append(" ");
                }
                sb.Append("#");

                DrawLine(boxX, y + b, sb.ToString());
                
                sb.Clear();
            }

            //Draw text
            DrawLine(x, y + 2, (i + 1) + ": "+ choices[i]);
        }

        // Reset screen to before
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

    #region Methods

    public string AwaitPlayerInput(ConsoleColor color = ConsoleColor.Yellow)
    {
        Console.ForegroundColor = color;
        Console.CursorVisible = true;
        Console.SetCursorPosition(3, Height + DialogueHeight);

        return Console.ReadLine();
    }

    public static void PlayMusic(string nameOfFile)
    {
        bgmSoundPlayer.SoundLocation = nameOfFile + ".wav";
        bgmSoundPlayer.PlayLooping();
    }

    public static void PlaySfx(string nameOfFile)
    {
        //Play sound effect

        //start music again at location in time?
    }

    #endregion

    #region Virtual Methods
    public virtual void Start()
    {
        DrawCanvas();
        DrawImage(GetObjectInFile("Backgrounds", "Hallway"));
        DrawSprite(GetObjectInFile("Characters", "Avery"));
        PrintNewLine("This is a very long sentence");
    }
    #endregion
}
