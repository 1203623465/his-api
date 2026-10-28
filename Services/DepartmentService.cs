using His.Api.Dtos;
using His.Api.Middleware;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 科室业务服务。
/// <para>业务规则：科室名称唯一；删除前校验是否被医生/挂号记录引用（有引用则拒绝删除）。
/// 启用状态：0 启用（可挂号），1 停用。</para>
/// <para>查询采用"实体分页 + 字典拼装"模式：官方 MySql.EntityFrameworkCore 无法翻译
/// 联表投影 DTO 后再排序的 LINQ，故先分页取实体，再在内存中拼装科室类型名称。</para>
/// </summary>
public interface IDepartmentService
{
    Task<PageResult<DepartmentListItemDto>> GetPagedAsync(PageRequest req);
    Task<IReadOnlyList<DepartmentListItemDto>> GetAllActiveAsync();
    Task<IReadOnlyList<DepartmentTypeDto>> GetTypesAsync();
    Task<DepartmentListItemDto> CreateAsync(DepartmentSaveRequest req);
    Task<DepartmentListItemDto> UpdateAsync(int id, DepartmentSaveRequest req);
    Task DeleteAsync(int id);
}

public class DepartmentService : IDepartmentService
{
    private readonly HisDbContext _db;

    public DepartmentService(HisDbContext db)
    {
        _db = db;
    }

    private async Task<Dictionary<int, string?>> GetTypeNameMapAsync()
    {
        return await _db.DeptTypeDicts.ToDictionaryAsync(t => t.Id, t => t.DetTypeName);
    }

    private static DepartmentListItemDto ToDto(DeptDict d, IReadOnlyDictionary<int, string?> typeNames)
    {
        string? typeName = null;
        if (d.DeptType.HasValue)
        {
            typeNames.TryGetValue(d.DeptType.Value, out typeName);
        }
        return new DepartmentListItemDto(d.Id, d.DeptName, d.DeptType, typeName, d.DeptNote, d.IsActive);
    }

    public async Task<PageResult<DepartmentListItemDto>> GetPagedAsync(PageRequest req)
    {
        var query = _db.DeptDicts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            query = query.Where(d => d.DeptName != null && d.DeptName.Contains(req.Keyword));
        }

        var total = await query.LongCountAsync();
        var entities = await query.OrderBy(d => d.Id).Skip(req.Skip).Take(req.Size).ToListAsync();
        var typeNames = await GetTypeNameMapAsync();
        var items = entities.Select(d => ToDto(d, typeNames)).ToList();
        return new PageResult<DepartmentListItemDto>(total, items);
    }

    public async Task<IReadOnlyList<DepartmentListItemDto>> GetAllActiveAsync()
    {
        var entities = await _db.DeptDicts.Where(d => d.IsActive == 0).OrderBy(d => d.Id).ToListAsync();
        var typeNames = await GetTypeNameMapAsync();
        return entities.Select(d => ToDto(d, typeNames)).ToList();
    }

    public async Task<IReadOnlyList<DepartmentTypeDto>> GetTypesAsync()
    {
        return await _db.DeptTypeDicts
            .OrderBy(t => t.Id)
            .Select(t => new DepartmentTypeDto(t.Id, t.DetTypeName))
            .ToListAsync();
    }

    public async Task<DepartmentListItemDto> CreateAsync(DepartmentSaveRequest req)
    {
        if (await _db.DeptDicts.AnyAsync(d => d.DeptName == req.DeptName))
        {
            throw new ApiException("科室名称已存在");
        }
        await EnsureTypeExistsAsync(req.DeptType);

        var entity = new DeptDict
        {
            DeptName = req.DeptName,
            DeptType = req.DeptType,
            DeptNote = req.DeptNote,
            IsActive = req.IsActive
        };
        _db.DeptDicts.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity, await GetTypeNameMapAsync());
    }

    public async Task<DepartmentListItemDto> UpdateAsync(int id, DepartmentSaveRequest req)
    {
        var entity = await _db.DeptDicts.FindAsync(id)
            ?? throw new ApiException("科室不存在", 404);

        if (await _db.DeptDicts.AnyAsync(d => d.DeptName == req.DeptName && d.Id != id))
        {
            throw new ApiException("科室名称已存在");
        }
        await EnsureTypeExistsAsync(req.DeptType);

        entity.DeptName = req.DeptName;
        entity.DeptType = req.DeptType;
        entity.DeptNote = req.DeptNote;
        entity.IsActive = req.IsActive;
        await _db.SaveChangesAsync();
        return ToDto(entity, await GetTypeNameMapAsync());
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.DeptDicts.FindAsync(id)
            ?? throw new ApiException("科室不存在", 404);

        if (await _db.Doctors.AnyAsync(d => d.DeptId == id))
        {
            throw new ApiException("该科室下存在医生，不能删除");
        }
        if (await _db.ClinicRegs.AnyAsync(r => r.DeptId == id))
        {
            throw new ApiException("该科室存在挂号记录，不能删除");
        }

        _db.DeptDicts.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureTypeExistsAsync(int? deptType)
    {
        if (deptType.HasValue && !await _db.DeptTypeDicts.AnyAsync(t => t.Id == deptType.Value))
        {
            throw new ApiException("科室类型不存在");
        }
    }
}
