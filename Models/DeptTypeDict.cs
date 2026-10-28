using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 科室类型字典表
/// </summary>
public partial class DeptTypeDict
{
    /// <summary>
    /// 自增主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 科室类型名称  临床 医技 行政之类的
    /// </summary>
    public string? DetTypeName { get; set; }
}
