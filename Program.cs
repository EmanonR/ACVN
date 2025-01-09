using System;
using System.Media;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;
using System.Threading;

class Program
{
    #region setup 

    // Method to set console to use Unicode and set window size
    static void PrepareConsoleForUnicode()
    {
        int windowY = height + dialogueHeight + 4;
        int windowX = width + 2;

        // Set console output to Unicode
        Console.OutputEncoding = Encoding.UTF8;

        Console.SetWindowSize(windowX, windowY);

        //Console.SetBufferSize(windowX + 1, windowY);
    }
    #endregion

    #region variables
    static int height = 24; // Example height
    static int width = 84;  // Example width

    static int dialogueHeight = 5; // Example height
    static int frameCount = 1;
    static int targetFPS = 12;
    static int frameTimeInTicks = 0;
    #endregion

    public static void Main(string[] args)
    {
        Game game = new Game();
        game.Start();
        
        // Create the stopwatch
        Stopwatch stopwatch = new Stopwatch();

        // Target frame time in milliseconds
        float targetFrameTime = 1000f / targetFPS;

        while (!game.isDone)
        {
            stopwatch.Restart(); // Restart the stopwatch
            
            game.Update();  // Update loop
            
            stopwatch.Stop(); // Stop the stopwatch

            // Calculate frame time in milliseconds
            float frameTime = stopwatch.ElapsedMilliseconds;

            // Calculate Delay based on target framerate
            float delay = targetFrameTime - frameTime;

            // Sleep if the delay is more than 0
            if (delay > 0)
            {
                Thread.Sleep((int)delay); // Sleep for the remaining time in milliseconds
            }
        }
    }

    public class Game
    {
        // Declare local variables here
        public bool isDone;

        // Do initial stuff here, like setting initial variable values, etc.
        public void Start()
        {
            PrepareConsoleForUnicode();

            // Draw the canvas
            DrawCanvas();

            DrawImage(GetObjectInFile("Backgrounds", "Hallway"));
        }

        // Every set amount of time, this function runs, do what happens in the game here.
        public void Update()
        {
            frameCount++;

            DrawCharacter(GetObjectInFile("Characters", "Avery"));

            DisplayText("This is a very long sentence" + frameTimeInTicks.ToString() + ", " + frameCount.ToString());

            EndOfLogic();

            // cannot be played async so its broken by my standards, aka the game pauces until song is over atm
            //PlayBGM("bgm");
        }

        public void EndGame()
        {
            isDone = true;
        }
    }



    #region Drawing
    static void DrawCanvas(ConsoleColor color = ConsoleColor.Red)
    {
        Console.ForegroundColor = color;

        // Improved drawing using StringBuilder for efficiency
        StringBuilder canvasBuilder = new StringBuilder();

        // Draw the top border
        canvasBuilder.AppendLine(new string('#', width + 2));

        // Draw the middle part with borders on each side and spaces in the middle
        for (int i = 0; i < height; i++)
        {
            canvasBuilder.Append('#');  // Left border
            canvasBuilder.Append(new string(' ', width));  // Empty space inside
            canvasBuilder.AppendLine("#");  // Right border
        }

        // Draw the bottom border (if height > 1)
        if (height > 1)
        {
            canvasBuilder.AppendLine(new string('#', width + 2));
        }

        // Dialogue area
        for (int i = 0; i < dialogueHeight; i++)
        {
            canvasBuilder.Append('#');  // Left border
            canvasBuilder.Append(new string(' ', width));  // Empty space inside
            canvasBuilder.AppendLine("#");  // Right border
        }

        if (dialogueHeight > 1)
        {
            canvasBuilder.AppendLine(new string('#', width + 2));
        }

        // Write the entire canvas at once
        Console.Write(canvasBuilder.ToString());
    }

    static void DrawImage(string[] image, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;

        for (int y = 0; y < height; y++)
        {
            char[] characters = image[y % image.Length].ToCharArray();

            for (int x = 0; x < width; x++)
            {
                DrawLine(x, y, RenderUnicodeChar(characters[x % characters.Length]));
            }
        }
    }

