using MCTG.Data.Repositories;

namespace MCTG.Services;

public class UserService
{
    private readonly TokenRepo _tokenRepo = new TokenRepo();
    private readonly CardRepo _cardRepo = new CardRepo();
    private readonly UserRepo _userRepo = new UserRepo();
    
    public async Task<List<Card>?> ShowCards(string token, string source)
    {
        Guid? userId = await _tokenRepo.GerUserUid(token);
        if (userId == null)
            return null;
        if (source == "Stack")
            return await _cardRepo.GetAllCardsFromStack(userId);
        return await _cardRepo.GetAllCardsFromDeck(userId);
    }

    public async Task<bool?> ConfigureDeck(List<Guid> stackIds, string token)
    {
        Guid? userId = await _tokenRepo.GerUserUid(token);
        if (userId == null)
            return null;
        // Check if the Deck is already Configured
        if (await _cardRepo.DeckConfigured(userId))
            return false;
        // Check if the Count of the stackIds is equal to 4
        if (stackIds.Count != 4)
            return false;
        List<Guid> addedStackIds = new();
        foreach (Guid stackId in stackIds)
        {
            // If adding a card fails, rollback and return
            if (!await _cardRepo.AddCardToDeck(stackId, userId))
            {
                foreach (Guid addedStackId in addedStackIds)
                {
                    await _cardRepo.DeleteCardFromDeck(addedStackId);
                }
                return false;
            }
            addedStackIds.Add(stackId);
        }
        return true;
    }

    public async Task<List<string>?> ShowUserData(string token, string name)
    {
        Guid? userId = await _tokenRepo.GerUserUid(token);
        if (userId == null)
            return null;
        string userName = await _userRepo.GetUserName(userId);
        if(userName != name)
            return null;
        return await _userRepo.GetUserData(userId);
    }

    public async Task<bool?> ChangeUserData(string token, string name, List<string> userData)
    {
        Guid? userId = await _tokenRepo.GerUserUid(token);
        if (userId == null)
            return null;
        string userName = await _userRepo.GetUserName(userId);
        if(userName != name)
            return null;
        return await _userRepo.ChangeUserData(userId,userData);
    }

    public async Task<List<string>?> ShowUserStats(string token)
    {
        Guid? userId = await _tokenRepo.GerUserUid(token);
        if (userId == null)
            return null;
        return await _userRepo.GetUserStats(userId);
    }
}