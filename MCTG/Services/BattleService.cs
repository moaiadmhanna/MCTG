using System.Threading.Channels;
using MCTG.Data;

namespace MCTG.Services;

enum Effectiveness
{
    IsEffective,
    NoEffective,
    NotEffective
}
public class BattleService
{
    private User Player1 {get; set;}
    private User Player2 {get; set;}
    private Deck _player1Deck;
    private Deck _player2Deck;
    private const int Rounds = 100;
    private static int _currentRound = 1;
    
    public void StartBattle(string player1, string player2)
    {
        //TODO should take the two players as parameter and not the name of the PLayer
        if (!Database.UserExists(player1) || !Database.UserExists(player2))
        {
            throw new ArgumentException("Users dont exist");
        }
        Player1 = Database.GetUser(player1);
        Player2 = Database.GetUser(player2);
        _player1Deck = Player1.UserDeck;
        _player2Deck = Player2.UserDeck;
        Random random = new Random();
        while (_currentRound < Rounds && !GameOver())
        {
            //TODO it should return if the round exceeded the Max Round
            //Thread.Sleep(2000);
            Card player1Card = _player1Deck.GetCard(random.Next(_player1Deck.Count()));
            Card player2Card = _player2Deck.GetCard(random.Next(_player2Deck.Count()));
            Attack(player1Card, player2Card);
            Console.WriteLine($"Count of Cards Player1 Deck : {_player1Deck.Count()} | Player2 Deck: {_player2Deck.Count()}");
        }
    }
    

    private void Attack(Card card1, Card card2)
    {
        Console.WriteLine($"--------Round:{_currentRound}--------");
        Console.WriteLine($"Type of the Card Player1 Card: {card1.GetType().Name} | Player2 Card: {card2.GetType().Name}");
        int card1Damage = card1.Damage;
        int card2Damage = card2.Damage;
        if (card1 is SpellCard) {card1Damage = CalcDamageOfSpellCard(card1, card2, ref card2Damage);}
        if (card2 is SpellCard) {card2Damage = CalcDamageOfSpellCard(card2, card1, ref card2Damage);}
        if (card1 is MonsterCard monstercard1 && card2 is MonsterCard monstercard2)
        {
            card1Damage = CalcDamageOfMonsterCard(monstercard1, monstercard2);
            card2Damage = CalcDamageOfMonsterCard(monstercard1, monstercard2);
            Console.WriteLine($"Card Monster type is  Player1: {monstercard1.Monster} -  Player2: {monstercard2.Monster}");
        }
        Console.WriteLine($"Card damage was Player1: {card1.Damage} -  Player2: {card2.Damage}");
        Console.WriteLine($"Card damage is now Player1: {card1Damage} -  Player2: {card2Damage}");
        if (card1Damage > card2Damage)
        {
            Console.WriteLine($"{Player1.UserName} is Dealing a damage of {card1Damage} to {Player2.UserName}.");
            _player1Deck.AddCardToDeck(card2);
            _player2Deck.RemoveCardFromCard(card2);
        }
        else if (card1Damage < card2Damage)
        {
            Console.WriteLine($"{Player2.UserName} is Dealing a damage of {card2Damage} to {Player1.UserName}.");
            _player2Deck.AddCardToDeck(card1);
            _player1Deck.RemoveCardFromCard(card1);
        }
        else
        {
            Console.WriteLine("Its a Tie of damage.");
        }
        _currentRound++;
        
    }

    private int CalcDamageOfSpellCard(Card card1, Card card2, ref int card2Damage)
    {
        if (card2 is MonsterCard { Monster: TypeOfMonster.Krake })
        {
            return 0;
        }

        if (card2 is MonsterCard { Monster: TypeOfMonster.Knight })
        {
            card2Damage = 0;
        }
        Effectiveness effectiveness = CheckEffectiveness(card1.Element, card2.Element);
        switch (effectiveness)
        {
            case Effectiveness.IsEffective:
                return card1.Damage * 2;
            case Effectiveness.NoEffective:
                return card1.Damage / 2;
            default:
                return card1.Damage;
        }
    }

    private int CalcDamageOfMonsterCard(MonsterCard card1, MonsterCard card2)
    {
        return (card1.Monster, card2.Monster) switch
        {
            (TypeOfMonster.Goblin,TypeOfMonster.Dragon) => 0,
            (TypeOfMonster.Ork,TypeOfMonster.Wizzard) => 0,
            (TypeOfMonster.Dragon,TypeOfMonster.FireElve) => 0,
            _ => card1.Damage
        };
    }
    private Effectiveness CheckEffectiveness(ElementType element1, ElementType element2)
    {
        return (element1, element2) switch
        {
            (ElementType.Water, ElementType.Fire) => Effectiveness.IsEffective,
            (ElementType.Fire, ElementType.Normal) => Effectiveness.IsEffective,
            (ElementType.Normal, ElementType.Water) => Effectiveness.IsEffective,
            (ElementType.Fire, ElementType.Water) => Effectiveness.NoEffective,
            (ElementType.Normal, ElementType.Fire) => Effectiveness.NoEffective,
            (ElementType.Water, ElementType.Normal) => Effectiveness.NotEffective,
            _ => Effectiveness.NotEffective
        };
    }


    private bool GameOver()
    {
        if (_player1Deck.Count() == 0 || _player2Deck.Count() == 0)
        {
            if (_player1Deck.Count() != 0)
            {
                Console.WriteLine($"{Player1.UserName} won the battle!.");
                Player1.UpdateElo(3);
                Player2.UpdateElo(-5);
            }
            else
            {
                Console.WriteLine($"{Player2.UserName} won the battle!.");
                Player2.UpdateElo(3);
                Player1.UpdateElo(-5);
            }
            return true;
        }
        return false;
    }
    
    
}