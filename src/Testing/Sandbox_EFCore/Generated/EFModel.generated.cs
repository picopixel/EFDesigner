//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor
//     https://github.com/msawczyn/EFDesigner
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sandbox_EFCore
{
   /// <inheritdoc/>
   public partial class EFModel : Microsoft.EntityFrameworkCore.DbContext
   {
      #region DbSets
      public virtual Microsoft.EntityFrameworkCore.DbSet<global::Sandbox_EFCore.Sandbox_EFCore> Sandbox_EFCore { get; set; }
      public virtual Microsoft.EntityFrameworkCore.DbSet<global::Sandbox_EFCore.User> Users { get; set; }
      #endregion DbSets

      /// <summary>
      /// Default connection string
      /// </summary>
      public static string ConnectionString { get; set; } = @"Data Source=.;Initial Catalog=Sandbox;Integrated Security=True";

      /// <inheritdoc />
      public EFModel() : base()
      {
      }

      /// <inheritdoc />
      public EFModel(DbContextOptions<EFModel> options) : base(options)
      {
      }

      partial void CustomInit(DbContextOptionsBuilder optionsBuilder);

      /// <inheritdoc />
      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      {
         CustomInit(optionsBuilder);
      }

      partial void OnModelCreatingImpl(ModelBuilder modelBuilder);
      partial void OnModelCreatedImpl(ModelBuilder modelBuilder);

      /// <inheritdoc />
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);
         OnModelCreatingImpl(modelBuilder);

         modelBuilder.HasDefaultSchema("dbo");

         modelBuilder.Entity<global::Sandbox_EFCore.Sandbox_EFCore>()
                     .ToTable("Sandbox_EFCore")
                     .HasKey(t => t.Id);
         modelBuilder.Entity<global::Sandbox_EFCore.Sandbox_EFCore>()
                     .Property(t => t.Id)
                     .IsRequired()
                     .ValueGeneratedOnAdd();

         modelBuilder.Entity<global::Sandbox_EFCore.User>()
                     .ToTable("Users")
                     .HasKey(t => t.Id);
         modelBuilder.Entity<global::Sandbox_EFCore.User>().HasIndex(t => t.Property1);
         modelBuilder.Entity<global::Sandbox_EFCore.User>()
                     .Property(t => t.Id)
                     .IsRequired()
                     .ValueGeneratedOnAdd();
         modelBuilder.Entity<global::Sandbox_EFCore.User>()
                     .HasOne(x => x.Role)
                     .WithOne()
                     .HasForeignKey<global::Sandbox_EFCore.User>("Sandbox_EFCore_Role_Id")
                     .IsRequired();

         OnModelCreatedImpl(modelBuilder);
      }
   }
}
