namespace Firefly.Models;

public class FireProduct
{
    public FireCell? C1 { get; set; }

    public FireCell? CertificateAndReportNumber { get; set; }

    public FireCell CertificateNumber { get; set; }

    public FireCell? Count { get; set; }

    public FireCell EnterpriseName { get; set; }

    public FireSystem FireSystem { get; set; }

    public bool IsRuleSkipped { get; set; } = false;

    public FireCell? ManufactureDate { get; set; }

    public FireCell Model { get; set; }

    public FireCell? ModelAndEnterpriseName { get; set; }

    public FireCell Name { get; set; }

    public FireCell? Remark { get; set; }

    public FireCell? ReportNumber { get; set; }

    public FireCell? Status { get; set; }

    public FireProduct(string fireSystem, FireCell? c1, FireCell name, FireCell? count,
        FireCell? modelEnterpriseName, FireCell model, FireCell enterpriseName,
        FireCell? certificateAndReportNumber, FireCell certificateNumber, FireCell? reportNumber,
        FireCell? status, FireCell? manufactureDate, FireCell? remark, bool isRuleSkipped = false)
    {
        FireSystem = fireSystem;

        C1 = c1;
        Name = name;
        Count = count;
        ModelAndEnterpriseName = modelEnterpriseName;
        Model = model;
        EnterpriseName = enterpriseName;
        CertificateAndReportNumber = certificateAndReportNumber;
        CertificateNumber = certificateNumber;
        ReportNumber = reportNumber;
        Status = status;
        ManufactureDate = manufactureDate;
        Remark = remark;
        IsRuleSkipped = isRuleSkipped;
    }
}
