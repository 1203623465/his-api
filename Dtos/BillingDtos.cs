using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class ChargeRequest
{
    [Required(ErrorMessage = "处方不能为空")]
    public long PresId { get; set; }

    /// <summary>1现金 2医保 3扫码</summary>
    [Range(1, 3, ErrorMessage = "支付方式只能是 1现金/2医保/3扫码")]
    public sbyte PayType { get; set; } = 1;
}

public class RefundRequest
{
    [Required(ErrorMessage = "处方不能为空")]
    public long PresId { get; set; }

    [StringLength(200, ErrorMessage = "退款原因长度不能超过200")]
    public string? Reason { get; set; }
}

public class DispenseRequest
{
    [Required(ErrorMessage = "处方不能为空")]
    public long PresId { get; set; }

    [StringLength(50, ErrorMessage = "药师姓名长度不能超过50")]
    public string? Operator { get; set; }
}

public class PayRecordDto
{
    public long Id { get; set; }
    public long? PatientId { get; set; }
    public string? PatientName { get; set; }
    public long? RegId { get; set; }
    public decimal? TotalAmount { get; set; }
    public sbyte? PayType { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? PayTime { get; set; }
    public bool Refunded { get; set; }

    public PayRecordDto(
        long id,
        long? patientId,
        string? patientName,
        long? regId,
        decimal? totalAmount,
        sbyte? payType,
        string? invoiceNo,
        DateTime? payTime,
        bool refunded)
    {
        Id = id;
        PatientId = patientId;
        PatientName = patientName;
        RegId = regId;
        TotalAmount = totalAmount;
        PayType = payType;
        InvoiceNo = invoiceNo;
        PayTime = payTime;
        Refunded = refunded;
    }
}
