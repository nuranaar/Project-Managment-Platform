﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PMP.ViewModels;
using PMP.Models;
using System.Data.Entity;
using PMP.Filter;

namespace PMP.Controllers
{
	[Auth]
    public class TeamController : BaseController
    {
        // GET: Team
        public ActionResult Index(string Slug, int AdminId)
        {
			if(db.Teams.FirstOrDefault(t => t.Slug == Slug && t.UserId == AdminId) == null ||Slug==null|| AdminId==0)
			{
				return HttpNotFound();
			}
			int userId = Convert.ToInt32(Session["UserId"]);
			TeamVm model = new TeamVm()
			{
				Admin = db.Users.FirstOrDefault(u=>u.Id==userId),
				Users = db.Users.ToList(),
				Team=db.Teams.FirstOrDefault(t=>t.Slug==Slug && t.UserId==AdminId),
				Tasks = db.Tasks.ToList(),
				TaskStages = db.TaskStages.ToList(),
				TaskMembers = db.TaskMembers.ToList(),
				Projects= db.Projects.OrderByDescending(p => p.StartTime).ToList(),
				ProjectMembers = db.ProjectMembers.ToList()
			};
			model.TeamMembers=db.TeamMembers.Where(m => m.TeamId == model.Team.Id).OrderByDescending(m=>m.Id).ToList();

			return View(model);
        }

		[HttpPost]
		public JsonResult TeamCreate(Team team, TeamMember teamMember, string member)
		{
			if (!ModelState.IsValid)
			{
				Response.StatusCode = 400;

				var errorList = ModelState.Values.SelectMany(m => m.Errors)
								 .Select(e => e.ErrorMessage)
								 .ToList();

				return Json(errorList, JsonRequestBehavior.AllowGet);
			}

			team.UserId = Convert.ToInt32(Session["UserId"]);

			db.Teams.Add(team);
			db.SaveChanges();

			TeamMember teamMem = new TeamMember()
			{
				UserId = team.UserId,
				TeamId = team.Id
			};
			db.TeamMembers.Add(teamMem);
			db.SaveChanges();

			string[] emails = member.Split(' ');
			foreach (var email in emails)
			{
				string e = email.Split(',', '\t', ';')[0];
				teamMember.TeamId = team.Id;
				var usr = db.Users.FirstOrDefault(u => u.Email == e);
				if (usr != null)
				{
					teamMember.UserId = usr.Id;
				}
				db.TeamMembers.Add(teamMember);
				db.SaveChanges();
			}

			Chat chat = new Chat()
			{
				TeamId = team.Id
			};
			db.Chats.Add(chat);
			Activity act = new Activity()
			{
				UserId = team.UserId,
				Desc = "create team" + team.Name,
				Date = DateTime.Now
			};
			db.Activities.Add(act);
			db.SaveChanges();
			return Json(new
			{
				team.Id,
				team.Name,
				team.Slug,
				team.UserId
			}, JsonRequestBehavior.AllowGet);

		}

		[HttpPost]
		public JsonResult AddMember(TeamMember teamMember, string member, int TeamId)
		{
			if (!ModelState.IsValid)
			{
				Response.StatusCode = 400;

				var errorList = ModelState.Values.SelectMany(m => m.Errors)
								 .Select(e => e.ErrorMessage)
								 .ToList();

				return Json(errorList, JsonRequestBehavior.AllowGet);
			}
			teamMember.TeamId = TeamId;

			string[] emails = member.Split(' ');
			foreach (var email in emails)
			{
				string e = email.Split(',', '\t', ';')[0];
				var usr = db.Users.FirstOrDefault(u => u.Email == e);
				if (usr != null)
				{
					teamMember.UserId = usr.Id;
				}
				db.TeamMembers.Add(teamMember);
				db.SaveChanges();
			}
			teamMember.Team = db.Teams.Find(teamMember.TeamId);
			teamMember.User = db.Users.Find(teamMember.UserId);

			return Json(new {
				teamMember.Id,
				User=teamMember.User.Name+" "+teamMember.User.Surname,
				teamMember.User.Photo,
				teamMember.User.Position
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult MemberDelete(int MemId)
		{

			if (!ModelState.IsValid)
			{
				Response.StatusCode = 400;

				var errorList = ModelState.Values.SelectMany(m => m.Errors)
								 .Select(e => e.ErrorMessage)
								 .ToList();

				return Json(errorList, JsonRequestBehavior.AllowGet);
			}

			TeamMember mem = db.TeamMembers.Find(MemId);

			db.TeamMembers.Remove(mem);
			db.SaveChanges();
			return Json("", JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult TeamDelete(string Slug)
		{
			Team team = db.Teams.FirstOrDefault(t=> t.Slug == Slug);

			if (team == null)
			{
				Response.StatusCode = 404;
				return Json(new
				{
					message = "Not Found!"
				}, JsonRequestBehavior.AllowGet);
			}

			var mems = db.TeamMembers.Where(tm => tm.TeamId == team.Id).ToList();
			foreach (var mem in mems)
			{
				db.TeamMembers.Remove(mem);

			}
			db.SaveChanges();
			db.Teams.Remove(team);
			db.SaveChanges();
			Activity act = new Activity()
			{
				UserId = team.UserId,
				Desc = "delete team " + team.Name,
				Date = DateTime.Now
			};
			db.Activities.Add(act);
			db.SaveChanges();
			return Json("", JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult TeamDetails(int TeamId)
		{
			Team team = db.Teams.FirstOrDefault(t => t.Id == TeamId);
			if (team == null)
			{
				Response.StatusCode = 404;
				return Json(new {
						message ="Not Found!"
					}, JsonRequestBehavior.AllowGet);
			}

			
			
			return Json(new
			{
				team.Id,
				team.Name,
				team.Slug
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult TeamEdit(Team teams)
		{

			if (!ModelState.IsValid)
			{
				Response.StatusCode = 400;

				var errorList = ModelState.Values.SelectMany(m => m.Errors)
								 .Select(e => e.ErrorMessage)
								 .ToList();

				return Json(errorList, JsonRequestBehavior.AllowGet);
			}
			Team team = db.Teams.FirstOrDefault(t => t.Id == teams.Id);
			team.Name = teams.Name;
			db.Entry(team).State = EntityState.Modified;
			db.SaveChanges();
			Activity act = new Activity()
			{
				UserId = team.UserId,
				Desc = "uptade team " + team.Name,
				Date = DateTime.Now
			};
			db.Activities.Add(act);
			db.SaveChanges();
			return Json(new
			{
				team.Id,
				team.Name,
				team.Slug
			}, JsonRequestBehavior.AllowGet);
		}
	}
}