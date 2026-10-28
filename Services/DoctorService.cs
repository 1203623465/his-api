using His.Api.Dtos;
using His.Api.Middleware;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 医生业务服务：分页/列表查询与增删改。
/// 删除保护：医生被挂号、就诊或处方引用后不可删除，避免产生孤儿业务数据。
/// 在职状态：0 在职（可挂号），1 离职（不可挂号）。
/// </summary>
public interface IDoctorService
{
    Task<PageResult<DoctorListItemDto>> GetPagedAsync(PageRequest req, int? deptId);
    Task<IReadOnlyList<DoctorListItemDto>> GetActiveListAsync(int? deptId);
    Task<DoctorListItemDto> CreateAsync(DoctorSaveRequest req);
    Task<DoctorListItemDto> UpdateAsync(long id, DoctorSaveRequest req);
    Task DeleteAsync(long id);
}

public class DoctorService : IDoctorService
{
    private readonly HisDbContext _db;

    public DoctorService(HisDbContext db)
    {
        _db = db;
    }

    private static DoctorListItemDto ToDto(Doctor d, IReadOnlyDictionary<int, string?> deptNames)
    {
        string? deptName = null;
        if (d.DeptId.HasValue)
        {
            deptNames.TryGetValue(d.DeptId.Value, out deptName);
        }
        return new DoctorListItemDto(d.Id, d.DoctorName, d.DoctorTitle, d.DeptId, deptName, d.IsActive);
    }

    private async Task<Dictionary<int, string?>> GetDeptNameMapAsync(IEnumerable<int?> deptIds)
    {
        var ids = deptIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        return await _db.DeptDicts.Where(d => ids.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.DeptName);
    }

    public async Task<PageResult<DoctorListItemDto>> GetPagedAsync(PageRequest req, int? deptId)
    {
        var query = _db.Doctors.AsQueryable();
        if (deptId.HasValue)
        {
            query = query.Where(d => d.DeptId == deptId.Value);
        }
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            query = query.Where(d =>
                (d.DoctorName != null && d.DoctorName.Contains(req.Keyword)) ||
                (d.DoctorTitle != null && d.DoctorTitle.Contains(req.Keyword)));
        }

        var total = await query.LongCountAsync();
        var entities = await query.OrderBy(d => d.Id).Skip(req.Skip).Take(req.Size).ToListAsync();
        var deptNames = await GetDeptNameMapAsync(entities.Select(e => e.DeptId));
        var items = entities.Select(d => ToDto(d, deptNames)).ToList();
        return new PageResult<DoctorListItemDto>(total, items);
    }

    public async Task<IReadOnlyList<DoctorListItemDto>> GetActiveListAsync(int? deptId)
    {
        var query = _db.Doctors.Where(d => d.IsActive == 0);
        if (deptId.HasValue)
        {
            query = query.Where(d => d.DeptId == deptId.Value);
        }

        var entities = await query.OrderBy(d => d.Id).ToListAsync();
        var deptNames = await GetDeptNameMapAsync(entities.Select(e => e.DeptId));
        return entities.Select(d => ToDto(d, deptNames)).ToList();
    }

    public async Task<DoctorListItemDto> CreateAsync(DoctorSaveRequest req)
    {
        await EnsureDeptExistsAsync(req.DeptId);

        var entity = new Doctor
        {
            DoctorName = req.DoctorName,
            DoctorTitle = req.DoctorTitle,
            DeptId = req.DeptId,
            IsActive = req.IsActive
        };
        _db.Doctors.Add(entity);
        await _db.SaveChangesAsync();
        var deptNames = await GetDeptNameMapAsync(new[] { entity.DeptId });
        return ToDto(entity, deptNames);
    }

    public async Task<DoctorListItemDto> UpdateAsync(long id, DoctorSaveRequest req)
    {
        var entity = await _db.Doctors.FindAsync(id)
            ?? throw new ApiException("医生不存在", 404);
        await EnsureDeptExistsAsync(req.DeptId);

        entity.DoctorName = req.DoctorName;
        entity.DoctorTitle = req.DoctorTitle;
        entity.DeptId = req.DeptId;
        entity.IsActive = req.IsActive;
        await _db.SaveChangesAsync();
        var deptNames = await GetDeptNameMapAsync(new[] { entity.DeptId });
        return ToDto(entity, deptNames);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _db.Doctors.FindAsync(id)
            ?? throw new ApiException("医生不存在", 404);

        if (await _db.ClinicRegs.AnyAsync(r => r.DoctorId == id) ||
            await _db.ClinicVisits.AnyAsync(v => v.DoctorId == id) ||
            await _db.Prescriptions.AnyAsync(p => p.DoctorId == id))
        {
            throw new ApiException("该医生存在挂号/就诊/处方记录，不能删除");
        }

        _db.Doctors.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureDeptExistsAsync(int? deptId)
    {
        if (deptId.HasValue && !await _db.DeptDicts.AnyAsync(d => d.Id == deptId.Value))
        {
            throw new ApiException("所属科室不存在");
        }
    }
}
