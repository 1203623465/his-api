using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace His.Api.Models;

public partial class HisDbContext : DbContext
{
    public HisDbContext(DbContextOptions<HisDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ClinicReg> ClinicRegs { get; set; }

    public virtual DbSet<ClinicVisit> ClinicVisits { get; set; }

    public virtual DbSet<DeptDict> DeptDicts { get; set; }

    public virtual DbSet<DeptTypeDict> DeptTypeDicts { get; set; }

    public virtual DbSet<DispenseRecord> DispenseRecords { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Drug> Drugs { get; set; }

    public virtual DbSet<DrugStock> DrugStocks { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PayRecord> PayRecords { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<RefundRecord> RefundRecords { get; set; }

    public virtual DbSet<ViewOnedaypay> ViewOnedaypays { get; set; }

    public virtual DbSet<ViewReg> ViewRegs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClinicReg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("clinic_reg");

            entity.Property(e => e.Id)
                .HasComment("自增主键")
                .HasColumnName("id");
            entity.Property(e => e.DeptId)
                .HasComment("科室ID")
                .HasColumnName("dept_id");
            entity.Property(e => e.DoctorId)
                .HasComment("医生ID")
                .HasColumnName("doctor_id");
            entity.Property(e => e.PatientId)
                .HasComment("患者ID")
                .HasColumnName("patient_id");
            entity.Property(e => e.RegFee)
                .HasPrecision(8)
                .HasComment("挂号费")
                .HasColumnName("reg_fee");
            entity.Property(e => e.RegStatus)
                .HasComment("1已挂号 2已就诊 3已退号")
                .HasColumnName("reg_status");
            entity.Property(e => e.RegTime)
                .HasComment("挂号时间")
                .HasColumnType("datetime")
                .HasColumnName("reg_time");
            entity.Property(e => e.RegType)
                .HasComment("1普通 2专家")
                .HasColumnName("reg_type");
        });

        modelBuilder.Entity<ClinicVisit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("clinic_visit", tb => tb.HasComment("就诊记录表"));

            entity.Property(e => e.Id)
                .HasComment("就诊id")
                .HasColumnName("id");
            entity.Property(e => e.DoctorId)
                .HasComment("医生id")
                .HasColumnName("doctor_id");
            entity.Property(e => e.PatientId)
                .HasComment("患者id")
                .HasColumnName("patient_id");
            entity.Property(e => e.RegId)
                .HasComment("挂号id")
                .HasColumnName("reg_id");
            entity.Property(e => e.VisitComplaint)
                .HasMaxLength(300)
                .HasComment("主诉")
                .HasColumnName("visit_complaint");
            entity.Property(e => e.VisitDiagnosis)
                .HasMaxLength(300)
                .HasComment("诊断结论")
                .HasColumnName("visit_diagnosis");
            entity.Property(e => e.VisitTime)
                .HasComment("就诊时间")
                .HasColumnType("datetime")
                .HasColumnName("visit_time");
        });

        modelBuilder.Entity<DeptDict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dept_dict", tb => tb.HasComment("科室字典表"));

            entity.Property(e => e.Id)
                .HasComment("自增主键")
                .HasColumnName("id");
            entity.Property(e => e.DeptName)
                .HasMaxLength(50)
                .HasComment("科室名称")
                .HasColumnName("dept_name");
            entity.Property(e => e.DeptNote)
                .HasMaxLength(255)
                .HasColumnName("dept_note");
            entity.Property(e => e.DeptType)
                .HasComment("科室类型 详情见det_type字典表 ")
                .HasColumnName("dept_type");
            entity.Property(e => e.IsActive)
                .HasComment("是否启用 0启用 1 停用")
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<DeptTypeDict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dept_type_dict", tb => tb.HasComment("科室类型字典表"));

