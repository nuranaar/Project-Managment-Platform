﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PMP.ViewModels;
using PMP.Models;
using System.IO;

namespace PMP.Controllers
{
	public class TaskController : BaseController
	{
		// GET: Task
		public ActionResult Index(string Slug)
		{

			TaskVm model = new TaskVm()
			{
				Users = db.Users.ToList(),
				Task = db.Tasks.FirstOrDefault(t => t.Slug == Slug),
				Activities = db.Activities.ToList()
			};
			model.Checklists = db.Checklists.Where(cl => cl.TaskId == model.Task.Id).OrderByDescending(cl=>cl.Id).ToList();
			model.Notes = db.Notes.Where(n => n.TaskId == model.Task.Id).OrderByDescending(n => n.Id).ToList();
			model.Files = db.Files.Where(f => f.TaskId == model.Task.Id).ToList();
			model.TaskMembers = db.TaskMembers.Where(tm => tm.TaskId == model.Task.Id).ToList();
			return View(model);
		}
		public JsonResult GetStages()
		{
			var stages = db.TaskStages.Select(ts => new
			{
				ts.Id,
				ts.Name
			});

			return Json(stages, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult TaskCreate(Task task,
							   TaskMember taskMember,
							   Models.File startfile,
							   HttpPostedFileBase fileBase,
							   string member)
		{

			if (!ModelState.IsValid)
			{

				Response.StatusCode = 400;

				var errorList = ModelState.Values.SelectMany(m => m.Errors)
								 .Select(e => e.ErrorMessage)
								 .ToList();

				return Json(errorList, JsonRequestBehavior.AllowGet);
			}
			if (fileBase == null)
			{
				ModelState.AddModelError("file", "Please select file");

			}
			string date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
			string filename = date + fileBase.FileName;
			string path = Path.Combine(Server.MapPath("~/Uploads"), filename);
			startfile.UserId = 1;
			fileBase.SaveAs(path);
			startfile.Name = filename;
			startfile.Weight = fileBase.ContentLength.ToString() + "-mb";
			startfile.Type = fileBase.ContentType;

			


			task.UserId = 1;
			db.Tasks.Add(task);
			db.SaveChanges();

			startfile.TaskId = task.Id;
			db.Files.Add(startfile);
			db.SaveChanges();

			string[] emails = member.Split(' ');
			foreach (var email in emails)
			{
				string e = email.Split(',', '\t', ';')[0];
				taskMember.TaskId = task.Id;
				var usr = db.Users.FirstOrDefault(u => u.Email == e);
				if (usr != null)
				{
					taskMember.UserId = usr.Id;
				}
				db.TaskMembers.Add(taskMember);
				db.SaveChanges();
			}
			task.TaskStage = db.TaskStages.Find(task.TaskStageId);
			return Json(new
			{
				task.Id,
				task.Slug,
				Stage = task.TaskStage.Name
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult ChecklistCreate(Checklist checklist, bool Check)
		{
			if (!ModelState.IsValid)
			{
				Response.StatusCode = 400;
				var errorList = ModelState.Values.SelectMany(m => m.Errors)
								 .Select(e => e.ErrorMessage)
								 .ToList();
				return Json(errorList, JsonRequestBehavior.AllowGet);
			}
			checklist.Checked = Check;
			db.Checklists.Add(checklist);
			db.SaveChanges();

			return Json(new
			{
				checklist.Checked,
				checklist.Id,
				checklist.Text
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult NoteCreate(Note note, int TaskId)
		{
			if (!ModelState.IsValid)
			{
				Response.StatusCode = 400;
				var errorList = ModelState.Values.SelectMany(m => m.Errors)
								 .Select(e => e.ErrorMessage)
								 .ToList();
				return Json(errorList, JsonRequestBehavior.AllowGet);
			}

			note.TaskId = TaskId;
			db.Notes.Add(note);
			db.SaveChanges();

			return Json(new
			{
				note.Title,
				note.Id,
				note.Desc
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult FileUpload(Models.File startfile,
							   HttpPostedFileBase fileBase,
							   int TaskId)
		{
			if (!ModelState.IsValid)
			{
				Response.StatusCode = 400;
				var errorList = ModelState.Values.SelectMany(m => m.Errors)
								 .Select(e => e.ErrorMessage)
								 .ToList();
				return Json(errorList, JsonRequestBehavior.AllowGet);
			}

			if (fileBase == null)
			{
				ModelState.AddModelError("file", "Please select file");

			}
			string date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
			string filename = date + fileBase.FileName;
			string path = Path.Combine(Server.MapPath("~/Uploads"), filename);
			fileBase.SaveAs(path);

			startfile.TaskId = TaskId;
			startfile.UserId = 1;
			startfile.Name = filename;
			startfile.Weight = fileBase.ContentLength.ToString() + "-mb";
			startfile.Type = fileBase.ContentType;

			db.Files.Add(startfile);
			db.SaveChanges();
			return Json(new
			{
				startfile.Id,
				startfile.Name,
				startfile.Weight
			}, JsonRequestBehavior.AllowGet);
		}
	}

}