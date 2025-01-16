using System.IO;

namespace Internal
{
    public class FileDataStorage : IDataStorage
    {
        public void Save(string path, string data)
        {
            File.WriteAllText(path, data);
        }

        public string Load(string path)
        {
            return File.ReadAllText(path);
        }

        public void Delete(string path)
        {
            if (Exists(path))
                File.Delete(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}