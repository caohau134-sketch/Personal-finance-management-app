using System;

namespace PersonalFinanceApp.Models
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public abstract string GetSummary();
    }
}