            entity.Property(e => e.Id)
                .HasComment("自增主键")
                .HasColumnName("id");
            entity.Property(e => e.DetTypeName)
                .HasMaxLength(50)
                .HasComment("科室类型名称  临床 医技 行政之类的")
                .HasColumnName("det_type_name");
        });

        modelBuilder.Entity<DispenseRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dispense_record", tb => tb.HasComment("发药记录"));

            entity.Property(e => e.Id)
                .HasComment("主键id")
                .HasColumnName("id");
            entity.Property(e => e.DispenseTime)
                .HasComment("发药时间")
                .HasColumnType("datetime")
                .HasColumnName("dispense_time");
            entity.Property(e => e.Operator)
                .HasMaxLength(50)
                .HasComment("药师")
                .HasColumnName("operator");
            entity.Property(e => e.PatientId)
                .HasComment("患者id")
                .HasColumnName("patient_id");
            entity.Property(e => e.PresId)
                .HasComment("处方id")
                .HasColumnName("pres_id");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("doctor", tb => tb.HasComment("医生表"));

            entity.Property(e => e.Id)
                .HasComment("自增主键")
                .HasColumnName("id");
            entity.Property(e => e.DeptId)
                .HasComment("所属科室")
                .HasColumnName("dept_id");
            entity.Property(e => e.DoctorName)
                .HasMaxLength(50)
                .HasComment("医生名称")
                .HasColumnName("doctor_name");
            entity.Property(e => e.DoctorTitle)
                .HasMaxLength(50)
                .HasComment("医生职称")
                .HasColumnName("doctor_title");
            entity.Property(e => e.IsActive)
                .HasComment("是否在职 0在职 1离职")
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<Drug>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("drug", tb => tb.HasComment("药品信息表"));

            entity.Property(e => e.Id)
                .HasComment("药品id")
                .HasColumnName("id");
            entity.Property(e => e.DrugName)
                .HasMaxLength(100)
                .HasComment("药品名称")
                .HasColumnName("drug_name");
            entity.Property(e => e.DrugPrice)
                .HasPrecision(10)
                .HasComment("单价")
                .HasColumnName("drug_price");
            entity.Property(e => e.DrugSpec)
                .HasMaxLength(50)
                .HasComment("药品规格(如 0.25g×24片)")
                .HasColumnName("drug_spec");
            entity.Property(e => e.DrugUnit)
                .HasMaxLength(10)
                .HasComment("单位（盒/瓶/支）")
                .HasColumnName("drug_unit");
            entity.Property(e => e.IsActive)
                .HasComment("是否启用 0是未启用 1已启用")
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<DrugStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("drug_stock", tb => tb.HasComment("药品库存"));

            entity.Property(e => e.Id)
                .HasComment("主键id")
                .HasColumnName("id");
            entity.Property(e => e.BatchNo)
                .HasMaxLength(30)
                .HasComment("批号")
                .HasColumnName("batch_no");
            entity.Property(e => e.DrugId)
                .HasComment("药品id")
                .HasColumnName("drug_id");
            entity.Property(e => e.ExpireDate)
                .HasComment("效期")
                .HasColumnType("datetime")
                .HasColumnName("expire_date");
            entity.Property(e => e.Quantity)
                .HasPrecision(10)
                .HasComment("库存数量")
                .HasColumnName("quantity");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("patient", tb => tb.HasComment("患者表"));

            entity.Property(e => e.Id)
                .HasComment("自增主键")
                .HasColumnName("id");
            entity.Property(e => e.CreateTime)
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.PatientAddress)
                .HasMaxLength(50)
                .HasComment("患者地址")
                .HasColumnName("patient_address");
            entity.Property(e => e.PatientBirthday)
                .HasComment("出生日期")
                .HasColumnType("datetime")
                .HasColumnName("patient_birthday");
            entity.Property(e => e.PatientIdno)
                .HasMaxLength(18)
                .IsFixedLength()
                .HasComment("身份证号")
                .HasColumnName("patient_idno");
            entity.Property(e => e.PatientName)
                .HasMaxLength(50)
                .HasComment("患者名称")
                .HasColumnName("patient_name");
            entity.Property(e => e.PatientPhone)
                .HasMaxLength(20)
                .HasComment("手机号")
                .HasColumnName("patient_phone");
            entity.Property(e => e.PatientSex)
                .HasComment("患者性别 1男 2女 0未设置")
                .HasColumnName("patient_sex");
        });

        modelBuilder.Entity<PayRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("pay_record", tb => tb.HasComment("支付结算表"));

            entity.Property(e => e.Id)
                .HasComment("收费结算id")
                .HasColumnName("id");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(30)
                .HasComment("发票号")
                .HasColumnName("invoice_no");
            entity.Property(e => e.PatientId)
                .HasComment("患者id")
                .HasColumnName("patient_id");
            entity.Property(e => e.PayTime)
                .HasComment("结算时间 ")
                .HasColumnType("datetime")
                .HasColumnName("pay_time");
            entity.Property(e => e.PayType)
                .HasComment("1现金 2医保 3扫码")
                .HasColumnName("pay_type");
            entity.Property(e => e.RegId)
                .HasComment("挂号id")
                .HasColumnName("reg_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10)
                .HasComment("合计金额")
                .HasColumnName("total_amount");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("prescription", tb => tb.HasComment("处方头"));

            entity.Property(e => e.Id)
                .HasComment("处方头id")
                .HasColumnName("id");
            entity.Property(e => e.DoctorId)
                .HasComment("医生id")
                .HasColumnName("doctor_id");
            entity.Property(e => e.PatientId)
                .HasComment("患者id")
                .HasColumnName("patient_id");
            entity.Property(e => e.PresStatus)
                .HasComment("开方状态 1未收费 2已收费 3已发药 4已退款 ")
                .HasColumnName("pres_status");
            entity.Property(e => e.PresTime)
                .HasComment("开方时间")
                .HasColumnType("datetime")
                .HasColumnName("pres_time");
            entity.Property(e => e.VisitId)
                .HasComment("就诊id")
                .HasColumnName("visit_id");
        });

        modelBuilder.Entity<PrescriptionDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("prescription_detail", tb => tb.HasComment("处方头"));

            entity.Property(e => e.Id)
                .HasComment("处方明细id")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10)
                .HasComment("小计= quantity x  unit_price")
                .HasColumnName("amount");
            entity.Property(e => e.DrugId)
                .HasComment("药品id")
                .HasColumnName("drug_id");
            entity.Property(e => e.PresId)
                .HasComment("处方id")
                .HasColumnName("pres_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(10)
                .HasComment("数量 数量存在小数比如 0.5片")
                .HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10)
                .HasComment("单价")
                .HasColumnName("unit_price");
        });

        modelBuilder.Entity<RefundRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refund_record", tb => tb.HasComment("药品库存"));

            entity.Property(e => e.Id)
                .HasComment("主键id")
                .HasColumnName("id");
            entity.Property(e => e.OriginalPayId)
                .HasComment("原结算单号")
                .HasColumnName("original_pay_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .HasComment("退款原因")
                .HasColumnName("reason");
            entity.Property(e => e.RefundAmount)
                .HasPrecision(10)
                .HasComment("退款金额")
                .HasColumnName("refund_amount");
            entity.Property(e => e.RefundTime)
                .HasComment("退款时间")
                .HasColumnType("datetime")
                .HasColumnName("refund_time");
        });

        modelBuilder.Entity<ViewOnedaypay>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("view_onedaypay");

            entity.Property(e => e.患者id).HasComment("患者id");
            entity.Property(e => e.科室名称)
                .HasMaxLength(50)
                .HasComment("科室名称");
            entity.Property(e => e.金额).HasPrecision(32);
        });

        modelBuilder.Entity<ViewReg>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("view_reg");

            entity.Property(e => e.DeptId)
                .HasComment("科室ID")
                .HasColumnName("dept_id");
            entity.Property(e => e.DoctorId)
                .HasComment("医生ID")
                .HasColumnName("doctor_id");
            entity.Property(e => e.Id)
                .HasComment("自增主键")
                .HasColumnName("id");
            entity.Property(e => e.PatientId)
                .HasComment("患者ID")
                .HasColumnName("patient_id");
            entity.Property(e => e.RegFee)
                .HasPrecision(8)
                .HasComment("挂号费")
                .HasColumnName("reg_fee");
            entity.Property(e => e.RegStatus)
                .HasComment("1已挂号 2已就诊 3已退号")
                .HasColumnName("reg_status");
            entity.Property(e => e.RegTime)
                .HasComment("挂号时间")
                .HasColumnType("datetime")
                .HasColumnName("reg_time");
            entity.Property(e => e.RegType)
                .HasComment("1普通 2专家")
                .HasColumnName("reg_type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
