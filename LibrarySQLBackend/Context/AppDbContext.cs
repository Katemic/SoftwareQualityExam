using System;
using System.Collections.Generic;
using LibrarySQLBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Boardgame> Boardgames { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Creator> Creators { get; set; }

    public virtual DbSet<Fine> Fines { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<Loaner> Loaners { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Boardgame>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Item).WithOne(p => p.Boardgame).HasConstraintName("fk_boardgame_item");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Item).WithOne(p => p.Book).HasConstraintName("fk_book_item");
        });

        modelBuilder.Entity<Creator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Status).HasDefaultValueSql("'unpaid'");

            entity.HasOne(d => d.Loan).WithMany(p => p.Fines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_fine_loan");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Status).HasDefaultValueSql("'available'");

            entity.HasOne(d => d.Item).WithMany(p => p.Inventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_inventory_item");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Language).WithMany(p => p.Items)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_item_language");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Items)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_item_publisher");

            entity.HasMany(d => d.Creators).WithMany(p => p.Items)
                .UsingEntity<Dictionary<string, object>>(
                    "ItemCreator",
                    r => r.HasOne<Creator>().WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_itemcreator_creator"),
                    l => l.HasOne<Item>().WithMany()
                        .HasForeignKey("ItemId")
                        .HasConstraintName("fk_itemcreator_item"),
                    j =>
                    {
                        j.HasKey("ItemId", "CreatorId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("item_creator");
                        j.HasIndex(new[] { "CreatorId" }, "creator_id_idx");
                        j.IndexerProperty<int>("ItemId").HasColumnName("item_id");
                        j.IndexerProperty<int>("CreatorId").HasColumnName("creator_id");
                    });

            entity.HasMany(d => d.Genres).WithMany(p => p.Items)
                .UsingEntity<Dictionary<string, object>>(
                    "ItemGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_itemgenre_genre"),
                    l => l.HasOne<Item>().WithMany()
                        .HasForeignKey("ItemId")
                        .HasConstraintName("fk_itemgenre_item"),
                    j =>
                    {
                        j.HasKey("ItemId", "GenreId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("item_genre");
                        j.HasIndex(new[] { "GenreId" }, "genre_id_idx");
                        j.IndexerProperty<int>("ItemId").HasColumnName("item_id");
                        j.IndexerProperty<int>("GenreId").HasColumnName("genre_id");
                    });

            entity.HasMany(d => d.Tags).WithMany(p => p.Items)
                .UsingEntity<Dictionary<string, object>>(
                    "ItemTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_itemtag_tag"),
                    l => l.HasOne<Item>().WithMany()
                        .HasForeignKey("ItemId")
                        .HasConstraintName("fk_itemtag_item"),
                    j =>
                    {
                        j.HasKey("ItemId", "TagId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("item_tag");
                        j.HasIndex(new[] { "TagId" }, "tag_id_idx");
                        j.IndexerProperty<int>("ItemId").HasColumnName("item_id");
                        j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Inventory).WithMany(p => p.Loans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_loan_inventory");

            entity.HasOne(d => d.Loaner).WithMany(p => p.Loans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_loan_loaner");
        });

        modelBuilder.Entity<Loaner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Cpr).IsFixedLength();
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Item).WithMany(p => p.Reservations).HasConstraintName("fk_reservation_item");

            entity.HasOne(d => d.Loaner).WithMany(p => p.Reservations).HasConstraintName("fk_reservation_loaner");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => new { e.LoanerId, e.ItemId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.HasOne(d => d.Item).WithMany(p => p.Reviews).HasConstraintName("fk_review_item");

            entity.HasOne(d => d.Loaner).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_review_loaner");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
