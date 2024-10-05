using MCTG.Data;

namespace MCTG;

public class Package
{
    public List<Card> Cards { get; private set; } = new List<Card>();
    private const int PackageSize = 5;

    public Package()
    {
        for (int count = 0; count < PackageSize; count++)
        {
            Card randomCard = Database.GetRandomCard();
            Cards.Add(randomCard);
        }
    }

}