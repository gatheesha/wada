using System;
using System.Collections.Generic;
using System.Text;

namespace wada.Models
{
    internal class ProjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;


    }

}
