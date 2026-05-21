using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace PersonalFinanceApp.DAL
{
    // Tích hợp đọc/ghi file JSON vào lớp Base
    public abstract class BaseDAL<T> : IDataAccess<T>
    {
        protected string FilePath;
        protected List<T> Storage = new List<T>();

        public BaseDAL(string fileName)
        {
            FilePath = fileName;
            LoadFromFile();
        }

        protected void SaveToFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Storage, options);
            File.WriteAllText(FilePath, json);
        }

        protected void LoadFromFile()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                Storage = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
        }

        public abstract bool Insert(T item);
        public abstract bool Update(T item);
        public abstract bool Delete(int id);
        public virtual List<T> GetAll() => Storage.ToList();
    }
}
