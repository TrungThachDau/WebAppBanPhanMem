using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using QLBanPhanMem.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<PhanMemModel> PhanMems { get; set; }
    public DbSet<NhaPhatHanhModel> NhaPhatHanhs { get; set; }
    public DbSet<LoaiPM> LoaiPMs { get; set; }
    public DbSet<ThuocLoaiPM> ThuocLoaiPMs { get; set; }
    public DbSet<AccountModel> Accounts { get; set; }

    public DbSet<CTHDKeyModel> CTHDKeys { get; set; }
    public DbSet<HoaDonModel> HoaDons { get; set; }
    public DbSet<KEYPMModel> KEYPMs { get; set; }
    public DbSet<ChiTietHoaDonModel> CTHDs { get; set; }
    public DbSet<ThongTinBoSungModel> TTBSs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhanMemModel>()

            .HasOne(p => p.NhaPhatHanh)
            .WithMany()
            .HasForeignKey(p => p.MANPH);

        modelBuilder.Entity<ThuocLoaiPM>()

            .HasKey(tlp => new { tlp.MAPM, tlp.MALOAI });

        modelBuilder.Entity<ThuocLoaiPM>()
            .HasOne(tlp => tlp.PhanMem)
            .WithMany()
            .HasForeignKey(tlp => tlp.MAPM);

        modelBuilder.Entity<ThuocLoaiPM>()
            .HasOne(tlp => tlp.LoaiPM)
            .WithMany()
            .HasForeignKey(tlp => tlp.MALOAI);
        modelBuilder.Entity<CTHDKeyModel>()
            .HasKey(ck => new { ck.MAHD, ck.MAPM, ck.MAKEY });
        modelBuilder.Entity<CTHDKeyModel>()
            .HasOne(ck => ck.HoaDon)
            .WithMany()
            .HasForeignKey(ck => ck.MAHD);
        modelBuilder.Entity<CTHDKeyModel>()
            .HasOne(ck => ck.PhanMem)
            .WithMany()
            .HasForeignKey(ck => ck.MAPM);
        modelBuilder.Entity<CTHDKeyModel>()
            .HasOne(ck => ck.KEYPM)
            .WithMany()
            .HasForeignKey(ck => ck.MAKEY);
        modelBuilder.Entity<HoaDonModel>()
            .HasOne(hd => hd.Account)
            .WithMany()
            .HasForeignKey(hd => hd.MATK);
        modelBuilder.Entity<KEYPMModel>()
            .HasOne(kp => kp.PhanMem)
            .WithMany()
            .HasForeignKey(kp => kp.MAPM);
        modelBuilder.Entity<ChiTietHoaDonModel>()
            .HasKey(cthd => new { cthd.MAHD, cthd.MAPM });
        modelBuilder.Entity<ChiTietHoaDonModel>()
            .HasOne(cthd => cthd.HoaDon)
            .WithMany()
            .HasForeignKey(cthd => cthd.MAHD);
        modelBuilder.Entity<ChiTietHoaDonModel>()
            .HasOne(cthd => cthd.PhanMem)
            .WithMany()
            .HasForeignKey(cthd => cthd.MAPM);
        modelBuilder.Entity<ThongTinBoSungModel>()
            .HasKey(ttbs => new { ttbs.MAHD, ttbs.MAPM,ttbs.STT });
        modelBuilder.Entity<ThongTinBoSungModel>()
            .HasOne(ttbs => ttbs.HoaDon)
            .WithMany()
            .HasForeignKey(ttbs => ttbs.MAHD);
        modelBuilder.Entity<ThongTinBoSungModel>()
            .HasOne(ttbs => ttbs.PhanMem)
            .WithMany()
            .HasForeignKey(ttbs => ttbs.MAPM);
        
    }
}
