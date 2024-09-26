namespace MCTG;

public class Stack
{
   private List<Card> cards = new List<Card>();

   public void addCardToStack(Card card)
   {
      cards.Add(card);
   }

   public void removeCardFromStack(Card card)
   {
      Console.WriteLine( cards.Remove(card) ? "Card removed Successfully" : "Card remove Failed");
   }
}