using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class RegistrationListItemDto
{
    public long Id { get; set; }
    public long? PatientId { get; set; }
    public string? PatientName { get; set; }
    public int? DeptId { get; set; }
    public string? DeptName { get; set; }
    public long? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public sbyte? RegType { get; set; }
    public DateTime? RegTime { get; set; }
    public sbyte? RegStatus { get; set; }
    public decimal? RegFee { get; set; }

    public RegistrationListItemDto(
        long id,
        long? patientId,
        string? patientName,
        int? deptId,
        string? deptName,
        long? doctorId,
        string? doctorName,
        sbyte? regType,
        DateTime? regTime,
        sbyte? regStatus,
        decimal? regFee)
    {
        Id = id;
        PatientId = patientId;
        PatientName = patientName;
        DeptId = deptId;
        DeptName = deptName;
        DoctorId = doctorId;
        DoctorName = doctorName;
        RegType = regType;
        RegTime = regTime;
        RegStatus = regStatus;
        RegFee = regFee;
    }
}

public class RegistrationCreateRequest
{
    [Required(ErrorMessage = "患者不能为空")]
    public long PatientId { get; set; }

    [Required(ErrorMessage = "科室不能为空")]
    public int DeptId { get; set; }

    [Required(ErrorMessage = "医生不能为空")]
    public long DoctorId { get; set; }

    /// <summary>1普通 2专家</summary>
    [Range(1, 2, ErrorMessage = "号别只能是 1普通 或 2专家")]
    public sbyte RegType { get; set; } = 1;
}
