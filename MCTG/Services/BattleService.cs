using System.Text;
using System.Threading.Channels;
using MCTG.Data;
using MCTG.Data.Repositories;

namespace MCTG.Services;
// TODO Change the Battle Service to work with the Database
// TODO Create the UML Sequence Diagram
enum Effectiveness
{
    IsEffective,
    NoEffective,
    NotEffective
}

enum BattleStatus
{
    Win,
    Lose,
    Tie,
}

enum CardStatus
{
    Survived,
    Destroyed,
    Tie,
    
}
public class BattleService
{
    // Queue for the Players
    private Queue<User> _matchMakingQueue = new Queue<User>();
    // private User Player1 {get; set;}
    // private User Player2 {get; set;}
    // private Deck _player1Deck;
    // private Deck _player2Deck;
    private const int Rounds = 100;
    private readonly UserRepo _userRepo = new UserRepo();
    
    // Method to add the Player to the Matchmaking Queue
    private void AddPlayerToQueue(User user)
    {
        _matchMakingQueue.Enqueue(user);
    }

    public string? Matchmaking()
    {
        //TODO Check if the User is in the DB before adding the user to Matchmaking queue + Matchmaking should take the token as parameter
        if (_matchMakingQueue.Count > 1)
        {
            User player1 = _matchMakingQueue.Dequeue();
            User player2 = _matchMakingQueue.Dequeue();
            return StartBattle(player1, player2);
        }
        return null;
    }
    private string StartBattle(User player1, User player2)
    {
        // Stream to Write into
        var stream = new MemoryStream();
        var encoding = Encoding.UTF8;
        StreamWriter writer = new StreamWriter(stream,encoding);
        Deck player1Deck = player1.UserDeck;
        Deck player2Deck = player2.UserDeck;
        Random random = new Random();
        int currentRound = 0;
        while (currentRound < Rounds && !GameOver(player1Deck,player2Deck))
        {
            WriteToStream(writer,$"--------Round:{currentRound}--------");
            Card player1Card = player1Deck.GetCard(random.Next(player1Deck.Count()));
            Card player2Card = player2Deck.GetCard(random.Next(player2Deck.Count()));
            CardStatus card1Status = Attack(player1Card, player2Card,writer);
            switch (card1Status)
            {
                case CardStatus.Survived:
                    player1Deck.AddCardToDeck(player2Card);
                    player2Deck.RemoveCardFromCard(player2Card);
                    break;
                case CardStatus.Destroyed:
                    player2Deck.AddCardToDeck(player1Card);
                    player1Deck.RemoveCardFromCard(player1Card);
                    break;
            }
            WriteToStream(writer,$"Count of Cards Player1 Deck : {player1Deck.Count()} | Player2 Deck: {player2Deck.Count()}");
            currentRound++;
        }

        BattleStatus battleStatus = CheckBattleStatusAndUpdatePlayer(player1, player2);
        if (battleStatus == BattleStatus.Win)
            WriteToStream(writer,$"{player1.UserName} defeated {player2.UserName} after {currentRound} Rounds");
        else if (battleStatus == BattleStatus.Lose)
            WriteToStream(writer, $"{player2.UserName} defeated {player1.UserName} after {currentRound} Rounds");
        else
            WriteToStream(writer, $"Its a draw between {player1.UserName} and {player2.UserName} after {currentRound} Rounds");
        stream.Position = 0;
        StreamReader reader = new StreamReader(stream,encoding);
        return reader.ReadToEnd();
    }
    

    private CardStatus Attack(Card card1, Card card2, StreamWriter writer)
    {
        WriteToStream(writer,$"Type of the Card Player1 Card: {card1.GetType().Name} | Player2 Card: {card2.GetType().Name}");
        int card1Damage = card1.Damage;
        int card2Damage = card2.Damage;
        if (card1 is SpellCard) {card1Damage = CalcDamageOfSpellCard(card1, card2, ref card2Damage);}
        if (card2 is SpellCard) {card2Damage = CalcDamageOfSpellCard(card2, card1, ref card1Damage);}
        if (card1 is MonsterCard monstercard1 && card2 is MonsterCard monstercard2)
        {
            card1Damage = CalcDamageOfMonsterCard(monstercard1, monstercard2);
            card2Damage = CalcDamageOfMonsterCard(monstercard2, monstercard1);
            WriteToStream(writer,$"Card Monster type is  Player1: {monstercard1.Monster} -  Player2: {monstercard2.Monster}");
        }
        WriteToStream(writer,$"Card damage was Player1: {card1.Damage} -  Player2: {card2.Damage}");
        WriteToStream(writer,$"Card damage is now Player1: {card1Damage} -  Player2: {card2Damage}");
        if (card1Damage > card2Damage)
        {
            return CardStatus.Survived;
        }
        if (card1Damage < card2Damage)
        {
            return CardStatus.Destroyed;
        }
        WriteToStream(writer,"Its a Tie of damage.");
        return CardStatus.Tie;

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


    private bool GameOver(Deck player1Deck, Deck player2Deck)
    {
        return player1Deck.Count() == 0 || player2Deck.Count() == 0;
    }

    private BattleStatus CheckBattleStatusAndUpdatePlayer(User player1, User player2)
    {
        // TODO Update Also in the DB
        if (player1.UserDeck.Count() == 0)
        {
            player1.UpdateElo(3);
            player2.UpdateElo(-5);
            return BattleStatus.Lose;
        }

        if (player2.UserDeck.Count() == 0)
        {
            player2.UpdateElo(3);
            player1.UpdateElo(-5);
            return BattleStatus.Win;
        }

        return BattleStatus.Tie;
    }

    private void WriteToStream(StreamWriter writer, string message)
    {
        writer.WriteLine(message);
        writer.Flush();
    }

}