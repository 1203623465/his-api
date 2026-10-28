using System;
using System.Collections.Generic;

namespace His.Api.Models;

public partial class ViewOnedaypay
{
    /// <summary>
    /// 科室名称
    /// </summary>
    public string? 科室名称 { get; set; }

    /// <summary>
    /// 患者id
    /// </summary>
    public long? 患者id { get; set; }

    public decimal? 金额 { get; set; }

    public ulong 排名 { get; set; }
}
