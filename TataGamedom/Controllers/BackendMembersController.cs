﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TataGamedom.Models.Dtos;
using TataGamedom.Models.EFModels;
using TataGamedom.Models.Infra;
using TataGamedom.Models.Infra.DapperRepositories;
using TataGamedom.Models.Interfaces;
using TataGamedom.Models.Services;
using TataGamedom.Models.ViewModels;
using TataGamedom.Models.ViewModels.Members;

namespace TataGamedom.Controllers
{
	[Flags]
	public enum Roles
	{
		Visitor = 0, Tataboss = 1, NNtata = 2, Producttata = 3, Ordertata = 4, Newstata = 5, Faqtata = 6, Memberstata = 7
	}
	public class BackendMembersController : Controller
    {
		private AppDbContext db = new AppDbContext();

		// GET: BackendMembers
		[Authorize]
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult LoginTest()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LoginTest(LoginVM vm)
		{
			if (ModelState.IsValid == false) return View();
			Result result = ValidLogin(vm);

			if (result.IsSuccess != true)
			{
				ModelState.AddModelError("", result.ErrorMessage);
				return View(vm);
			}
			const bool rememberMe = false;

			var processResult = ProcessLogin(vm.Account, rememberMe);
			Response.Cookies.Add(processResult.cookie);

			// 在登录成功后的逻辑中获取BackendMembersRoleId，并存储在Session中
			int backendMembersRoleId = GetBackendMembersRoleIdByUsername(vm.Account); // 根据用户名查询BackendMembersRoleId的逻辑，你需要根据实际情况实现该方法
			HttpContext.Session["BackendMembersRoleId"] = backendMembersRoleId;

			return Redirect(processResult.returnUrl);
		}



		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginVM vm)
		{
			if (ModelState.IsValid == false) return View();
			Result result = ValidLogin(vm);

			if (result.IsSuccess != true)
			{
				ModelState.AddModelError("", result.ErrorMessage);
				return View(vm);
			}
			const bool rememberMe = false;

			var processResult = ProcessLogin(vm.Account, rememberMe);
			Response.Cookies.Add(processResult.cookie);

			// 在登录成功后的逻辑中获取BackendMembersRoleId，并存储在Session中
			int backendMembersRoleId = GetBackendMembersRoleIdByUsername(vm.Account); // 根据用户名查询BackendMembersRoleId的逻辑，你需要根据实际情况实现该方法
			HttpContext.Session["BackendMembersRoleId"] = backendMembersRoleId;

			return Redirect(processResult.returnUrl);
		}



		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public ActionResult Login(LoginVM vm)
		//{
		//	if (ModelState.IsValid == false) return View();
		//	Result result = ValidLogin(vm);

		//	if (result.IsSuccess != true)
		//	{
		//		ModelState.AddModelError("", result.ErrorMessage);
		//		return View(vm);
		//	}
		//	const bool rememberMe = false;

		//	var processResult = ProcessLogin(vm.Account, rememberMe);
		//	Response.Cookies.Add(processResult.cookie);
		//	return Redirect(processResult.returnUrl);
		//}

		public ActionResult Logout()
		{
			Session.Abandon();
			FormsAuthentication.SignOut();
			return Redirect("/BackendMembers/Login");
		}

