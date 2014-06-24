﻿namespace QuizWiz.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class ExamContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        public ExamContext()
            : base("DefaultConnection")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSet<Exam> Exams { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSet<Section> Sections { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSet<Question> Questions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSet<Answer> Answers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSet<Submission> Submissions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSet<Response> Responses { get; set; }

        /// <summary>
        /// Overriding to allow mapping private members with entity framework (code first).
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder
              .Types()
              .Configure(c =>
              {
                  var nonPublicProperties = c.ClrType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);

                  foreach (var p in nonPublicProperties)
                  {
                      c.Property(p).HasColumnName(p.Name.First() + string.Join("", p.Name.Skip(1)));
                  }
              });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Table("Exam")]
    public class Exam
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long ExamId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [UIHint("MultilineText")]
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowRetries { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Section> Sections { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Table("Section")]
    public class Section
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long SectionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Question> Questions { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Table("Question")]
    public class Question
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long QuestionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Answer> Answers { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Table("Answer")]
    public class Answer
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long AnswerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenEnded { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Table("Submission")]
    public class Submission
    {
        /// <summary>
        /// 
        /// </summary>
        private string heartbeat;

        /// <summary>
        /// 
        /// </summary>
        private int elapsed;

        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long SubmissionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Exam Exam { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public DateTime Heartbeat
        {
            get
            {
                return DateTime.Parse(this.heartbeat);
            }

            set
            {
                this.heartbeat = value.ToShortTimeString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public TimeSpan Elapsed
        {
            get
            {
                return TimeSpan.FromSeconds(this.elapsed);
            }

            set
            {
                this.elapsed = value.Seconds;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Response> Responses { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Table("Response")]
    public class Response
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long ResponseId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Answer Answer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ExamSectionModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Submission Submission { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Section Section { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private int elapsed;

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public TimeSpan Elapsed { 
            get 
            {
                return TimeSpan.FromSeconds(this.elapsed);
            }
            set 
            {
                this.elapsed = value.Seconds;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ExamQuestionModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Response Response { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ExamIndexModel
    {
        /// <summary>
        /// 
        /// </summary>
        public IList<Exam> Exams { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
    }
}