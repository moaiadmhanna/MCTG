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
      Console.WriteLine( cards.Remove(card) ? "Card removed Successfully" : "Card remove Failed");
   }
}