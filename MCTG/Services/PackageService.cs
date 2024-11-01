using MCTG.Data;
using MCTG.Data.Repositories;

namespace MCTG.Services;

public class PackageService
{
    private const int PackageCost = 5;
    private const int PackageSize = 5;
    private CardRepo _cardRepo = new CardRepo();
    private UserRepo _userRepo = new UserRepo();

    public async Task<bool> PurchasePackage(User user)
    {
        string username = user.UserName;
        if (user.Coins >= PackageCost)
        {
            if (await _cardRepo.GetNumberOfCards() < 5)
            {
                Console.WriteLine("There is not enough cards to buy");
                return false;
            }
            Console.WriteLine($"Purchased package for user {username}");
            for (int cardCount = 0; cardCount < PackageSize; cardCount++)
            {
                Card newCard = await _cardRepo.GetRandomCard();
                user.UserStack.AddCardToStack(newCard);
                await _cardRepo.UpdateUserStack(username, newCard.Name);
            }
            user.UpdateCoins(-PackageCost);
            await _userRepo.UpdateCoins(PackageCost,username);
            Console.WriteLine($"The Stack of {username} has been updated");
        }
        else
        {
            Console.WriteLine($"User {username} does not have enough Coins");
            return false;
        }
        return true;
    }
}