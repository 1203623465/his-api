using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 药品库存
/// </summary>
public partial class RefundRecord
{
    /// <summary>
    /// 主键id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 原结算单号
    /// </summary>
    public long? OriginalPayId { get; set; }

    /// <summary>
    /// 退款金额
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// 退款时间
    /// </summary>
    public DateTime? RefundTime { get; set; }

    /// <summary>
    /// 退款原因
    /// </summary>
    public string? Reason { get; set; }
}
