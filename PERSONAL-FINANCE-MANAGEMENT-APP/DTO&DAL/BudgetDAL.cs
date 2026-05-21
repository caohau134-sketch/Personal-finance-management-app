using System.Linq;
using PersonalFinanceApp.DTO;

namespace PersonalFinanceApp.DAL
{
    public class BudgetDAL : BaseDAL<BudgetDTO>
    {
        public BudgetDAL() : base("budgets.json") { }

        public override bool Insert(BudgetDTO item)
        {
            item.Id = Storage.Any() ? Storage.Max(b => b.Id) + 1 : 1;
            Storage.Add(item);
            SaveToFile();
            return true;
        }

        public override bool Update(BudgetDTO item)
        {
            int index = Storage.FindIndex(b => b.Id == item.Id);
            if (index == -1) return false;

            Storage[index] = item;
            SaveToFile();
            return true;
        }

        public override bool Delete(int id)
        {
            var item = Storage.Find(b => b.Id == id);
            if (item == null) return false;

            Storage.Remove(item);
            SaveToFile();
            return true;
        }
    }
}