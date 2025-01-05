using MCTG;
using NSubstitute;
using MCTG.Data.Repositories;

namespace MCTG_UnitTest
{
    [TestFixture]
    public class RepositoryTests
    {
        private ITokenRepo _mockTokenRepo;
        private ICardRepo _mockCardRepo;
        private IUserRepo _mockUserRepo;

        [SetUp]
        public void Setup()
        {
            _mockTokenRepo = Substitute.For<ITokenRepo>();
            _mockCardRepo = Substitute.For<ICardRepo>();
            _mockUserRepo = Substitute.For<IUserRepo>();
        }

        #region ITokenRepo Tests

        [Test]
        public async Task GetUserUid_ShouldReturnValidGuid_WhenTokenIsValid()
        {
            // Arrange
            var token = "validToken";
            var expectedUserId = Guid.NewGuid();

            _mockTokenRepo.GetUserUid(token).Returns(expectedUserId);

            // Act
            var result = await _mockTokenRepo.GetUserUid(token);

            // Assert
            Assert.AreEqual(expectedUserId, result);
            await _mockTokenRepo.Received(1).GetUserUid(token);
        }

        [Test]
        public async Task GetUserUid_ShouldReturnEmptyGuid_WhenTokenIsInvalid()
        {
            // Arrange
            var token = "invalidToken";

            _mockTokenRepo.GetUserUid(token).Returns(Guid.Empty);

            // Act
            var result = await _mockTokenRepo.GetUserUid(token);

            // Assert
            Assert.AreEqual(Guid.Empty, result);
            await _mockTokenRepo.Received(1).GetUserUid(token);
        }

        #endregion

        #region ICardRepo Tests

        [Test]
        public async Task GetAllCardsFromDeck_ShouldReturnCards_WhenUserHasCardsInDeck()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockCards = new List<Card>
            {
                new MonsterCard("Dragon", 10, ElementType.Fire, TypeOfMonster.Dragon)
            };

            _mockCardRepo.GetAllCardsFromDeck(userId).Returns(mockCards);

            // Act
            var result = await _mockCardRepo.GetAllCardsFromDeck(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockCards, result);
            await _mockCardRepo.Received(1).GetAllCardsFromDeck(userId);
        }

        [Test]
        public async Task AddCardToDeck_ShouldReturnTrue_WhenCardIsAddedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cardId = Guid.NewGuid();

            _mockCardRepo.AddCardToDeck(cardId, userId).Returns(true);

            // Act
            var result = await _mockCardRepo.AddCardToDeck(cardId, userId);

            // Assert
            Assert.IsTrue(result);
            await _mockCardRepo.Received(1).AddCardToDeck(cardId, userId);
        }

        [Test]
        public async Task DeckConfigured_ShouldReturnTrue_WhenDeckIsConfigured()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockCardRepo.DeckConfigured(userId).Returns(true);

            // Act
            var result = await _mockCardRepo.DeckConfigured(userId);

            // Assert
            Assert.IsTrue(result);
            await _mockCardRepo.Received(1).DeckConfigured(userId);
        }

        [Test]
        public async Task DeckConfigured_ShouldReturnFalse_WhenDeckIsNotConfigured()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockCardRepo.DeckConfigured(userId).Returns(false);

            // Act
            var result = await _mockCardRepo.DeckConfigured(userId);

            // Assert
            Assert.IsFalse(result);
            await _mockCardRepo.Received(1).DeckConfigured(userId);
        }

        #endregion

        #region IUserRepo Tests

        [Test]
        public async Task AddUser_ShouldReturnTrue_WhenUserIsAddedSuccessfully()
        {
            // Arrange
            var newUser = new User("NewUser", "Password123", new byte[0], 0, 0);

            _mockUserRepo.AddUser(newUser).Returns(true);

            // Act
            var result = await _mockUserRepo.AddUser(newUser);

            // Assert
            Assert.IsTrue(result);
            await _mockUserRepo.Received(1).AddUser(newUser);
        }

        [Test]
        public async Task UserExists_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var username = "ExistingUser";

            _mockUserRepo.UserExists(username).Returns(true);

            // Act
            var result = await _mockUserRepo.UserExists(username);

            // Assert
            Assert.IsTrue(result);
            await _mockUserRepo.Received(1).UserExists(username);
        }

        [Test]
        public async Task UserExists_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "NonExistingUser";

            _mockUserRepo.UserExists(username).Returns(false);

            // Act
            var result = await _mockUserRepo.UserExists(username);

            // Assert
            Assert.IsFalse(result);
            await _mockUserRepo.Received(1).UserExists(username);
        }

        [Test]
        public async Task GetUser_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockUser = new User("ExistingUser", "Password123", new byte[0], 100, 1500);

            _mockUserRepo.GetUser(userId).Returns(mockUser);

            // Act
            var result = await _mockUserRepo.GetUser(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockUser, result);
            await _mockUserRepo.Received(1).GetUser(userId);
        }

        [Test]
        public async Task GetUser_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepo.GetUser(userId).Returns((User?)null);

            // Act
            var result = await _mockUserRepo.GetUser(userId);

            // Assert
            Assert.IsNull(result);
            await _mockUserRepo.Received(1).GetUser(userId);
        }

        #endregion
    }
}
