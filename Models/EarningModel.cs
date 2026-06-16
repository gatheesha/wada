using System;
using System.Collections.Generic;
using System.Text;

namespace wada.Models
{
    class EarningModel
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        // Foreign Keys (Nullable if a transaction isn't tied to a specific client/project)
        public int? ProjectId { get; set; }
        public int? ClientId { get; set; }

        // Display Properties for the DataGrid
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
    }
}
