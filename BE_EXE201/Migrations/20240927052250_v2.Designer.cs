﻿// <auto-generated />
using System;
using BE_EXE201.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BE_EXE201.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240927052250_v2")]
    partial class v2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("BE_EXE201.Entities.Home", b =>
                {
                    b.Property<int>("HomeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("HomeId"), 1L, 1);

                    b.Property<string>("ApproveStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Bathroom")
                        .HasColumnType("int");

                    b.Property<int?>("Bedrooms")
                        .HasColumnType("int");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("CreateDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("HouseStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ImageId")
                        .HasColumnType("int");

                    b.Property<int?>("LocationId")
                        .HasColumnType("int");

                    b.Property<string>("ModifyBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("ModifyDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<float?>("Price")
                        .HasColumnType("real");

                    b.Property<float?>("Size")
                        .HasColumnType("real");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UtilitiesId")
                        .HasColumnType("int");

                    b.HasKey("HomeId");

                    b.HasIndex("ImageId");

                    b.HasIndex("LocationId");

                    b.HasIndex("UtilitiesId");

                    b.ToTable("Home");
                });

            modelBuilder.Entity("BE_EXE201.Entities.Image", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ImageId"), 1L, 1);

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("ImageId");

                    b.ToTable("Image");
                });

            modelBuilder.Entity("BE_EXE201.Entities.Location", b =>
                {
                    b.Property<int>("LocationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LocationId"), 1L, 1);

                    b.Property<string>("District")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("HouseNumber")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Province")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Street")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Town")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("LocationId");

                    b.ToTable("Location");
                });

            modelBuilder.Entity("BE_EXE201.Entities.Report", b =>
                {
                    b.Property<int>("ReportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReportId"), 1L, 1);

                    b.Property<int?>("HomeId")
                        .HasColumnType("int");

                    b.Property<bool?>("IsResolved")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("ReportDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ReportReason")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("ReportedBy")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ReportId");

                    b.HasIndex("HomeId");

                    b.ToTable("Report");
                });

            modelBuilder.Entity("BE_EXE201.Entities.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"), 1L, 1);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AvatarUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OTPCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoleID")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserRoleRoleId")
                        .HasColumnType("int");

                    b.Property<float?>("Wallet")
                        .HasColumnType("real");

                    b.HasKey("UserId");

                    b.HasIndex("UserRoleRoleId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("BE_EXE201.Entities.UserRole", b =>
                {
                    b.Property<int>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RoleId"), 1L, 1);

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("RoleId");

                    b.ToTable("UserRole");
                });

            modelBuilder.Entity("BE_EXE201.Entities.Utilities", b =>
                {
                    b.Property<int>("UtilitiesId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UtilitiesId"), 1L, 1);

                    b.Property<bool?>("AirConditioner")
                        .HasColumnType("bit");

                    b.Property<bool?>("Balcony")
                        .HasColumnType("bit");

                    b.Property<bool?>("Elevator")
                        .HasColumnType("bit");

                    b.Property<bool?>("Gym")
                        .HasColumnType("bit");

                    b.Property<bool?>("Parking")
                        .HasColumnType("bit");

                    b.Property<bool?>("Refrigerator")
                        .HasColumnType("bit");

                    b.Property<bool?>("SwimmingPool")
                        .HasColumnType("bit");

                    b.Property<bool?>("TV")
                        .HasColumnType("bit");

                    b.HasKey("UtilitiesId");

                    b.ToTable("Utilities");
                });

            modelBuilder.Entity("BE_EXE201.Entities.Home", b =>
                {
                    b.HasOne("BE_EXE201.Entities.Image", "Image")
                        .WithMany()
                        .HasForeignKey("ImageId");

                    b.HasOne("BE_EXE201.Entities.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId");

                    b.HasOne("BE_EXE201.Entities.Utilities", "Utilities")
                        .WithMany()
                        .HasForeignKey("UtilitiesId");

                    b.Navigation("Image");

                    b.Navigation("Location");

                    b.Navigation("Utilities");
                });

            modelBuilder.Entity("BE_EXE201.Entities.Report", b =>
                {
                    b.HasOne("BE_EXE201.Entities.Home", "Home")
                        .WithMany()
                        .HasForeignKey("HomeId");

                    b.Navigation("Home");
                });

            modelBuilder.Entity("BE_EXE201.Entities.User", b =>
                {
                    b.HasOne("BE_EXE201.Entities.UserRole", "UserRole")
                        .WithMany()
                        .HasForeignKey("UserRoleRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserRole");
                });
#pragma warning restore 612, 618
        }
    }
}
