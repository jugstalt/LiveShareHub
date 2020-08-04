using LiveShareHub.Core.Abstraction;
using LiveShareHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveShareHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HubGroupController
    {
        private readonly ILogger<HubGroupController> _logger;
        private readonly IGroupIdProvider _groupIdProvider;

        public HubGroupController(ILogger<HubGroupController> logger,
                                  IGroupIdProvider groupIdProvider)
        {
            _logger = logger;
            _groupIdProvider = groupIdProvider;
        }

        [HttpGet]
        public LiveShareGroup Get()
        {
            var groupId = _groupIdProvider.GenerateGroupId();

            return new LiveShareGroup()
            {
                groupId = groupId,
                groupOwnerPassword = _groupIdProvider.GenerateGroupOwnerPassword(groupId),
                groupClientPassword = _groupIdProvider.GenerateGroupClientPassword(groupId)
            };
        }
    }
}
