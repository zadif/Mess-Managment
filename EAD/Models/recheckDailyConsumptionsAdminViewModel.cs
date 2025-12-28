namespace EAD.Models
{
    public class recheckDailyConsumptionAdminViewModel
    {

        public int Id { get; set; }



        public DateOnly ConsumptionDate { get; set; }

        public int Quantity { get; set; }


        public int? BillId { get; set; }

        public bool WasUserPresent { get; set; }


        public virtual User User { get; set; } = null!;

        public virtual MealItem MealItem { get; set; } = null!;


    }
}
