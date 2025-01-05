using System.Security.Cryptography;
using System.Text;

namespace MCTG.Services;

public class PasswordService
{
    public string PasswordHash(string plainPassword, byte[] salt)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Getting the byte of the plain password
            byte[] passwordBytes = Encoding.UTF8.GetBytes(plainPassword);
            // Creating the salted password byte array
            byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
            // Copy the salt and password into the saltedPassword array
            // Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
            Array.Copy(salt,0,saltedPassword,0,salt.Length);
            Array.Copy(passwordBytes,0,saltedPassword,salt.Length,passwordBytes.Length);
            // Now the Hashing is being done
            byte[] hashBytes = sha256Hash.ComputeHash(saltedPassword);
            // converting the byte to string and then return its value
            return Convert.ToBase64String(hashBytes);
        }
    }
    public byte[] GenerateSalt()
    {
        // Creating salt from 16 byte
        byte[] salt = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            // this code will randomize the Bytes in the salt array
            rng.GetBytes(salt);
        }
        return salt;
    }
    public bool ValidatePassword(string storedPassword, string plainPassword, byte[] salt)
    {
        return storedPassword == PasswordHash(plainPassword, salt);
    }
}