using MCTG.Services;

namespace MCTG;

class Program
{
    static void Main(string[] args)
    {
        User player1 = new User("Muayad", "12345");
        User player2 = new User("Mina", "12345");
        Console.WriteLine(player1.UserName);
        MonsterCard mcard1 = new MonsterCard("Monster 1", 40, ElementType.Water,TypeOfMonster.Goblin);
        MonsterCard mcard2 = new MonsterCard("Monster 2", 30, ElementType.Fire,TypeOfMonster.Knight);
        MonsterCard mcard3 = new MonsterCard("Monster 3", 20, ElementType.Normal,TypeOfMonster.Krake);
        MonsterCard mcard4 = new MonsterCard("Monster 4", 60, ElementType.Fire,TypeOfMonster.Wizzard);
        SpellCard scard1 = new SpellCard("Spell 1", 10, ElementType.Normal);
        SpellCard scard2 = new SpellCard("Spell 2", 15, ElementType.Fire);
        SpellCard scard3 = new SpellCard("Spell 3", 12, ElementType.Normal);
        SpellCard scard4 = new SpellCard("Spell 4", 20, ElementType.Water);
        player1.UserDeck.AddCardToDeck(mcard1);
        player1.UserDeck.AddCardToDeck(mcard2);
        player1.UserDeck.AddCardToDeck(scard1);
        player1.UserDeck.AddCardToDeck(scard2);
        player2.UserDeck.AddCardToDeck(mcard3);
        player2.UserDeck.AddCardToDeck(mcard4);
        player2.UserDeck.AddCardToDeck(scard4);
        player2.UserDeck.AddCardToDeck(scard3);
        BattleService battleService = new BattleService(player1, player2);
        battleService.StartBattle();
    }
}