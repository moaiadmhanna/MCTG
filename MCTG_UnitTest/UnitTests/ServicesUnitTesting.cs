using MCTG.Services;

namespace MCTG_UnitTest;

public class ServicesUnitTesting
{
    [TestFixture]
    public class PasswordServiceTests
    {
        private PasswordService service;
        [SetUp]
        public void Setup()
        {
            service = new PasswordService();
        }
        [Test]
        public void PasswordHash_ReturnsExpectedHash_WhenGivenPasswordAndSalt()
        {
            // Arrange
            string plainPassword = "MySecurePassword";
            byte[] salt = new byte[16]; // Example static salt for testing
            for (int i = 0; i < salt.Length; i++) salt[i] = (byte)(i + 1);

            // Act
            string hash = service.PasswordHash(plainPassword, salt);

            // Assert
            // Expected hash value (pre-computed for this password and salt)
            string expectedHash = "YknIhi6z5lilWZ+2HN2rK8ydEt7kvnvOHvB517fH8nk=";
            Assert.That(hash, Is.EqualTo(expectedHash));
        }
        [Test]
        public void ValidatePassword_ReturnsTrue_WhenPasswordIsValid()
        {
            // Arrange
            string plainPassword = "ValidPassword";
            byte[] salt = service.GenerateSalt();
            string storedPassword = service.PasswordHash(plainPassword, salt);
            // act
            bool isValid = service.ValidatePassword(storedPassword,plainPassword, salt);
            // assert
            Assert.That(isValid,Is.True);
        }
        [Test]
        public void ValidatePassword_ReturnsFalse_WhenPasswordIsInvalid()
        {
            // Arrange
            string plainPassword = "ValidPassword";
            byte[] salt = service.GenerateSalt();
            string storedPassword = service.PasswordHash(plainPassword, salt);
            // act
            bool isValid = service.ValidatePassword(storedPassword, "InvalidPassword", salt);
            // assert
            Assert.That(isValid,Is.False);
        }
        
    }
}