using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 支付结算表
/// </summary>
public partial class PayRecord
{
    /// <summary>
    /// 收费结算id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 患者id
    /// </summary>
    public long? PatientId { get; set; }

    /// <summary>
    /// 挂号id
    /// </summary>
    public long? RegId { get; set; }

    /// <summary>
    /// 合计金额
    /// </summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// 1现金 2医保 3扫码
    /// </summary>
    public sbyte? PayType { get; set; }

    /// <summary>
    /// 发票号
    /// </summary>
    public string? InvoiceNo { get; set; }

    /// <summary>
    /// 结算时间 
    /// </summary>
    public DateTime? PayTime { get; set; }
}
