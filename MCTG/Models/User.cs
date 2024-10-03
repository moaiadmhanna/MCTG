namespace MCTG;

public class User
{
    public string UserName { get; private set; }
    public string Password { get; private set; }
    public Stack UserStack { get; } = new Stack();
    public Deck UserDeck { get; } = new Deck();
    public double Coins { get; private set; }
    public double Elo { get; private set; }

    public User(string userName, string password)
    {
        UserName = userName;
        Password = password;
        Coins = 20;
        Elo = 100;
    }
    public void UpdateElo(double x) => Elo += x;
}