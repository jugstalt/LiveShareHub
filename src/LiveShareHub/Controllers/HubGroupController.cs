using LiveShareHub.Core.Abstraction;
using LiveShareHub.Core.Reflection;
using LiveShareHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
        [AuthorizeAccess(AuthorizationType.Basic)]
        async public Task<LiveShareGroup> Get(bool simplify = false)
        {
            try
            {
                var groupId = _groupIdProvider.GenerateGroupId();

                return new LiveShareGroup()
                {
                    groupId = groupId,
                    groupOwnerPassword = _groupIdProvider.GenerateGroupOwnerPassword(groupId),
                    groupClientPassword = _groupIdProvider.GenerateGroupClientPassword(groupId),
                    simpleGroupId = simplify ? await _groupIdProvider.SimplyGroupId(groupId) : null
                };
            }
            catch (Exception ex)
            {
                return new LiveShareGroup(ex);
            }
        }

        [HttpGet("{id}")]
        [AuthorizeAccess(AuthorizationType.Basic)]
        async public Task<LiveShareGroup> GetUnsimplify(string id)
        {
            try
            {
                return new LiveShareGroup()
                {
                    groupId = await _groupIdProvider.UnsimplyGroupId(id),
                    simpleGroupId = id
                };
            }
            catch (Exception ex)
            {
                return new LiveShareGroup(ex);
            }
        }
    }
}
