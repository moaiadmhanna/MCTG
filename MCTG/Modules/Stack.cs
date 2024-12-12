namespace MCTG;

public class Stack
{
   private List<Card> cards = new List<Card>();

   public void AddCardToStack(Card card)
   {
      cards.Add(card);
   }

   public void RemoveCardFromStack(Card card)
   {
      if(!cards.Remove(card)){throw new Exception("Card not found");};
   }
   public int Count() => cards.Count;
   public Card getCard(int index) => cards[index];
}