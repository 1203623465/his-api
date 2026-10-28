using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class DepartmentListItemDto
{
    public int Id { get; set; }
    public string? DeptName { get; set; }
    public int? DeptType { get; set; }
    public string? DeptTypeName { get; set; }
    public string? DeptNote { get; set; }
    public sbyte? IsActive { get; set; }

    public DepartmentListItemDto(
        int id,
        string? deptName,
        int? deptType,
        string? deptTypeName,
        string? deptNote,
        sbyte? isActive)
    {
        Id = id;
        DeptName = deptName;
        DeptType = deptType;
        DeptTypeName = deptTypeName;
        DeptNote = deptNote;
        IsActive = isActive;
    }
}

public class DepartmentTypeDto
{
    public int Id { get; set; }
    public string? TypeName { get; set; }

    public DepartmentTypeDto(int id, string? typeName)
    {
        Id = id;
        TypeName = typeName;
    }
}

public class DepartmentSaveRequest
{
    [Required(ErrorMessage = "科室名称不能为空")]
    [StringLength(50, ErrorMessage = "科室名称长度不能超过50")]
    public string DeptName { get; set; } = string.Empty;

    public int? DeptType { get; set; }

    [StringLength(255, ErrorMessage = "备注长度不能超过255")]
    public string? DeptNote { get; set; }

    /// <summary>0启用 1停用</summary>
    [Range(0, 1, ErrorMessage = "启用状态只能是 0 或 1")]
    public sbyte IsActive { get; set; } = 0;
}
