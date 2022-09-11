using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PromocodesManagementSystem.Model;

namespace PromocodesManagementSystem.Data
{
    public partial class PromocodesContext : DbContext
    {
        public PromocodesContext()
        {
        }

        public PromocodesContext(DbContextOptions<PromocodesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TblPromocode> TblPromocodes { get; set; } = null!;
        public virtual DbSet<TblTokenuser> TblTokenusers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<TblPromocode>(entity =>
            {
                entity.ToTable("tbl_promocodes");

                entity.Property(e => e.Id).HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Phone).HasMaxLength(20);

                entity.Property(e => e.PromoCodes).HasMaxLength(11);

                entity.Property(e => e.Qrimage).HasColumnName("QRImage");
            });

            modelBuilder.Entity<TblTokenuser>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tbl_tokenuser");

                entity.Property(e => e.Password).HasMaxLength(30);

                entity.Property(e => e.UserName).HasMaxLength(30);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