    static void DrawCharacter(string[] image, ConsoleColor color = ConsoleColor.Cyan)
    {
        Console.ForegroundColor = color;

        #region determine animation
        bool isAnimation = false;

        Animation anim = SepperateAnimationFrames(image);

        if (anim.frames.Length > 1)
        {
            isAnimation = true;
        }
        #endregion

        #region find longestLine
        int longestLine;

        longestLine = image[0].ToCharArray().Length;

        foreach (string line in image)
        {
            if (line.ToCharArray().Length > longestLine)
            {
                longestLine = line.ToCharArray().Length;
            }
        }
        #endregion

        string[] currentFrame = anim.frames[0].lines; ;

        if (isAnimation)
        {
            int currentAnimFrameInd = (frameCount % anim.frames.Length);

            //Get currentFrame
            currentFrame = anim.frames[currentAnimFrameInd].lines;
        }


        //Bottom to top
        for (int y = height; y > 0; y--)
        {
            string[] targetImage;


            if (isAnimation)
            {
                targetImage = currentFrame;
            }
            else
            {
                targetImage = image;
            }

            //If i is more than Image Height
            if (height - y >= targetImage.Length)
            {
                break;
            }

            //Get the characters on image Line by height
            char[] characters = targetImage[targetImage.Length - (height - y) - 1].ToCharArray();

            //For each pixel on Line
            for (int x = 0; x < characters.Length; x++)
            {
                //Draw it at position
                if (characters[x] != ' ')
                {
                    DrawLine((width / 2) - (longestLine / 2) + x, y - 1, RenderUnicodeChar(characters[x]));
                }
            }
        }
    }

    static void DisplayText(string text)
    {
        // Improved text display with Unicode support
        StringBuilder textBuilder = new StringBuilder();
        foreach (char c in text)
        {
            textBuilder.Append(RenderUnicodeChar(c));
        }
        DrawLine(2, height + 2, textBuilder.ToString());
    }

    #endregion

    static void PlayBGM(string filename)
    {
        // Use absolute path for more reliability
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename + ".wav");

        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"File not found: {fullPath}");
            return;
        }

        using (SoundPlayer bgmPlayer = new SoundPlayer(fullPath))
        {
            bgmPlayer.Load();

            if (bgmPlayer.IsLoadCompleted)
            {
                bgmPlayer.PlaySync();
            }
        }
    }

    #region Helper
    public static string[] GetObjectInFile(string fileName, string objectName)
    {
        // Read all lines from the file using UTF-8 encoding
        string[] contents = File.ReadAllLines(
            fileName + ".txt",
            Encoding.UTF8
        );

        List<string> objectContent = new List<string>();
        bool insideTargetObject = false;

        foreach (string line in contents)
        {
            // Check if this is the start of the target object
            if (line.StartsWith(objectName + ":"))
            {
                insideTargetObject = true;
                continue;
            }

            // If we're inside the target object
            if (insideTargetObject)
            {
                // Start collecting content after the opening bracket
                if (line == "{")
                {
                    continue;
                }

                // Stop collecting when we hit the closing bracket
                if (line == "}")
                {
                    break;
                }

                // Add the line to our object content
                objectContent.Add(line);
            }
        }

        return objectContent.ToArray();
    }


    //I think this works
    public static Animation SepperateAnimationFrames(string[] data)
    {
        //The animation we want to return
        Animation animation = new Animation();

        //all the frames
        List<Frame> frames = new List<Frame>();

        //all the lines in currentFrame
        List<string> frameLines = new List<string>();



        //For each line in string
        for (int i = 0; i < data.Length; i++)
        {
            // Check if this is a new frame
            if (data[i] == ":")
            {
                // Start of New frame
                Frame frame = new Frame();
                frame.lines = frameLines.ToArray();
                frameLines.Clear();


                frames.Add(frame);
                continue;
            }

            // Write Line to frame
            frameLines.Add(data[i]);

            // Check if this is last frame
            if (i == data.Length - 1)
            {
                // Start of New frame
                Frame frame = new Frame();
                frame.lines = frameLines.ToArray();
                frameLines.Clear();

                frames.Add(frame);
            }

        }

        animation.frames = frames.ToArray();
        return animation;
    }


    static void DrawLine(int x, int y, string line)
    {
        // Move the cursor to the (x, y) position
        Console.SetCursorPosition(x + 1, y + 1); // Adjusting by 1 to avoid the borders

        // Place the character at the specified position
        Console.Write(line);
    }


    // Enhanced Unicode character rendering
    static string RenderUnicodeChar(char c)
    {
        // Allow all Unicode characters, including Braille and block characters
        if (c == '\0' || char.IsWhiteSpace(c))
        {
            return " "; // Replace null or whitespace with a space
        }

        return c.ToString(); // Return the character as-is
    }
    #endregion

    #region Logic
    static void EndOfLogic()
    {
        // Display the prompt below the canvas
        DisplayPrompt(height + dialogueHeight);
    }

    static void DisplayPrompt(int height)
    {
        // Move the cursor to a new line below the canvas for the prompt
        Console.SetCursorPosition(0, height + 3);  // You can adjust this depending on your height
    }
    #endregion


    public class Animation
    {
        public Frame[] frames;
    }

    public class Frame
    {
        public string[] lines;
    }
}
