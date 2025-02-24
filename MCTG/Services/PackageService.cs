using MCTG.Data;
using MCTG.Data.Repositories;

namespace MCTG.Services;

public class PackageService
{
    private const int PackageCost = 5;
    private const int PackageSize = 5;
    private CardRepo _cardRepo = new CardRepo();
    private UserRepo _userRepo = new UserRepo();
    private TokenRepo _tokenRepo = new TokenRepo();

    public async Task<string?> AcquirePackage(string token)
    {
        User? user = await GetUser(token);
        if (user == null)
            return null;
        try
        {
            string username = user.UserName;
            if (user.Coins >= PackageCost)
            {
                List<Card>? packageCards = await _cardRepo.GetAllCardsFromPackage();
                if (packageCards == null)
                    return "No available Packages";
                for (int cardCount = 0; cardCount < PackageSize; cardCount++)
                {
                    Card newCard = packageCards[cardCount];
                    user.UserStack.AddCardToStack(newCard);
                    await _cardRepo.UpdateUserStack(username, newCard.Name);
                }
                Console.WriteLine($"Purchased package for user {username}");
                user.UpdateCoins(-PackageCost);
                await _userRepo.UpdateCoins(PackageCost, username);
                Console.WriteLine($"The Stack of {username} has been updated");
            }
            else
            {
                Console.WriteLine($"User {username} does not have enough Coins");
                return "Not enough Money";
            }

            return "Package acquired successfully";
        }
        catch (Exception e)
        {
            return "Error by acquiring the package";
        }
    }
    public async Task<bool> CreatePackage(List<Guid> cardIds)
    {
        try
        {
            Guid packageId = Guid.NewGuid();
            if (await _cardRepo.GetNumberOfCards() < 5)
            {
                Console.WriteLine("There is not enough cards to buy");
                return false;
            }
            // To Check if the Card exists in the DB
            foreach (Guid cardId in cardIds)
            {
                if (!await _cardRepo.CardExists(cardId))
                    return false;
            }
            // TO Add the Cards to the Package table
            foreach (Guid cardId in cardIds)
            {
                await _cardRepo.AddCardToPackages(packageId, cardId);
            }

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    private async Task<User?> GetUser(string token)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        return await _userRepo.GetUser(userId);
    }
}