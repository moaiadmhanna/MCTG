using MCTG.Data;
using MCTG.Services;

namespace MCTG;

class Program
{
    static void Main(string[] args)
    {
        RegisterService registerService = new RegisterService();
        LoginService loginService = new LoginService();
        PackageService packageService = new PackageService();
        BattleService battleService = new BattleService();
        registerService.RegisterUser("Muayad", "Muayad1234");
        User loginUser1 = loginService.LoginUser("Muayad","Muayad1234");
        packageService.PurchasePackage(loginUser1.UserName);
        registerService.RegisterUser("Mahmoud","Muayad12345");
        User loginUser2 = loginService.LoginUser("Mahmoud","Muayad12345");
        packageService.PurchasePackage("Mahmoud");
        for (int cardCount = 0; cardCount < loginUser1.UserStack.Count(); cardCount++)
        {
            loginUser1.UserDeck.AddCardToDeck(loginUser1.UserStack.getCard(cardCount));
            loginUser2.UserDeck.AddCardToDeck(loginUser2.UserStack.getCard(cardCount));
        }
        battleService.StartBattle(loginUser1, loginUser2);
        
    }
}