namespace EAD.Models
{

    //When we are sending data to Daily Meals , there we are using it 
    public class DailyMenuViewModel
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = null!;
        public string MealType { get; set; } = null!;
        public string MealItemName { get; set; } = "Not Set";
        public string Description { get; set; }="Lazeez";

        public decimal Price { get; set; }
        public string Category { get; set; } = "";
    }
}
