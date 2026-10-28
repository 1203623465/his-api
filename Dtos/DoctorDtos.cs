using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class DoctorListItemDto
{
    public long Id { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorTitle { get; set; }
    public int? DeptId { get; set; }
    public string? DeptName { get; set; }
    public sbyte? IsActive { get; set; }

    public DoctorListItemDto(
        long id,
        string? doctorName,
        string? doctorTitle,
        int? deptId,
        string? deptName,
        sbyte? isActive)
    {
        Id = id;
        DoctorName = doctorName;
        DoctorTitle = doctorTitle;
        DeptId = deptId;
        DeptName = deptName;
        IsActive = isActive;
    }
}

public class DoctorSaveRequest
{
    [Required(ErrorMessage = "医生姓名不能为空")]
    [StringLength(50, ErrorMessage = "医生姓名长度不能超过50")]
    public string DoctorName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "职称长度不能超过50")]
    public string? DoctorTitle { get; set; }

    public int? DeptId { get; set; }

    /// <summary>0在职 1离职</summary>
    [Range(0, 1, ErrorMessage = "在职状态只能是 0 或 1")]
    public sbyte IsActive { get; set; } = 0;
}
