using System.Linq;
using PersonalFinanceApp.DTO;

namespace PersonalFinanceApp.DAL
{
    public class TransactionDAL : BaseDAL<TransactionDTO>
    {
        public TransactionDAL() : base("transactions.json") { }

        public override bool Insert(TransactionDTO item)
        {
            item.Id = Storage.Any() ? Storage.Max(t => t.Id) + 1 : 1;
            Storage.Add(item);
            SaveToFile();
            return true;
        }

        public override bool Update(TransactionDTO item)
        {
            int index = Storage.FindIndex(t => t.Id == item.Id);
            if (index == -1) return false;
            
            Storage[index] = item;
            SaveToFile();
            return true;
        }

        public override bool Delete(int id)
        {
            var item = Storage.Find(t => t.Id == id);
            if (item == null) return false;
            
            Storage.Remove(item);
            SaveToFile();
            return true;
        }
    }
}