		[Authorize]
		public ActionResult EditProfile()
		{
			var currentUserAccount = User.Identity.Name;

			var model = GetBackendMemberProfile(currentUserAccount);

			return View(model);
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditProfile(EditProfileVM vm)
		{
			var currentUserAccount = User.Identity.Name;

			if (ModelState.IsValid == false) return View();

			Result updateResult = UpdateProfile(vm);
			if (updateResult.IsSuccess) return RedirectToAction("Index");

			ModelState.AddModelError(string.Empty, updateResult.ErrorMessage);
			return View(vm);
		}
		
		[Authorize]
		public ActionResult EditPassword()
		{
			return View();
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditPassword(EditPasswordVM vm)
		{
			if (ModelState.IsValid == false) return View(vm);
			var currentUserAccount = User.Identity.Name;
			Result result = ChangePassword(currentUserAccount, vm);
			if (result.IsSuccess == false)
			{
				ModelState.AddModelError(string.Empty, result.ErrorMessage);
				return View(vm);
			}
			return RedirectToAction("Index");
		}

		public ActionResult NotAuthorize()
		{
			return View();
		}

		//public ActionResult Register()
		//{
		//	return View();
		//}

		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public ActionResult Register(RegisterVM vm)
		//{

		//	ViewBag.BackendMembersRoleId = new SelectList(db.BackendMembersRolesCodes, "Id", "Name");

		//	if (ModelState.IsValid == false) return View(vm);


		//	// 建立新會員
		//	Result result = RegisterBackendmember(vm);
		//	if (result.IsSuccess)
		//	{

		//		// 生成email連結
		//		var urlTemplate = Request.Url.Scheme + "://" +
		//			Request.Url.Authority + "/" +
		//			"Members/ActiveRegister?memberId={0}&confirmCode={1}";
		//		// 若成功，寄送郵件
		//		Result mailResult = ProcessRegister(vm.Account, vm.Email, urlTemplate);

		//		if (mailResult.IsSuccess)
		//		{
		//			vm.RegistrationDate = DateTime.Now;
		//			return View("ConfirmRegister");
		//		}
		//		else
		//		{
		//			ModelState.AddModelError(string.Empty, mailResult.ErrorMessage);
		//			return View(vm);
		//		}
		//	}
		//	else
		//	{
		//		ModelState.AddModelError(string.Empty, result.ErrorMessage);
		//		return View(vm);
		//	}
		//}


		//分一下 之後再改3層

		//private Result RegisterBackendmember(RegisterVM vm)
		//{
		//	IBackendMemberRepositiry repo = new BackendMemberDapperRepository();
		//	BackendMemberService service = new BackendMemberService(repo);
		//	return service.Register(vm.ToDto());
		//}

		//private Result ProcessRegister(string account, string email, string urlTemplate)
		//{
		//	var db = new AppDbContext();

		//	//檢查account, email正確性
		//	var memberInDb = db.Members.FirstOrDefault(m => m.Account == account);

		//	if (memberInDb == null) return Result.Fail("帳號或email錯誤"); //故意不告知確切錯誤原因

		//	if (string.Compare(email, memberInDb.Email, StringComparison.CurrentCultureIgnoreCase) != 0) return Result.Fail("帳號或email錯誤");

		//	//更新紀錄，重給一個confirmCode
		//	var confirmCode = Guid.NewGuid().ToString("N");
		//	memberInDb.ConfirmCode = confirmCode;
		//	db.SaveChanges();

		//	//發email
		//	var url = string.Format(urlTemplate, memberInDb.Id, confirmCode);
		//	new EmailHelper().SendConfirmRegisterEmail(url, memberInDb.Name, email);
		//	return Result.Success();
		//}




		private EditProfileVM GetBackendMemberProfile(string account)
		{
			IBackendMemberRepositiry repo = new BackendMemberDapperRepository();
			BackendMemberService service = new BackendMemberService(repo);
			return service.GetBackendMemberProfile(account);
		}

		private Result ChangePassword(string account, EditPasswordVM vm)
		{
			var salt = HashUtility.GetSalt();
			var hashOrigPwd = HashUtility.ToSHA256(vm.OringinalPassword, salt);

			var db = new AppDbContext();

			var memberInDb = db.BackendMembers.FirstOrDefault(m => m.Account == account && m.Password == hashOrigPwd);
			if (memberInDb == null) return Result.Fail("找不到要修改的會員紀錄");

			var hashPwd = HashUtility.ToSHA256(vm.CreatePassword, salt);

			memberInDb.Password = hashPwd;
			db.SaveChanges();

			return Result.Success();
		}

		private Result UpdateProfile(EditProfileVM vm)
		{
			// 取得在db裡的原始記錄
			var db = new AppDbContext();

			var currentUserAccount = User.Identity.Name;
			var memberInDb = db.BackendMembers.FirstOrDefault(m => m.Account == currentUserAccount);
			if (memberInDb == null) return Result.Fail("找不到要修改的會員記錄");

			// 更新記錄
			memberInDb.Name = vm.Name;
			memberInDb.Email = vm.Email;
			memberInDb.Phone = vm.Phone;

			db.SaveChanges();

			return Result.Success();
		}

		private Result ValidLogin(LoginVM vm)
		{
			IBackendMemberRepositiry repo = new BackendMemberDapperRepository();
			BackendMemberService service = new BackendMemberService(repo);
			return service.ValidLogin(vm.ToDto());
		}
		private (string returnUrl, HttpCookie cookie) ProcessLogin(string account, bool rememberMe)
		{
			var roles = string.Empty; // 在本範例, 沒有用到角色權限,所以存入空白

			// 建立一張認證票
			var ticket =
				new FormsAuthenticationTicket(
					1,          // 版本別, 沒特別用處
					account, 
					DateTime.Now,   // 發行日
					DateTime.Now.AddDays(31), // 到期日
					rememberMe,     // 是否續存
					roles,          // userdata
					"/" // cookie位置
				);

			// 將它加密
			var value = FormsAuthentication.Encrypt(ticket);

			// 存入cookie
			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, value);

			//var url = "/BackendMembers/Index/";
			// 取得return url
			var url = FormsAuthentication.GetRedirectUrl(account, true); //第二個引數沒有用處  
			return (url, cookie);  //會跳回MEMBER

		}

		private int GetBackendMembersRoleIdByUsername(string account)
		{
			using (var db = new AppDbContext()) // 用你的DbContext替代YourDbContext
			{
				var backendMember = db.BackendMembers.FirstOrDefault(m => m.Account == account);
				if (backendMember != null)
				{
					return backendMember.BackendMembersRoleId;
				}
			}

			// 若未找到对应的BackendMember记录，则返回一个默认的BackendMembersRoleId
			return 0; // 或者其他你认为合适的默认值
		}
	}
}