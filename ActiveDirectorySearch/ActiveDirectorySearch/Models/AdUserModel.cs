using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActiveDirectorySearch.Models
{
    public class AdUserModel
    {
        public string AdLogin { get; set; }

        public string Department { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }
    }
}