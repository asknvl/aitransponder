﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using botplatform.Models.pmprocessor.db_storage;

#nullable disable

namespace botplatform.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.19");

            modelBuilder.Entity("botplatform.Models.pmprocessor.db_storage.User", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ai_off_code")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ai_off_time")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ai_on")
                        .HasColumnType("INTEGER");

                    b.Property<string>("bcId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("created_date")
                        .HasColumnType("TEXT");

                    b.Property<string>("fn")
                        .HasColumnType("TEXT");

                    b.Property<string>("geotag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ln")
                        .HasColumnType("TEXT");

                    b.Property<long>("tg_id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("un")
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.HasIndex("geotag", "tg_id")
                        .IsUnique()
                        .HasDatabaseName("IX_User_geotag_tg_id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
