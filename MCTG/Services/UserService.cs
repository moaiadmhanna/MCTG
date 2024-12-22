using MCTG.Data.Repositories;

namespace MCTG.Services;

public class UserService
{
    private readonly TokenRepo _tokenRepo = new TokenRepo();
    private readonly CardRepo _cardRepo = new CardRepo();
    
    public async Task<List<Card>?> ShowCards(string token)
    {
        Guid? userId = await _tokenRepo.GerUserUid(token);
        if (userId == null)
            return null;
        return await _cardRepo.GetCardsAllFromStack(userId);
    }
}