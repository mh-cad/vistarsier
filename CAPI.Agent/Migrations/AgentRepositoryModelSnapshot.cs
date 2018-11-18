﻿// <auto-generated />
using System;
using CAPI.Agent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CAPI.Agent.Migrations
{
    [DbContext(typeof(AgentRepository))]
    partial class AgentRepositoryModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CAPI.Agent.Models.Case", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Accession");

                    b.Property<int>("AdditionMethod");

                    b.Property<string>("Comment");

                    b.Property<string>("Status");

                    b.HasKey("Id");

                    b.ToTable("Cases");
                });

            modelBuilder.Entity("CAPI.Agent.Models.Job", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("BiasFieldCorrection");

                    b.Property<string>("BiasFieldCorrectionParams");

                    b.Property<string>("CurrentAccession");

                    b.Property<string>("DefaultDestination");

                    b.Property<DateTime>("End");

                    b.Property<bool>("ExtractBrain");

                    b.Property<string>("ExtractBrainParams");

                    b.Property<string>("PatientBirthDate");

                    b.Property<string>("PatientFullName");

                    b.Property<string>("PatientId");

                    b.Property<string>("PriorAccession");

                    b.Property<string>("ReferenceSeries");

                    b.Property<bool>("Register");

                    b.Property<string>("SourceAet");

                    b.Property<DateTime>("Start");

                    b.Property<string>("Status");

                    b.HasKey("Id");

                    b.ToTable("Jobs");
                });
#pragma warning restore 612, 618
        }
    }
}
