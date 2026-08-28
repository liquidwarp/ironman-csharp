namespace IronManServer.Services;

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.Preload + 10)]
public class HideoutService(HideoutTable hideoutTable) : IOnLoad {

    public Task OnLoadAsync(CancellationToken ct = default)
    {
        var stashArea = hideoutTable.Areas.First(x => x.Id == "5d484fc0654e76006657e0ab");
        var stageLevel1 = stashArea.Stages?["1"];
        
        stageLevel1?.Requirements?.Add(new StageRequirement
        {
            TemplateId = ItemTpl.MONEY_ROUBLES,
            Count = 500000,
            IsFunctional = false,
            IsEncoded = false,
            IsSpawnedInSession = false,
            Type = "Item"
        });
        
        stageLevel1?.Requirements?.Add(new StageRequirement
        {
            TemplateId = ItemTpl.MONEY_DOLLARS,
            Count = 500,
            IsFunctional = false,
            IsEncoded = false,
            IsSpawnedInSession = false,
            Type = "Item"
        });
        
        stageLevel1?.Requirements?.Add(new StageRequirement
        {
            TemplateId = ItemTpl.MONEY_EUROS,
            Count = 100,
            IsFunctional = false,
            IsEncoded = false,
            IsSpawnedInSession = false,
            Type = "Item"
        });

        stageLevel1?.ConstructionTime = 14400;
        
        return Task.CompletedTask;
    }
}
