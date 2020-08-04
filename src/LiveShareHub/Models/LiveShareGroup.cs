using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveShareHub.Models
{
    public class LiveShareGroup
    {
        public string groupId { get; set; }
        public string groupOwnerPassword { get; set; }
        public string groupClientPassword { get; set; }
    }
}
