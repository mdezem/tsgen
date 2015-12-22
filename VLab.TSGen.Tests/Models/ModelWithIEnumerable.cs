using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLab.TSGen.Tests.Models
{
    public class ModelWithIEnumerable
    {
        public int Id { get; set; }
        public IEnumerable<Prop> PropsEnumerable { get; set; }
    }
}
