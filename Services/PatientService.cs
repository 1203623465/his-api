using His.Api.Dtos;
using His.Api.Middleware;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 患者档案业务服务。
/// <para>业务规则：身份证号唯一（可空）；删除前校验是否被挂号/就诊/处方/收费引用，
/// 存在任何业务数据则拒绝删除，保证历史记录可追溯。</para>
/// </summary>
public interface IPatientService
{
    Task<PageResult<PatientListItemDto>> GetPagedAsync(PageRequest req);
    Task<PatientListItemDto> GetByIdAsync(long id);
    Task<PatientListItemDto> CreateAsync(PatientSaveRequest req);
    Task<PatientListItemDto> UpdateAsync(long id, PatientSaveRequest req);
    Task DeleteAsync(long id);
}

public class PatientService : IPatientService
{
    private readonly HisDbContext _db;

    public PatientService(HisDbContext db)
    {
        _db = db;
    }

    private static PatientListItemDto ToDto(Patient p) => new(
        p.Id, p.PatientName, p.PatientSex, p.PatientBirthday,
        p.PatientIdno, p.PatientPhone, p.PatientAddress, p.CreateTime);

    public async Task<PageResult<PatientListItemDto>> GetPagedAsync(PageRequest req)
    {
        var query = _db.Patients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            query = query.Where(p =>
                (p.PatientName != null && p.PatientName.Contains(req.Keyword)) ||
                (p.PatientPhone != null && p.PatientPhone.Contains(req.Keyword)) ||
                (p.PatientIdno != null && p.PatientIdno.Contains(req.Keyword)));
        }

        var total = await query.LongCountAsync();
        var items = await query.OrderByDescending(p => p.Id).Skip(req.Skip).Take(req.Size).ToListAsync();
        return new PageResult<PatientListItemDto>(total, items.Select(ToDto).ToList());
    }

    public async Task<PatientListItemDto> GetByIdAsync(long id)
    {
        var entity = await _db.Patients.FindAsync(id)
            ?? throw new ApiException("患者不存在", 404);
        return ToDto(entity);
    }

    public async Task<PatientListItemDto> CreateAsync(PatientSaveRequest req)
    {
        await EnsureIdnoUniqueAsync(req.PatientIdno, null);

        var entity = new Patient
        {
            PatientName = req.PatientName,
            PatientSex = req.PatientSex,
            PatientBirthday = req.PatientBirthday,
            PatientIdno = req.PatientIdno,
            PatientPhone = req.PatientPhone,
            PatientAddress = req.PatientAddress,
            CreateTime = DateTime.Now
        };
        _db.Patients.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<PatientListItemDto> UpdateAsync(long id, PatientSaveRequest req)
    {
        var entity = await _db.Patients.FindAsync(id)
            ?? throw new ApiException("患者不存在", 404);
        await EnsureIdnoUniqueAsync(req.PatientIdno, id);

        entity.PatientName = req.PatientName;
        entity.PatientSex = req.PatientSex;
        entity.PatientBirthday = req.PatientBirthday;
        entity.PatientIdno = req.PatientIdno;
        entity.PatientPhone = req.PatientPhone;
        entity.PatientAddress = req.PatientAddress;
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _db.Patients.FindAsync(id)
            ?? throw new ApiException("患者不存在", 404);

        if (await _db.ClinicRegs.AnyAsync(r => r.PatientId == id) ||
            await _db.ClinicVisits.AnyAsync(v => v.PatientId == id) ||
            await _db.Prescriptions.AnyAsync(p => p.PatientId == id) ||
            await _db.PayRecords.AnyAsync(p => p.PatientId == id))
        {
            throw new ApiException("该患者存在业务数据，不能删除");
        }

        _db.Patients.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureIdnoUniqueAsync(string? idno, long? excludeId)
    {
        if (string.IsNullOrWhiteSpace(idno))
        {
            return;
        }
        var exists = await _db.Patients.AnyAsync(p => p.PatientIdno == idno && (!excludeId.HasValue || p.Id != excludeId.Value));
        if (exists)
        {
            throw new ApiException("身份证号已存在");
        }
    }
}
