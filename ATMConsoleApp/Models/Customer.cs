using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMConsoleApp.Models {
    public class Customer {

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CardCode { get; set; }
        public int PinCode { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

    }
}
