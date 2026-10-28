using System;
using System.Collections.Generic;

namespace His.Api.Models;

public partial class ViewReg
{
    /// <summary>
    /// 自增主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 患者ID
    /// </summary>
    public long? PatientId { get; set; }

    /// <summary>
    /// 科室ID
    /// </summary>
    public int? DeptId { get; set; }

    /// <summary>
    /// 医生ID
    /// </summary>
    public long? DoctorId { get; set; }

    /// <summary>
    /// 1普通 2专家
    /// </summary>
    public sbyte? RegType { get; set; }

    /// <summary>
    /// 挂号时间
    /// </summary>
    public DateTime? RegTime { get; set; }

    /// <summary>
    /// 1已挂号 2已就诊 3已退号
    /// </summary>
    public sbyte? RegStatus { get; set; }

    /// <summary>
    /// 挂号费
    /// </summary>
    public decimal? RegFee { get; set; }
}
