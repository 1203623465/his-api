using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class PrescriptionListItemDto
{
    public long Id { get; set; }
    public long? VisitId { get; set; }
    public long? PatientId { get; set; }
    public string? PatientName { get; set; }
    public long? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime? PresTime { get; set; }
    public sbyte? PresStatus { get; set; }
    public decimal TotalAmount { get; set; }

    public PrescriptionListItemDto(
        long id,
        long? visitId,
        long? patientId,
        string? patientName,
        long? doctorId,
        string? doctorName,
        DateTime? presTime,
        sbyte? presStatus,
        decimal totalAmount)
    {
        Id = id;
        VisitId = visitId;
        PatientId = patientId;
        PatientName = patientName;
        DoctorId = doctorId;
        DoctorName = doctorName;
        PresTime = presTime;
        PresStatus = presStatus;
        TotalAmount = totalAmount;
    }
}

public class PrescriptionItemDto
{
    public long Id { get; set; }
    public long? DrugId { get; set; }
    public string? DrugName { get; set; }
    public string? DrugSpec { get; set; }
    public string? DrugUnit { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Amount { get; set; }

    public PrescriptionItemDto(
        long id,
        long? drugId,
        string? drugName,
        string? drugSpec,
        string? drugUnit,
        decimal? quantity,
        decimal? unitPrice,
        decimal? amount)
    {
        Id = id;
        DrugId = drugId;
        DrugName = drugName;
        DrugSpec = drugSpec;
        DrugUnit = drugUnit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Amount = amount;
    }
}

public class PrescriptionDetailDto
{
    public long Id { get; set; }
    public long? VisitId { get; set; }
    public long? PatientId { get; set; }
    public string? PatientName { get; set; }
    public long? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime? PresTime { get; set; }
    public sbyte? PresStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public IReadOnlyList<PrescriptionItemDto> Items { get; set; }

    public PrescriptionDetailDto(
        long id,
        long? visitId,
        long? patientId,
        string? patientName,
        long? doctorId,
        string? doctorName,
        DateTime? presTime,
        sbyte? presStatus,
        decimal totalAmount,
        IReadOnlyList<PrescriptionItemDto> items)
    {
        Id = id;
        VisitId = visitId;
        PatientId = patientId;
        PatientName = patientName;
        DoctorId = doctorId;
        DoctorName = doctorName;
        PresTime = presTime;
        PresStatus = presStatus;
        TotalAmount = totalAmount;
        Items = items;
    }
}

public class PrescriptionItemRequest
{
    [Required(ErrorMessage = "药品不能为空")]
    public long DrugId { get; set; }

    [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "数量必须大于0")]
    public decimal Quantity { get; set; }
}

public class PrescriptionCreateRequest
{
    [Required(ErrorMessage = "就诊记录不能为空")]
    public long VisitId { get; set; }

    [MinLength(1, ErrorMessage = "处方至少包含一条药品明细")]
    public List<PrescriptionItemRequest> Items { get; set; } = new();
}
