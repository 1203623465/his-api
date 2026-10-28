using System.ComponentModel.DataAnnotations;

namespace His.Api.Dtos;

public class DrugListItemDto
{
    public long Id { get; set; }
    public string? DrugName { get; set; }
    public string? DrugSpec { get; set; }
    public string? DrugUnit { get; set; }
    public decimal? DrugPrice { get; set; }
    public sbyte? IsActive { get; set; }
    public decimal TotalStock { get; set; }

    public DrugListItemDto(
        long id,
        string? drugName,
        string? drugSpec,
        string? drugUnit,
        decimal? drugPrice,
        sbyte? isActive,
        decimal totalStock)
    {
        Id = id;
        DrugName = drugName;
        DrugSpec = drugSpec;
        DrugUnit = drugUnit;
        DrugPrice = drugPrice;
        IsActive = isActive;
        TotalStock = totalStock;
    }
}

public class DrugSaveRequest
{
    [Required(ErrorMessage = "药品名称不能为空")]
    [StringLength(100, ErrorMessage = "药品名称长度不能超过100")]
    public string DrugName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "规格长度不能超过50")]
    public string? DrugSpec { get; set; }

    [StringLength(10, ErrorMessage = "单位长度不能超过10")]
    public string? DrugUnit { get; set; }

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "单价不能为负数")]
    public decimal? DrugPrice { get; set; }

    /// <summary>0未启用 1已启用</summary>
    [Range(0, 1, ErrorMessage = "启用状态只能是 0 或 1")]
    public sbyte IsActive { get; set; } = 1;
}

public class DrugStockDto
{
    public long Id { get; set; }
    public long? DrugId { get; set; }
    public decimal? Quantity { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpireDate { get; set; }

    public DrugStockDto(
        long id,
        long? drugId,
        decimal? quantity,
        string? batchNo,
        DateTime? expireDate)
    {
        Id = id;
        DrugId = drugId;
        Quantity = quantity;
        BatchNo = batchNo;
        ExpireDate = expireDate;
    }
}

public class StockAddRequest
{
    [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "入库数量必须大于0")]
    public decimal Quantity { get; set; }

    [StringLength(30, ErrorMessage = "批号长度不能超过30")]
    public string? BatchNo { get; set; }

    public DateTime? ExpireDate { get; set; }
}

public class StockUpdateRequest
{
    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "库存数量不能为负数")]
    public decimal Quantity { get; set; }

    [StringLength(30, ErrorMessage = "批号长度不能超过30")]
    public string? BatchNo { get; set; }

    public DateTime? ExpireDate { get; set; }
}
