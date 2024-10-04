using MCTG.Data;

namespace MCTG.Services;

public class PackageService
{
    private const int PackageCost = 5;

    public void PurchasePackage(string username)
    {
        User user = Database.getUser(username);
        if (user == null)
        {
            Console.WriteLine($"User {username} not found");
            return;
        }

        if (user.Coins >= PackageCost)
        {
            Package newPackage = new Package();
            Console.WriteLine($"Purchased package for user {username}");
            for (int cardCount = 0; cardCount < newPackage.Cards.Count; cardCount++)
            {
                user.UserDeck.AddCardToDeck(newPackage.Cards[cardCount]);
            }
            user.UpdateCoins(-PackageCost);
            Console.WriteLine($"The Deck of {username} has been updated");
        }
        else
        {
            Console.WriteLine($"User {username} does not have enough Coins");
        }
    }
}