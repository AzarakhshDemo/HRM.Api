using meTesting.HRM.Services;

namespace meTesting.HRM.API;

public class Mock(IPersonelService personelService, IChartService chartService, IPersonelAttrributeService personelAttrributeService)
{
    public void inflate()
    {
        var p1 = personelService.Create(new Personel { Name = "hadi" });
        var p2 = personelService.Create(new Personel { Name = "hoda" });

        personelAttrributeService.SetAttribute(p1.Id, PersonelAttributeEnum.HIRE_DATE, DateTime.Now.AddYears(-2));
        personelAttrributeService.SetAttribute(p2.Id, PersonelAttributeEnum.HIRE_DATE, DateTime.Now.AddYears(-1));

        var c1 = chartService.Create(new() { Name = "Modir" });
        var c2 = chartService.Create(new() { Name = "Moaven" });
        var c3 = chartService.Create(new() { Name = "Karshenas" });



        chartService.AssignToPosition(p1.Id, c2.Id);
        chartService.AssignToPosition(p2.Id, c3.Id);
    }
}