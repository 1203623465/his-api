using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 发药记录
/// </summary>
public partial class DispenseRecord
{
    /// <summary>
    /// 主键id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 处方id
    /// </summary>
    public long? PresId { get; set; }

    /// <summary>
    /// 患者id
    /// </summary>
    public long? PatientId { get; set; }

    /// <summary>
    /// 发药时间
    /// </summary>
    public DateTime? DispenseTime { get; set; }

    /// <summary>
    /// 药师
    /// </summary>
    public string? Operator { get; set; }
}
