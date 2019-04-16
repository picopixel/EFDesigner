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

namespace Testing_CoreV2NetStd.Model
{
   /// <inheritdoc/>
   public partial class EFModel1 : Microsoft.EntityFrameworkCore.DbContext
   {
      #region DbSets
      public virtual Microsoft.EntityFrameworkCore.DbSet<global::Testing_CoreV2NetStd.Model.Entity1> Entity1 { get; set; }
      #endregion DbSets

      /// <summary>
      /// Default connection string
      /// </summary>
      public static string ConnectionString { get; set; } = @"Data Source=.;Initial Catalog=ModelTest;Integrated Security=True";

      /// <inheritdoc />
      public EFModel1() : base()
      {
      }

      /// <inheritdoc />
      public EFModel1(DbContextOptions<EFModel1> options) : base(options)
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

         modelBuilder.Entity<global::Testing_CoreV2NetStd.Model.Entity1>()
                     .ToTable("Entity1")
                     .HasKey(t => t.Id);
         modelBuilder.Entity<global::Testing_CoreV2NetStd.Model.Entity1>()
                     .Property(t => t.Id)
                     .IsRequired()
                     .ValueGeneratedOnAdd();

         OnModelCreatedImpl(modelBuilder);
      }
   }
}
