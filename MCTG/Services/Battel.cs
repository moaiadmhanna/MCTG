namespace MCTG.Services;

public class Battel
{
    private User Player1 {get; set;}
    private User Player2 {get; set;}
    private const int Rounds = 100;

    public Battel(User player1, User player2)
    {
        Player1 = player1;
        Player2 = player2;

    }
}