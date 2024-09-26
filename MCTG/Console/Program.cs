using MCTG.Services;

namespace MCTG;

class Program
{
    static void Main(string[] args)
    {
        User player1 = new User("Muayad", "12345");
        User player2 = new User("Mina", "12345");
        Console.WriteLine(player1.UserName);
        MonsterCard mcard1 = new MonsterCard("Monster 1", 50, ElementType.Water);
        MonsterCard mcard2 = new MonsterCard("Monster 2", 50, ElementType.Fire);
        SpellCard scard1 = new SpellCard("Spell 1", 50, ElementType.Normal);
        player1.UserStack.addCardToStack(mcard1);
        player1.UserStack.addCardToStack(mcard2);
        player2.UserStack.addCardToStack(scard1);
        player1.UserStack.removeCardFromStack(mcard2);
        Battel battel = new Battel(player1, player2);
    }
}