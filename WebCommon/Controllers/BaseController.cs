using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tools.Models;
using Tools.Response.Json;
using We7Tools.Extend;

namespace WebCommon.Controllers
{
    public class BaseController<T, P> : Controller where T : BaseData<P>
    {
        protected T thisData;
        protected bool hasIdentity;
        protected string UniacID
        {
            get
            {
                var uniacid = Request.Headers["uniacid"];
                return uniacid;
            }
        }
        protected BaseController(bool hasIdentity = false)
        {
            thisData = Activator.CreateInstance<T>();
            this.hasIdentity = hasIdentity;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (hasIdentity)
            {
                if (!context.HttpContext.Session.HasWe7Data())
                {
                    context.Result = GoUserTimeOutPage();
                    return;
                }
            }
            base.OnActionExecuting(context);
        }

        protected IActionResult DoJsonObjData<M>(Action<string, M> doData)
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                M qiNiuModel = JsonConvert.DeserializeObject<M>(json);
                doData(HttpContext.Session.GetUniacID(), qiNiuModel);
                return this.JsonSuccessStatus();
            }
            catch (ExceptionModel em)
            {
                return this.JsonOtherStatus(em.ExceptionParam);
            }
            catch (Exception)
            {
                return this.JsonErrorStatus();
            }
        }
        /// <summary>
        /// 获取session中的uniacid
        /// </summary>
        /// <returns>uniacid</returns>
        protected string GetSessionUniacID()
        {
            var uniacid = HttpContext.Session.GetUniacID();

            return uniacid;
        }

        /// <summary>
        /// 执行api数据 返回状态
        /// </summary>
        /// <typeparam name="K">参数类型</typeparam>
        /// <param name="action">要执行的数据方法</param>
        /// <param name="k">参数实体</param>
        /// <returns>执行状态</returns>

        public static IActionResult GoUserTimeOutPage()
        {
            return new RedirectToActionResult("Index", "WebError", new { errorType = ErrorType.ErrorNoUserOrTimeOut });
        }



        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="k"></param>
        /// <param name="requestClientType"></param>
        /// <param name="action"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        protected IActionResult DoData<K>(K k, RequestClientType requestClientType = RequestClientType.Api, Action<string, K> action = null, Func<string, K, IActionResult> func = null)
        {
            try
            {
                var uniacid = "";
                IActionResult iar;
                CheckUniacID(ref uniacid, requestClientType, out iar);
                if (iar != null)
                    return iar;
                if (action != null)
                {
                    action(uniacid, k);
                    return this.JsonSuccessStatus();
                }
                if (func != null)
                {
                    return func(uniacid, k);
                }
                return this.JsonErrorStatus();
            }
            catch (ExceptionModel em)
            {
                return this.JsonOtherStatus(em.ExceptionParam);
            }
            catch (Exception e)
            {
                e.Save();
                return this.JsonErrorStatus();
            }
        }

        private void CheckUniacID(ref string uniacid, RequestClientType requestClientType, out IActionResult iar)
        {
            iar = null;
            switch (requestClientType)
            {
                case RequestClientType.Api:
                    uniacid = UniacID;
                    if (string.IsNullOrEmpty(uniacid))
                        throw new ExceptionModel { ExceptionParam = Tools.Response.ResponseStatus.uniacid为空 };
                    iar = null;
                    break;
                case RequestClientType.Web:
                    uniacid = GetSessionUniacID();
                    if (string.IsNullOrEmpty(uniacid))
                        iar = GoUserTimeOutPage();
                    break;
                case RequestClientType.Both:
                    uniacid = GetSessionUniacID() ?? UniacID;
                    if (string.IsNullOrEmpty(uniacid))
                        iar = GoUserTimeOutPage();
                    break;
                default:
                    iar = null;
                    break;
            }
        }

        protected enum RequestClientType
        {
            Api = 0, Web = 1, Both = 3
        }
    }
}
