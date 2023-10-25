﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ThreeL.Blob.Infra.Repository.EfCore.Mysql;

#nullable disable

namespace ThreeL.Blob.Migrations.Migrations
{
    [DbContext(typeof(MySqlDbContext))]
    [Migration("20231024063110_AddUploadingFinishTime")]
    partial class AddUploadingFinishTime
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.HasCharSet(modelBuilder, "utf8mb4 ");

            modelBuilder.Entity("ThreeL.Blob.Domain.Aggregate.FileObject.FileObject", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnOrder(1);

                    b.Property<string>("Code")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<long>("CreateBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false)
                        .HasColumnOrder(2);

                    b.Property<bool>("IsFolder")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime");

                    b.Property<string>("Location")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<long?>("ParentFolder")
                        .HasColumnType("bigint");

                    b.Property<long?>("Size")
                        .HasColumnType("bigint");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<string>("TempFileLocation")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<DateTime>("UploadFinishTime")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.ToTable("FileObject");
                });

            modelBuilder.Entity("ThreeL.Blob.Domain.Aggregate.User.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnOrder(1);

                    b.Property<long>("CreateBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime");

                    b.Property<long>("DaliyUploadMaxSizeLimit")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValue(10737418240L);

                    b.Property<long?>("DownloadSpeedLimit")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false)
                        .HasColumnOrder(2);

                    b.Property<DateTime?>("LastLoginTime")
                        .HasColumnType("datetime");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<long?>("MaxSpaceSize")
                        .HasColumnType("bigint");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<long>("TodayUploadMaxSize")
                        .HasColumnType("bigint");

                    b.Property<long>("UploadMaxSizeLimit")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValue(1073741824L);

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)");

                    b.HasKey("Id");

                    b.ToTable("User");
                });
#pragma warning restore 612, 618
        }
    }
}
