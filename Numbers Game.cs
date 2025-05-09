using System;
using System.Collections.Generic;

public class Script : Program
{
    public bool IsDone { get; private set; } = false;

    public static int low;
    public static int high;
    public static int score; // 100 * interval / (guess + maxGuesses)
    public static List<int> scoreList = new List<int>(); // max should be DialogueHeight - 2
    public static List<string> scoreListNames = new List<string>();

    public static Random random = new Random();
    public static int ranNbr;
    public static int guessTimes = 0;
    public static int maxGuessTimes = 1;


    public override void Start()
    {
        Height = 19;
        DialogueHeight = 7;

        DrawCanvas();
        DrawImage("Backgrounds", "Hallway");
        DrawSprite("Characters", "Avery");

        StartOfGame();
    }

    public void StartOfGame()
    {
        NewLine("Welcome back user.");
        NewLine("Today i will guess what number you are thinking about.");

        if (scoreList.Count > 0)
        {
            NewLine("Do you want to see the leaderboard first? Y/N", true);

            if (AwaitPlayerYN() == "y")
                LeaderBoard();
        }

        StartTestSetup();
    }

    public void StartTestSetup()
    {
        NewLine("What is your name?", true);
        string name = AwaitPlayerInput();

        while (name == "")
        {
            NewLine("Please type in a name", true);
            name = AwaitPlayerInput();
        }

        ChooseNumber();
        Setup();
        Guessing();

        NewLine("The total amount of times you guessed was: ", true);
        AddToLine("" + guessTimes, ConsoleColor.Yellow);

        score = CalculateScore(high - low, guessTimes);

        NewLine("Your final score is: ", true);
        AddToLine("" + score, ConsoleColor.Yellow);

        // Add to LeaderBoard
        AddToScoreList(name, score);

        guessTimes = 0;

        NewLine("Restarting...");
        StartOfGame();
    }

    public void LeaderBoard()
    {
        ClearTextBox();
        for (int i = 0; i < scoreList.Count; i++)
        {
            if (i == 0)
                NewLine(i + 1 + " " + scoreListNames[i] + ": " + scoreList[i], true);
            else
                AddToLine(i + 1 + " " + scoreListNames[i] + ": " + scoreList[i], 1, true);
        }

        Console.ReadKey();
        StartTestSetup();
    }

    public void ChooseNumber()
    {
        NewLine("Please input the lowest possible number", true);
        while (int.TryParse(AwaitPlayerInput(), out low) == false)
            NewLine("Please input a number", true);


        NewLine("Please input the highest possible number", true);

        for (int i = 0; i < 1;)
        {
            while (int.TryParse(AwaitPlayerInput(), out high) == false)
            {
                NewLine("Please input a number", true);
            }

            if (low < high)
                i++;
            else
            {
                NewLine("This number needs to be higher than the last number you chose: ", true);
                AddToLine("" + low, ConsoleColor.Yellow, true);
            }
        }

        NewLine("The number will be between ", true);
        AddToLine(low + " - " + high, ConsoleColor.Yellow);
        AddToLine(", Will that be alright? Y/N", true);

        string reply = AwaitPlayerYN();

        if (reply == "n") ChooseNumber();
    }

    public void Setup()
    {
        for (int j = 0; j < 1;)
        {
            NewLine("How many guesses do you want? This will affect your score", true);

            while (int.TryParse(AwaitPlayerInput(), out maxGuessTimes) == false)
            {
                NewLine("Please input a number", true);
            }

            if (maxGuessTimes > 0)
                j++;
            else
            {
                NewLine("The number needs to be higher than 0");
            }
        }

        int maxScore = CalculateScore(high - low, 1);

        NewLine("Your current settings are: ", true);
        AddToLine("Lowest: " + low + ", Highest: " + high + ", Attempts: " + maxGuessTimes, ConsoleColor.Yellow);
        AddToLine("Max possible score: ", 1,true);
        AddToLine("" + maxScore, ConsoleColor.Yellow);

        NewLine("Are these settings alright? Y/N", true);
        string reply = AwaitPlayerYN();

        if (reply == "n") ChooseNumber();
    }

    public void Guessing()
    {
        ranNbr = random.Next(low, high + 1);

        int guessNbr;
        NewLine("Starting test");

        NewLine("Please make a guess between: ", true);
        AddToLine(low + " - " + high, ConsoleColor.Yellow, true);

        for (int i = 0; i < 1;)
        {
            guessTimes++;
            while (int.TryParse(AwaitPlayerInput(), out guessNbr) == false)
            {
                NewLine("Please input a number, Remaining attempts: ", true);
                AddToLine("" + (maxGuessTimes - guessTimes), ConsoleColor.Yellow, true);
            }

            // Guess replies
            if (guessNbr == ranNbr)
            {
                NewLine("That is correct!");
                i++;
            }
            else if (guessTimes == maxGuessTimes)
            {
                NewLine("You have guessed too many times, concluding test");
                NewLine("The number was: ", true);
                AddToLine("" + ranNbr, ConsoleColor.Yellow);
                guessTimes = 0;
                StartOfGame();
                i++;
            }
            else
            {
                if (guessNbr < ranNbr)
                {
                    NewLine("Guess a bit ", true);
                    AddToLine("higher", ConsoleColor.Green, true);
                    AddToLine(", Remaining attempts: ", true);
                    AddToLine("" + (maxGuessTimes - guessTimes), ConsoleColor.Yellow, true);
                }
                else if (guessNbr > ranNbr)
                {
                    NewLine("Guess a bit ", true);
                    AddToLine("Lower", ConsoleColor.Green, true);
                    AddToLine(", Remaining attempts: ", true);
                    AddToLine("" + (maxGuessTimes - guessTimes), ConsoleColor.Yellow, true);
                }
            }
        }
    }

    public void AddToScoreList(string name, int score)
    {
        if (scoreList.Count == 0)
        {
            scoreList.Add(score);
            scoreListNames.Add(name);

            return;
        }

        for (int i = 0; i < scoreList.Count; i++)
        {
            if (scoreList.Count == DialogueHeight - 2)
            {
                if (score > scoreList[i])
                {
                    scoreList.Insert(i, score);
                    scoreListNames.Insert(i, name);

                    scoreList.RemoveAt(i + 1);
                    scoreListNames.RemoveAt(i + 1);
                }
                return;
            }

            if (score > scoreList[i])
            {
                scoreList.Insert(i, score);
                scoreListNames.Insert(i, name);
                return;
            }
        }

        scoreList.Add(score);
        scoreListNames.Add(name);
    }

    public int CalculateScore(int interval, int tries)
    {
        return 100 * interval / (tries + maxGuessTimes);
    }

    public void EndGame()
    {
        IsDone = true;
    }
}
