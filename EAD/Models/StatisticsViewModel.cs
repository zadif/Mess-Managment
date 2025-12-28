using Microsoft.AspNetCore.Mvc;

namespace EAD.Models
{
    public class StatisticsViewModel 
    {
       public  int numUsers;
       public  int numBills;
       
       public  decimal total = 0;
             
             public   int numPaidBills ;
       public  int numMenuItems;
        public int numInactiveUsers;
    }
}
