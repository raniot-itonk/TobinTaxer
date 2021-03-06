﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TobinTaxer.DB;

namespace TobinTaxer.Migrations
{
    [DbContext(typeof(TobinTaxerContext))]
    [Migration("20190226220941_TobinTaxer")]
    partial class TobinTaxer
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TobinTaxer.DB.TaxHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Amount");

                    b.Property<Guid>("Buyer");

                    b.Property<double>("Price");

                    b.Property<Guid>("Seller");

                    b.Property<string>("StockName");

                    b.Property<double>("Tax");

                    b.Property<double>("TaxRate");

                    b.HasKey("Id");

                    b.ToTable("TaxHistories");
                });
#pragma warning restore 612, 618
        }
    }
}
