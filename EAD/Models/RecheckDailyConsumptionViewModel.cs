namespace EAD.Models
{
    public class RecheckDailyConsumptionViewModel
    {
       
            public int Id { get; set; }

        

            public DateOnly ConsumptionDate { get; set; }

            public int Quantity { get; set; }

            public bool IsBilled { get; set; }

            public int? BillId { get; set; }

            public bool WasUserPresent { get; set; }


            public virtual MealItem MealItem { get; set; } = null!;
        public virtual Bill Bill { get; set; } = null!;



    }
}
