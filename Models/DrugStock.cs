using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 药品库存
/// </summary>
public partial class DrugStock
{
    /// <summary>
    /// 主键id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 药品id
    /// </summary>
    public long? DrugId { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// 批号
    /// </summary>
    public string? BatchNo { get; set; }

    /// <summary>
    /// 效期
    /// </summary>
    public DateTime? ExpireDate { get; set; }
}
