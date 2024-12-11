namespace MCTG;

public class Deck
{
    private readonly List<Card> _cards = new List<Card>();
    private const int MaxCards = 100;
    private readonly int MinCards = 100;
    
    public void AddCardToDeck(Card card)
    {
        _cards.Add(card);
    }

    public void RemoveCardFromCard(Card card)
    {
        if (!_cards.Remove(card))
        {
            throw new Exception("Card not found");

        }
    }

    public int Count() => _cards.Count;

    public Card GetCard(int index)
    {
        return _cards[index];
    }
}