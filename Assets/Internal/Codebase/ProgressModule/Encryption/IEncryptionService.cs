namespace Internal
{
    public interface IEncryptionService
    {
        public string Encrypt(string plainText);
        public string Decrypt(string cipherText);
        public bool IsEncrypted(string data);
    }
}