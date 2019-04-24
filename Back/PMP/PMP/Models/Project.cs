﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PMP.Models
{
	public class Project
	{
		public int Id { get; set; }

		[Column("Project Admin")]
		public int UserId { get; set; }

		public string Name { get; set; }

		[Column(TypeName = "ntext")]
		public string Desc { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public string Slug { get; set; }

		public User User { get; set; }

		public List<ProjectMember> ProjectMembers { get; set; }

		public List<Task> Tasks { get; set; }

	}
}