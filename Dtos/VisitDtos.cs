using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class VisitListItemDto
{
    public long Id { get; set; }
    public long? PatientId { get; set; }
    public string? PatientName { get; set; }
    public long? RegId { get; set; }
    public long? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime? VisitTime { get; set; }
    public string? VisitComplaint { get; set; }
    public string? VisitDiagnosis { get; set; }

    public VisitListItemDto(
        long id,
        long? patientId,
        string? patientName,
        long? regId,
        long? doctorId,
        string? doctorName,
        DateTime? visitTime,
        string? visitComplaint,
        string? visitDiagnosis)
    {
        Id = id;
        PatientId = patientId;
        PatientName = patientName;
        RegId = regId;
        DoctorId = doctorId;
        DoctorName = doctorName;
        VisitTime = visitTime;
        VisitComplaint = visitComplaint;
        VisitDiagnosis = visitDiagnosis;
    }
}

public class VisitStartRequest
{
    [Required(ErrorMessage = "挂号记录不能为空")]
    public long RegId { get; set; }
}

public class VisitSaveRequest
{
    [StringLength(300, ErrorMessage = "主诉长度不能超过300")]
    public string? VisitComplaint { get; set; }

    [StringLength(300, ErrorMessage = "诊断结论长度不能超过300")]
    public string? VisitDiagnosis { get; set; }
}
