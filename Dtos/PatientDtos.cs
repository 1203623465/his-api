using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class PatientListItemDto
{
    public long Id { get; set; }
    public string? PatientName { get; set; }
    public sbyte? PatientSex { get; set; }
    public DateTime? PatientBirthday { get; set; }
    public string? PatientIdno { get; set; }
    public string? PatientPhone { get; set; }
    public string? PatientAddress { get; set; }
    public DateTime? CreateTime { get; set; }

    public PatientListItemDto(
        long id,
        string? patientName,
        sbyte? patientSex,
        DateTime? patientBirthday,
        string? patientIdno,
        string? patientPhone,
        string? patientAddress,
        DateTime? createTime)
    {
        Id = id;
        PatientName = patientName;
        PatientSex = patientSex;
        PatientBirthday = patientBirthday;
        PatientIdno = patientIdno;
        PatientPhone = patientPhone;
        PatientAddress = patientAddress;
        CreateTime = createTime;
    }
}

public class PatientSaveRequest
{
    [Required(ErrorMessage = "患者姓名不能为空")]
    [StringLength(50, ErrorMessage = "患者姓名长度不能超过50")]
    public string PatientName { get; set; } = string.Empty;

    /// <summary>1男 2女 0未设置</summary>
    [Range(0, 2, ErrorMessage = "性别只能是 0/1/2")]
    public sbyte PatientSex { get; set; } = 0;

    public DateTime? PatientBirthday { get; set; }

    [StringLength(18, MinimumLength = 18, ErrorMessage = "身份证号必须为18位")]
    public string? PatientIdno { get; set; }

    [StringLength(20, ErrorMessage = "手机号长度不能超过20")]
    public string? PatientPhone { get; set; }

    [StringLength(50, ErrorMessage = "地址长度不能超过50")]
    public string? PatientAddress { get; set; }
}
