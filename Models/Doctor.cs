using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 医生表
/// </summary>
public partial class Doctor
{
    /// <summary>
    /// 自增主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 医生名称
    /// </summary>
    public string? DoctorName { get; set; }

    /// <summary>
    /// 医生职称
    /// </summary>
    public string? DoctorTitle { get; set; }

    /// <summary>
    /// 所属科室
    /// </summary>
    public int? DeptId { get; set; }

    /// <summary>
    /// 是否在职 0在职 1离职
    /// </summary>
    public sbyte? IsActive { get; set; }
}
