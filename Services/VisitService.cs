using His.Api.Dtos;
using His.Api.Middleware;
using His.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Services;

/// <summary>
/// 就诊业务服务。
/// <para>核心业务规则：</para>
/// <list type="bullet">
/// <item>开始就诊：仅"已挂号(1)"的挂号单可接诊；一个挂号单只允许生成一条就诊记录；
/// 接诊时将挂号状态回写为"已就诊(2)"</item>
/// <item>病历保存：可多次修改主诉与诊断结论</item>
/// </list>
/// 就诊记录是连接挂号与处方的枢纽：处方经由就诊记录继承患者/医生信息。
/// </summary>
public interface IVisitService
{
    Task<PageResult<VisitListItemDto>> GetPagedAsync(PageRequest req);
    Task<VisitListItemDto> GetByIdAsync(long id);
    Task<VisitListItemDto> StartAsync(VisitStartRequest req);
    Task<VisitListItemDto> SaveRecordAsync(long id, VisitSaveRequest req);
}

public class VisitService : IVisitService
{
    private readonly HisDbContext _db;

    public VisitService(HisDbContext db)
    {
        _db = db;
    }

    private async Task<List<VisitListItemDto>> ToDtosAsync(List<ClinicVisit> entities)
    {
        var patientIds = entities.Where(e => e.PatientId.HasValue).Select(e => e.PatientId!.Value).Distinct().ToList();
        var doctorIds = entities.Where(e => e.DoctorId.HasValue).Select(e => e.DoctorId!.Value).Distinct().ToList();

        var patients = await _db.Patients.Where(p => patientIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.PatientName);
        var doctors = await _db.Doctors.Where(d => doctorIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.DoctorName);

        return entities.Select(e => new VisitListItemDto(
            e.Id,
            e.PatientId,
            e.PatientId.HasValue && patients.TryGetValue(e.PatientId.Value, out var pn) ? pn : null,
            e.RegId,
            e.DoctorId,
            e.DoctorId.HasValue && doctors.TryGetValue(e.DoctorId.Value, out var dn) ? dn : null,
            e.VisitTime, e.VisitComplaint, e.VisitDiagnosis)).ToList();
    }

    public async Task<PageResult<VisitListItemDto>> GetPagedAsync(PageRequest req)
    {
        var query = _db.ClinicVisits.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
        {
            var patientIds = await _db.Patients
                .Where(p => p.PatientName != null && p.PatientName.Contains(req.Keyword))
                .Select(p => p.Id)
                .ToListAsync();
            query = query.Where(v => v.PatientId.HasValue && patientIds.Contains(v.PatientId.Value));
        }

        var total = await query.LongCountAsync();
        var entities = await query.OrderByDescending(v => v.Id).Skip(req.Skip).Take(req.Size).ToListAsync();
        return new PageResult<VisitListItemDto>(total, await ToDtosAsync(entities));
    }

    public async Task<VisitListItemDto> GetByIdAsync(long id)
    {
        var entity = await _db.ClinicVisits.FindAsync(id)
            ?? throw new ApiException("就诊记录不存在", 404);
        return (await ToDtosAsync(new List<ClinicVisit> { entity }))[0];
    }

    public async Task<VisitListItemDto> StartAsync(VisitStartRequest req)
    {
        var reg = await _db.ClinicRegs.FindAsync(req.RegId)
            ?? throw new ApiException("挂号记录不存在", 404);
        if (reg.RegStatus != 1)
        {
            throw new ApiException("仅已挂号状态可以开始就诊");
        }
        if (await _db.ClinicVisits.AnyAsync(v => v.RegId == req.RegId))
        {
            throw new ApiException("该挂号已存在就诊记录");
        }

        var entity = new ClinicVisit
        {
            PatientId = reg.PatientId,
            RegId = reg.Id,
            DoctorId = reg.DoctorId,
            VisitTime = DateTime.Now
        };
        _db.ClinicVisits.Add(entity);
        reg.RegStatus = 2;
        await _db.SaveChangesAsync();
        return (await ToDtosAsync(new List<ClinicVisit> { entity }))[0];
    }

    public async Task<VisitListItemDto> SaveRecordAsync(long id, VisitSaveRequest req)
    {
        var entity = await _db.ClinicVisits.FindAsync(id)
            ?? throw new ApiException("就诊记录不存在", 404);

        entity.VisitComplaint = req.VisitComplaint;
        entity.VisitDiagnosis = req.VisitDiagnosis;
        await _db.SaveChangesAsync();
        return (await ToDtosAsync(new List<ClinicVisit> { entity }))[0];
    }
}
