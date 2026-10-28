using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 就诊记录表
/// </summary>
public partial class ClinicVisit
{
    /// <summary>
    /// 就诊id
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
    /// 医生id
    /// </summary>
    public long? DoctorId { get; set; }

    /// <summary>
    /// 就诊时间
    /// </summary>
    public DateTime? VisitTime { get; set; }

    /// <summary>
    /// 主诉
    /// </summary>
    public string? VisitComplaint { get; set; }

    /// <summary>
    /// 诊断结论
    /// </summary>
    public string? VisitDiagnosis { get; set; }
}
