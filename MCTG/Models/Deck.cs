namespace MCTG;

public class Deck
{
    private readonly List<Card> _cards = new List<Card>();
    private const int MaxCards = 100;
    private readonly int MinCards = 100;
    
    public void AddCardToDeck(Card card)
    {
        _cards.Add(card);
        Console.WriteLine("Card added to Deck successfully");
    }

    public void RemoveCardFromCard(Card card)
    {
        Console.WriteLine( _cards.Remove(card) ? "Card removed Successfully" : "Card remove Failed");
    }
    public int Count() => _cards.Count;

    public Card GetCard(int index)
    {
        return _cards[index];
    }
}