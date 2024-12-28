using MCTG.Data.Repositories;

namespace MCTG.Services;

public class UserService
{
    private readonly TokenRepo _tokenRepo = new TokenRepo();
    private readonly CardRepo _cardRepo = new CardRepo();
    
    public async Task<List<Card>?> ShowCards(string token, string source)
    {
        Guid? userId = await _tokenRepo.GerUserUid(token);
        if (userId == null)
            return null;
        if (source == "Stack")
            return await _cardRepo.GetAllCardsFromStack(userId);
        return await _cardRepo.GetAllCardsFromDeck(userId);
    }
}