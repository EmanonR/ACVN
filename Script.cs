public class Script : Engine
{
    public bool IsDone { get; private set; } = false;

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public void EndGame()
    {
        IsDone = true;
    }
}
