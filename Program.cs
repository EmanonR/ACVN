using System;
using System.IO;
using System.Text;
using System.Media;
using System.Collections.Generic;

public class Program
{
    public static void Main(string[] args)
    {
        Script script = new Script();
        Run(script);
    }

    #region Setup
    static SoundPlayer bgmSoundPlayer = new SoundPlayer();

    public static int Width = 84;
    public static int Height = 21;
    public static int DialogueHeight = 5;

    public static string loadedSpriteID, loadedImageID, loadedSpriteFile, loadedImageFile;
    public static string savedSpriteID, savedImageID, savedSpriteFile, savedImageFile;
    public static string wholeLine;
    public static string currentLine;
    public static int currentLineBreak = 0;

    public static ConsoleColor DefaultBorderColor = ConsoleColor.Red;
    public static ConsoleColor DefaultSpriteColor = ConsoleColor.Green;
    public static ConsoleColor DefaultBackgroundColor = ConsoleColor.Gray;
    public static ConsoleColor DefaultTextColor = ConsoleColor.Blue;
    public static ConsoleColor DefaultInputColor = ConsoleColor.Yellow;


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

    private static void DrawCanvasBase()
    {
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
    public static void DrawCanvas()
    {
        // Canvas color
        Console.ForegroundColor = DefaultBorderColor;

        DrawCanvasBase();
    }
    public static void DrawCanvas(ConsoleColor color)
    {
        // Canvas color
        Console.ForegroundColor = color;

        DrawCanvasBase();
    }


    private static void DrawImageBase(string fileName, string imageID)
    {
        string[] image = GetObjectInFile(fileName, imageID);
        loadedImageFile = fileName;
        loadedImageID = imageID;

        for (int y = 0; y < Height; y++)
        {
            char[] characters = image[y % image.Length].ToCharArray();

            for (int x = 0; x < Width; x++)
            {
                DrawLine(x, y, RenderUnicodeChar(characters[x % characters.Length]));
            }
        }
    }
    public static void DrawImage(string fileName, string imageID)
    {
        Console.ForegroundColor = DefaultBackgroundColor;

        DrawImageBase(fileName, imageID);
    }
    public static void DrawImage(string fileName, string imageID, ConsoleColor color)
    {
        Console.ForegroundColor = color;

        DrawImageBase(fileName, imageID);
    }

    private static void DrawSpriteBase(string fileName, string spriteID)
    {
        string[] sprite = GetObjectInFile(fileName, spriteID);
        loadedSpriteFile = fileName;
        loadedSpriteID = spriteID;

        // for each line in image
        for (int y = 0; y < sprite.Length; y++)
        {
            if (y >= Height)
            {
                break;
            }

            int longest = 0;
            //Find longest line
            for (int o = 0; o < sprite.Length; o++)
            {
                if (o == 0)
                    longest = sprite[o].Length;
                else
                {
                    if (sprite[o].Length > longest)
                        longest = sprite[o].Length;
                }
            }

            // Characters in line
            char[] characters = sprite[y].ToCharArray();

            // For each character
            for (int x = 0; x < characters.Length; x++)
            {
                if (characters[x].ToString() == "`")
                    continue;


                int yError = sprite.Length - Height;
                int yPos = 0;

                int xPos = (Width / 2) - (longest / 2) + x;
                if (xPos > Width) xPos = Width;

                if (yError > 0)
                    yPos = (Height - sprite.Length + yError + y);
                else
                    yPos = (Height - sprite.Length + y);

                if (yPos > Height) yPos = Height;

                // Draw each character by position
                DrawLine(xPos, yPos, RenderUnicodeChar(characters[x]));
            }
        }
    }
    public static void DrawSprite(string fileName, string spriteID)
    {
        Console.ForegroundColor = DefaultSpriteColor;

        DrawSpriteBase(fileName, spriteID);
    }
    public static void DrawSprite(string fileName, string spriteID, ConsoleColor color)
    {
        Console.ForegroundColor = color;

        DrawSpriteBase(fileName, spriteID);
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

    #region NewLine
    private static void NewLineBase(String text, bool autoForward = false, Character character = null, ConsoleColor color = ConsoleColor.Yellow)
    {
        wholeLine = text;
        currentLine = text;
        currentLineBreak = 0;

        if (character != null)
            Console.ForegroundColor = character.TextColor;
        else
            Console.ForegroundColor = color;

        ClearTextBox();
        PrintText(text);

        // if not autoForward, "false"
        if (!autoForward)
            Console.ReadKey(false);
    }

    public static void NewLine(string text)
    {
        NewLineBase(text, false, null, DefaultTextColor);
    }

    public static void NewLine(string text, Character character = null)
    {
        NewLineBase(text, false, character);
    }

    public static void NewLine(string text, ConsoleColor color)
    {
        NewLineBase(text, false, null, color);
    }

    public static void NewLine(string text, bool autoForward = false)
    {
        NewLineBase(text, autoForward, null, DefaultTextColor);
    }

    public static void NewLine(string text, ConsoleColor color, bool autoForward = false)
    {
        NewLineBase(text, autoForward, null, color);
    }

    public static void NewLine(string text, Character character = null, bool autoForward = false)
    {
        NewLineBase(text, autoForward, character, DefaultTextColor);
    }
    #endregion

    #region AddToLine
    private static void AddToLineBase(String text, int lineBreaks, bool autoForward = false, Character character = null, ConsoleColor color = ConsoleColor.Blue)
    {
        // Color
        Console.ForegroundColor = character != null ? character.TextColor : color;

        // Line breaks
        if (lineBreaks == 0)
        {
            wholeLine += text;
            PrintText(text, currentLine.Length, currentLineBreak);
            currentLine += text;
        }
        else
        {
            for (int i = 0; i < lineBreaks; i++)
            {
                wholeLine += ";";
                currentLineBreak++;
            }
            currentLine = text;
            wholeLine += text;
            PrintText(text, 0, currentLineBreak);
        }

        // if not autoForward, "false"
        if (!autoForward)
            Console.ReadKey(false);
    }

    public static void AddToLine(string text, int lineBreaks = 0)
    {
        AddToLineBase(text, lineBreaks);
    }

    public static void AddToLine(string text, int linebreaks = 0, Character character = null)
    {
        AddToLineBase(text, linebreaks, false, character);
    }

    public static void AddToLine(string text, bool autoForward = false)
    {
        AddToLineBase(text, 0, autoForward, null, DefaultTextColor);
    }

    public static void AddToLine(string text, int linebreaks = 0, bool autoForward = false)
    {
        AddToLineBase(text, linebreaks, autoForward, null, DefaultTextColor);
    }

    public static void AddToLine(string text, Character character = null)
    {
        AddToLineBase(text, 0, false, character);
    }

    public static void AddToLine(string text, int linebreaks, ConsoleColor color)
    {
        AddToLineBase(text, linebreaks, false, null, color);
    }

    public static void AddToLine(string text, int linebreaks, ConsoleColor color, bool autoForward = false)
    {
        AddToLineBase(text, linebreaks, autoForward, null, color);
    }

    public static void AddToLine(string text, ConsoleColor color, bool autoForward = false)
    {
        AddToLineBase(text, 0, autoForward, null, color);
    }

    public static void AddToLine(string text, Character character = null, bool autoForward = false)
    {
        AddToLineBase(text, 0, autoForward, character, DefaultTextColor);
    }

    public static void AddToLine(string text, int linebreaks, Character character = null, bool autoForward = false)
    {
        AddToLineBase(text, linebreaks, autoForward, character, DefaultTextColor);
    }
    #endregion

    public static void PrintText(string text, int offsetX = 0, int offsetY = 0)
    {
        StringBuilder textBuilder = new StringBuilder();

        // Counting character in string
        int characterCounter = offsetX;

        // All lines, devided by length, Auto wrapping
        List<string> lines = new List<string>();

        // For each char in whole text
        for (int c = 0; c < text.Length; c++)
        {
            // if line break
            if (text[c] == ';')
            {
                lines.Add(" ");
                textBuilder.Clear();
                continue;
            }

            // Add character to string
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

            // Limit Y axis
            if (lines.Count == DialogueHeight - 2)
            {
                break;
            }
        }

        for (int l = 0; l < lines.Count; l++)
        {
            DrawLine(2 + offsetX, Height + 2 + l + offsetY, lines[l]);
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

    private static void DisplayChoicesBase(List<string> choices, ConsoleColor color)
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
            DrawLine(x, y + 2, (i + 1) + ": " + choices[i]);
        }

        // Reset screen to before
    }
    public static void DisplayChoices(List<string> choices, ConsoleColor color)
    {
        Console.ForegroundColor = color;

        DisplayChoicesBase(choices, color);
    }
    public static void DisplayChoices(List<string> choices)
    {
        Console.ForegroundColor = DefaultInputColor;

        DisplayChoicesBase(choices, DefaultInputColor);

        
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

    private static string AwaitPlayerInputBase(ConsoleColor color = ConsoleColor.Yellow)
    {
        Console.ForegroundColor = color;
        Console.CursorVisible = true;

        Console.SetCursorPosition(3, Height + DialogueHeight);
        string line = Console.ReadLine();

        Console.CursorVisible = false;

        return line;
    }

    #region Player Input
    public string AwaitPlayerInput(ConsoleColor color = ConsoleColor.Yellow)
    {
        return AwaitPlayerInputBase(color);
    }
    public string AwaitPlayerInput()
    {
        return AwaitPlayerInputBase(DefaultInputColor);
    }

    public string AwaitPlayerYN(ConsoleColor errorColor = ConsoleColor.Blue, ConsoleColor color = ConsoleColor.Yellow)
    {
        string reply = AwaitPlayerInputBase(color).Trim().ToLower();

        while (reply != "y" && reply != "n")
        {
            NewLine("Please reply with Y or N", errorColor, true);
            reply = AwaitPlayerInput();
        }

        Console.CursorVisible = false;

        return reply;
    }
    public string AwaitPlayerYN(ConsoleColor errorColor = ConsoleColor.Blue)
    {
        string reply = AwaitPlayerInput(DefaultInputColor).Trim().ToLower();

        while (reply != "y" && reply != "n")
        {
            NewLine("Please reply with Y or N", errorColor, true);
            reply = AwaitPlayerInput();
        }

        Console.CursorVisible = false;

        return reply;
    }
    public string AwaitPlayerYN()
    {
        string reply = AwaitPlayerInput(DefaultInputColor).Trim().ToLower();

        while (reply != "y" && reply != "n")
        {
            NewLine("Please reply with Y or N", DefaultTextColor, true);
            reply = AwaitPlayerInput();
        }

        Console.CursorVisible = false;

        return reply;
    }
    #endregion

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

    public static void RedrawScene()
    {
        RedrawImage();
        RedrawSprite();
    }

    public static void RedrawImage()
    {
        if (savedImageID == null || savedImageFile == null)
            return;

        DrawImage(savedImageFile, savedImageID);
    }

    public static void RedrawSprite()
    {
        if (savedSpriteID == null || savedSpriteFile == null)
            return;

        DrawSprite(savedSpriteFile, savedSpriteID);
    }

    public static void SaveScene()
    {
        SaveImage();
        SaveSprite();
    }

    public static void SaveImage()
    {
        savedImageFile = loadedImageFile;
        savedImageID = loadedImageID;
    }

    public static void SaveSprite()
    {
        savedSpriteFile = loadedSpriteFile;
        savedSpriteID = loadedSpriteID;
    }

    #endregion

    #region Virtual Methods
    public virtual void Start()
    {
        DrawCanvas();
        DrawImage("Backgrounds", "Hallway");
        DrawSprite("Characters", "Avery");
        NewLine("This is a very long sentence");
    }
    #endregion

    #region Classes

    public class Character
    {
        public ConsoleColor TextColor = ConsoleColor.Green;
    }

    #endregion
}
