namespace IronManServer.Router;

using Callbacks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Utils;

[Injectable(TypePriority = OnLoadOrder.Routers + 67)]
public class StaticRouters(JsonUtil jsonUtil, StaticRouterCallbacks staticRouterCallbacks) : StaticRouter(
jsonUtil,  
[
    new RouteAction<EmptyRequestData>(
    "/ironman/profile/status",
    async (url, info, sessionId, _, _) => await staticRouterCallbacks.GetProfile(url, info, sessionId)
    ),
    new RouteAction<EmptyRequestData>(
    "/ironman/profile/downgrade/accept",
    async (url, info, sessionId, _, _) => await staticRouterCallbacks.AcceptDowngrade(url, info, sessionId)
    ),
    new RouteAction<EmptyRequestData>(
    "/ironman/profile/downgrade/decline",
    async (url, info, sessionId, _, _) => await staticRouterCallbacks.DeclineDowngrade(url, info, sessionId)
    )
])
{ }