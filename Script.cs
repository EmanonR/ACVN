using System;
using System.Collections.Generic;

public class Script : Program
{
    public bool IsDone { get; private set; } = false;

    public override void Start()
    {
        base.Start();
    }

    public void EndGame()
    {
        IsDone = true;
    }
}
