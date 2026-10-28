using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 处方头
/// </summary>
public partial class PrescriptionDetail
{
    /// <summary>
    /// 处方明细id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 处方id
    /// </summary>
    public long? PresId { get; set; }

    /// <summary>
    /// 药品id
    /// </summary>
    public long? DrugId { get; set; }

    /// <summary>
    /// 数量 数量存在小数比如 0.5片
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 小计= quantity x  unit_price
    /// </summary>
    public decimal? Amount { get; set; }
}
