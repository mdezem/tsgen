using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLab.TSGen.Tests.Models
{
    public enum Gender
    {
        Male,
        Female
    }

    public class Prop
    {
        public string Name { get;set; }
        public object Value { get; set; }
    }

    public class CustomerModel
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
        public Gender Gender { get; set; }
        public List<string> Emails { get; set; }
        public List<Prop> Properties { get; set; }
    }
}

