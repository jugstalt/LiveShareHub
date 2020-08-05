using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveShareHub.Models
{
    public class LiveShareGroup
    {
        public LiveShareGroup()
        {
            success = true;
        }

        public LiveShareGroup(Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
        }

        public string groupId { get; set; }
        public string groupOwnerPassword { get; set; }
        public string groupClientPassword { get; set; }

        public string simpleGroupId { get; set; }

        public bool success { get; set; }
        public string errorMessage { get; set; }
    }
}
