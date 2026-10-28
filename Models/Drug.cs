using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 药品信息表
/// </summary>
public partial class Drug
{
    /// <summary>
    /// 药品id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 药品名称
    /// </summary>
    public string? DrugName { get; set; }

    /// <summary>
    /// 药品规格(如 0.25g×24片)
    /// </summary>
    public string? DrugSpec { get; set; }

    /// <summary>
    /// 单位（盒/瓶/支）
    /// </summary>
    public string? DrugUnit { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    public decimal? DrugPrice { get; set; }

    /// <summary>
    /// 是否启用 0是未启用 1已启用
    /// </summary>
    public sbyte? IsActive { get; set; }
}
