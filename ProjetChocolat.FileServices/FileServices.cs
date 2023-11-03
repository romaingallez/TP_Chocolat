using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjetChocolat.Models;

namespace ProjetChocolat.FileServices
{
    public interface IFileService<T>
    {
        List<T> ReadFromFile(string path);
        void WriteToFile(string path, List<T> data);
    }

    public class AdministrateurFileService : IFileService<Administrateur>
    {
        public List<Administrateur> ReadFromFile(string path)
        {
            var jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Administrateur>>(jsonString);
        }

        public void WriteToFile(string path, List<Administrateur> data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(path, jsonString);
            
        }
    }

    public class AcheteurFileService : IFileService<Acheteur>
    {
        public List<Acheteur> ReadFromFile(string path)
        {
            var jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Acheteur>>(jsonString);
        }

        public void WriteToFile(string path, List<Acheteur> data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(path, jsonString);
        }
    }

    public class ArticleFileService : IFileService<Article>
    {
        public List<Article> ReadFromFile(string path)
        {
            var jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Article>>(jsonString);
        }

        public void WriteToFile(string path, List<Article> data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(path, jsonString);
        }
    }

    public class ArticleAcheteFileService : IFileService<ArticleAchete>
    {
        public List<ArticleAchete> ReadFromFile(string path)
        {
            var jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<ArticleAchete>>(jsonString);
        }

        public void WriteToFile(string path, List<ArticleAchete> data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(path, jsonString);
        }
    }
}
