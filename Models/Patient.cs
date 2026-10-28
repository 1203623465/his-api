using System;
using System.Collections.Generic;

namespace His.Api.Models;

/// <summary>
/// 患者表
/// </summary>
public partial class Patient
{
    /// <summary>
    /// 自增主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 患者名称
    /// </summary>
    public string? PatientName { get; set; }

    /// <summary>
    /// 患者性别 1男 2女 0未设置
    /// </summary>
    public sbyte? PatientSex { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    public DateTime? PatientBirthday { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    public string? PatientIdno { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? PatientPhone { get; set; }

    /// <summary>
    /// 患者地址
    /// </summary>
    public string? PatientAddress { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
}
