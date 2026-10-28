using His.Api.Dtos;
using His.Api.Middleware;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 挂号业务服务。
/// <para>核心业务规则：</para>
/// <list type="bullet">
/// <item>挂号费固定价：普通号 10 元，专家号 30 元（RegFees 字典维护）</item>
/// <item>挂号校验：科室须存在且启用；医生须在职且属于所选科室</item>
/// <item>状态流转：1 已挂号 → 2 已就诊（接诊时回写） / 3 已退号（仅"已挂号"可退号）</item>
/// </list>
/// 查询采用"实体分页 + 字典拼装"模式：官方 MySql.EntityFrameworkCore 无法翻译
/// 联表投影 DTO 后再排序的 LINQ，故先分页取实体，再批量补患者/科室/医生名称。
/// </summary>
public interface IRegistrationService
{
    Task<PageResult<RegistrationListItemDto>> GetPagedAsync(PageRequest req, sbyte? status, DateTime? date);
    Task<IReadOnlyList<RegistrationListItemDto>> GetQueueAsync(long? doctorId);
    Task<RegistrationListItemDto> CreateAsync(RegistrationCreateRequest req);
    Task CancelAsync(long id);
}

public class RegistrationService : IRegistrationService
{
    private static readonly Dictionary<sbyte, decimal> RegFees = new()
    {
        { 1, 10.00m },
        { 2, 30.00m }
    };

    private readonly HisDbContext _db;

    public RegistrationService(HisDbContext db)
    {
        _db = db;
    }

    private async Task<List<RegistrationListItemDto>> ToDtosAsync(List<ClinicReg> entities)
    {
        var patientIds = entities.Where(e => e.PatientId.HasValue).Select(e => e.PatientId!.Value).Distinct().ToList();
        var deptIds = entities.Where(e => e.DeptId.HasValue).Select(e => e.DeptId!.Value).Distinct().ToList();
        var doctorIds = entities.Where(e => e.DoctorId.HasValue).Select(e => e.DoctorId!.Value).Distinct().ToList();

        var patients = await _db.Patients.Where(p => patientIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.PatientName);
        var depts = await _db.DeptDicts.Where(d => deptIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.DeptName);
        var doctors = await _db.Doctors.Where(d => doctorIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.DoctorName);

        return entities.Select(e => new RegistrationListItemDto(
            e.Id,
            e.PatientId,
            e.PatientId.HasValue && patients.TryGetValue(e.PatientId.Value, out var pn) ? pn : null,
            e.DeptId,
            e.DeptId.HasValue && depts.TryGetValue(e.DeptId.Value, out var dn) ? dn : null,
            e.DoctorId,
            e.DoctorId.HasValue && doctors.TryGetValue(e.DoctorId.Value, out var dc) ? dc : null,
            e.RegType, e.RegTime, e.RegStatus, e.RegFee)).ToList();
    }

    public async Task<PageResult<RegistrationListItemDto>> GetPagedAsync(PageRequest req, sbyte? status, DateTime? date)
    {
        var query = _db.ClinicRegs.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(r => r.RegStatus == status.Value);
        }
        if (date.HasValue)
        {
            var day = date.Value.Date;
            var nextDay = day.AddDays(1);
            query = query.Where(r => r.RegTime >= day && r.RegTime < nextDay);
        }
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var patientIds = await _db.Patients
                .Where(p => p.PatientName != null && p.PatientName.Contains(req.Keyword))
                .Select(p => p.Id)
                .ToListAsync();
            query = query.Where(r => r.PatientId.HasValue && patientIds.Contains(r.PatientId.Value));
        }

        var total = await query.LongCountAsync();
        var entities = await query.OrderByDescending(r => r.Id).Skip(req.Skip).Take(req.Size).ToListAsync();
        return new PageResult<RegistrationListItemDto>(total, await ToDtosAsync(entities));
    }

    public async Task<IReadOnlyList<RegistrationListItemDto>> GetQueueAsync(long? doctorId)
    {
        var query = _db.ClinicRegs.Where(r => r.RegStatus == 1);
        if (doctorId.HasValue)
        {
            query = query.Where(r => r.DoctorId == doctorId.Value);
        }

        var entities = await query.OrderBy(r => r.RegTime).ToListAsync();
        return await ToDtosAsync(entities);
    }

    public async Task<RegistrationListItemDto> CreateAsync(RegistrationCreateRequest req)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == req.PatientId))
        {
            throw new ApiException("患者不存在");
        }
        if (!await _db.DeptDicts.AnyAsync(d => d.Id == req.DeptId && d.IsActive == 0))
        {
            throw new ApiException("科室不存在或已停用");
        }

        var doctor = await _db.Doctors.FindAsync(req.DoctorId)
            ?? throw new ApiException("医生不存在");
        if (doctor.IsActive != 0)
        {
            throw new ApiException("该医生已离职，不能挂号");
        }
        if (doctor.DeptId != req.DeptId)
        {
            throw new ApiException("该医生不属于所选科室");
        }

        var entity = new ClinicReg
        {
            PatientId = req.PatientId,
            DeptId = req.DeptId,
            DoctorId = req.DoctorId,
            RegType = req.RegType,
            RegTime = DateTime.Now,
            RegStatus = 1,
            RegFee = RegFees[req.RegType]
        };
        _db.ClinicRegs.Add(entity);
        await _db.SaveChangesAsync();
        return (await ToDtosAsync(new List<ClinicReg> { entity }))[0];
    }

    public async Task CancelAsync(long id)
    {
        var entity = await _db.ClinicRegs.FindAsync(id)
            ?? throw new ApiException("挂号记录不存在", 404);
        if (entity.RegStatus != 1)
        {
            throw new ApiException("仅已挂号状态可以退号");
        }

        entity.RegStatus = 3;
        await _db.SaveChangesAsync();
    }
}
