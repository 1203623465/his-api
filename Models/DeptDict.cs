using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 科室字典表
/// </summary>
public partial class DeptDict
{
    /// <summary>
    /// 自增主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 科室名称
    /// </summary>
    public string? DeptName { get; set; }

    /// <summary>
    /// 科室类型 详情见det_type字典表 
    /// </summary>
    public int? DeptType { get; set; }

    public string? DeptNote { get; set; }

    /// <summary>
    /// 是否启用 0启用 1 停用
    /// </summary>
    public sbyte? IsActive { get; set; }
}
