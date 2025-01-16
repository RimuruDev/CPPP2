namespace Internal
{
    public interface IDataStorage
    {
        public void Save(string path, string data);
        public string Load(string path);
        public void Delete(string path);
        public bool Exists(string path);
    }
}