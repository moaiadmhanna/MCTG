namespace MCTG;

public class User
{
    public string UserName { get; private set; }
    public string Password { get; private set; }
    private Stack userStack = new Stack();
    private Card[] userDeck = new Card[4];
    public double Coins { get; private set; }
    public double Elo { get; private set; }

    public User(string userName, string password)
    {
        UserName = userName;
        Password = password;
        Coins = 20;
        Elo = 100;
    }
}