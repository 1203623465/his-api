using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 处方头
/// </summary>
public partial class Prescription
{
    /// <summary>
    /// 处方头id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 就诊id
    /// </summary>
    public long? VisitId { get; set; }

    /// <summary>
    /// 患者id
    /// </summary>
    public long? PatientId { get; set; }

    /// <summary>
    /// 医生id
    /// </summary>
    public long? DoctorId { get; set; }

    /// <summary>
    /// 开方时间
    /// </summary>
    public DateTime? PresTime { get; set; }

    /// <summary>
    /// 开方状态 1未收费 2已收费 3已发药 4已退款 
    /// </summary>
    public sbyte? PresStatus { get; set; }
}
