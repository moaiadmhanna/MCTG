using MCTG;

namespace MCTG_UnitTest;



[TestFixture]
public class CardTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestMonsterCardNameAllocation()
    {
        // arrange
        Card testCard = new MonsterCard("Dragon",20,ElementType.Fire,TypeOfMonster.Dragon);
        // act
        string expected = "Dragon";
        // assert
        Assert.That(expected, Is.EqualTo(testCard.Name));
    }
    [Test]
    public void TestMonsterCardDamageAllocation()
    {
        //arrange
        Card testCard = new MonsterCard("Dragon",20,ElementType.Fire,TypeOfMonster.Dragon);
        // act
        int expected = 20;
        // assert
        Assert.That(expected, Is.EqualTo(testCard.Damage));
    }
    [Test]
    public void TestMonsterCardTypeAllocation()
    {
        //arrange
        MonsterCard testCard = new MonsterCard("Dragon",20,ElementType.Fire,TypeOfMonster.Dragon);
        // act
        TypeOfMonster expected = TypeOfMonster.Dragon;
        // assert
        Assert.That(expected, Is.EqualTo(testCard.Monster));
    }
}

[TestFixture]
public class UserTests
{
    private User testuser;
    [SetUp]
    public void Setup()
    {
        // Creating salt just for the Test
        byte[] salt = new byte[16];
        testuser = new User("Muayad", "3123123331",salt);
    }

    [Test]
    public void TestUserNameAllocation()
    {
        // act
        string expected = "Muayad";
        // assert
        Assert.That(expected, Is.EqualTo(testuser.UserName));
    }

    [Test]
    public void TestUserPasswordAllocation()
    {
        // act
        string expected = "3123123331";
        // assert
        Assert.That(expected, Is.EqualTo(testuser.Password));
    }

    [Test]
    public void TestUserEloUpdate()
    {
        // arrange
        testuser.UpdateElo(12);
        // act
        int expected = 112;
        // assert
        Assert.That(expected, Is.EqualTo(testuser.Elo));
    }
    [Test]
    public void TestUserCoinsUpdate()
    {
        // arrange
        testuser.UpdateCoins(-5);
        // act
        int expected = 15;
        // assert
        Assert.That(expected, Is.EqualTo(testuser.Coins));
    }
}

[TestFixture]
public class StackTests
{
    private Stack testStack;
    private Card testCard;

    [SetUp]
    public void Setup()
    {
        testStack = new Stack();
        testCard = new MonsterCard("Dragon",20,ElementType.Fire,TypeOfMonster.Dragon);
    }

    [Test]
    public void TestAddCardToStack()
    {
        // arrange 
        testStack.AddCardToStack(testCard);
        // act
        Card expected = testCard;
        // assert
        Assert.That(expected,Is.EqualTo(testStack.getCard(0)));
    }
    [Test]
    public void TestRemoveCardFromStack()
    {
        // arrange
        testStack.AddCardToStack(testCard);
        var oldCard = testStack.getCard(0);
        // act
        testStack.RemoveCardFromStack(oldCard);
        // assert
        Assert.That(testStack.Count(), Is.EqualTo(0));
        
    }
}
