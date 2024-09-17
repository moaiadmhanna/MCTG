namespace MCTG;

internal class User
{
    private string userName { get; set; }
    private string password { get; set; }
    private Stack userStack = new Stack();
    private Card[] userDeck = new Card[4];

    public User(string userName, string password)
    {
        this.userName = userName;
        this.password = password;
    }
    

}