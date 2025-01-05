using MCTG.Data.Repositories;

namespace MCTG.Services;

public class UserService
{
    private readonly ITokenRepo _tokenRepo;
    private readonly ICardRepo _cardRepo;
    private readonly IUserRepo _userRepo;
    private readonly TradeRepo _tradeRepo;
    
    public UserService()
    {
        _tokenRepo = new TokenRepo();
        _cardRepo = new CardRepo();
        _userRepo = new UserRepo();
        _tradeRepo = new TradeRepo();
    }

    // Constructor for Dependency Injection
    public UserService(ITokenRepo tokenRepo, ICardRepo cardRepo, IUserRepo userRepo)
    {
        _tokenRepo = tokenRepo;
        _cardRepo = cardRepo;
        _userRepo = userRepo;
    }

    public virtual async Task<List<Card>?> ShowCards(string token, string source)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        if (source == "Stack")
            return await _cardRepo.GetAllCardsFromStack(userId);
        return await _cardRepo.GetAllCardsFromDeck(userId);
    }

    public virtual async Task<bool?> ConfigureDeck(List<Guid> stackIds, string token)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        if (await _cardRepo.DeckConfigured(userId))
            return false;
        if (stackIds.Count != 4)
            return false;
        List<Guid> addedStackIds = new();
        foreach (Guid stackId in stackIds)
        {
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

    public virtual async Task<List<string>?> ShowUserData(string token, string name)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        string userName = await _userRepo.GetUserName(userId);
        if (userName != name)
            return null;
        return await _userRepo.GetUserData(userId);
    }

    public virtual async Task<bool?> ChangeUserData(string token, string name, List<string> userData)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        string userName = await _userRepo.GetUserName(userId);
        if (userName != name)
            return null;
        return await _userRepo.ChangeUserData(userId, userData);
    }

    public virtual async Task<List<string>?> ShowUserStats(string token)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        return await _userRepo.GetUserStats(userId);
    }

    public virtual async Task<List<string>?> ShowScoreboard(string token)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        return await _userRepo.GetScoreboard();
    }

    public async Task<List<string>?> ShowTrades(string token)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        return await _tradeRepo.GetTrades();
    }

    public async Task<bool?> CreateTrade(string token, Trade tradeData)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        if (userId == null)
            return null;
        if (!await _cardRepo.CardExistsInUserStack(tradeData.CardToTrade,userId) || await _cardRepo.ExistInUserDeck(tradeData.Id))
            return false;
        return await _tradeRepo.CreateTrade(userId, tradeData);
    }

    public async Task<bool?> DeleteTrade(string token, string tradeId)
    {
        Guid? userId = await _tokenRepo.GetUserUid(token);
        Guid? traderId = await _tradeRepo.GetUserUid(tradeId);
        if(userId == null || userId != traderId)
            return null;
        return await _tradeRepo.DeleteTrade(tradeId);
    }

    public async Task<bool?> AcceptTrade(string token, string tradeId, string cardToTradeId)
    {
        // user that need to trade
        Guid? userId = await _tokenRepo.GetUserUid(token);
        // user that offer the trade
        Guid? traderId = await _tradeRepo.GetUserUid(tradeId);
        if(userId == null || userId == traderId)
            return null;
        try
        {
            // check if the card to trade exist in the user stack
            if (!await _cardRepo.CardExistsInUserStack(Guid.Parse(cardToTradeId), userId))
                return false;
            // username of the user that need to trade 
            string username = await _userRepo.GetUserName(userId);
            // username of the trader
            string traderUsername = await _userRepo.GetUserName(traderId);
            Guid? cardId = await _tradeRepo.GetCardId(tradeId);
            // card name
            string cardName = await _cardRepo.GetCardName(cardId);
            // update the user stack add the card from trade
            await _cardRepo.UpdateUserStack(username, cardName);
            // update the trader stack remove the card from trade
            await _cardRepo.UpdateUserStack(traderUsername, cardName, true);
            // Add card to trade to the trader stack
            // card to trade name
            string cardToTradeName = await _cardRepo.GetCardName(Guid.Parse(cardToTradeId));
            // update the user stack add the card from trade
            await _cardRepo.UpdateUserStack(traderUsername, cardToTradeName);
            // update the trader stack remove the card from trade
            await _cardRepo.UpdateUserStack(username, cardToTradeName, true);
            await _tradeRepo.DeleteTrade(tradeId);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}
